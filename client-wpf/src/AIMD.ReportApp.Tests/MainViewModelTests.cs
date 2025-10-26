using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AIMD.ReportApp.Models;
using AIMD.ReportApp.Services;
using AIMD.ReportApp.ViewModels;

namespace AIMD.ReportApp.Tests;

[TestClass]
public sealed class MainViewModelTests
{
    [TestMethod]
    public async Task GenerateAsync_PopulatesFindingsAndImpression()
    {
        var fakeService = new FakeReportService();
        var viewModel = new MainViewModel(fakeService)
        {
            Dictation = "Sample dictation with liver findings."
        };

        await viewModel.GenerateAsync();

        Assert.IsNotNull(fakeService.LastRequest);
        Assert.AreEqual("CT ABDOMEN PELVIS", fakeService.LastRequest!.Exam);
        var liverSection = viewModel.Findings.First(section => section.Name == "LIVER");
        Assert.AreEqual("Liver steatosis.", liverSection.Content);
        Assert.IsFalse(string.IsNullOrWhiteSpace(viewModel.ImpressionText));
    }

    private sealed class FakeReportService : IReportService
    {
        public GenerateRequestPayload? LastRequest { get; private set; }

        public Task<GenerateResponsePayload?> GenerateAsync(
            GenerateRequestPayload payload,
            CancellationToken cancellationToken = default
        )
        {
            LastRequest = payload;
            var response = new GenerateResponsePayload
            {
                Exam = payload.Exam,
                Findings = new Dictionary<string, string>
                {
                    ["LIVER"] = "Liver steatosis.",
                    ["SPLEEN"] = "Normal spleen."
                },
                Impression = new List<string> { "No acute abnormality." },
                Unmatched = new List<string>()
            };

            return Task.FromResult<GenerateResponsePayload?>(response);
        }

        public void Dispose()
        {
        }
    }
}
