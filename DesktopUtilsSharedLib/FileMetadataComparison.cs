using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DesktopUtilsSharedLib;

public static partial class FileMetadataComparison
{
    // By Path

    public static IEnumerable<string> GetOldestByModified(IEnumerable<string> paths)
    {
        Dictionary<string, DateTime> modified = paths.ToDictionary(x => x, x => new FileInfo(x).LastWriteTime);
        int length = modified.Count;
        if (length < 2)
            throw new InvalidOperationException("Cannot compare less than 2 files.");

        DateTime oldest = modified.First().Value;
        foreach (var dt in modified.Values)
        {
            if (dt < oldest)
                oldest = dt;
        }

        return modified.Where(x => x.Value == oldest).Select(x => x.Key);
    }

    public static IEnumerable<string> GetOldestByCreation(IEnumerable<string> paths)
    {
        Dictionary<string, DateTime> created = paths.ToDictionary(x => x, x => new FileInfo(x).CreationTime);
        int length = created.Count;
        if (length < 2)
            throw new InvalidOperationException("Cannot compare less than 2 files.");

        DateTime oldest = created.First().Value;
        foreach (var dt in created.Values)
        {
            if (dt < oldest)
                oldest = dt;
        }

        return created.Where(x => x.Value == oldest).Select(x => x.Key);
    }

    // By FileInfo

    public static IEnumerable<FileInfo> GetOldestByModified(IEnumerable<FileInfo> fileInfos)
    {
        if (fileInfos.Count() < 2)
            throw new InvalidOperationException("Cannot compare less than 2 files.");

        var first = fileInfos.First();
        DateTime oldest = first.LastWriteTime;
        foreach (var dt in fileInfos.Select(x => x.LastWriteTime))
        {
            if (dt < oldest)
                oldest = dt;
        }

        return fileInfos.Where(x => x.LastWriteTime == oldest);
    }

    public static IEnumerable<FileInfo> GetOldestByCreation(IEnumerable<FileInfo> fileInfos)
    {
        if (fileInfos.Count() < 2)
            throw new InvalidOperationException("Cannot compare less than 2 files.");

        var first = fileInfos.First();
        DateTime oldest = first.CreationTime;
        foreach (var dt in fileInfos.Select(x => x.CreationTime))
        {
            if (dt < oldest)
                oldest = dt;
        }

        return fileInfos.Where(x => x.CreationTime == oldest);
    }
}