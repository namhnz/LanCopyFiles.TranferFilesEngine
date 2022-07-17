using System;
using System.Diagnostics;
// using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

// using System.Timers;

namespace LanCopyFiles.TransferFilesEngine.Client;

public class FileReaderEx : IDisposable
{
    private FileStream _fileStream;
    public long ReceiveFilePointer { get; set; }
    //
    // public int ProgressValue =>
    //     (int)Math.Ceiling((double)ReceiveFilePointer / (double)_fileStream.Length * 100);

    public FileReaderEx(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.Open);

        // var timer = new Timer();
        // timer.Interval = 1000;
        // timer.AutoReset = true;
        // timer.Elapsed += (sender, args) =>
        // {
        //     Debug.WriteLine("Client: File can read? " + _fileStream.CanRead);
        // };
        // timer.Start();
    }

    public async Task<FileReadResult> ReadPartAsync()
    {
        var offset = ReceiveFilePointer;

        // Debug.WriteLine("Client read file: total file length: " + _fileStream.Length);
        // Debug.WriteLine("Client read file: stream position: " + offset);

        if (offset != _fileStream.Length)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            int tempBufferLength = (int)(_fileStream.Length - offset < 10000 ? _fileStream.Length - offset : 10000);
            byte[] tempBuffer = new byte[tempBufferLength];
            await _fileStream.ReadAsync(tempBuffer, 0, tempBuffer.Length);
            // Debug.WriteLine("Client read file: read bytes length: " + tempBuffer.Length);

            return new FileReadResult()
            {
                DataRead = tempBuffer,
                ReadResultNum = 127
            };
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