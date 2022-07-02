using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LanCopyFiles.TransferFilesEngine.Client;
using Unclassified.Net;

namespace LanCopyFiles.TransferFilesEngine.Server;

public class TFEServer
{
    private async Task RunServer()
    {
        int port = 8085;

        FileWriterEx fileWriter = null;

        string saveTo = @"C:\";

        var server = new AsyncTcpListener
        {
            IPAddress = IPAddress.Any,
            Port = port,
            ClientConnectedCallback = tcpClient =>
                new AsyncTcpClient
                {
                    ServerTcpClient = tcpClient,
                    ConnectedCallback = async (serverClient, isReconnected) =>
                    {
                        // await Task.Delay(500);
                        // byte[] bytes = Encoding.UTF8.GetBytes($"Hello, {tcpClient.Client.RemoteEndPoint}, my name is Server. Talk to me.");
                        // await serverClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));

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

                                // receiveFilePointer = long.Parse(Encoding.UTF8.GetString(dataBytes));
                                //
                                // ClientCommandHandlerEx.SetCommandNum(cmdNum);

                                switch (cmdNum)
                                {
                                    case 101:
                                        //download++;
                                        return;
                                    case 125:
                                    {
                                        // fs = new FileStream(@"" + SaveTo + Encoding.UTF8.GetString(recv_data), FileMode.CreateNew);
                                        // byte[] data_to_send = CreateDataPacket(Encoding.UTF8.GetBytes("126"), Encoding.UTF8.GetBytes(Convert.ToString(current_file_pointer)));
                                        // ns.Write(data_to_send, 0, data_to_send.Length);
                                        // ns.Flush();

                                        fileWriter =
                                            new FileWriterEx(@"" + saveTo + Encoding.UTF8.GetString(dataReceivedBytes));

                                        var dataToSendBytes = CreateDataPacket(Encoding.UTF8.GetBytes("126"),
                                            Encoding.UTF8.GetBytes(Convert.ToString(fileWriter.CurrentFilePointer)));

                                        await serverClient.Send(new ArraySegment<byte>(dataToSendBytes, 0,
                                            dataToSendBytes.Length));
                                    }
                                        break;
                                    case 127:
                                    {
                                        // fs.Seek(current_file_pointer, SeekOrigin.Begin);
                                        // fs.Write(recv_data, 0, recv_data.Length);
                                        // current_file_pointer = fs.Position;
                                        // byte[] data_to_send = CreateDataPacket(Encoding.UTF8.GetBytes("126"), Encoding.UTF8.GetBytes(Convert.ToString(current_file_pointer)));
                                        // ns.Write(data_to_send, 0, data_to_send.Length);
                                        // ns.Flush();
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
                                        // fs.Close();
                                        // loop_break = true;

                                        // Let the server close the connection
                                        serverClient.Disconnect();
                                    }
                                        break;
                                    default:
                                        break;
                                }
                            }


                            // return Task.CompletedTask;
                        }


                        // byte[] bytes = serverClient.ByteBuffer.Dequeue(count);
                        // string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
                        // Console.WriteLine("Server client: received: " + message);
                        //
                        // bytes = Encoding.UTF8.GetBytes("You said: " + message);
                        // await serverClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));
                        //
                        // if (message == "bye")
                        // {
                        //     // Let the server close the connection
                        //     serverClient.Disconnect();
                        // }
                    }
                }.RunAsync()
        };
        // server.Message += (s, a) => Console.WriteLine("Server: " + a.Message);
        // var serverTask = server.RunAsync();
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