using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace XamlToSvgConverter;

internal class Runner
{
    public static void ConvertIcons(IEnumerable<XamlIconSource> sources)
    {
        foreach (var source in sources)
        {
            ConvertIcons(source);
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

            string? svg = SvgConverter.ConvertXamlToSvg(xamlText);

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
