namespace IsArama.Web.Models;

public class JobListItem
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string PublishedAt { get; set; } = "";
    public string LogoUrl { get; set; } = "";
    public string Company { get; set; } = "";
    public string City { get; set; } = "";
    public string WorkModel { get; set; } = "";
    public string WorkType { get; set; } = "";
    public int? SourceId { get; set; }
    public string SourceName { get; set; } = "kariyer.net";
    [System.Text.Json.Serialization.JsonPropertyName("source_logo_url")]
    public string SourceLogoUrl { get; set; } = "https://www.kariyer.net/favicon.ico";
}

public class JobsApiResponse
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<JobListItem> Jobs { get; set; } = [];
}

public class LookupData
{
    public List<NamedItem> Cities { get; set; } = [];
    [System.Text.Json.Serialization.JsonPropertyName("work_models")]
    public List<NamedItem> WorkModels { get; set; } = [];
    [System.Text.Json.Serialization.JsonPropertyName("work_types")]
    public List<NamedItem> WorkTypes { get; set; } = [];
    public List<NamedItem> Sectors { get; set; } = [];
    public List<SourceItem> Sources { get; set; } = [];
}

public class SourceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    [System.Text.Json.Serialization.JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = "";
    [System.Text.Json.Serialization.JsonPropertyName("favicon_url")]
    public string FaviconUrl { get; set; } = "";
}

public class JobListViewModel
{
    public JobsApiResponse Response { get; set; } = new();
    public LookupData Lookup { get; set; } = new();
    public string? Keyword { get; set; }
    public int? CityId { get; set; }
    public int? WorkModelId { get; set; }
    public int? WorkTypeId { get; set; }
    public int? SectorId { get; set; }
    public int? SourceId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class CompanyVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string LogoUrl { get; set; } = "";
    public string Followers { get; set; } = "";
    public string ProfileUrl { get; set; } = "";
}

public class NamedItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class JobDetailInfo
{
    public int Id { get; set; }
    public string ApplyCount { get; set; } = "";
    public string DescriptionHtml { get; set; } = "";
    public string PublishDate { get; set; } = "";
    public string ClosingDate { get; set; } = "";
    public string HeaderImage { get; set; } = "";
    public NamedItem? Sector { get; set; }
    public NamedItem? ExperienceLevel { get; set; }
    public NamedItem? PositionLevel { get; set; }
}

public class JobDetailViewModel
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string PublishedAt { get; set; } = "";
    public string LogoUrl { get; set; } = "";
    public CompanyVm? Company { get; set; }
    public NamedItem? City { get; set; }
    public NamedItem? District { get; set; }
    public NamedItem? WorkModel { get; set; }
    public NamedItem? WorkType { get; set; }
    public JobDetailInfo? Detail { get; set; }
    public SourceItem? Source { get; set; }
}
