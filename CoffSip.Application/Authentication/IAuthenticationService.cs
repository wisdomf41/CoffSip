using System;
using System.Collections.Generic;
using System.Text;

namespace CoffSip.Application.Authentication
{
    // Defines the contracts for user registration and credential verification.
    public sealed record RegisterUserCommand(
        string Email,
        string Password);

    public sealed record RegisterUserResult(
        Guid? UserId,
        IReadOnlyDictionary<string, string[]> Errors);

    public sealed record LoginUserCommand(
        string Email,
        string Password);

    public sealed record AuthenticatedUser(
        Guid UserId,
        string Email);

    public interface IAuthenticationService
    {
        Task<RegisterUserResult> RegisterAsync(
            RegisterUserCommand command);

        Task<AuthenticatedUser?> AuthenticateAsync(
            LoginUserCommand command);
    }

}
