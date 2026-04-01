namespace IsArama.Domain.Entities;

public class JobDetail
{
    public int Id { get; set; }
    public int JobListingId { get; set; }

    public string ApplyCount { get; set; } = string.Empty;
    public string DescriptionHtml { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
    public string HeaderImage { get; set; } = string.Empty;
    public string PublishDate { get; set; } = string.Empty;
    public string ClosingDate { get; set; } = string.Empty;
    public DateTime ScrapedAt { get; set; } = DateTime.UtcNow;

    public int? ExperienceLevelId { get; set; }
    public ExperienceLevel? ExperienceLevel { get; set; }

    public int? SectorId { get; set; }
    public Sector? Sector { get; set; }

    public int? PositionLevelId { get; set; }
    public PositionLevel? PositionLevel { get; set; }

    public JobListing JobListing { get; set; } = null!;
}
