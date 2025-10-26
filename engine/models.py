from __future__ import annotations

from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Sequence

from pydantic import BaseModel, Field


@dataclass(slots=True)
class LexiconEntry:
    section: str
    keyword: str
    synonyms: Sequence[str]
    priority: int = 0
    negation_safe: bool = False

    def all_terms(self) -> List[str]:
        terms = [self.keyword]
        terms.extend(self.synonyms)
        return [t.lower() for t in terms]


class TemplateSection(BaseModel):
    name: str
    default: str


class TemplateDefinition(BaseModel):
    exam: str
    sections: List[TemplateSection]
    impression_style: Dict[str, Optional[bool]] = Field(default_factory=dict)


class GenerateRequest(BaseModel):
    exam: str = Field(..., description="Exam name, e.g. CT ABDOMEN PELVIS.")
    dictation: str = Field(..., description="Raw free-text dictation.")
    indication: Optional[str] = Field(default=None)
    comparison: Optional[str] = Field(default=None)


class SectionFinding(BaseModel):
    section: str
    content: str
    source_sentences: List[str] = Field(default_factory=list)


class GenerateResponse(BaseModel):
    exam: str
    findings: Dict[str, str]
    impression: List[str]
    unmatched: List[str]
    timings_ms: Dict[str, float] = Field(default_factory=dict)
    metadata: Dict[str, str] = Field(default_factory=dict)


class HealthResponse(BaseModel):
    status: str
    timestamp: datetime = Field(default_factory=datetime.utcnow)
    templates_loaded: List[str] = Field(default_factory=list)
