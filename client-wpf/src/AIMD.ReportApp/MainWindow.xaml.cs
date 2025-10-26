using System;
using System.Windows;
using System.Windows.Input;

using AIMD.ReportApp.ViewModels;

namespace AIMD.ReportApp;

public partial class MainWindow : Window
{
    public static readonly RoutedUICommand FocusIndicationCommand = new("Indication", "FocusIndicationCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FocusComparisonCommand = new("Comparison", "FocusComparisonCommand", typeof(MainWindow));
    public static readonly RoutedUICommand FocusDictationCommand = new("Dictation", "FocusDictationCommand", typeof(MainWindow));

    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private void HandleFocusIndication(object sender, ExecutedRoutedEventArgs e)
    {
        IndicationInput.Focus();
        IndicationInput.SelectAll();
    }

    private void HandleFocusComparison(object sender, ExecutedRoutedEventArgs e)
    {
        ComparisonInput.Focus();
        ComparisonInput.SelectAll();
    }

    private void HandleFocusDictation(object sender, ExecutedRoutedEventArgs e)
    {
        DictationInput.Focus();
        DictationInput.SelectAll();
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}
