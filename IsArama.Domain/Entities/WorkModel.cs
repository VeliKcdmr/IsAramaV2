namespace IsArama.Domain.Entities;

public class WorkModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();
}
