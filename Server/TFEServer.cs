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
                    MaxConnectTimeout = TimeSpan.FromMinutes(60),
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
                            var initializeByte = serverClient.ByteBuffer.Dequeue(1)[0];
                            if (initializeByte == 2)
                            {
                                // Doc thong tin tu client gui den server

                                var cmdBuffer = await serverClient.ByteBuffer.DequeueAsync(3);

                                var separatorByte = (await serverClient.ByteBuffer.DequeueAsync(1))[0];

                                var dataReceivedBytes = await serverClient.ByteBuffer.DequeueAsync(count - 5);

                                int b = 0;

                                string buffLength = "";

                                while ((b = serverClient.ByteBuffer.Dequeue(1)[0]) != 4)
                                {
                                    buffLength += (char)b;
                                }

                                int dataLength = Convert.ToInt32(buffLength);

                                var dataBuff = new byte[dataLength];

                                int byteRead = 0;
                                int byteOffset = 0;
                                while (byteOffset < dataLength)
                                {
                                    byteRead = ns.Read(dataBuff, byteOffset, dataLength - byteOffset);
                                    byteOffset += byteRead;

                                    var dataRead = await serverClient.ByteBuffer.DequeueAsync(dataLength - byteOffset);
                                }


                                // Debug.WriteLine("Server received: data length: " + dataReceivedBytes.Length);

                                if (separatorByte == 4)
                                {
                                    var cmdNum = Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer));

                                    // Debug.WriteLine("Line 71: Server received from client command: " + cmdNum + " Description: " + TransferCodeDescription.GetDescription(cmdNum));


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
                                    }
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