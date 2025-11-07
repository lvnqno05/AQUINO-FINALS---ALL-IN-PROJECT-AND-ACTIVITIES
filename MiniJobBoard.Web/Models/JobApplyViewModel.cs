using System.ComponentModel.DataAnnotations;

namespace MiniJobBoard.Web.Models;

public class JobApplyViewModel
{
    [Required]
    public Guid JobId { get; set; }

    [StringLength(4000)]
    public string? CoverLetter { get; set; }
}
