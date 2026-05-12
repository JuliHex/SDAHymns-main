using Avalonia.Controls;
using SDAHymns.Android.ViewModels;

namespace SDAHymns.Android.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        DataContext = new AndroidRemoteViewModel();
    }
}
