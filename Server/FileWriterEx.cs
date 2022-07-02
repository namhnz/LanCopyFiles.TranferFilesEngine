using System;
using System.IO;
using System.Threading.Tasks;

namespace LanCopyFiles.TransferFilesEngine.Server;

public class FileWriterEx
{
    private FileStream _fileStream;
    public long CurrentFilePointer { get; set; }

    public FileWriterEx(string filePath)
    {
        _fileStream = new FileStream(filePath, FileMode.CreateNew);
    }

    public async Task WritePartAsync(byte[] receiveData)
    {
        _fileStream.Seek(CurrentFilePointer, SeekOrigin.Begin);
        await _fileStream.WriteAsync(receiveData, 0, receiveData.Length);
        CurrentFilePointer = _fileStream.Position;
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