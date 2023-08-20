using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XamlToSvgConverter;

public record IconSet(string Name, List<string> Icons)
{
    private static readonly string[] _extensions = new[] { ".svg", ".png" };

    internal static IEnumerable<IconSet> CreateSetsFrom(string directory)
    {
        return GetSets(directory);
    }


    private static IEnumerable<IconSet> GetSets(string rootPath)
    {
        DirectoryInfo root = new(rootPath);
        foreach (var dir in root.GetDirectories())
        {
            var relativePath = dir.FullName[(root.FullName.Length + 1)..];
            List<string> icons = new();

            foreach (var e in _extensions)
            {
                var files = dir.GetFiles("*" + e, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    icons.Add(Path.Combine(relativePath, file.Name));
                }
            }

            yield return new IconSet(dir.Name, icons);
        }
    }
}
