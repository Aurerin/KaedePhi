using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private object _currentPage = null!;

    public object CurrentPage
    {
        get => _currentPage;
        set { _currentPage = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
