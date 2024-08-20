namespace DesktopUtilsSharedLib.Extensions;

public static partial class FileSystemExtensions
{
    public static void RecycleFile(string path)
    {
        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
            file: path,
            showUI: Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
            recycle: Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
    }
}