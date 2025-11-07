using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MiniJobBoard.Infrastructure.Data;
using MiniJobBoard.Infrastructure.Entities;

namespace MiniJobBoard.Web.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<RegisterModel> _logger;
    private readonly AppDbContext _db;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RegisterModel> logger,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _logger = logger;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = default!;

        [Required]
        public string Role { get; set; } = "Worker"; // Employer or Worker
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, Input.Password);
            if (result.Succeeded)
            {
                // Ensure roles exist
                foreach (var roleName in new[] { "Employer", "Worker" })
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                        await _roleManager.CreateAsync(new IdentityRole(roleName));
                }

                var selectedRole = (Input.Role == "Employer") ? "Employer" : "Worker";
                await _userManager.AddToRoleAsync(user, selectedRole);

                if (selectedRole == "Employer")
                {
                    // Create EmployerProfile for this user
                    _db.EmployerProfiles.Add(new EmployerProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        CompanyName = "",
                        Description = null,
                        Website = null
                    });
                    await _db.SaveChangesAsync();
                }

                _logger.LogInformation("User created a new account with password.");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(ReturnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
