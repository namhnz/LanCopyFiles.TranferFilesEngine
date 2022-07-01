using System.IO;
using System.Text;

namespace LanCopyFiles.TransferFilesEngine.Client;

public class FileReaderEx
{
    private FileStream _fileStream;

    public FileReaderEx(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Open);
    }

    public FileReadResult ReadLine(long offset)
    {

        if (offset != _fileStream.Length)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            int tempBufferLength = (int)(_fileStream.Length - offset < 20000 ? _fileStream.Length - offset : 20000);
            byte[] tempBuffer = new byte[tempBufferLength];
            _fileStream.Read(tempBuffer, 0, tempBuffer.Length);
            
            return new FileReadResult()
            {
                DataRead = tempBuffer,
                ReadResultNum = 127
            };

            // ProgressValue = (int)Math.Ceiling((double)recv_file_pointer / (double)_fileStream.Length * 100);
        }
        else
        {
            _fileStream.Close();
            _fileStream = null;

            return new FileReadResult()
            {
                DataRead = Encoding.UTF8.GetBytes("Close"),
                ReadResultNum = 128
            };
            
        }
    }
}