using System.Diagnostics;
using LanCopyFiles.TransferFilesEngine.Client;
using LanCopyFiles.TransferFilesEngine.Server;

namespace LanCopyFiles.TransferFilesEngine.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists(@"G:\User Data\Desktop\commandos_3_-_destination_berln.rar"))
            {
                File.Delete(@"G:\User Data\Desktop\commandos_3_-_destination_berln.rar");
            }

            if (File.Exists(@"E:\Desktop\commandos_3_-_destination_berln.rar"))
            {
                File.Delete(@"E:\Desktop\commandos_3_-_destination_berln.rar");
            }

            if (File.Exists(@"E:\Desktop\1_001_C22THD_61_26355.pdf"))
            {
                File.Delete(@"E:\Desktop\1_001_C22THD_61_26355.pdf");
            }

            var server = new TFEServer(8085, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            var serverTask = server.StartServer();

            var client = new TFEClient(8085);
            // var clientTask = client.StartClient(@"E:\Nam's Docs\Chu ky\huong_dan_tao_chu_ky.txt");
            var clientTask = client.StartClient(@"D:\commandos_3_-_destination_berln.rar");
            var clientTask2 = client.StartClient(@"D:\1_001_C22THD_61_26355.pdf");

            Task.Delay(100 * 1000).GetAwaiter().GetResult();

            Debug.WriteLine("Da cho xong 100s");

            Task.WhenAll(serverTask, clientTask, clientTask2).GetAwaiter().GetResult();

            Console.ReadKey();
        }
    }
}