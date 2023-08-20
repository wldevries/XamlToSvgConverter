using Humanizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XamlToSvgConverter;

public class IconHtmlPageCreator
{
    public static string CreateWebPage(string directory, IEnumerable<IconSet> sets)
    {
        StringBuilder sb = new();
        sb.AppendLine("<!DOCTYPE html><html><head><title>Icons</title>");
        sb.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />\r\n");
        sb.AppendLine("<style>");
        sb.AppendLine(File.ReadAllText("style.css"));
        sb.AppendLine("</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine(File.ReadAllText("ColorPicker.html"));

        sb.AppendLine("<nav><ul>");
        foreach (var set in sets)
        {
            sb.AppendLine($"<li><a href=\"#{set.Name}\">{set.Name}</a></li>");
        }
        sb.AppendLine("</ul></nav>");

        sb.AppendLine("<main>");
        foreach (var set in sets)
        {
            sb.AppendLine("<section class=\"icon-set\">");
            sb.AppendLine($"<h1 class=\"header-product\" id=\"{set.Name}\">{set.Name}</h1>");

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
                var webPath = file.Replace('\\', '/');

                sb.Append($"<div class=\"icon\">");
                sb.Append($"<div class=\"icon-image-container\">");
                sb.Append($"<img class=\"icon-image\" src=\"{webPath}\" alt=\"name\" />");
                sb.Append($"</div>");
                sb.Append($"<span class=\"icon-name\">{name}</span>");
                sb.AppendLine($"</div>");
            }
            sb.AppendLine("</div></section>");
        }
        sb.AppendLine("</main></body></html>");

        string filename = Path.Combine(directory, "index.html");
        if (File.Exists(filename))
        {
            filename = GetFilename(filename);
        }
        File.WriteAllText(filename, sb.ToString());

        return Path.GetFullPath(filename);
    }

    private static string GetFilename(string filename)
    {
        var dir = Path.GetDirectoryName(filename);
        var name = Path.GetFileNameWithoutExtension(filename);
        var extension = Path.GetExtension(filename);
        int index = 1;
        while (File.Exists(fullName(name, index, extension)))
        {
            index++;
        }

        return fullName(name, index, extension);

        string fullName(string? name, int index, string? extension)
        {
            if (dir is null)
            {
                return $"{name}{index}{extension}";
            }
            return Path.Combine(dir, $"{name}{index}{extension}");
        }
    }
}
