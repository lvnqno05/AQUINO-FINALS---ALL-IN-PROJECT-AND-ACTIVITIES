namespace MiniJobBoard.Infrastructure.Entities;

public class JobApplication
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; } // FK -> Job
    public string ApplicantUserId { get; set; } = default!; // FK -> ApplicationUser
    public string? CoverLetter { get; set; }
    public string? ResumePath { get; set; }
    public string Status { get; set; } = "Pending"; // Pending/Accepted/Rejected
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = default!;
    public ApplicationUser Applicant { get; set; } = default!;
}
