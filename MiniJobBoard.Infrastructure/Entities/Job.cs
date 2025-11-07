namespace MiniJobBoard.Infrastructure.Entities;

public class Job
{
    public Guid Id { get; set; }
    public Guid EmployerId { get; set; } // FK -> EmployerProfile
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Location { get; set; }
    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EmployerProfile Employer { get; set; } = default!;
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
}
