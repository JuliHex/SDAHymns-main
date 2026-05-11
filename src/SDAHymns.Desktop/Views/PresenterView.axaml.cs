using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SDAHymns.Desktop.ViewModels;

namespace SDAHymns.Desktop.Views;

public partial class PresenterView : Window
{
    public PresenterView()
    {
        InitializeComponent();
        KeyDown += PresenterView_KeyDown;
        
        DataContextChanged += (s, e) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.RequestClose += () => this.Close();
            }
        };
    }

    private void PresenterView_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        var vm = DataContext as ViewModelBase;
        // Access MainWindowViewModel's commands via reflection or cast if possible
        // But safer to just use the commands defined in the UI or common patterns
        
        if (DataContext is MainWindowViewModel mainVm)
        {
            switch (e.Key)
            {
                case Avalonia.Input.Key.Right:
                case Avalonia.Input.Key.Down:
                case Avalonia.Input.Key.Space:
                    if (mainVm.NextVerseCommand.CanExecute(null))
                        mainVm.NextVerseCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Avalonia.Input.Key.Left:
                case Avalonia.Input.Key.Up:
                case Avalonia.Input.Key.Back:
                    if (mainVm.PreviousVerseCommand.CanExecute(null))
                        mainVm.PreviousVerseCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Avalonia.Input.Key.Escape:
                    this.Close();
                    break;
            }
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
