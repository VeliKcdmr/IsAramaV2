namespace IsArama.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string ProfileUrl { get; set; } = string.Empty;
    public string Followers { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;

    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();
}
