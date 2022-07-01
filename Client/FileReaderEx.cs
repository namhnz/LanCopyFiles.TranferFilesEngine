using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LanCopyFiles.TransferFilesEngine.Client;

public class FileReaderEx: IDisposable
{
    private FileStream _fileStream;
    public long ReceiveFilePointer { get; set; }

    public FileReaderEx(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Open);
    }

    public async Task<FileReadResult> ReadPartAsync()
    {
        var offset = ReceiveFilePointer;
        
        if (offset != _fileStream.Length)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            int tempBufferLength = (int)(_fileStream.Length - offset < 20000 ? _fileStream.Length - offset : 20000);
            byte[] tempBuffer = new byte[tempBufferLength];
            await _fileStream.ReadAsync(tempBuffer, 0, tempBuffer.Length);
            
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

    private bool disposed;

    ~FileReaderEx()
    {
        this.Dispose(false);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed resources here.
            }

            // Dispose unmanaged resources here.
        }

        disposed = true;
    }
}