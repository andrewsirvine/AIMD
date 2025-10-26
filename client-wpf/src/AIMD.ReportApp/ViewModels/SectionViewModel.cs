using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIMD.ReportApp.ViewModels;

public sealed class SectionViewModel : INotifyPropertyChanged
{
    private string _content = string.Empty;
    private bool _isUserEdited;
    private string _lastEngineSuggestion = string.Empty;

    public SectionViewModel(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public string Content
    {
        get => _content;
        set
        {
            if (_content == value)
            {
                return;
            }

            _content = value;
            _isUserEdited = true;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsUserEdited));
        }
    }

    public bool IsUserEdited => _isUserEdited;

    public string LastEngineSuggestion
    {
        get => _lastEngineSuggestion;
        private set
        {
            if (_lastEngineSuggestion == value)
            {
                return;
            }

            _lastEngineSuggestion = value;
            OnPropertyChanged();
        }
    }

    public void ApplyEngineContent(string content, bool preserveUserChanges)
    {
        LastEngineSuggestion = content;

        if (preserveUserChanges && _isUserEdited)
        {
            return;
        }

        if (_content != content)
        {
            _content = content;
            OnPropertyChanged(nameof(Content));
        }

        if (_isUserEdited)
        {
            _isUserEdited = false;
            OnPropertyChanged(nameof(IsUserEdited));
        }
    }

    public void Reset()
    {
        _content = string.Empty;
        _isUserEdited = false;
        _lastEngineSuggestion = string.Empty;
        OnPropertyChanged(nameof(Content));
        OnPropertyChanged(nameof(IsUserEdited));
        OnPropertyChanged(nameof(LastEngineSuggestion));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
