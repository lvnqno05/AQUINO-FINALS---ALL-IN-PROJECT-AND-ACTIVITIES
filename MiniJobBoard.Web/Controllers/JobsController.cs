using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniJobBoard.Infrastructure.Data;
using MiniJobBoard.Infrastructure.Entities;
using MiniJobBoard.Web.Models;

namespace MiniJobBoard.Web.Controllers;

public class JobsController : Controller
{
    private readonly AppDbContext _db;
    private readonly ILogger<JobsController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public JobsController(AppDbContext db, ILogger<JobsController> logger, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _logger = logger;
        _userManager = userManager;
    }

    // GET: /Jobs
    public async Task<IActionResult> Index()
    {
        var jobs = await _db.Jobs
            .AsNoTracking()
            .Include(j => j.Employer)
            .Where(j => j.IsActive)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return View(jobs);
    }

    // GET: /Jobs/Manage (Employer's postings)
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Manage()
    {
        var userId = _userManager.GetUserId(User);
        var employer = await _db.EmployerProfiles.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == userId);
        if (employer == null)
        {
            return Forbid();
        }

        var jobs = await _db.Jobs
            .AsNoTracking()
            .Where(j => j.EmployerId == employer.Id)
            .Include(j => j.Applications)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return View(jobs);
    }

    // POST: /Jobs/ToggleStatus/{id}
    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        var employer = await _db.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
        if (employer == null) return Forbid();

        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == employer.Id);
        if (job == null) return NotFound();

        job.IsActive = !job.IsActive;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Manage));
    }

    // GET: /Jobs/Applicants/{id}
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Applicants(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        var employer = await _db.EmployerProfiles.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == userId);
        if (employer == null) return Forbid();

        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id && j.EmployerId == employer.Id);
        if (job == null) return NotFound();

        var apps = await _db.JobApplications
            .AsNoTracking()
            .Where(a => a.JobId == id)
            .Include(a => a.Applicant)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        ViewBag.JobTitle = job.Title;
        ViewBag.JobId = id;
        return View(apps);
    }

    // POST: /Jobs/SetApplicationStatus/{id}
    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetApplicationStatus(Guid id, string status)
    {
        var userId = _userManager.GetUserId(User);
        var app = await _db.JobApplications
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (app == null) return NotFound();

        var employer = await _db.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
        if (employer == null || app.Job.EmployerId != employer.Id) return Forbid();

        var normalized = (status ?? "").Trim().ToLowerInvariant();
        if (normalized == "accept" || normalized == "accepted") app.Status = "Accepted";
        else if (normalized == "reject" || normalized == "rejected") app.Status = "Rejected";
        else if (normalized == "pending") app.Status = "Pending";
        else return BadRequest("Invalid status.");

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Applicants), new { id = app.JobId });
    }

    // GET: /Jobs/MyApplications (Worke
    [Authorize(Roles = "Worker")]
    public async Task<IActionResult> MyApplications()
    {
        var userId = _userManager.GetUserId(User);
        var apps = await _db.JobApplications
            .AsNoTracking()
            .Where(a => a.ApplicantUserId == userId)
            .Include(a => a.Job)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(apps);
    }

    // POST: /Jobs/CancelApplication/{id}
    [Authorize(Roles = "Worker")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelApplication(Guid id)
    {
        var userId = _userManager.GetUserId(User);
        var app = await _db.JobApplications.FirstOrDefaultAsync(a => a.Id == id && a.ApplicantUserId == userId);
        if (app == null) return NotFound();

        if (app.Status == "Accepted" || app.Status == "Rejected")
        {
            // Don't allow cancel after decision
            return BadRequest("Application already processed.");
        }

        app.Status = "Cancelled";
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(MyApplications));
    }

    // GET: /Jobs/Details/{id}
    public async Task<IActionResult> Details(Guid id)
    {
        var job = await _db.Jobs.AsNoTracking()
            .Include(j => j.Employer)
            .FirstOrDefaultAsync(j => j.Id == id && j.IsActive);
        if (job == null) return NotFound();
        return View(job);
    }

    // GET: /Jobs/Create
    [Authorize(Roles = "Employer")]
    public IActionResult Create()
    {
        return View(new JobCreateViewModel());
    }

    // POST: /Jobs/Create
    [Authorize(Roles = "Employer")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JobCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var userId = _userManager.GetUserId(User);
        var employer = await _db.EmployerProfiles.FirstOrDefaultAsync(e => e.UserId == userId);
        if (employer == null)
        {
            ModelState.AddModelError(string.Empty, "You need an employer profile to post jobs.");
            return View(vm);
        }

        var job = new Job
        {
            Id = Guid.NewGuid(),
            EmployerId = employer.Id,
            Title = vm.Title,
            Description = vm.Description,
            Location = vm.Location,
            SalaryMin = vm.SalaryMin,
            SalaryMax = vm.SalaryMax,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Jobs/Apply/{id}
    [Authorize]
    public async Task<IActionResult> Apply(Guid id)
    {
        var job = await _db.Jobs.AsNoTracking()
            .Include(j => j.Employer)
            .FirstOrDefaultAsync(j => j.Id == id && j.IsActive);
        if (job == null) return NotFound();
        ViewBag.JobTitle = job.Title;
        ViewBag.Company = job.Employer?.CompanyName;
        return View(new JobApplyViewModel { JobId = id });
    }

    // POST: /Jobs/Apply/{id}
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(JobApplyViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == vm.JobId && j.IsActive);
        if (job == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;

        var exists = await _db.JobApplications.AnyAsync(a => a.JobId == vm.JobId && a.ApplicantUserId == userId);
        if (exists)
        {
            ModelState.AddModelError(string.Empty, "You have already applied to this job.");
            return View(vm);
        }

        var app = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobId = vm.JobId,
            ApplicantUserId = userId,
            CoverLetter = vm.CoverLetter,
            Status = "Pending",
            AppliedAt = DateTime.UtcNow
        };

        _db.JobApplications.Add(app);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
