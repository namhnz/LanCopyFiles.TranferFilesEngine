using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LanCopyFiles.TransferFilesEngine.Client;

public class FileReaderEx
{
    private readonly FileStream _fileReaderStream;

    private bool _isFileReaderStreamClosed;

    public long CurrentPointerPosition { get; set; }

    public int ReadingProgressValue
    {
        get
        {
            if (_isFileReaderStreamClosed) return 0;
            if (_fileReaderStream.Length == 0) return 0;
            return (int)Math.Ceiling((double)CurrentPointerPosition / (double)_fileReaderStream.Length * 100);
        }
    }

    public FileReaderEx(string filePath)
    {
        _fileReaderStream = new FileStream(filePath, FileMode.Open);
        _isFileReaderStreamClosed = false;
    }

    public async Task<FileReadResult> ReadPartAsync()
    {
        var offset = CurrentPointerPosition;

        if (offset != _fileReaderStream.Length)
        {
            _fileReaderStream.Seek(offset, SeekOrigin.Begin);
            int tempBufferLength = (int)(_fileReaderStream.Length - offset < 10000 ? _fileReaderStream.Length - offset : 10000);
            byte[] tempBuffer = new byte[tempBufferLength];
            await _fileReaderStream.ReadAsync(tempBuffer, 0, tempBuffer.Length);

            return new FileReadResult()
            {
                DataRead = tempBuffer,
                ReadResultNum = 127
            };
        }
        else
        {
            _fileReaderStream.Close();
            _isFileReaderStreamClosed = true;

            return new FileReadResult()
            {
                DataRead = Encoding.UTF8.GetBytes("Close"),
                ReadResultNum = 128
            };
        }
    }
}