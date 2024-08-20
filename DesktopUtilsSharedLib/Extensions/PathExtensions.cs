using System.IO;

namespace DesktopUtilsSharedLib.Extensions;

public static partial class PathExtensions
{
    public static string ChangeExtension(string file, string extension)
        => file[..^Path.GetExtension(file).Length] + extension;
}