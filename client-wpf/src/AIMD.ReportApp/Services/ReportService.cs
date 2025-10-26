using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using AIMD.ReportApp.Models;

namespace AIMD.ReportApp.Services;

public sealed class ReportService : IReportService
{
    private readonly HttpClient _httpClient;
    private bool _disposed;

    public ReportService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8000", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(5),
        };
    }

    public async Task<GenerateResponsePayload?> GenerateAsync(
        GenerateRequestPayload payload,
        CancellationToken cancellationToken = default
    )
    {
        using var response = await _httpClient.PostAsJsonAsync("/generate", payload, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException($"Engine returned {(int)response.StatusCode}: {detail}");
        }

        return await response.Content.ReadFromJsonAsync<GenerateResponsePayload>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }
}
