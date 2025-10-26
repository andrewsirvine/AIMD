# AIMDiagnostic Imaging Report Builder

Local-first toolkit that converts minimal radiology dictation into complete, PowerScribe-ready reports. The monorepo bundles the FastAPI engine, WPF desktop client, AutoHotkey automation, and shared schemas/templates described in the PRD (v3.2).

## Repository Layout

- engine/ — Python FastAPI service for /generate and /health, lexicon compiler, templates, and pytest suite.
- client-wpf/ — .NET 8 WPF client with MVVM architecture, keyboard shortcuts, and PowerScribe clipboard workflow. Includes MSTest project for the primary view model.
- utomation/ — AutoHotkey helpers for deterministic clipboard pasting into PowerScribe.
- shared/ — JSON schemas, impression rules, and cross-domain scripts/templates.
- docs/ — PRD copy plus space for ADRs and future product documentation.

## Backend Quickstart

`ash
cd engine
python -m venv .venv
. .venv/Scripts/activate  # Windows
pip install -r requirements.txt
uvicorn app:app --reload
`

The /generate endpoint accepts a JSON payload that matches shared/schemas/generate-request.schema.json and returns structured findings, impression bullets, unmatched sentences, and timing metadata.

Run tests:

`ash
python -m pytest
`

## WPF Client Quickstart

`ash
cd client-wpf
dotnet build AIMD.ReportApp.sln
# optional: dotnet test AIMD.ReportApp.sln
`

Launch AIMD.ReportApp.exe from src/AIMD.ReportApp/bin/Debug/net8.0-windows/. The client exposes the CT AP/Chest/CAP templates, preserves radiologist edits across regenerations, and copies findings/impression to the clipboard for PowerScribe (Ctrl+Alt+P).

## Automation Helper

The AutoHotkey v2 script at utomation/ahk/paste_powerscribe.ahk verifies clipboard content (length + CRC32) before issuing the paste/tab sequence into PowerScribe, then restores the previous clipboard contents.

## CI Workflows

- .github/workflows/backend-ci.yml runs pytest against the engine on Ubuntu.
- .github/workflows/frontend-ci.yml builds and tests the WPF solution on Windows.

## Next Steps

- Flesh out lexicon/rule coverage for remaining CT variants.
- Add timing benchmarks and golden fixtures per the PRD test requirements.
- Integrate the AutoHotkey workflow with the WPF client (e.g., launching scripts or verifying paste status).
