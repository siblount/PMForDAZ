using Microsoft.VisualBasic.FileIO;

namespace DAZ_Installer.Core
{
    public struct DPProcessSettings
    {
        string TempPath;
        string DestinationPath;
        RecycleOption DeleteAction;
        string[] ContentFolders;
        Dictionary<string, string> ContentRedirectFolders;
    }
}
