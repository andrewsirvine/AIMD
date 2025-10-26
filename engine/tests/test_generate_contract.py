from engine.models import GenerateRequest
from engine.services.generator import ReportGenerator


def test_generate_ct_ap_pipeline_roundtrip():
    generator = ReportGenerator()
    request = GenerateRequest(
        exam="CT ABDOMEN PELVIS",
        dictation=(
            "Liver shows mild steatosis. No splenic injury. "
            "There is hydronephrosis of the right kidney. "
            "No evidence of appendicitis."
        ),
        indication="Abdominal pain",
        comparison="Prior CT 01/2023",
    )

    response = generator.generate(request)

    assert response.exam == "CT ABDOMEN PELVIS"
    assert "LIVER" in response.findings
    assert response.findings["LIVER"].startswith("Liver shows")
    assert response.findings["KIDNEYS/URETERS"].startswith("There is hydronephrosis")
    assert response.metadata["indication"] == "Abdominal pain"
    assert response.metadata["comparison"] == "Prior CT 01/2023"
    assert response.impression, "Impression should default to at least one statement."
