using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace XamlToSvgConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            var items = this.xamlIconSourceList.Items.OfType<XamlIconSource>().ToList();

            var json = JsonSerializer.Serialize(items);
            Settings.Default.XamlIconSources = json;
            Settings.Default.Save();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var json = Settings.Default.XamlIconSources as string;
                if (json != null)
                {
                    var items = JsonSerializer.Deserialize<XamlIconSource[]>(json);
                    foreach (var item in items ?? Array.Empty<XamlIconSource>())
                    {
                        this.xamlIconSourceList.Items.Add(item);
                    }
                }
            }
            catch (Exception) { }
        }

        private void Convert(object sender, RoutedEventArgs e)
        {
            var items = this.xamlIconSourceList.Items.OfType<XamlIconSource>();
            Runner.ConvertIcons(items);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dlg = new();
            if (dlg.ShowDialog() == true)
            {
                var folder = dlg.SelectedPath;
                var name = Path.GetFileName(folder);
                XamlIconSource source = new()
                {
                    Name = name,
                    Path = folder
                };
                this.xamlIconSourceList.Items.Add(source);
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            var selected = this.xamlIconSourceList.SelectedItem;
            if (selected != null)
            {
                this.xamlIconSourceList.Items.Remove(selected);
            }
        }

        private void CreatePage(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog vistaFolderBrowserDialog = new();
            if (vistaFolderBrowserDialog.ShowDialog() == true)
            {
                var directory = vistaFolderBrowserDialog.SelectedPath;
                var iconSets = IconSet.CreateSetsFrom(directory);
                var file = IconHtmlPageCreator.CreateWebPage(directory, iconSets);
                using Process p = new()
                {
                    StartInfo = new ProcessStartInfo(file)
                    {
                        UseShellExecute = true
                    }
                };
                p.Start();
            }

        }
    }
}
