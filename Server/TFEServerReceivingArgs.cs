using System;

namespace LanCopyFiles.TransferFilesEngine.Server;

public class TFEServerReceivingArgs : EventArgs
{
    public string FileName { get; set; }
    public string FromIPAddress { get; set; }
}