using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LanCopyFiles.TranferFilesEngine;

public class FileReaderEx
{
	public static async /*Task<string>*/ Task<byte[]> ReadLineAsync(CancellationToken cancellationToken)
    {
        string selectedFile = FilePath;
        string fileName = Path.GetFileName(selectedFile);
        FileStream fs = new FileStream(selectedFile, FileMode.Open);

        string cmd = "";

        switch (Convert.ToInt32(cmd))
        {
            case 126:
                long recvFilePointer = long.Parse(Encoding.UTF8.GetString(recv_data));
                if (recvFilePointer != fs.Length)
                {
                    fs.Seek(recvFilePointer, SeekOrigin.Begin);
                    int tempBufferLength = (int)(fs.Length - recvFilePointer < 20000 ? fs.Length - recvFilePointer : 20000);
                    byte[] tempBuffer = new byte[tempBufferLength];
                    fs.Read(tempBuffer, 0, tempBuffer.Length);
                    byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("127"), tempBuffer);
                    // ns.Write(data_to_send, 0, data_to_send.Length);
                    // ns.Flush();
                    // ProgressValue = (int)Math.Ceiling((double)recvFilePointer / (double)fs.Length * 100);
                    return dataToSend;
                }
                else
                {
                    byte[] dataToSend = CreateDataPacket(Encoding.UTF8.GetBytes("128"), Encoding.UTF8.GetBytes("Close"));
                    // ns.Write(dataToSend, 0, dataToSend.Length);
                    // ns.Flush();
                    // fs.Close();
                    // loop_break = true;
                    return dataToSend;
                }
                break;
            default:
                break;
        }


        // string message = "";
        // while (true)
        // {
        //     while (!Console.KeyAvailable)
        //     {
        //         cancellationToken.ThrowIfCancellationRequested();
        //         await Task.Delay(10);
        //     }
        //     var keyInfo = Console.ReadKey(true);
        //     switch (keyInfo.KeyChar)
        //     {
        //         case '\0':
        //             break;
        //         case '\b':
        //             // Delete previous character
        //             if (message.Length > 0)
        //             {
        //                 message = message.Substring(0, message.Length - 1);
        //                 if (Console.CursorLeft > 0)
        //                 {
        //                     Console.CursorLeft--;
        //                     Console.Write(" ");
        //                     Console.CursorLeft--;
        //                 }
        //             }
        //             break;
        //         case '\r':
        //             // Return key, execute command
        //             Console.WriteLine();
        //             return message;
        //         default:
        //             // Input character
        //             message += keyInfo.KeyChar;
        //             Console.Write(keyInfo.KeyChar);
        //             break;
        //     }
        // }
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