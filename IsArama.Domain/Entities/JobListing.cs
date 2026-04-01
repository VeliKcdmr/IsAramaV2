namespace IsArama.Domain.Entities;

public class JobListing
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PublishedAt { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public int CityId { get; set; }
    public City City { get; set; } = null!;

    public int? DistrictId { get; set; }
    public District? District { get; set; }

    public int WorkModelId { get; set; }
    public WorkModel WorkModel { get; set; } = null!;

    public int WorkTypeId { get; set; }
    public WorkType WorkType { get; set; } = null!;

    public int? SourceId { get; set; }
    public Source? Source { get; set; }

    public JobDetail? Detail { get; set; }
}
