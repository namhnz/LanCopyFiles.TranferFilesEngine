using LanCopyFiles.TransferFilesEngine.Client;
using LanCopyFiles.TransferFilesEngine.Server;

namespace LanCopyFiles.TransferFilesEngine.Test
{
    public class Program
    {
        static void Main(string[] args)
        {
            var server = new TFEServer(8085, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            server.StartServer();

            var client = new TFEClient(8085);
            client.StartClient(@"E:\Nam's Docs\Chu ky\huong_dan_tao_chu_ky.txt");

            Console.ReadKey();
        }
    }
}