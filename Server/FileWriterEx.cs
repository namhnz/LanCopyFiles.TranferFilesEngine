using System;
using System.IO;
using System.Threading.Tasks;

namespace LanCopyFiles.TransferFilesEngine.Server;

public class FileWriterEx
{
    private FileStream _fileWriterStream;
    public long CurrentFilePointer { get; set; }

    public FileWriterEx(string filePath)
    {
        _fileWriterStream = new FileStream(filePath, FileMode.CreateNew);
        
    }

    public async Task WritePartAsync(byte[] receiveData)
    {
        _fileWriterStream.Seek(CurrentFilePointer, SeekOrigin.Begin);
        await _fileWriterStream.WriteAsync(receiveData, 0, receiveData.Length);
        CurrentFilePointer = _fileWriterStream.Position;
    }

    public void Close()
    {
        _fileWriterStream.Close();
        _fileWriterStream = null;
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