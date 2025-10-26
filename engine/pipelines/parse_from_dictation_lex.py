from __future__ import annotations

import re
from typing import Dict, List, Sequence, Tuple

from engine.models import LexiconEntry

SENTENCE_SPLIT_RE = re.compile(r"(?<=[.!?])\s+")
LEXEME_RE = re.compile(r"[^\w\s]+", re.UNICODE)


def _normalize(text: str) -> str:
    return LEXEME_RE.sub("", text or "").lower()


def _is_negated(sentence: str, term: str) -> bool:
    window = sentence.lower()
    term_lower = term.lower()
    negation_tokens = ["no ", "without ", "absent ", "denies "]
    for token in negation_tokens:
        idx = window.find(token)
        if idx != -1:
            scope = window[idx + len(token) :]
            if term_lower in scope.split(",")[0]:
                return True
    return False


def parse_dictation(
    dictation: str, lexicon: Sequence[LexiconEntry]
) -> Tuple[Dict[str, List[str]], List[str]]:
    """
    Split dictation into sentences and route them to sections using lexicon keywords.
    Returns a mapping of section -> matched sentences and a list of unmatched sentences.
    """
    sentences = [
        sentence.strip()
        for sentence in SENTENCE_SPLIT_RE.split(dictation.strip())
        if sentence.strip()
    ]

    section_hits: Dict[str, List[str]] = {}
    unmatched: List[str] = []

    for sentence in sentences:
        normalized = _normalize(sentence)
        matched_section = None

        for entry in lexicon:
            for term in entry.all_terms():
                if term and term in normalized:
                    if not entry.negation_safe and _is_negated(sentence, term):
                        continue
                    matched_section = entry.section
                    break

            if matched_section:
                section_hits.setdefault(matched_section, []).append(sentence)
                break

        if not matched_section:
            unmatched.append(sentence)

    return section_hits, unmatched
