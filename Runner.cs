using Humanizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace XamlToSvgConverter;

internal class Runner
{
    public static void Run()
    {
        var sources = new XamlIconSource[]
        {
        };

        List<IconSet> iconSets = sources.Select(ConvertIcons).ToList();

        iconSets.AddRange(GetPngs());
        iconSets = iconSets
            .OrderBy(s => s.Name)
            .ToList();

        CreateWebPage(iconSets);
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


    private static void CreateWebPage(List<IconSet> sets)
    {
        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html><html><head><title>Icons</title>");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\r\n");
        sb.AppendLine("<style>");
        sb.AppendLine(File.ReadAllText("style.css"));
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");

        foreach (var set in sets)
        {
            sb.AppendLine($"<h1 class=\"header-product\">{set.Name}</h1>");

            sb.AppendLine("<section class=\"icon-set\">");

            foreach (var file in set.Icons)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                name = name
                    .Replace("-b-64x64", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("-64x64", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("icon", "", StringComparison.OrdinalIgnoreCase)
                    .Kebaberize()
                    .Trim('-');
                ;
                sb.Append($"<div class=\"icon\">");
                sb.Append($"<img class=\"icon-image\" src=\"{file}\" width=\"30\" height=\"30\"/>");
                sb.Append($"<span class=\"icon-name\">{name}</span>");
                sb.AppendLine($"</div>");
            }
            sb.AppendLine("</section>");
        }
        sb.AppendLine("</body></html>");
        File.WriteAllText("index.html", sb.ToString());
    }
}
