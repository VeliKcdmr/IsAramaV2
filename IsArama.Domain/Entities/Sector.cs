namespace IsArama.Domain.Entities;

public class Sector
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<JobDetail> JobDetails { get; set; } = new List<JobDetail>();
}
