using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XamlToSvgConverter;

public class IconHtmlPageCreator
{
    public static string CreateWebPage(IEnumerable<IconSet> sets)
    {
        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html><html><head><title>Icons</title>");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\r\n");
        sb.AppendLine("<style>");
        sb.AppendLine(File.ReadAllText("style.css"));
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine(File.ReadAllText("ColorPicker.html"));

        sb.AppendLine("<main>");
        foreach (var set in sets)
        {
            sb.AppendLine("<section class=\"icon-set\">");
            sb.AppendLine($"<h1 class=\"header-product\">{set.Name}</h1>");

            sb.AppendLine("<section class=\"icon-set-container\">");

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
                sb.Append($"<img class=\"icon-image\" src=\"{file}\" alt=\"name\" width=\"30\" height=\"30\"/>");
                sb.Append($"<span class=\"icon-name\">{name}</span>");
                sb.AppendLine($"</div>");
            }
            sb.AppendLine("</div></section>");
        }
        sb.AppendLine("</main></body></html>");

        string filename = "index.html";
        if (File.Exists(filename))
        {
            filename = GetFilename(filename);
        }
        File.WriteAllText(filename, sb.ToString());

        return Path.GetFullPath(filename);
    }

    private static string GetFilename(string filename)
    {
        var name = Path.GetFileNameWithoutExtension(filename);
        var extension = Path.GetExtension(filename);
        int index = 1;
        while (File.Exists($"{name}{index}{extension}"))
        {
            index++;
        }
        return $"{name}{index}{extension}";
    }
}
