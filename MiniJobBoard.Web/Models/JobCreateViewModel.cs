using System.ComponentModel.DataAnnotations;

namespace MiniJobBoard.Web.Models;

public class JobCreateViewModel
{
    [Required, StringLength(200)]
    public string Title { get; set; } = default!;

    [Required]
    public string Description { get; set; } = default!;

    [StringLength(200)]
    public string? Location { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SalaryMin { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? SalaryMax { get; set; }
}
