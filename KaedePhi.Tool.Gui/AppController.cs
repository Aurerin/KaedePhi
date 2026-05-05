using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using KaedePhi.Tool.Common;
using KaedePhi.Tool.Gui.Services;
using KaedePhi.Tool.Gui.ViewModels;
using static KaedePhi.Tool.Localization.GuiLocalizationString;

namespace KaedePhi.Tool.Gui;

internal sealed class AppController
{
    private readonly MainViewModel _main;
    private readonly GuiChartService _chart;
    private readonly LogService _log;
    private readonly ConfigService _config;
    private readonly Window _window;

    private readonly ImportViewModel _importVm;
    private readonly ToolViewModel _toolVm;
    private readonly ExportViewModel _exportVm;
    private readonly ProcessingViewModel _processingVm;
    private readonly SettingsViewModel _settingsVm;

    private ChartType _detectedType;
    private CancellationTokenSource? _cts;
    private bool _isFileProcessing;

    public AppController(MainViewModel main, GuiChartService chart, LogService log, ConfigService config, Window window)
    {
        _main = main;
        _chart = chart;
        _log = log;
        _config = config;
        _window = window;

        _importVm = new ImportViewModel();
        _toolVm = new ToolViewModel();
        _exportVm = new ExportViewModel();
        _processingVm = new ProcessingViewModel();
        _settingsVm = new SettingsViewModel(config);

        WireEvents();
    }

    public void Initialize()
    {
        NavigateToImport();
    }

    private void WireEvents()
    {
        _importVm.FileSelected += OnFileSelected;
        _toolVm.RequestRun += OnToolRun;
        _toolVm.RequestExport += OnToolExport;
        _toolVm.RequestSettings += NavigateToSettings;
        _toolVm.PropertyChanged += OnToolVmPropertyChanged;
        _exportVm.RequestExport += OnExportExecute;
        _exportVm.RequestReturnToImport += OnReturnToImport;
        _processingVm.RequestReturnToTools += NavigateToTool;
        _processingVm.RequestReturnToImport += OnReturnToImport;
        _processingVm.RequestGoToExport += NavigateToExport;
        _settingsVm.RequestReturnToTools += OnReturnFromSettings;
    }

    private void OnToolVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ToolViewModel.SelectedTool) && _toolVm.SelectedTool != null)
            _toolVm.ApplyConfigDefaults(_config.Config);
    }

    private void NavigateToImport()
    {
        _chart.ClearWorkspace();
        _importVm.UseStream = false;
        _main.CurrentPage = _importVm;
        _log.Info(log_navigate_import);
    }

    private void NavigateToTool()
    {
        _toolVm.StatusText = string.Empty;
        _main.CurrentPage = _toolVm;
    }

    private void NavigateToExport()
    {
        _exportVm.SelectedFormat = ChartType.RePhiEdit;
        _exportVm.UseStream = false;
        _exportVm.StatusText = string.Empty;
        _exportVm.IsExporting = false;
        _main.CurrentPage = _exportVm;
    }

    private void NavigateToSettings()
    {
        _settingsVm.StatusText = string.Empty;
        _main.CurrentPage = _settingsVm;
    }

    private void OnReturnFromSettings()
    {
        _toolVm.ApplyConfigDefaults(_config.Config);
        NavigateToTool();
    }

    private void NavigateToProcessing()
    {
        _processingVm.Progress = 0;
        _processingVm.CurrentStep = string.Empty;
        _processingVm.StatusMessage = string.Empty;
        _processingVm.IsCompleted = false;
        _processingVm.HasError = false;
        _processingVm.ErrorMessage = string.Empty;
        _processingVm.LogFilePath = string.Empty;
        _main.CurrentPage = _processingVm;
    }

    private void OnReturnToImport()
    {
        NavigateToImport();
    }

    private async void OnFileSelected(string filePath, bool useStream)
    {
        if (_isFileProcessing) return;
        _isFileProcessing = true;

        try
        {
            await _chart.CopyToWorkspaceAsync(filePath, useStream, CancellationToken.None);

            var (_, detectedType) = await _chart.LoadAndDetectFromWorkspaceAsync(CancellationToken.None);
            _detectedType = detectedType;

            _toolVm.CurrentFileName = System.IO.Path.GetFileName(filePath);
            _toolVm.DetectedFormat = detectedType.ToString();
            _toolVm.SelectedTool = null;
            _toolVm.StatusText = string.Empty;

            NavigateToTool();
        }
        catch (Exception ex)
        {
            _log.Error(log_load_failed, ex);
        }
        finally
        {
            _isFileProcessing = false;
        }
    }

    private async void OnToolRun()
    {
        if (_toolVm.SelectedTool == null || _toolVm.IsProcessing) return;

        _toolVm.IsProcessing = true;
        _toolVm.StatusText = status_processing;
        _cts = new CancellationTokenSource();

        try
        {
            var toolId = _toolVm.SelectedTool.ToolId;
            NavigateToProcessing();

            // Step 0: Load from workspace
            _processingVm.SetStep(0, log_step_loading);
            var (text, detectedType) = await _chart.LoadAndDetectFromWorkspaceAsync(_cts.Token);
            _detectedType = detectedType;

            // Step 1: Detect format
            _processingVm.SetStep(1, string.Format(log_step_detected, detectedType));

            // Step 2: Convert to KPC
            _processingVm.SetStep(2, log_step_converting);
            var kpcChart = await Task.Run(() => _chart.ConvertToKpc(text, detectedType), _cts.Token);

            // Step 3: Run tool
            _processingVm.SetStep(3, string.Format(log_running_tool, toolId));
            var toolProgress = new Progress<ToolProgress>(p =>
            {
                var overall = p.OverallPercentage >= 0 ? p.OverallPercentage : p.Percentage;
                _processingVm.SetToolProgress(p.Percentage, overall, p.Detail);
            });
            await Task.Run(() =>
            {
                switch (toolId)
                {
                    case "unbind":
                        _chart.RunFatherUnbind(kpcChart, _toolVm.Precision, _toolVm.Tolerance,
                            _toolVm.ClassicMode, _toolVm.DisableCompress, toolProgress);
                        break;
                    case "layermerge":
                        _chart.RunLayerMerge(kpcChart, _toolVm.Precision, _toolVm.Tolerance,
                            _toolVm.ClassicMode, _toolVm.DisableCompress, toolProgress);
                        break;
                    case "cut":
                        _chart.RunCutEvent(kpcChart, _toolVm.Precision, _toolVm.Tolerance,
                            _toolVm.DisableCompress, toolProgress);
                        break;
                    case "fit":
                        _chart.RunFitEvent(kpcChart, _toolVm.Tolerance, toolProgress);
                        break;
                    case "render":
                        _chart.RunRender(kpcChart, _toolVm.PixelsPerBeat,
                            _toolVm.ChannelWidth, _toolVm.SamplesPerEvent, _toolVm.BeatSubdivisions, toolProgress);
                        break;
                }
            }, _cts.Token);

            // Step 4: Convert back to original format
            _processingVm.SetStep(4, step_from_kpc);

            // Step 5: Save back to workspace
            _processingVm.SetStep(5, step_saving);
            await _chart.SaveKpcToWorkspaceAsync(kpcChart, detectedType, false, _cts.Token);

            _processingVm.SetCompleted(string.Format(log_tool_completed, toolId));
            _log.Info(string.Format(log_tool_completed, toolId));
        }
        catch (Exception ex)
        {
            _log.Error(log_tool_failed, ex);
            _processingVm.SetError(ex.Message, _log.CurrentLogFile);
        }
        finally
        {
            _toolVm.IsProcessing = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void OnToolExport()
    {
        NavigateToExport();
    }

    private async void OnExportExecute()
    {
        _exportVm.IsExporting = true;
        _exportVm.StatusText = status_exporting;

        try
        {
            var topLevel = TopLevel.GetTopLevel(_window);
            if (topLevel == null)
            {
                _exportVm.StatusText = status_cannot_access_picker;
                return;
            }

            var targetFormat = _exportVm.SelectedFormat;
            var formatName = targetFormat.ToString();

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = export_title,
                SuggestedFileName = $"export_{formatName}.json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON Files") { Patterns = new[] { "*.json" } }
                }
            });

            if (file != null)
            {
                var outputPath = file.TryGetLocalPath();
                if (!string.IsNullOrEmpty(outputPath))
                {
                    _log.Info(string.Format(log_exporting_to, outputPath, targetFormat));

                    // Read current workspace, detect, convert to KPC, then export as target
                    var (text, detectedType) = await _chart.LoadAndDetectFromWorkspaceAsync(CancellationToken.None);
                    var kpcChart = _chart.ConvertToKpc(text, detectedType);
                    await _chart.ConvertFromKpcAndSaveAsync(
                        kpcChart, targetFormat, outputPath, _exportVm.UseStream, CancellationToken.None);

                    _exportVm.StatusText = string.Format(status_exported_to, outputPath);
                    _log.Info(log_export_done);
                }
            }
            else
            {
                _exportVm.StatusText = status_export_cancelled;
                _log.Info(log_export_cancelled);
            }
        }
        catch (Exception ex)
        {
            _log.Error(log_export_failed, ex);
            _exportVm.StatusText = string.Format(status_error_with_log, ex.Message, _log.CurrentLogFile);
        }
        finally
        {
            _exportVm.IsExporting = false;
        }
    }
}
