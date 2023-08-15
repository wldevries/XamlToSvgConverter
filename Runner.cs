using Humanizer;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace XamlToSvgConverter;

internal class Runner
{
    public const string Source = @"D:\Icons\Set1";
    public const string Source2 = @"D:\Icons\Set2";

    public static void Run()
    {
        
        DirectoryInfo sourcedir = new(Source);
        var flow = ConvertIcons(new(Source2), "Flow");
        var rephagia = ConvertIcons(new(Source), "Rephagia");

        CreateWebPage(new() { flow, rephagia });
    }

    private static IconSet ConvertIcons(DirectoryInfo sourcedir, string setName)
    {
        List<string> svgFileNames = new();

        foreach (var file in sourcedir.GetFiles("*.xaml"))
        {
            Debug.WriteLine(file.Name);

            var xamlText = File.ReadAllText(file.FullName);

            string svg = SvgConverter.ConvertXamlToSvg(xamlText);

            if (svg != null)
            {
                Directory.CreateDirectory(setName);

                string svgFileName = Path.ChangeExtension(file.Name, ".svg");
                var svgPath = Path.Combine(setName, svgFileName);
                File.WriteAllText(svgPath, svg);
                svgFileNames.Add(svgPath);
            }
        }
        return new(setName, svgFileNames);
    }

    public record IconSet(string Product, List<string> Icons);

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
            sb.AppendLine($"<h1 class=\"header-product\">{set.Product}</h1>");

            sb.AppendLine("<section class=\"icon-set\">");

            foreach (var file in set.Icons)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                name = name.Kebaberize();
                sb.Append($"<div class=\"icon\">");
                sb.Append($"<img class=\"icon-svg\" src=\"{file}\" width=\"30\" height=\"30\"/>");
                sb.Append($"<span class=\"icon-name\">{name}</span>");
                sb.AppendLine($"</div>");
            }
            sb.AppendLine("</section>");
        }
        sb.AppendLine("</body></html>");
        File.WriteAllText("index.html", sb.ToString());
    }
}
