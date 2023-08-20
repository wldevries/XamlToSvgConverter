using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace XamlToSvgConverter;

internal class Runner
{
    public static void Run(IEnumerable<XamlIconSource> sources)
    {
        List<IconSet> iconSets = sources.Select(ConvertIcons).ToList();

        iconSets.AddRange(GetPngs());
        iconSets = iconSets
            .OrderBy(s => s.Name)
            .ToList();

        var file = IconHtmlPageCreator.CreateWebPage(iconSets);
        using Process p = new()
        {
            StartInfo = new ProcessStartInfo(file)
            {
                UseShellExecute = true
            }
        };
        p.Start();
    }

    private static IEnumerable<IconSet> GetPngs()
    {
        DirectoryInfo root = new(@"Png");
        foreach (var dir in root.GetDirectories())
        {
            List<string> icons = new();
            var pngs = dir.GetFiles("*.png");
            if (pngs.Length > 0)
            {
                var path = Path.Combine("png", dir.Name);
                icons.AddRange(pngs.Select(x => Path.Combine(path, x.Name)));
            }

            foreach (var subdir in dir.GetDirectories())
            {
                pngs = subdir.GetFiles("*.png");
                if (pngs.Length > 0)
                {
                    var path = Path.Combine("png", dir.Name, subdir.Name);
                    icons.AddRange(pngs.Select(x => Path.Combine(path, x.Name)));
                }
            }

            yield return new IconSet(dir.Name + "_png", icons);
        }
    }

    private static IconSet ConvertIcons(XamlIconSource source)
    {
        DirectoryInfo sourcedir = new(source.Path);
        List<string> svgFileNames = new();

        foreach (var file in sourcedir.GetFiles("*.xaml"))
        {
            Debug.WriteLine(file.Name);

            var xamlText = File.ReadAllText(file.FullName);

            string svg = SvgConverter.ConvertXamlToSvg(xamlText);

            if (svg != null)
            {
                Directory.CreateDirectory(source.Name);

                string svgFileName = Path.ChangeExtension(file.Name, ".svg");
                var svgPath = Path.Combine(source.Name, svgFileName);
                File.WriteAllText(svgPath, svg);
                svgFileNames.Add(svgPath);
            }
        }
        return new(source.Name, svgFileNames);
    }
}
