namespace IsArama.Domain.Entities;

public class WorkType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();
}
