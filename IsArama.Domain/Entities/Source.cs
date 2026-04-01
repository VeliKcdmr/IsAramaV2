namespace IsArama.Domain.Entities;

public class Source
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string FaviconUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;

    public ICollection<JobListing> JobListings { get; set; } = [];
}
