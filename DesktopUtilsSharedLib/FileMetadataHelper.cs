using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DesktopUtilsSharedLib;

public static partial class FileMetadataHelper
{
    public static IEnumerable<string> FilesUnder(this IEnumerable<string> files, long size)
        => files.Where(x => new FileInfo(x).Length < size);

    public static T FilesUnder<T>(this T files, long size) where T : ICollection<string>, new()
        => [.. files.Where(x => new FileInfo(x).Length < size)];

    public static T FilesWithin<T>(this T files, long minsize, long maxsize) where T : ICollection<string>, new()
        => [.. files.Where(x => new FileInfo(x).Length is long length && (minsize <= 0 || length >= minsize) && (maxsize <= 0 || length < maxsize))];
}