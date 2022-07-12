namespace LanCopyFiles.TransferFilesEngine.Helpers;

public class TransferCodeDescription
{
    public static string GetDescription(int code)
    {
        switch (code)
        {
            case 101:
                return "Server handle <- Client send: No thing";
            case 125:
                return "Server handle <- Client send: File name";
            case 126:
                return "Server Send -> Client handle: Continue read and send data part";
            case 127:
                return "Server handle <- Client send: File data part";
            case 128:
                return "Server handle <- Client send: Close - end of file";
            default: 
                return "Unknown";
        }
    }
}