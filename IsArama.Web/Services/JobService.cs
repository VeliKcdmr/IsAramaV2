using System.Text.Json;
using IsArama.Web.Models;

namespace IsArama.Web.Services;

public class JobService
{
    private readonly HttpClient _apiClient;
    private readonly IHttpClientFactory _factory;

    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public JobService(IHttpClientFactory factory)
    {
        _factory = factory;
        _apiClient = factory.CreateClient("IsAramaApi");
    }

    public async Task<JobsApiResponse> GetJobsAsync(
        int page = 1, string? keyword = null,
        int? cityId = null, int? workModelId = null, int? workTypeId = null, int pageSize = 20, int? sectorId = null, int? sourceId = null)
    {
        var query = $"jobs?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(keyword)) query += $"&keyword={Uri.EscapeDataString(keyword)}";
        if (cityId.HasValue) query += $"&cityId={cityId}";
        if (workModelId.HasValue) query += $"&workModelId={workModelId}";
        if (workTypeId.HasValue) query += $"&workTypeId={workTypeId}";
        if (sectorId.HasValue) query += $"&sectorId={sectorId}";
        if (sourceId.HasValue) query += $"&sourceId={sourceId}";

        var response = await _apiClient.GetAsync(query);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JobsApiResponse>(content, _options)!;
    }

    public async Task<LookupData> GetLookupAsync()
    {
        try
        {
            var response = await _apiClient.GetAsync("lookup");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<LookupData>(content, _options) ?? new();
        }
        catch { return new(); }
    }

    public async Task<JobDetailViewModel?> GetJobDetailAsync(string slug)
    {
        var response = await _apiClient.GetAsync($"jobs/{slug}");
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JobDetailViewModel>(content, _options);
    }

    public async Task ScrapeDetailAsync(string url)
    {
        try
        {
            var scraper = _factory.CreateClient("Scraper");
            await scraper.PostAsync(
                $"scrape/detail?url={Uri.EscapeDataString(url)}", null);
        }
        catch
        {
            // Scrape başarısız olsa da sayfayı kırmayalım
        }
    }
}
