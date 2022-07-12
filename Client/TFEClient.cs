using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LanCopyFiles.TransferFilesEngine.Helpers;
using Unclassified.Net;
using Timer = System.Timers.Timer;

namespace LanCopyFiles.TransferFilesEngine.Client
{
    public class TFEClient
    {
        private readonly int _port;

        public TFEClient(int port)
        {
            _port = port;
        }

        public Task StartClient(string selectedFilePath)
        {
            return RunClientAsync(selectedFilePath);
        }

        /// <summary>
        /// Demonstrates the client and server by using the classes directly with callback methods.
        /// </summary>
        /// <returns></returns>
        private Task RunClientAsync(string filePath)
        {
            FileReaderEx fileReader = null;

            var client = new AsyncTcpClient
            {
                IPAddress = IPAddress.IPv6Loopback,
                Port = _port,
                //AutoReconnect = true,
                MaxConnectTimeout = TimeSpan.FromMinutes(60),
                ConnectedCallback = async (c, isReconnected) =>
                {
                    Debug.WriteLine("Client connected to server");
                    // Gui ten file
                    // var filePath = @"C:\";

                    // Debug.WriteLine("Client byte buffer size: " + c.ByteBuffer.Capacity);

                    // var timer = new Timer();
                    // timer.Interval = 1000;
                    // timer.AutoReset = true;
                    // timer.Elapsed += (sender, args) =>
                    // {
                    //     Debug.WriteLine("Client is closing? " + c.IsClosing);
                    // };
                    // timer.Start();

                    try
                    {
                        string selectedFile = filePath;

                        fileReader = new FileReaderEx(selectedFile);

                        string fileName = Path.GetFileName(selectedFile);

                        byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("125"),
                            Encoding.UTF8.GetBytes(fileName));

                        // Truyen den server
                        await c.Send(new ArraySegment<byte>(dataToSend, 0, dataToSend.Length));

                        //// Wait for server response or closed connection
                        //await c.ByteBuffer.WaitAsync();
                        //if (c.IsClosing)
                        //{
                        //    break;
                        //}

                        while (true)
                        {
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
                                // Debug.WriteLine("Line 97: Client received from server command: " + serverCommandNum +
                                //                 " Description: " +
                                //                 TransferCodeDescription.GetDescription(serverCommandNum.Value));

                                // Debug.WriteLine("Client: sending progress: " + fileReader.ProgressValue * 100 + "%");

                                var fileReaderResult = await fileReader.ReadPartAsync();

                                // Debug.WriteLine("Client send: data length: " + fileReaderResult.DataRead.Length);

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
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                // NOTE: The client connection will NOT be closed automatically when this method
                //       returns. It has to be closed explicitly when desired.
                },
                ReceivedCallback = (c, count) =>
                {
                    try
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

                                // Debug.WriteLine("Line 151: Client received from server command: " + cmdNum + " Description: " + TransferCodeDescription.GetDescription(cmdNum));

                                fileReader.ReceiveFilePointer = long.Parse(Encoding.UTF8.GetString(dataBytes));

                                ServerCommandHandlerEx.SetCommandNum(cmdNum);
                            }
                        }

                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        throw;
                    }
                }
            };

            client.Message += (s, a) => Debug.WriteLine("Client: " + a.Message);
            var clientTask = client.RunAsync();

            // client.Dispose();
            // await clientTask;

            return clientTask;
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

            // Debug.WriteLine("Client create data package: cmd: " + Encoding.UTF8.GetString(cmd));

            // ms.Write(dataLength, 0, dataLength.Length);
            ms.Write(separator, 0, separator.Length);
            ms.Write(data, 0, data.Length);

            // Debug.WriteLine("Client create data package: data length: " + data.Length);

            return ms.ToArray();
        }
    }
}