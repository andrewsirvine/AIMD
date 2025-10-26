# AIMD Reporting Engine

FastAPI microservice that transforms minimal dictation into structured radiology reports using JSON templates and a CSV-driven lexicon.

## Running locally

```bash
poetry install
poetry run uvicorn app:app --reload
```

## Testing

```bash
poetry run pytest
```
