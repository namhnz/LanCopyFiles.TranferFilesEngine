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
    private readonly int _port;
    private string _saveTo;

    public TFEServer(int port, string saveTo)
    {
        _port = port;

        // Neu saveTo la null hoac empty thi lay folder mac dinh la desktop
        var folderForSaving = string.IsNullOrEmpty(saveTo)
            ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            : saveTo;
        if (!folderForSaving.EndsWith(@"\"))
        {
            folderForSaving += @"\";
        }

        _saveTo = folderForSaving;
    }

    public void StartServer()
    {
        RunServerAsync();
    }

    private async Task RunServerAsync()
    {
        FileWriterEx fileWriter = null;

        var server = new AsyncTcpListener
        {
            IPAddress = IPAddress.IPv6Any,
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
                        var initializeByte = serverClient.ByteBuffer.Dequeue(1)[0];
                        if (initializeByte == 2)
                        {
                            var cmdBuffer = serverClient.ByteBuffer.Dequeue(3);

                            var separatorByte = serverClient.ByteBuffer.Dequeue(1)[0];

                            var dataReceivedBytes = serverClient.ByteBuffer.Dequeue(count - 5);

                            if (separatorByte == 4)
                            {
                                var cmdNum = Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer));

                                switch (cmdNum)
                                {
                                    case 101:

                                        break;
                                    case 125:
                                    {
                                        fileWriter =
                                            new FileWriterEx(@"" + _saveTo +
                                                             Encoding.UTF8.GetString(dataReceivedBytes));

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
                                            await fileWriter.WritePartAsync(dataReceivedBytes);
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
                                        // Let the server close the connection
                                        serverClient.Disconnect();
                                    }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }.RunAsync()
        };

        server.Message += (s, a) => Debug.WriteLine("Server: " + a.Message);
        var serverTask = server.RunAsync();
        // await serverTask;
    }

    private byte[] CreateDataPacket(byte[] cmd, byte[] data)
    {
        byte[] initialize = new byte[1];
        initialize[0] = 2;
        byte[] separator = new byte[1];
        separator[0] = 4;
        // byte[] dataLength = Encoding.UTF8.GetBytes(Convert.ToString(data.Length));
        MemoryStream ms = new MemoryStream();
        ms.Write(initialize, 0, initialize.Length);
        ms.Write(cmd, 0, cmd.Length);
        // ms.Write(dataLength, 0, dataLength.Length);
        ms.Write(separator, 0, separator.Length);
        ms.Write(data, 0, data.Length);

        return ms.ToArray();
    }
}