using System;
using System.Threading;
using System.Threading.Tasks;

using AIMD.ReportApp.Models;

namespace AIMD.ReportApp.Services;

public interface IReportService : IDisposable
{
    Task<GenerateResponsePayload?> GenerateAsync(
        GenerateRequestPayload payload,
        CancellationToken cancellationToken = default
    );
}
