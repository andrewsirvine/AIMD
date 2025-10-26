# AIMD Report WPF Client

MVVM desktop client targeting Windows 10/11 with fast keyboard-driven workflows.

## Key bindings

- Ctrl+Alt+I — focus Indication input
- Ctrl+Alt+C — focus Comparison input
- Ctrl+Alt+D — focus Dictation input
- Ctrl+Enter — generate report
- Ctrl+Alt+R — regenerate (preserves sections you edited)
- Ctrl+Alt+P — copy Findings + Impression to clipboard
- Esc — clear dictation and reset sections

## HTTP configuration

The client assumes the FastAPI service is reachable at http://localhost:8000. Update ReportService if you need a different host or port.

## Tests

`ash
dotnet test AIMD.ReportApp.sln
`

