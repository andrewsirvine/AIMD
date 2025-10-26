using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using AIMD.ReportApp.Commands;
using AIMD.ReportApp.Models;
using AIMD.ReportApp.Services;

namespace AIMD.ReportApp.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly Dictionary<string, (string Name, string Default)[]> TemplateSeed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [
                "CT ABDOMEN PELVIS"
            ] = new[]
            {
                ("LOWER CHEST", "Normal."),
                ("LIVER", "Normal."),
                ("GALLBLADDER/BILIARY", "Normal."),
                ("PANCREAS", "Normal."),
                ("SPLEEN", "Normal."),
                ("ADRENALS", "Normal."),
                ("KIDNEYS/URETERS", "Normal."),
                ("URINARY BLADDER", "Normal."),
                ("REPRODUCTIVE", "Normal."),
                ("STOMACH", "Normal."),
                ("SMALL BOWEL", "Normal."),
                ("COLON", "Normal."),
                ("APPENDIX", "Normal."),
                ("PERITONEUM", "Normal."),
                ("LYMPH NODES", "None enlarged."),
                ("VASCULATURE", "Normal."),
                ("BODY WALL", "Normal."),
                ("MUSCULOSKELETAL", "No acute abnormality.")
            },
            [
                "CT CHEST"
            ] = new[]
            {
                ("LOWER NECK", "Normal."),
                ("HEART", "Normal."),
                ("VESSELS", "Normal."),
                ("MEDIASTINUM", "Normal."),
                ("LUNGS/PLEURA", "Normal."),
                ("LYMPH NODES", "None enlarged."),
                ("CHEST WALL", "Normal."),
                ("MUSCULOSKELETAL", "No acute abnormality."),
                ("UPPER ABDOMEN", "No significant abnormality.")
            },
            [
                "CT CHEST ABDOMEN PELVIS"
            ] = new[]
            {
                ("LOWER NECK", "Normal."),
                ("HEART", "Normal."),
                ("VESSELS", "Normal."),
                ("MEDIASTINUM", "Normal."),
                ("LUNGS/PLEURA", "Normal."),
                ("LYMPH NODES", "None enlarged."),
                ("CHEST WALL", "Normal."),
                ("LIVER", "Normal."),
                ("GALLBLADDER/BILIARY", "Normal."),
                ("PANCREAS", "Normal."),
                ("SPLEEN", "Normal."),
                ("ADRENALS", "Normal."),
                ("KIDNEYS/URETERS", "Normal."),
                ("URINARY BLADDER", "Normal."),
                ("REPRODUCTIVE", "Normal."),
                ("GASTROINTESTINAL", "Normal."),
                ("PERITONEUM", "Normal."),
                ("MUSCULOSKELETAL", "No acute abnormality.")
            },
        };

    private readonly IReportService _reportService;
    private readonly AsyncRelayCommand _generateCommand;
    private readonly AsyncRelayCommand _regenerateCommand;
    private readonly RelayCommand _clearCommand;
    private readonly RelayCommand _pasteCommand;

    private string _selectedExam = TemplateSeed.Keys.First();
    private string _indication = string.Empty;
    private string _comparison = string.Empty;
    private string _dictation = string.Empty;
    private string _impressionText = string.Empty;
    private string _unmatchedJson = "[]";
    private string _statusMessage = "Ready.";
    private bool _isBusy;
    private bool _disposed;

    public MainViewModel()
        : this(new ReportService())
    {
    }

    public MainViewModel(IReportService reportService)
    {
        _reportService = reportService;
        Findings = new ObservableCollection<SectionViewModel>();
        ExamOptions = new ObservableCollection<string>(TemplateSeed.Keys.OrderBy(x => x));

        _generateCommand = new AsyncRelayCommand(() => GenerateAsync(preserveUserChanges: false), CanGenerate);
        _regenerateCommand = new AsyncRelayCommand(() => GenerateAsync(preserveUserChanges: true), CanGenerate);
        _clearCommand = new RelayCommand(ClearReport);
        _pasteCommand = new RelayCommand(CopyToClipboard, () => HasReport);

        ApplyTemplateDefaults(forceReset: true);
    }

    public ObservableCollection<string> ExamOptions { get; }

    public ObservableCollection<SectionViewModel> Findings { get; }

    public ICommand GenerateCommand => _generateCommand;

    public ICommand RegenerateCommand => _regenerateCommand;

    public ICommand ClearCommand => _clearCommand;

    public ICommand PasteCommand => _pasteCommand;

    public string SelectedExam
    {
        get => _selectedExam;
        set
        {
            if (string.Equals(_selectedExam, value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _selectedExam = value;
            OnPropertyChanged();
            ApplyTemplateDefaults(forceReset: true);
            StatusMessage = $"Loaded template for {_selectedExam}.";
            UpdateCommandStates();
        }
    }

    public string Indication
    {
        get => _indication;
        set
        {
            if (_indication == value)
            {
                return;
            }

            _indication = value;
            OnPropertyChanged();
        }
    }

    public string Comparison
    {
        get => _comparison;
        set
        {
            if (_comparison == value)
            {
                return;
            }

            _comparison = value;
            OnPropertyChanged();
        }
    }

    public string Dictation
    {
        get => _dictation;
        set
        {
            if (_dictation == value)
            {
                return;
            }

            _dictation = value;
            OnPropertyChanged();
            UpdateCommandStates();
        }
    }

    public string ImpressionText
    {
        get => _impressionText;
        set
        {
            if (_impressionText == value)
            {
                return;
            }

            _impressionText = value;
            OnPropertyChanged();
            UpdateCommandStates();
        }
    }

    public string UnmatchedJson
    {
        get => _unmatchedJson;
        set
        {
            if (_unmatchedJson == value)
            {
                return;
            }

            _unmatchedJson = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value)
            {
                return;
            }

            _isBusy = value;
            OnPropertyChanged();
            UpdateCommandStates();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool HasReport =>
        Findings.Any(section => !string.IsNullOrWhiteSpace(section.Content))
        || !string.IsNullOrWhiteSpace(ImpressionText);

    public Task GenerateAsync(bool preserveUserChanges = false) => GenerateInternalAsync(preserveUserChanges);

    private async Task GenerateInternalAsync(bool preserveUserChanges)
    {
        if (string.IsNullOrWhiteSpace(Dictation))
        {
            StatusMessage = "Dictation is required before generating.";
            return;
        }

        IsBusy = true;
        try
        {
            var payload = new GenerateRequestPayload
            {
                Exam = SelectedExam,
                Dictation = Dictation,
                Indication = string.IsNullOrWhiteSpace(Indication) ? null : Indication,
                Comparison = string.IsNullOrWhiteSpace(Comparison) ? null : Comparison,
            };

            var response = await _reportService.GenerateAsync(payload).ConfigureAwait(true);
            if (response is null)
            {
                StatusMessage = "Engine returned an empty response.";
                return;
            }

            ApplyResponse(response, preserveUserChanges);
            StatusMessage = BuildStatusMessage(response);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Generate failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string BuildStatusMessage(GenerateResponsePayload response)
    {
        if (response.Timings.Count == 0)
        {
            return $"Generated report for {response.Exam}.";
        }

        var total = response.Timings.Values.Sum();
        var details = string.Join(
            ", ",
            response.Timings.Select(kvp => $"{kvp.Key.Replace("_", " ")}: {kvp.Value:0.##} ms")
        );

        return $"Generated {response.Exam} in {total:0.##} ms ({details}).";
    }

    private void ApplyResponse(GenerateResponsePayload response, bool preserveUserChanges)
    {
        SyncSections(response.Findings, preserveUserChanges);
        ImpressionText = string.Join(Environment.NewLine, response.Impression);

        var options = new JsonSerializerOptions { WriteIndented = true };
        UnmatchedJson = JsonSerializer.Serialize(response.Unmatched, options);
        UpdateCommandStates();
    }

    private void SyncSections(Dictionary<string, string> findings, bool preserveUserChanges)
    {
        var existing = Findings.ToDictionary(section => section.Name, StringComparer.OrdinalIgnoreCase);
        var ordered = new List<SectionViewModel>();

        foreach (var kvp in findings)
        {
            if (!existing.TryGetValue(kvp.Key, out var section))
            {
                section = new SectionViewModel(kvp.Key);
            }

            section.ApplyEngineContent(kvp.Value, preserveUserChanges);
            ordered.Add(section);
        }

        Findings.Clear();
        foreach (var section in ordered)
        {
            Findings.Add(section);
        }
    }

    private void ApplyTemplateDefaults(bool forceReset)
    {
        if (!TemplateSeed.TryGetValue(SelectedExam, out var sections))
        {
            Findings.Clear();
            return;
        }

        var existing = Findings.ToDictionary(section => section.Name, StringComparer.OrdinalIgnoreCase);
        Findings.Clear();

        foreach (var (name, @default) in sections)
        {
            if (!existing.TryGetValue(name, out var section) || forceReset)
            {
                section = new SectionViewModel(name);
            }

            section.ApplyEngineContent(@default, preserveUserChanges: false);
            Findings.Add(section);
        }

        ImpressionText = string.Empty;
        UnmatchedJson = "[]";
        UpdateCommandStates();
    }

    private void ClearReport()
    {
        Dictation = string.Empty;
        Indication = string.Empty;
        Comparison = string.Empty;
        ApplyTemplateDefaults(forceReset: true);
        StatusMessage = "Cleared current report.";
    }

    private void CopyToClipboard()
    {
        if (!HasReport)
        {
            StatusMessage = "Nothing to copy.";
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine("FINDINGS:");
        foreach (var section in Findings)
        {
            builder.AppendLine($"{section.Name}: {section.Content}");
        }

        builder.AppendLine();
        builder.AppendLine("IMPRESSION:");
        builder.AppendLine(ImpressionText);

        Clipboard.SetText(builder.ToString());
        StatusMessage = "Findings and impression copied to clipboard.";
    }

    private bool CanGenerate() => !IsBusy && !string.IsNullOrWhiteSpace(Dictation);

    private void UpdateCommandStates()
    {
        _generateCommand.RaiseCanExecuteChanged();
        _regenerateCommand.RaiseCanExecuteChanged();
        _pasteCommand.RaiseCanExecuteChanged();
        _clearCommand.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _reportService.Dispose();
        _disposed = true;
    }
}
