using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
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
		private async Task RunAsync2()
		{
			int port = 8085;

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

                            

                        },
						ReceivedCallback = async (serverClient, count) =>
						{
							byte[] bytes = serverClient.ByteBuffer.Dequeue(count);
							string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
							Console.WriteLine("Server client: received: " + message);

							bytes = Encoding.UTF8.GetBytes("You said: " + message);
							await serverClient.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));

							if (message == "bye")
							{
								// Let the server close the connection
								serverClient.Disconnect();
							}
						}
					}.RunAsync()
			};
			server.Message += (s, a) => Console.WriteLine("Server: " + a.Message);
			var serverTask = server.RunAsync();

			var client = new AsyncTcpClient
			{
				IPAddress = IPAddress.IPv6Loopback,
				Port = port,
				//AutoReconnect = true,
				ConnectedCallback = async (c, isReconnected) =>
				{
					// await c.WaitAsync();   // Wait for server banner
					// await Task.Delay(50);   // Let the banner land in the console window
					// Console.WriteLine("Client: type a message at the prompt, or empty to quit (server shutdown in 10s)");

					Debug.WriteLine("Client connected to server");

					while (true)
					{
						// Console.Write("> ");
						// var consoleReadCts = new CancellationTokenSource();
						// var consoleReadTask = ConsoleEx.ReadLineAsync(consoleReadCts.Token);

						// // Wait for user input or closed connection
						// var completedTask = await Task.WhenAny(consoleReadTask, c.ClosedTask);
						// if (completedTask == c.ClosedTask)
						// {
						// 	// Closed connection
						// 	consoleReadCts.Cancel();
						// 	break;
						// }
						//
						// // User input
						// string enteredMessage = await consoleReadTask;
						// if (enteredMessage == "")
						// {
						// 	// Close the client connection
						// 	c.Disconnect();
						// 	break;
						// }

                        // Gui ten file
						var filePath = @"C:\";

                        string selectedFile = filePath;
                        string fileName = Path.GetFileName(selectedFile);

                        byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("125"), Encoding.UTF8.GetBytes(fileName));

						// byte[] bytes = Encoding.UTF8.GetBytes(enteredMessage);

						// await c.Send(new ArraySegment<byte>(bytes, 0, bytes.Length));

                        // Truyen den server
						await c.Send(new ArraySegment<byte>(dataToSend, 0, dataToSend.Length));

						// Wait for server response or closed connection
						await c.ByteBuffer.WaitAsync();
						if (c.IsClosing)
						{
							break;
						}
					}
					// NOTE: The client connection will NOT be closed automatically when this method
					//       returns. It has to be closed explicitly when desired.

                    

                    // FileStream fs = new FileStream(Selected_file, FileMode.Open);
                    // TcpClient tc = new TcpClient(TargetIP, Port);
                    // NetworkStream ns = tc.GetStream();


				},
				ReceivedCallback = (c, count) =>
				{
					// byte[] bytes = c.ByteBuffer.Dequeue(count);
					// string message = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
					// Console.WriteLine("Client: received: " + message);
					// return Task.CompletedTask;

                    var initializeByte = c.ByteBuffer.Dequeue(1)[0];


					if (initializeByte == 2)
                    {

						// byte[] cmdBuffer = new byte[3];
						// ns.Read(cmdBuffer, 0, cmdBuffer.Length);

                        var cmdBuffer = c.ByteBuffer.Dequeue(3);

                        var separatorByte = c.ByteBuffer.Dequeue(1)[0];

						var dataBytes = c.ByteBuffer.Dequeue(count - 5);

                        if (separatorByte == 4)
                        {
							switch (Convert.ToInt32(Encoding.UTF8.GetString(cmdBuffer)))
                            {
                                case 126:
                                    long receivedFilePointer = long.Parse(Encoding.UTF8.GetString(dataBytes));
                                    if (receivedFilePointer != fs.Length)
                                    {
                                        fs.Seek(receivedFilePointer, SeekOrigin.Begin);
                                        int tempBufferLength = (int)(fs.Length - receivedFilePointer < 20000 ? fs.Length - receivedFilePointer : 20000);
                                        byte[] tempBuffer = new byte[tempBufferLength];
                                        fs.Read(tempBuffer, 0, tempBuffer.Length);
                                        byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("127"), tempBuffer);
                                        ns.Write(dataToSend, 0, dataToSend.Length);
                                        ns.Flush();
                                        ProgressValue = (int)Math.Ceiling((double)receivedFilePointer / (double)fs.Length * 100);
                                    }
                                    else
                                    {
                                        byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("128"), Encoding.UTF8.GetBytes("Close"));
                                        ns.Write(dataToSend, 0, dataToSend.Length);
                                        ns.Flush();
                                        fs.Close();
                                        // loop_break = true;
                                    }
                                    // break;
								default:
                                    // break;
                            }
						}


					}
                    return Task.CompletedTask;

				}
			};
			client.Message += (s, a) => Console.WriteLine("Client: " + a.Message);
			var clientTask = client.RunAsync();

			await Task.Delay(10 * 1000);
			Console.WriteLine("Program: stopping server");
			server.Stop(true);
			await serverTask;

			client.Dispose();
			await clientTask;
		}

        private static byte[] ReadStream(/*NetworkStream ns*/ byte[] receivedBuff)
        {
            // byte[] dataBuff = null;

            // int b = 0;
            // string buffLength = "";
            // while ((b = ns.ReadByte()) != 4)
            // {
            //     buffLength += (char)b;
            // }
            int separatorIndex = 0;
            for (int i = 0; i < receivedBuff.Length; i++)
            {
                if (receivedBuff[i] == 4)
                {
                    separatorIndex = i;
					break;
                }
            }

            var buffLength = Encoding.UTF8.GetString(receivedBuff[0..(separatorIndex - 1)]);
            var dataBuff = receivedBuff[(separatorIndex + 1) .. receivedBuff.Length]; /*https://stackoverflow.com/a/55498674*/

			// int dataLength = Convert.ToInt32(buffLength);
   //          dataBuff = new byte[dataLength];
   //          int byteRead = 0;
   //          int byteOffset = 0;
   //          while (byteOffset < dataLength)
   //          {
   //              byteRead = ns.Read(dataBuff, byteOffset, dataLength - byteOffset);
   //              byteOffset += byteRead;
   //          }

            return dataBuff;
        }

		private static byte[] CreateDataPacket(byte[] cmd, byte[] data)
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
