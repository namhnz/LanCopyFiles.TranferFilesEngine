using System;
// using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

// using System.Timers;

namespace LanCopyFiles.TransferFilesEngine.Server;

public class FileWriterEx
{
    private FileStream _fileStream;
    public long CurrentFilePointer { get; set; }

    public FileWriterEx(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.CreateNew);

        // var timer = new Timer();
        // timer.Interval = 1000;
        // timer.AutoReset = true;
        // timer.Elapsed += (sender, args) =>
        // {
        //     Debug.WriteLine("Server: File can write? " + _fileStream.CanWrite);
        // };
        // timer.Start();
    }

    public async Task WritePartAsync(byte[] receiveData)
    {
        _fileStream.Seek(CurrentFilePointer, SeekOrigin.Begin);
        await _fileStream.WriteAsync(receiveData, 0, receiveData.Length);
        CurrentFilePointer = _fileStream.Position;
        // Debug.WriteLine("Server write file stream position: " + CurrentFilePointer);
    }

    public void Close()
    {
        _fileStream.Close();
        _fileStream = null;
        Dispose();
    }

    private bool disposed;

    ~FileWriterEx()
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