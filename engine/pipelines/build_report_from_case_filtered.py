from __future__ import annotations

from typing import Dict, Iterable, List, Sequence, Tuple

from engine.models import TemplateDefinition, TemplateSection


def _normalize_section_name(name: str) -> str:
    return (name or "").strip().upper()


def _section_text(section: TemplateSection, sentences: Sequence[str]) -> str:
    return " ".join(sentences).strip() if sentences else section.default


def build_report(
    template: TemplateDefinition,
    section_hits: Dict[str, List[str]],
    unmatched: Iterable[str],
) -> Tuple[Dict[str, str], List[str]]:
    findings: Dict[str, str] = {}
    impression: List[str] = []
    unmatched_list = [sentence for sentence in unmatched]

    for section in template.sections:
        key = _normalize_section_name(section.name)
        sentences = section_hits.get(key, [])
        findings[section.name] = _section_text(section, sentences)
        if sentences:
            bullet = f"{section.name.title()}: {' '.join(sentences)}"
            impression.append(bullet)

    if not impression:
        impression.append("No acute abnormality.")

    if unmatched_list:
        impression.append(f"Unmapped: {len(unmatched_list)} sentence(s) available for review.")

    return findings, impression
