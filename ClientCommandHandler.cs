using System;
using System.Threading;
using System.Threading.Tasks;

namespace LanCopyFiles.TranferFilesEngine;

public static class ClientCommandHandler
{
    public static bool CommandAvailable { get; set; }

    public static int CommandNum { get; set; }

    public static void SetCommandNum(int commandNum)
    {
        CommandNum = commandNum;

        CommandAvailable = true;
    }

    public static int ReadCommandNum()
    {
        int commandNum = CommandNum;

        CommandNum = 0;
        CommandAvailable = false;

        return commandNum;
    }

    public static async Task<int> GetCommandAsync(CancellationToken cancellationToken)
    {
        string message = "";

        while (true)
        {
            while (!CommandAvailable)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(10);
            }
            var commandInfo = ReadCommandNum();
            switch (commandInfo)
            {
                case 126:
                    
                    break;
                default:
                    // Input character
                    message += commandInfo.KeyChar;
                    Console.Write(commandInfo.KeyChar);
                    break;
            }
        }
	}
}