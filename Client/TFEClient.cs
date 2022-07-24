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
        private readonly int _port;
        private readonly string _server;

        public int ProgressValue { get; set; }

        public TFEClient(string server, int port)
        {
            _server = server;
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
            var serverCommandHandlerCts = new CancellationTokenSource();

            var client = new AsyncTcpClient
            {
                IPAddress = /*IPAddress.IPv6Loopback*/ IPAddress.Parse(_server),
                Port = _port,
                //AutoReconnect = true,
                // AutoReconnect = true,
                ConnectedCallback = async (client, isReconnected) =>
                {
                    Debug.WriteLine("Client connected to server");

                    try
                    {
                        string selectedFile = filePath;

                        fileReader = new FileReaderEx(selectedFile);

                        string fileName = Path.GetFileName(selectedFile);

                        byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("125"),
                            Encoding.UTF8.GetBytes(fileName));

                        // Truyen den server
                        await client.Send(new ArraySegment<byte>(dataToSend, 0, dataToSend.Length));

                        while (true)
                        {

                            var serverCommandHandlerTask =
                                ServerCommandHandlerEx.GetCommandAsync(serverCommandHandlerCts.Token);
                            var completedTask = await Task.WhenAny(serverCommandHandlerTask, client.ClosedTask);

                            if (completedTask == client.ClosedTask)
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

                                await client.Send(new ArraySegment<byte>(fileDataToSendBytes, 0,
                                    fileDataToSendBytes.Length));

                                // Thiet dat tien trinh gui file
                                ProgressValue = fileReader.ReadingProgressValue;

                                // Wait for server response or closed connection
                                // await client.ByteBuffer.WaitAsync();
                                // if (client.IsClosing)
                                // {
                                //     break;
                                // }
                            }
                            else
                            {
                                // Close the client connection
                                client.Disconnect();
                                break;
                            }
                        }

                        client.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        serverCommandHandlerCts.Cancel();

                        Debug.WriteLine(ex);
                        throw;
                    }
                    // NOTE: The client connection will NOT be closed automatically when this method
                    //       returns. It has to be closed explicitly when desired.
                },
                ReceivedCallback = (client, count) =>
                {
                    try
                    {
                        // Doc thong tin tu server gui lai cho client, dinh dang thong du lieu: 2 cmd data-length 4 data-bytes

                        // Lay byte dau tien cua du lieu nhan, neu bat dau bang 2 la du lieu do client gui den
                        var initializeByte = client.ByteBuffer.Dequeue(1)[0];
                        if (initializeByte == 2)
                        {
                            // Lay command tu client gui den server, bao gom: 127, 128 (co 3 ky tu, do dai 3 byte) 
                            var cmdBuffer = client.ByteBuffer.Dequeue(3);

                            // Lay do dai du lieu tu client gui den
                            int dataLengthTempByte = 0;
                            string dataReceiveLengthString = "";

                            while ((dataLengthTempByte = client.ByteBuffer.Dequeue(1)[0]) != 4)
                            {
                                dataReceiveLengthString += (char)dataLengthTempByte;
                            }

                            var dataReceiveLengthInt = Convert.ToInt32(dataReceiveLengthString);

                            // Lay byte separator
                            // var separatorByte = client.ByteBuffer.Dequeue(1)[0];

                            // Lay du lieu duoc gui tu client duoi dang byte array
                            var dataReceivedBuffer = client.ByteBuffer.Dequeue(dataReceiveLengthInt);

                            if (fileReader != null)
                            {
                                var cmdNum = Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer));

                                fileReader.CurrentPointerPosition = long.Parse(Encoding.UTF8.GetString(dataReceivedBuffer));

                                ServerCommandHandlerEx.SetCommandNum(cmdNum);
                            }
                        }

                        return Task.CompletedTask;
                    }
                    catch (Exception ex)
                    {
                        serverCommandHandlerCts.Cancel();

                        Debug.WriteLine(ex);
                        throw;
                    }
                },
                ClosedCallback = (client, closedByRemote) =>
                {
                    // Neu server da xong dong client
                    if (closedByRemote)
                    {
                        serverCommandHandlerCts.Cancel();
                    }
                }
            };

            client.Message += (s, a) => Debug.WriteLine("Client: " + a.Message);
            var clientTask = client.RunAsync();

            // client.Dispose();
            // await clientTask;

            return clientTask;
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
}