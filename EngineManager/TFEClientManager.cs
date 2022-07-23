using System;
using System.Threading.Tasks;
using LanCopyFiles.TransferFilesEngine.Client;

namespace LanCopyFiles.TransferFilesEngine.EngineManager;

public class TFEClientManager
{
    
    public TFEClientManager()
    {
        
    }

    private static TFEClient _tfeClient;

    public static int ProgressValue
    {
        get
        {
            if (_tfeClient == null)
            {
                return 0;
            }

            return _tfeClient.ProgressValue;
        }
    }

    private static bool _isClientBusy;

    public static async Task<SendingResponse> Send(string filePath, string serverIP, int serverPort)
    {
        if (_isClientBusy)
        {
            throw new InvalidOperationException("The client is sending file to server");
        }

        _isClientBusy = true;

        _tfeClient = new TFEClient(serverIP, serverPort);
        
        try
        {
            await _tfeClient.StartClient(filePath);

            return new SendingResponse()
            {
                Status = 1,
                Description = "Send successfully"
            };
        }
        catch (Exception ex)
        {
            _isClientBusy = false;

            return new SendingResponse()
            {
                Status = -1,
                Description = "Error: " + ex.Message
            };
        }
    }

    public static async Task<SendingResponse> SendManyAsync(string[] filePaths, string serverIP, int serverPort)
    {
        if (_isClientBusy)
        {
            throw new InvalidOperationException("The client is sending file to server");
        }

        _isClientBusy = true;

        _tfeClient = new TFEClient(serverIP, serverPort);
        try
        {
            foreach (var filePath in filePaths)
            {
                await _tfeClient.StartClient(filePath);
            }

            return new SendingResponse()
            {
                Status = 1,
                Description = "Send successfully"
            };
        }
        catch (Exception ex)
        {
            _isClientBusy = false;

            return new SendingResponse()
            {
                Status = -1,
                Description = "Error: " + ex.Message
            };
        }

    }
}