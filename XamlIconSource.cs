using CommunityToolkit.Mvvm.ComponentModel;

namespace XamlToSvgConverter;

public partial class XamlIconSource : ObservableObject
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string path = "";
}
