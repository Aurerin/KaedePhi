using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    public object CurrentPage
    {
        get => field ?? throw new InvalidOperationException("CurrentPage is not set.");
        set { field = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
