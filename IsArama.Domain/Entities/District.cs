namespace IsArama.Domain.Entities;

public class District
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CityId { get; set; }

    public City City { get; set; } = null!;
    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();
}
