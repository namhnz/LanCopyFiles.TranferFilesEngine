using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using LanCopyFiles.TransferFilesEngine.Helpers;
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

    public Task StartServer()
    {
        return RunServer();
    }

    private Task RunServer()
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
                        
                        // Debug.WriteLine("Server byte buffer size: " + serverClient.ByteBuffer.Capacity);

                        // var timer = new Timer();
                        // timer.Interval = 1000;
                        // timer.AutoReset = true;
                        // timer.Elapsed += (sender, args) =>
                        // {
                        //     Debug.WriteLine("Server is closing? " + serverClient.IsClosing);
                        // };
                        // timer.Start();
                    },
                    ReceivedCallback = async (serverClient, count) =>
                    {
                        try
                        {
                            // Doc thong tin tu client gui den server, dinh dang thong du lieu: 2 cmd data-length 4 data-bytes
                            
                            // Lay byte dau tien cua du lieu nhan, neu bat dau bang 2 la du lieu do client gui den
                            var initializeByte = serverClient.ByteBuffer.Dequeue(1)[0];
                            if (initializeByte == 2)
                            {
                                Debug.WriteLine("Ser received new data, count: " + count);

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

                                // Lay du lieu duoc gui tu client duoi dang byte array
                                // int bytesLeft = dataReceiveLengthInt;
                                //
                                // var dataReceivedBuffer = new byte[dataReceiveLengthInt];
                                //
                                // int bufferSize = 1000;
                                // int bytesRead = 0;
                                //
                                // while (bytesLeft > 0)
                                // {
                                //     int curDataSize = Math.Min(bufferSize, bytesLeft);
                                //     if (count  < curDataSize)
                                //         curDataSize = count; //This saved me
                                //
                                //     // bytes = stream.Read(data, bytesRead, curDataSize);
                                //
                                //     dataReceivedBuffer = await serverClient.ByteBuffer.DequeueAsync(dataReceiveLengthInt);
                                //
                                //     bytesRead += curDataSize;
                                //     bytesLeft -= curDataSize;
                                // }

                                byte[] dataReceivedBuffer;
                                // Tru cho 1 byte khoi tao, 3 byte cmd, cac byte chua so luong byte client gui den, 1 byte separator da dequeue them
                                var bytesReceivedLeft = count - 1 - 3 - dataReceiveLengthString.Length - 1;

                                Debug.WriteLine("Byte left: " + bytesReceivedLeft);
                                Debug.WriteLine("Byte client send indicator: " + dataReceiveLengthInt);

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


                                // var dataReceivedBuffer = await serverClient.ByteBuffer.DequeueAsync(dataReceiveLengthInt);



                                #region temp

                                // int bytesLeft = dataReceiveLengthInt;
                                // int bytesRead = 0;
                                //
                                // while (bytesLeft > 0)
                                // {
                                //     int curDataSize = Math.Min(bufferSize, bytesLeft);
                                //     if (client.Available < curDataSize)
                                //         curDataSize = client.Available; //This saved me
                                //
                                //     bytes = stream.Read(data, bytesRead, curDataSize);
                                //
                                //     bytesRead += curDataSize;
                                //     bytesLeft -= curDataSize;
                                // }

                                // await serverClient.ByteBuffer.DequeueAsync(count - 5);

                                // int bytesRead = 0;
                                // int bytesOffset = 0;
                                // while (bytesOffset < dataReceiveLengthInt)
                                // {
                                //     bytesRead =
                                //         await serverClient.ByteBuffer.DequeueAsync(dataReceiveLengthInt - bytesOffset);
                                //     ns.Read(data_buff, bytesOffset, data_Length - bytesOffset);
                                //     bytesOffset += bytesRead;
                                // }




                                // int byteRead = 0;
                                // int byteOffset = 0;
                                // while (byteOffset < dataLength)
                                // {
                                //     byteRead = ns.Read(dataBuff, byteOffset, dataLength - byteOffset);
                                //     byteOffset += byteRead;
                                //
                                //     var dataRead = await serverClient.ByteBuffer.DequeueAsync(dataLength - byteOffset);
                                // }

                                #endregion


                                // Debug.WriteLine("Server received: data length: " + dataReceivedBytes.Length);

                                // if (separatorByte == 4)
                                // {
                                var cmdNum = Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer));

                                    Debug.WriteLine("Line 71: Server received from client command: " + cmdNum + " Description: " + TransferCodeDescription.GetDescription(cmdNum));


                                    switch (cmdNum)
                                    {
                                        // case 101:
                                        //
                                        //     break;
                                        case 125:
                                            {
                                                fileWriter =
                                                    new FileWriterEx(@"" + _saveTo +
                                                                     Encoding.UTF8.GetString(dataReceivedBuffer));

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
                                            }
                                            break;
                                        default:
                                            break;
                                    // }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
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