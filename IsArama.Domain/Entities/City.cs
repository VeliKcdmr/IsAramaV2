namespace IsArama.Domain.Entities;

public class City
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<District> Districts { get; set; } = new List<District>();
    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();
}
