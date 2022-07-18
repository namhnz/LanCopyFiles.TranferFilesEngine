using System;
using System.Threading.Tasks;
using LanCopyFiles.TransferFilesEngine.Client;

namespace LanCopyFiles.TransferFilesEngine.EngineManager;

public class TFEClientManager
{
    private static TFEClientManager _instance;
    public static TFEClientManager Instance => _instance ?? (_instance = new TFEClientManager());

    public TFEClientManager()
    {
        
    }

    private TFEClient _tfeClient;

    public int ProgressValue
    {
        get
        {
            if (!_isClientRunning)
            {
                return 0;
            }
        }
    }

    private bool _isClientRunning;

    public async Task Send(string filePath, string serverIP, int serverPort)
    {
        if (_isClientRunning)
        {
            throw new InvalidOperationException("The client is sending file to server");
        }

        _isClientRunning = true;

        _tfeClient = new TFEClient(serverIP, serverPort);
        await _tfeClient.StartClient(filePath);

        _isClientRunning = false;
    }
}