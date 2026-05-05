using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui.ViewModels;

public sealed class ProcessingViewModel : INotifyPropertyChanged
{
    private double _progress;
    private string _currentStep = string.Empty;
    private string _statusMessage = string.Empty;
    private bool _isCompleted;
    private bool _hasError;
    private string _errorMessage = string.Empty;
    private string _logFilePath = string.Empty;

    public List<string> Steps { get; } = new()
    {
        step_loading,
        step_detecting,
        step_to_kpc,
        step_running_tool,
        step_from_kpc,
        step_saving
    };

    public double Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    public string CurrentStep
    {
        get => _currentStep;
        set { _currentStep = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set { _isCompleted = value; OnPropertyChanged(); }
    }

    public bool HasError
    {
        get => _hasError;
        set { _hasError = value; OnPropertyChanged(); }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); }
    }

    public string LogFilePath
    {
        get => _logFilePath;
        set { _logFilePath = value; OnPropertyChanged(); }
    }

    public event Action? RequestReturnToTools;
    public event Action? RequestReturnToImport;
    public event Action? RequestGoToExport;

    public void OnReturnToToolsClicked() => RequestReturnToTools?.Invoke();
    public void OnReturnToImportClicked() => RequestReturnToImport?.Invoke();
    public void OnGoToExportClicked() => RequestGoToExport?.Invoke();

    public void SetStep(int index, string detail = "")
    {
        if (index < 0 || index >= Steps.Count) return;
        CurrentStep = Steps[index];
        StatusMessage = string.IsNullOrEmpty(detail) ? Steps[index] : detail;
        Progress = (double)(index + 1) / Steps.Count * 100;
    }

    public void SetCompleted(string message)
    {
        IsCompleted = true;
        HasError = false;
        StatusMessage = message;
        Progress = 100;
    }

    public void SetError(string message, string logPath)
    {
        IsCompleted = true;
        HasError = true;
        ErrorMessage = message;
        LogFilePath = logPath;
        StatusMessage = processing_error;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
