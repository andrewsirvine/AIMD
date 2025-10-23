# 🏷️ AIMDiagnostic Imaging Report Builder — Product Requirements Document (v3.2)

**Version:** 3.2 (Template-integrated, multimodal roadmap)  
**Owner:** Founder (Radiologist)  
**Tech Lead:** CTO  
**Platform:** Windows 10/11 (Local-first, PowerScribe-compatible)

---

## 1. Purpose & Vision
Create a **local-first, modular radiology reporting system** that turns **minimal dictation** into **complete structured reports** for **CT, MRI, X-ray, and Ultrasound** exams.  

The system must:
- Enable **sub-second report generation**
- Keep **all sections editable by default**
- Allow **seamless PowerScribe paste (Findings + Impression)**
- Support **incremental expansion** to all imaging modalities

Initial rollout:
- CT Abdomen/Pelvis (CT AP)  
- CT Chest  
- CT Chest/Abdomen/Pelvis (CT CAP)

---

## 2. Core Architecture

| Layer | Function | Script |
|--------|-----------|--------|
| **Lexicon Compiler** | CSV → JSON rule objects (`trigger`, `require`, `exclude`, `priority`) | `compile_lexicon.py` |
| **Dictation Parser** | Text → Case JSON (routes dictation to sections) | `parse_from_dictation_lex.py` |
| **Report Builder** | Case JSON → Report JSON (fills defaults & templates) | `build_report_from_case_filtered.py` |
| **API Layer** | FastAPI microservice (`/generate`, `/health`) | `app.py` |

### `/generate` flow
1. Compile lexicon if stale.  
2. Parse dictation into structured case.  
3. Apply exam-specific template and rules.  
4. Return findings, impression, and unmatched JSON.

---

## 3. WPF Client

**Layout**
- **Left:** Inputs (Indication, Comparison, Dictation, Generate)  
- **Right:** Findings / Impression / Unmapped Tabs  

**Keybindings**
| Action | Shortcut |
|--------|-----------|
| Indication | Ctrl + Alt + I |
| Comparison | Ctrl + Alt + C |
| Generate | Ctrl + Enter |
| Regenerate | Ctrl + Alt + R |
| Paste to PowerScribe | Ctrl + Alt + P |
| Clear Dictation | Esc |

**Behavior**
- Findings + Impression always editable.  
- Regenerate merges intelligently (per-section diffs).  
- AHK paste validates clipboard content (length/hash), navigates PowerScribe via keystrokes, and rolls back if incomplete.  

---

## 4. Example Templates (from production data)

### CT Abdomen/Pelvis
```json
{
  "exam": "CT ABDOMEN PELVIS",
  "sections": [
    {"name": "LOWER CHEST", "default": "Normal"},
    {"name": "LIVER", "default": "Normal."},
    {"name": "GALLBLADDER/BILIARY", "default": "Normal."},
    {"name": "PANCREAS", "default": "Normal."},
    {"name": "SPLEEN", "default": "Normal."},
    {"name": "ADRENALS", "default": "Normal."},
    {"name": "KIDNEYS/URETERS", "default": "Normal."},
    {"name": "URINARY BLADDER", "default": "Normal."},
    {"name": "REPRODUCTIVE", "default": "Normal."},
    {"name": "STOMACH", "default": "Normal."},
    {"name": "SMALL BOWEL", "default": "Normal."},
    {"name": "COLON", "default": "Normal."},
    {"name": "APPENDIX", "default": "Normal."},
    {"name": "PERITONEUM", "default": "Normal."},
    {"name": "LYMPH NODES", "default": "None enlarged."},
    {"name": "VASCULATURE", "default": "Normal."},
    {"name": "BODY WALL", "default": "Normal."},
    {"name": "MUSCULOSKELETAL", "default": "No acute abnormality."}
  ],
  "impression_style": {"bulleted": true, "terse": true}
}
```

### CT Chest
```json
{
  "exam": "CT CHEST",
  "sections": [
    {"name": "LOWER NECK", "default": "Normal."},
    {"name": "HEART", "default": "Normal."},
    {"name": "VESSELS", "default": "Normal."},
    {"name": "MEDIASTINUM", "default": "Normal."},
    {"name": "LUNGS/PLEURA", "default": "Normal."},
    {"name": "LYMPH NODES", "default": "None enlarged."},
    {"name": "CHEST WALL", "default": "Normal."},
    {"name": "MUSCULOSKELETAL", "default": "No acute abnormality."},
    {"name": "UPPER ABDOMEN", "default": "No significant abnormality."}
  ],
  "impression_style": {"bulleted": true, "terse": true}
}
```

---

## 5. Data & Config

### Lexicon
CSV-driven lexicon defines section routing:
```
section,keyword,synonyms,priority,negation_safe
LIVER,hepatic,"liver,hepatic",10,true
BODY WALL,hernia,"umbilical hernia,inguinal hernia",7,true
KIDNEYS/URETERS,hydronephrosis,"hydroureteronephrosis",10,true
```

### Rules
- **Disambiguation:** Generic keywords require organ qualifier ±5 tokens.  
- **Impression templates:** Severity-based phrasing (critical → incidental).  
- **Comparison normalization:** Convert free text → `YYYY-MM-DD` where clear.

---

## 6. Functional Requirements

| Function | Description |
|-----------|--------------|
| **Report Generation** | End-to-end compile → parse → build. |
| **Editable Sections** | All fields editable by default. |
| **Safe Regeneration** | Non-destructive diff merge. |
| **Clipboard Paste** | Deterministic PowerScribe flow. |
| **Comparison Parsing** | Date normalization. |
| **Unmapped Management** | Visible list + one-click routing. |

---

## 7. Non-Functional Requirements
- **Speed:** `/generate` < 1s (goal ≤600 ms).  
- **Security:** Offline-only; optional AES-256 encryption for drafts.  
- **Maintainability:** Templates & lexicons hot-reloadable.  
- **Compatibility:** Windows 10/11, Citrix-friendly (keyboard-only).  
- **Reliability:** Paste rollback on partial failure.

---

## 8. Testing & Acceptance
- **Unit tests:** compile, parse, build, comparison, negation.  
- **Integration tests:** AHK paste validation.  
- **Golden fixtures:** 10+ de-identified dictations per exam.  
- **MVP sign-off:** CT AP, Chest, CAP pass schema + timing tests.

---

## 9. Expanded Roadmap (Multimodal Readiness)

| Phase | Milestone | Core Deliverables | Expansion Focus |
|--------|------------|-------------------|-----------------|
| **MVP (v3.2)** | CT AP / Chest / CAP | Core engine + WPF + PowerScribe paste | Establish CT pipeline baseline |
| **v1.1** | Site Profiles | Paste profiles + phrasing variants | Local customization & settings UI |
| **v1.2** | Dragon Adapter | Local dictation listener | Real-time dictation capture |
| **v1.3** | Modular Template Loader | Unified JSON schema for all exams | One template → any modality |
| **v1.4** | **MRI Templates** | MRI Brain, Spine, Abdomen, MSK | Expand lexicon & rules engine |
| **v1.5** | **Ultrasound Templates** | Abdomen, Pelvis, OB, Thyroid, Carotid | Add protocol-driven lexicon triggers |
| **v1.6** | **X-ray Templates** | Chest, Abdomen, MSK, Spine | Include view-count + technique parsing |
| **v2.0** | Universal Reporting Framework | Auto-template detection from dictation | Cross-modality report generation |
| **v2.1** | Custom Exam Builder UI | Drag-and-drop section creation | Radiologists can design new templates |
| **v2.2** | Lexicon Learning Mode | Automatically suggest new mappings | “Teach mode” for unmatched sentences |
| **v3.0+** | LLM-Assisted Workflow | Context-aware phrasing & recommendations | Intelligent impression synthesis |

**Architectural design requirement:**  
All lexicons, templates, and rules must follow a consistent JSON/CSV structure so any new modality (e.g., “MRI Brain” or “US Pelvis”) can be added via configuration only — no backend changes required.

---

## 10. Deliverables for AI Agent Implementation
1. **FastAPI service** (`/generate`, `/health`) with modular exam registry.  
2. **WPF client** with keyboard navigation and PowerScribe integration.  
3. **AutoHotkey v2 helpers** for deterministic paste sequences.  
4. **Test suite** (`pytest` + fixtures) for routing, timing, impression.  
5. **Seed templates** (CT AP, CT Chest, CT CAP) and example lexicon.  
6. **Roadmap hooks** for MRI, US, and XR expansion (shared schema).

---

## 11. Reference Repository Structure (Monorepo Blueprint)

**Purpose:**  
To standardize how the AIMDiagnostic Imaging Report Builder codebase is organized, enabling parallel development across frontend (WPF), backend (FastAPI), and automation (AHK), while maintaining a single shared source of truth for schemas, templates, and tests.

**Repository Layout**
```
aimd-reporting/
├─ docs/                          # Design, product, and architecture documentation
│  ├─ PRD/
│  │  └─ AIMDiagnostic_Report_Builder_PRD_v3.2.md
│  └─ ADR/                        # Architecture Decision Records
│
├─ engine/                        # Backend (FastAPI / Python)
│  ├─ engine/
│  │  ├─ app.py
│  │  ├─ models.py
│  │  ├─ services/
│  │  │  └─ generator.py
│  │  └─ pipelines/              # Future: compile/parse/build logic
│  ├─ data/
│  │  └─ templates/
│  │     ├─ ct_ap.template.json
│  │     └─ ct_chest.template.json
│  ├─ tests/
│  │  └─ test_generate_contract.py
│  ├─ pyproject.toml
│  └─ README.md
│
├─ client-wpf/                    # Frontend (WPF .NET 8)
│  ├─ AIMD.ReportApp.sln
│  ├─ src/
│  │  ├─ AIMD.ReportApp/
│  │  │  ├─ Views/
│  │  │  ├─ ViewModels/
│  │  │  ├─ Services/            # HTTP client to /generate
│  │  │  └─ App.xaml.cs
│  │  └─ AIMD.ReportApp.Tests/
│  └─ README.md
│
├─ automation/                    # AutoHotkey / PowerScribe paste logic
│  ├─ ahk/
│  │  └─ paste_powerscribe.ahk
│  └─ README.md
│
├─ shared/                        # Shared schemas and rules
│  ├─ schemas/                    # JSON schemas for templates/responses
│  ├─ rules/                      # YAML rules for impression logic
│  └─ scripts/                    # Utility scripts, pre-commit, lint
│
├─ .github/
│  ├─ workflows/
│  │  ├─ backend-ci.yml
│  │  └─ frontend-ci.yml
│  └─ CODEOWNERS
│
├─ .editorconfig
├─ .gitignore
└─ README.md
```

**Design Principles**
- **Monorepo-first:** Shared templates and schema stay synchronized between backend and frontend.  
- **Layer separation:** Each domain (engine, client, automation) is self-contained but cross-compatible.  
- **Test-first:** Each subproject must include a working test harness.  
- **CI-ready:** GitHub Actions workflows enforce linting and tests on every PR.  

**Why Monorepo:**  
- Keeps API contracts and client logic synchronized.  
- Allows AI agents to work in parallel (backend agent vs frontend agent).  
- Simplifies shared dataset evolution (lexicons, templates, schemas).  

**When to Split (Future):**  
After schema stability (e.g., `/generate` v1 frozen), individual repositories may be created:  
- `aimd-engine` (FastAPI backend)  
- `aimd-client-wpf` (frontend)  
- `aimd-contracts` (shared schemas & templates)
---

✅ **v3.2 Final Summary:**  
AIMDiagnostic Imaging Report Builder is now specified for **multimodal expansion** — allowing any future imaging type (CT, MRI, XR, US) to be added by simply defining new JSON templates and lexicon entries, with zero code modification.  
This architecture ensures long-term scalability across **all radiology subspecialties**.
