from __future__ import annotations

import json
import time
from functools import lru_cache
from pathlib import Path
from typing import Dict, List, Sequence

from engine.models import (
    GenerateRequest,
    GenerateResponse,
    HealthResponse,
    LexiconEntry,
    TemplateDefinition,
)
from engine.pipelines.build_report_from_case_filtered import build_report
from engine.pipelines.compile_lexicon import compile_lexicon
from engine.pipelines.parse_from_dictation_lex import parse_dictation


class ReportGenerator:
    def __init__(
        self,
        data_dir: Path | None = None,
        templates_dir: Path | None = None,
        lexicon_name: str = "lexicon.csv",
    ) -> None:
        self.root = Path(__file__).resolve().parents[1]
        self.data_dir = data_dir or (self.root / "data")
        self.templates_dir = templates_dir or (self.data_dir / "templates")
        self.lexicon_path = self.data_dir / lexicon_name
        self._templates_cache: Dict[str, TemplateDefinition] = {}

    def _load_template(self, exam: str) -> TemplateDefinition:
        normalized = exam.strip().lower().replace(" ", "_")
        filename = f"{normalized}.template.json"
        path = self.templates_dir / filename
        if not path.exists():
            raise ValueError(f"No template registered for exam '{exam}'")

        cached = self._templates_cache.get(path.as_posix())
        if cached:
            return cached

        with path.open(encoding="utf-8") as handle:
            template = TemplateDefinition.model_validate_json(handle.read())

        self._templates_cache[path.as_posix()] = template
        return template

    @lru_cache(maxsize=1)
    def _lexicon(self) -> Sequence[LexiconEntry]:
        return compile_lexicon(self.lexicon_path)

    def refresh(self) -> None:
        """Invalidate cached artifacts."""
        self._templates_cache.clear()
        self._lexicon.cache_clear()  # type: ignore[attr-defined]

    def generate(self, request: GenerateRequest) -> GenerateResponse:
        timings: Dict[str, float] = {}
        start = time.perf_counter()
        template = self._load_template(request.exam)
        timings["load_template_ms"] = (time.perf_counter() - start) * 1000

        start = time.perf_counter()
        lexicon = self._lexicon()
        timings["compile_lexicon_ms"] = (time.perf_counter() - start) * 1000

        start = time.perf_counter()
        section_hits, unmatched = parse_dictation(request.dictation, lexicon)
        timings["parse_dictation_ms"] = (time.perf_counter() - start) * 1000

        start = time.perf_counter()
        findings, impression = build_report(template, section_hits, unmatched)
        timings["build_report_ms"] = (time.perf_counter() - start) * 1000

        metadata = {}
        if request.indication:
            metadata["indication"] = request.indication
        if request.comparison:
            metadata["comparison"] = request.comparison

        return GenerateResponse(
            exam=template.exam,
            findings=findings,
            impression=impression,
            unmatched=unmatched,
            timings_ms={k: round(v, 2) for k, v in timings.items()},
            metadata=metadata,
        )

    def health(self) -> HealthResponse:
        templates: List[str] = []
        for path in sorted(self.templates_dir.glob("*.template.json")):
            templates.append(path.stem.replace(".template", "").upper())
        return HealthResponse(status="ok", templates_loaded=templates)
