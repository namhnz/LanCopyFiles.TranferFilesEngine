using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Unclassified.Net;

namespace LanCopyFiles.TransferFilesEngine.Server;

public class TFEServer
{
    #region Events

    // Nguon: https://stackoverflow.com/a/85188/7182661
    public event EventHandler<TFEServerReceivingArgs> StartReceivingEvent;

    public void OnStartReceiving(string receivingFileName, string fromIPAddress)
    {
        EventHandler<TFEServerReceivingArgs> handler = StartReceivingEvent;

        if (null != handler)
            handler(this, new TFEServerReceivingArgs()
            {
                FileName = receivingFileName,
                FromIPAddress = fromIPAddress
            });
    }

    public event EventHandler<TFEServerReceivingArgs> FinishReceivingEvent;

    public void OnFinishReceiving(string receivingFileName, string fromIPAddress)
    {
        EventHandler<TFEServerReceivingArgs> handler = FinishReceivingEvent;
        if (null != handler)
            handler(this, new TFEServerReceivingArgs()
            {
                FileName = receivingFileName,
                FromIPAddress = fromIPAddress
            });
    }

    #endregion

    private readonly int _port;
    private string _saveTo;

    public TFEServer(string saveTo, int port)
    {

        if (port < 0 || port >= 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "The port value must be from 0 to 65,534");
        }

        _port = port;

        // Neu saveTo la null hoac empty thi lay folder mac dinh la desktop
        var folderForSaving = string.IsNullOrEmpty(saveTo)
            ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            : saveTo;
        if (!folderForSaving.EndsWith(@"\"))
        {
            folderForSaving += @"\";
        }

        if (!Directory.Exists(saveTo))
        {
            throw new DirectoryNotFoundException("The folder to save files is not found");
        }

        _saveTo = folderForSaving;
    }

    public Task StartServer()
    {
        return RunServer();
    }

    private Task RunServer()
    {
        FileWriterEx fileWriter = null;
        string receiveFileName = string.Empty;

        var server = new AsyncTcpListener
        {
            Port = _port,
            ClientConnectedCallback = tcpClient =>
                new AsyncTcpClient
                {
                    ServerTcpClient = tcpClient,

                    ConnectedCallback = async (serverClient, isReconnected) =>
                    {
                        Debug.WriteLine($"New connection from: {tcpClient.Client.RemoteEndPoint}");
                    },
                    ReceivedCallback = async (serverClient, count) =>
                    {
                        // Lay dia chi client gui den server
                        var sourceIPAddress = tcpClient.Client.RemoteEndPoint.ToString();

                        try
                        {
                            // Doc thong tin tu client gui den server, dinh dang thong du lieu: 2 cmd data-length 4 data-bytes

                            // Lay byte dau tien cua du lieu nhan, neu bat dau bang 2 la du lieu do client gui den
                            var initializeByte = serverClient.ByteBuffer.Dequeue(1)[0];
                            if (initializeByte == 2)
                            {
                                // Lay command tu client gui den server, bao gom: 127, 128 (co 3 ky tu, do dai 3 byte) 
                                var cmdBuffer = await serverClient.ByteBuffer.DequeueAsync(3);

                                // Lay do dai du lieu tu client gui den
                                int dataLengthTempByte = 0;
                                string dataReceiveLengthString = "";

                                while ((dataLengthTempByte = (await serverClient.ByteBuffer.DequeueAsync(1))[0]) != 4)
                                {
                                    dataReceiveLengthString += (char)dataLengthTempByte;
                                }

                                var dataReceiveLengthInt = Convert.ToInt32(dataReceiveLengthString);

                                // // Lay byte separator
                                // var separatorByte = (await serverClient.ByteBuffer.DequeueAsync(1))[0];

                                byte[] dataReceivedBuffer;
                                // Tru cho 1 byte khoi tao, 3 byte cmd, cac byte chua so luong byte client gui den, 1 byte separator da dequeue them
                                var bytesReceivedLeft = count - 1 - 3 - dataReceiveLengthString.Length - 1;

                                if (bytesReceivedLeft < dataReceiveLengthInt)
                                {
                                    dataReceivedBuffer = await serverClient.ByteBuffer.DequeueAsync(bytesReceivedLeft);
                                }
                                else
                                {
                                    dataReceivedBuffer =
                                        await serverClient.ByteBuffer.DequeueAsync(dataReceiveLengthInt);
                                }
                                // TODO: kiem tra lan gui cuoi cung xem da gui du byte hay chua


                                var cmdNum = Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer));


                                switch (cmdNum)
                                {
                                    // case 101:
                                    //     break;
                                    case 125:
                                    {

                                        // Lay ten file duoc gui tu client
                                        string fileName =Encoding.UTF8.GetString(dataReceivedBuffer);
                                        receiveFileName = fileName;

                                        // Thong bao ten file va dia chi may gui file
                                        OnStartReceiving(fileName, sourceIPAddress);

                                        fileWriter = new FileWriterEx(@"" + _saveTo + fileName);

                                        var dataToSendBytes = CreateDataPacket(Encoding.UTF8.GetBytes("126"),
                                            Encoding.UTF8.GetBytes(Convert.ToString(fileWriter.CurrentFilePointer)));

                                        await serverClient.Send(new ArraySegment<byte>(dataToSendBytes, 0,
                                            dataToSendBytes.Length));
                                    }
                                        break;
                                    case 127:
                                    {
                                        if (fileWriter != null)
                                        {
                                            await fileWriter.WritePartAsync(dataReceivedBuffer);
                                            var dataToSendBytes = CreateDataPacket(Encoding.UTF8.GetBytes("126"),
                                                Encoding.UTF8.GetBytes(
                                                    Convert.ToString(fileWriter.CurrentFilePointer)));

                                            await serverClient.Send(new ArraySegment<byte>(dataToSendBytes, 0,
                                                dataToSendBytes.Length));
                                        }
                                    }
                                        break;
                                    case 128:
                                    {
                                        if (fileWriter != null)
                                        {
                                            fileWriter.Close();
                                        }

                                        // Let the server close the connection
                                        serverClient.Disconnect();

                                        OnFinishReceiving(receiveFileName, sourceIPAddress);
                                    }
                                        break;
                                    default:
                                        break;

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);

                            OnFinishReceiving(receiveFileName, sourceIPAddress);
                            serverClient.Disconnect();
                            throw;
                        }
                    }
                }.RunAsync()
        };

        server.Message += (s, a) => Debug.WriteLine("Server: " + a.Message);
        var serverTask = server.RunAsync();

        return serverTask;
    }

    private byte[] CreateDataPacket(byte[] cmd, byte[] data)
    {
        byte[] initialize = new byte[1];
        initialize[0] = 2;
        byte[] separator = new byte[1];
        separator[0] = 4;
        byte[] dataLength = Encoding.UTF8.GetBytes(Convert.ToString(data.Length));
        MemoryStream ms = new MemoryStream();
        ms.Write(initialize, 0, initialize.Length);
        ms.Write(cmd, 0, cmd.Length);
        ms.Write(dataLength, 0, dataLength.Length);
        ms.Write(separator, 0, separator.Length);
        ms.Write(data, 0, data.Length);

        return ms.ToArray();
    }
}