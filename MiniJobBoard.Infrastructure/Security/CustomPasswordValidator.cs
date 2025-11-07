using Microsoft.AspNetCore.Identity;
using MiniJobBoard.Infrastructure.Entities;

namespace MiniJobBoard.Infrastructure.Security;

public class CustomPasswordValidator : IPasswordValidator<ApplicationUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        if (string.IsNullOrEmpty(password))
            return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Password is required." }));

        int uppercase = 0, digits = 0, symbols = 0;
        foreach (var c in password)
        {
            if (char.IsUpper(c)) uppercase++;
            else if (char.IsDigit(c)) digits++;
            else if (!char.IsLetterOrDigit(c)) symbols++;
        }

        var errors = new List<IdentityError>();
        if (uppercase < 2) errors.Add(new IdentityError { Description = "Password must contain at least 2 uppercase letters." });
        if (digits < 3) errors.Add(new IdentityError { Description = "Password must contain at least 3 digits." });
        if (symbols < 3) errors.Add(new IdentityError { Description = "Password must contain at least 3 symbols." });

        return Task.FromResult(errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
    }
}
