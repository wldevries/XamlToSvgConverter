using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XamlToSvgConverter;

public record IconSet(string Name, List<string> Icons, int Depth)
{
    private static readonly string[] _extensions = new[] { ".svg", ".png" };

    internal static IEnumerable<IconSet> CreateSetsFrom(string directory)
    {
        return GetSets(directory);
    }


    private static IEnumerable<IconSet> GetSets(string rootPath)
    {
        return GetSets(rootPath, rootPath, 1);
    }


    private static IEnumerable<IconSet> GetSets(string rootPath, string dirPath, int depth)
    {
        DirectoryInfo dir = new(dirPath);
        List<string> icons = new();

        var setRelativePath = dirPath[(rootPath.Length)..].TrimStart('\\');
        foreach (var e in _extensions)
        {
            var files = dir.GetFiles("*" + e);
            foreach (var file in files)
            {
                icons.Add(Path.Combine(setRelativePath, file.Name));
            }
        }

        if (icons.Any())
        {
            var setName = setRelativePath.Replace('\\', ' ').Trim();
            yield return new IconSet(setName, icons, 1);
        }

        foreach (var subdir in dir.GetDirectories())
        {
            foreach (var set in GetSets(rootPath, subdir.FullName, depth + 1))
            {
                yield return set;
            }
        }
    }
}
