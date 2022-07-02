// using System.Threading;
// using System.Threading.Tasks;
//
// namespace LanCopyFiles.TransferFilesEngine.Server;
//
// public static class ClientCommandHandlerEx
// {
//     public static bool CommandAvailable { get; set; }
//
//     public static int CommandNum { get; set; }
//
//     public static void SetCommandNum(int commandNum)
//     {
//         CommandNum = commandNum;
//
//         CommandAvailable = true;
//     }
//
//     public static int ReadCommandNum()
//     {
//         int commandNum = CommandNum;
//
//         CommandNum = 0;
//         CommandAvailable = false;
//
//         return commandNum;
//     }
//
//     public static async Task<int?> GetCommandAsync(CancellationToken cancellationToken)
//     {
//         while (true)
//         {
//             while (!CommandAvailable)
//             {
//                 cancellationToken.ThrowIfCancellationRequested();
//                 await Task.Delay(10);
//             }
//             var commandInfo = ReadCommandNum();
//             switch (commandInfo)
//             {
//                 case 101:
//                     return null;
//                 case 125:
//                     return 125;
//                 case 127:
//                     return 127;
//                 case 128:
//                     return 128;
//                 default:
//                     return null;
//             }
//         }
// 	}
// }