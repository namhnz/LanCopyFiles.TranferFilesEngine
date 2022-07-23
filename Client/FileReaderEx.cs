using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LanCopyFiles.TransferFilesEngine.Client;

public class FileReaderEx
{
    private readonly FileStream _fileToRead;

    public long CurrentPointerPosition { get; set; }

    public int ReadingProgressValue
    {
        get
        {
            if (_fileToRead.Length == 0) return 0;
            return (int)Math.Ceiling((double)CurrentPointerPosition / (double)_fileToRead.Length * 100);
        }
    }

    public FileReaderEx(string filePath)
    {
        _fileToRead = new FileStream(filePath, FileMode.Open);
    }

    public async Task<FileReadResult> ReadPartAsync()
    {
        var offset = CurrentPointerPosition;

        if (offset != _fileToRead.Length)
        {
            _fileToRead.Seek(offset, SeekOrigin.Begin);
            int tempBufferLength = (int)(_fileToRead.Length - offset < 10000 ? _fileToRead.Length - offset : 10000);
            byte[] tempBuffer = new byte[tempBufferLength];
            await _fileToRead.ReadAsync(tempBuffer, 0, tempBuffer.Length);

            return new FileReadResult()
            {
                DataRead = tempBuffer,
                ReadResultNum = 127
            };
        }
        else
        {
            _fileToRead.Close();
            // _fileToRead = null;

            return new FileReadResult()
            {
                DataRead = Encoding.UTF8.GetBytes("Close"),
                ReadResultNum = 128
            };
        }
    }
}