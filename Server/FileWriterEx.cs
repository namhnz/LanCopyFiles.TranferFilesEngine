using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LanCopyFiles.TransferFilesEngine.Client;

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
        // var offset = CurrentFilePointer;

        // if (offset != _fileStream.Length)
        // {
            // _fileStream.Seek(offset, SeekOrigin.Begin);
            // int tempBufferLength = (int)(_fileStream.Length - offset < 20000 ? _fileStream.Length - offset : 20000);
            // byte[] tempBuffer = new byte[tempBufferLength];
            // await _fileStream.ReadAsync(tempBuffer, 0, tempBuffer.Length);
            //
            // return new FileReadResult()
            // {
            //     DataRead = tempBuffer,
            //     ReadResultNum = 127
            // };

            // ProgressValue = (int)Math.Ceiling((double)recv_file_pointer / (double)_fileStream.Length * 100);

            _fileStream.Seek(CurrentFilePointer, SeekOrigin.Begin);
            await _fileStream.WriteAsync(receiveData, 0, receiveData.Length);
            CurrentFilePointer = _fileStream.Position;
            
        // }
        // else
        // {
        //     _fileStream.Close();
        //     _fileStream = null;
        //
        //     return new FileReadResult()
        //     {
        //         DataRead = Encoding.UTF8.GetBytes("Close"),
        //         ReadResultNum = 128
        //     };
        //
        // }
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