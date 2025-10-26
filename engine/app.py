from __future__ import annotations

from fastapi import FastAPI, HTTPException

from engine.models import GenerateRequest, GenerateResponse, HealthResponse
from engine.services.generator import ReportGenerator


def create_app() -> FastAPI:
    app = FastAPI(title="AIMD Reporting Engine", version="0.1.0")
    generator = ReportGenerator()

    @app.post("/generate", response_model=GenerateResponse)
    async def generate_report(request: GenerateRequest) -> GenerateResponse:
        try:
            return generator.generate(request)
        except ValueError as exc:  # template not found
            raise HTTPException(status_code=404, detail=str(exc)) from exc

    @app.get("/health", response_model=HealthResponse)
    async def health() -> HealthResponse:
        return generator.health()

    return app


app = create_app()
