using CoffSip.Application.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoffSip.Infrastructure.Identity
{
    // Manage registration and credential checks with ASP.NET Core Identity.
    public sealed class AuthenticationService(
        UserManager<ApplicationUser> userManager)
        : IAuthenticationService
    {
        public async Task<RegisterUserResult> RegisterAsync(
            RegisterUserCommand command)
        {
            var email = command.Email.Trim();

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(
                user,
                command.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(error => error.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error => error.Description)
                            .ToArray());

                return new RegisterUserResult(null, errors);
            }

            return new RegisterUserResult(
                user.Id,
                new Dictionary<string, string[]>());
        }

        public async Task<AuthenticatedUser?> AuthenticateAsync(
            LoginUserCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Email) ||
                string.IsNullOrWhiteSpace(command.Password))
            {
                return null;
            }

            var email = command.Email.Trim();
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return null;
            }

            if (await userManager.IsLockedOutAsync(user))
            {
                return null;
            }

            var validPassword = await userManager.CheckPasswordAsync(
                user,
                command.Password);

            if (!validPassword)
            {
                await userManager.AccessFailedAsync(user);
                return null;
            }

            if (user.TwoFactorEnabled)
            {
                return null;
            }

            var resetResult =
                await userManager.ResetAccessFailedCountAsync(user);

            if (!resetResult.Succeeded)
            {
                return null;
            }

            return new AuthenticatedUser(
                user.Id,
                user.Email ?? email);
        }
    }

}
