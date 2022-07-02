using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unclassified.Net;

namespace LanCopyFiles.TransferFilesEngine.Client
{
    public class TFEClient
    {
        /// <summary>
        /// Demonstrates the client and server by using the classes directly with callback methods.
        /// </summary>
        /// <returns></returns>
        private async Task RunClientAsync()
        {
            int port = 8085;

            FileReaderEx fileReader = null;

            var client = new AsyncTcpClient
            {
                IPAddress = IPAddress.Loopback,
                Port = port,
                //AutoReconnect = true,
                ConnectedCallback = async (c, isReconnected) =>
                {
                    Debug.WriteLine("Client connected to server");
                    // Gui ten file
                    var filePath = @"C:\";

                    string selectedFile = filePath;

                    fileReader = new FileReaderEx(selectedFile);

                    while (true)
                    {
                        string fileName = Path.GetFileName(selectedFile);

                        byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("125"),
                            Encoding.UTF8.GetBytes(fileName));

                        // Truyen den server
                        await c.Send(new ArraySegment<byte>(dataToSend, 0, dataToSend.Length));

                        // Wait for server response or closed connection
                        await c.ByteBuffer.WaitAsync();
                        if (c.IsClosing)
                        {
                            break;
                        }

                        var serverCommandHandlerCts = new CancellationTokenSource();

                        var serverCommandHandlerTask =
                            ServerCommandHandlerEx.GetCommandAsync(serverCommandHandlerCts.Token);

                        // Wait for receive stream or closed connection
                        var completedTask = await Task.WhenAny(serverCommandHandlerTask, c.ClosedTask);
                        if (completedTask == c.ClosedTask)
                        {
                            // Closed connection
                            serverCommandHandlerCts.Cancel();
                            break;
                        }

                        var serverCommandNum = await serverCommandHandlerTask;
                        if (serverCommandNum == 126)
                        {
                            var fileReaderResult = await fileReader.ReadPartAsync();

                            var fileDataToSendBytes =
                                CreateDataPacket(Encoding.UTF8.GetBytes(fileReaderResult.ReadResultNum.ToString()),
                                    fileReaderResult.DataRead);

                            await c.Send(new ArraySegment<byte>(fileDataToSendBytes, 0, fileDataToSendBytes.Length));

                            // Wait for server response or closed connection
                            await c.ByteBuffer.WaitAsync();
                            if (c.IsClosing)
                            {
                                break;
                            }
                        }
                        else
                        {
                            // Close the client connection
                            c.Disconnect();
                            break;
                        }
                    }
                    // NOTE: The client connection will NOT be closed automatically when this method
                    //       returns. It has to be closed explicitly when desired.
                },
                ReceivedCallback = (c, count) =>
                {
                    var initializeByte = c.ByteBuffer.Dequeue(1)[0];


                    if (initializeByte == 2)
                    {
                        var cmdBuffer = c.ByteBuffer.Dequeue(3);

                        var separatorByte = c.ByteBuffer.Dequeue(1)[0];

                        var dataBytes = c.ByteBuffer.Dequeue(count - 5);

                        if (separatorByte == 4 && fileReader != null)
                        {
                            var cmdNum = Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer));

                            fileReader.ReceiveFilePointer = long.Parse(Encoding.UTF8.GetString(dataBytes));

                            ServerCommandHandlerEx.SetCommandNum(cmdNum);
                        }
                    }

                    return Task.CompletedTask;
                }
            };

            client.Message += (s, a) => Console.WriteLine("Client: " + a.Message);
            var clientTask = client.RunAsync();

            client.Dispose();
            await clientTask;
        }

        private static byte[] CreateDataPacket(byte[] cmd, byte[] data)
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
}