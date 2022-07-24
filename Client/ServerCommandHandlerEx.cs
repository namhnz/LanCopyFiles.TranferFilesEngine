using System.Threading;
using System.Threading.Tasks;

namespace LanCopyFiles.TransferFilesEngine.Client;

public static class ServerCommandHandlerEx
{
    public static bool CommandAvailable { get; set; }

    public static int CommandNum { get; set; }

    private static bool _isSettingCommandNum;


    public static void SetCommandNum(int commandNum)
    {
        _isSettingCommandNum = true;

        CommandNum = commandNum;

        CommandAvailable = true;

        _isSettingCommandNum = false;
    }

    public static int? ReadCommandNum()
    {
        if (_isSettingCommandNum)
        {
            return null;
        }

        int commandNum = CommandNum;

        CommandNum = 0;
        CommandAvailable = false;

        return commandNum;
    }

    public static async Task<int?> GetCommandAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            while (!_isSettingCommandNum && !CommandAvailable || _isSettingCommandNum)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // await Task.Delay(1);
            }

            int? commandInfo = null;

            while (commandInfo == null)
            {
                commandInfo = ReadCommandNum();
            }
            
            switch (commandInfo)
            {
                case 126:
                    return 126;
                default:
                    return null;
            }
        }
    }
}