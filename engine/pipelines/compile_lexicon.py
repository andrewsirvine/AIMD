from __future__ import annotations

import csv
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Sequence, Tuple

from engine.models import LexiconEntry


@dataclass
class _CacheState:
    mtime: float
    entries: List[LexiconEntry]


_cache: Dict[Path, _CacheState] = {}


def compile_lexicon(csv_path: Path) -> Sequence[LexiconEntry]:
    """
    Load lexicon entries from CSV. Results are cached using the file mtime.
    """
    csv_path = csv_path.resolve()
    if not csv_path.exists():
        raise FileNotFoundError(f"Lexicon CSV not found at {csv_path}")

    mtime = csv_path.stat().st_mtime
    cached = _cache.get(csv_path)
    if cached and cached.mtime == mtime:
        return cached.entries

    entries: List[LexiconEntry] = []
    with csv_path.open(newline="", encoding="utf-8") as handle:
        reader = csv.DictReader(handle)
        for row in reader:
            section = row.get("section", "").strip()
            keyword = row.get("keyword", "").strip()
            synonyms_raw = row.get("synonyms", "") or ""
            priority_raw = row.get("priority", "0") or "0"
            negation_flag = (row.get("negation_safe", "false") or "false").lower()

            if not section or not keyword:
                continue

            synonyms = [s.strip() for s in synonyms_raw.split(",") if s.strip()]
            try:
                priority = int(priority_raw)
            except ValueError:
                priority = 0

            entry = LexiconEntry(
                section=section.upper(),
                keyword=keyword.lower(),
                synonyms=synonyms,
                priority=priority,
                negation_safe=negation_flag in {"true", "1", "yes"},
            )
            entries.append(entry)

    entries.sort(key=lambda e: e.priority, reverse=True)
    _cache[csv_path] = _CacheState(mtime=mtime, entries=entries)
    return entries
