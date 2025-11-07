namespace MiniJobBoard.Infrastructure.Entities;

public class EmployerProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = default!; // FK -> ApplicationUser
    public string CompanyName { get; set; } = default!;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = default!;
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}