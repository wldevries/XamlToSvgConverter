using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace XamlToSvgConverter;

public class SvgConverter
{ 
    public static string? ConvertXamlToSvg(string xamlText)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // parse the xaml
        var xaml = XamlReader.Parse(xamlText) as FrameworkElement;

        var paths = FindPaths(xaml, out double? width, out double? height);

        StringBuilder sb = new();
        if (width == null || height == null)
        {
            sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\">");
        }
        else
        {
            width = Math.Ceiling(width.Value);
            height = Math.Ceiling(height.Value);
            sb.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{width}\" height=\"{height}\" viewBox=\"0 0 {width} {height}\">");
        }

        bool pathsFound = false;

        foreach (var shape in paths)
        {
            pathsFound = true;
            if (shape is Path p)
                ProcessPath(sb, p);
            if (shape is Ellipse e)
                ProcessEllipse(sb, e);
            if (shape is Rectangle r)
                ProcessRectangle(sb, r);
            if (shape is Line l)
                ProcessLine(sb, l);
            if (shape is Polygon po)
                ProcessPolygon(sb, po);
        }

        if (pathsFound)
        {
            sb.AppendLine("</svg>");
            var svg = sb.ToString();
            return svg;
        }

        return null;
    }

    private static void ProcessPolygon(StringBuilder sb, Polygon polygon)
    {
        var left = Canvas.GetLeft(polygon);
        var top = Canvas.GetTop(polygon);
        left = double.IsNormal(left) ? left : 0;
        top = double.IsNormal(top) ? top : 0;

        var points = polygon.Points
            .Select(p => $"{left + p.X},{top + p.Y}")
            .Aggregate((a, b) => $"{a} {b}");

        sb.Append("<polygon points=\"");
        sb.Append(points);
        sb.Append("\"");
        AddShapeProperties(sb, polygon);
        sb.AppendLine("/>");
    }

    private static void ProcessEllipse(StringBuilder sb, Ellipse ellipse)
    {
        var left = Canvas.GetLeft(ellipse);
        var top = Canvas.GetTop(ellipse);

        var cx = double.IsNormal(left) ? left : 0 + ellipse.Width / 2;
        var cy = double.IsNormal(top) ? top : 0 + ellipse.Height / 2;

        // WPF draws the inside the ellipse path, SVG draws on the ellipse path
        var rx = ellipse.Width / 2 - ellipse.StrokeThickness / 2;
        var ry = ellipse.Height / 2 - ellipse.StrokeThickness / 2;
         
        sb.Append($"<ellipse cx=\"{cx}\" cy=\"{cy}\" rx=\"{rx}\" ry=\"{ry}\"");
        AddShapeProperties(sb, ellipse);
        sb.AppendLine("/>");
    }

    private static void ProcessRectangle(StringBuilder sb, Rectangle path)
    {
        throw new NotImplementedException();
    }

    private static void ProcessLine(StringBuilder sb, Line path)
    {
        throw new NotImplementedException();
    }

    private static void ProcessPath(StringBuilder sb, Path path)
    {
        var pathMarkup = path.Data
            .GetFlattenedPathGeometry()
            .ToString();

        if (pathMarkup.StartsWith("F1"))
            pathMarkup = pathMarkup[2..];

        sb.Append("<path d=\"");
        sb.Append(pathMarkup);
        sb.Append('"');

        AddShapeProperties(sb, path);

        var left = Canvas.GetLeft(path);
        var top = Canvas.GetTop(path);
        if (double.IsNormal(left) || double.IsNormal(top))
        {
            left = double.IsNormal(left) ? left : 0;
            top = double.IsNormal(top) ? top : 0;
            sb.Append($" transform=\"translate({left},{top})\"");
        }
        else if (path.Margin != default)
        {
            sb.Append($" transform=\"translate({path.Margin.Left},{path.Margin.Top})\"");
        }

        sb.AppendLine("/>");
    }

    private static void AddShapeProperties(StringBuilder sb, Shape shape)
    {
        if (shape.Fill != null)
            sb.Append($" fill=\"{ToWeb(shape.Fill)}\"");
        if (shape.Stroke != null)
            sb.Append($" stroke=\"{ToWeb(shape.Stroke)}\"");
        if (double.IsNormal(shape.StrokeThickness))
            sb.Append($" stroke-width=\"{shape.StrokeThickness.ToString().ToLower()}\"");
        if (shape.StrokeLineJoin != PenLineJoin.Miter)
            sb.Append($" stroke-linejoin=\"{ToSvg(shape.StrokeLineJoin)}\"");
        if (shape.StrokeStartLineCap != PenLineCap.Flat)
            sb.Append($" stroke-linecap=\"{ToSvg(shape.StrokeStartLineCap)}\"");
        else if (shape.StrokeEndLineCap != PenLineCap.Flat)
            sb.Append($" stroke-linecap=\"{ToSvg(shape.StrokeEndLineCap)}\"");
    }

    private static List<Shape> FindPaths(FrameworkElement? root, out double? width, out double? height)
    {
        List<Shape> paths = new();
        width = height = null;
        Queue<FrameworkElement> elements = new();
        if (root is not null)
        {
            elements.Enqueue(root);
        }

        while (elements.TryDequeue(out var element))
        {
            if (element is Viewbox vb && vb.Child is FrameworkElement fe)
            {
                if (double.IsNormal(vb.Width))
                {
                    width = vb.Width;
                    height = vb.Height;
                }

                elements.Enqueue(fe);
            }
            else if (element is Canvas c)
            {
                if (width == null)
                {
                    width = c.Width;
                    height = c.Height;
                }

                foreach (var child in c.Children)
                {
                    if (child is FrameworkElement cfe)
                    {
                        elements.Enqueue(cfe);
                    }
                }
            }
            else if (element is Shape p)
            {
                paths.Add(p);
            }
        }
        return paths;
    }

    private static string ToSvg(PenLineJoin penLineJoin) => penLineJoin switch
    {
        PenLineJoin.Miter => "miter",
        PenLineJoin.Bevel => "bevel",
        PenLineJoin.Round => "round",
        _ => throw new NotImplementedException(),
    };

    private static string ToSvg(PenLineCap penLineCap) => penLineCap switch
    {
        PenLineCap.Flat => "butt",
        PenLineCap.Round => "round",
        PenLineCap.Square => "square",
        _ => throw new NotImplementedException(),
    };

    public static string ToWeb(Brush brush)
    {
        // assuming it's a solidcolorbrush
        string wpfHex = brush.ToString();
        if (wpfHex.Length == 9)
        {
            return "#" + wpfHex[3..];
        }
        return wpfHex;
    }
}
