using CoffSip.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using CoffSip.Api.Authentication;

namespace CoffSip.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(
    IAuthenticationService authenticationService,
    JwtTokenService jwtTokenService)
    : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request)
        {
            var result = await authenticationService.RegisterAsync(
                new RegisterUserCommand(
                    request.Email,
                    request.Password));

            if (result.UserId is null)
            {
                return ValidationProblem(
                    new ValidationProblemDetails(
                        result.Errors.ToDictionary(
                            item => item.Key,
                            item => item.Value))
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Registration failed."
                    });
            }

            return StatusCode(
                StatusCodes.Status201Created,
                new { userId = result.UserId });
        }

        // Verify credentials and issue a short-lived JWT for successful logins.
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            var user = await authenticationService.AuthenticateAsync(
                new LoginUserCommand(
                    request.Email,
                    request.Password));

            if (user is null)
            {
                return Unauthorized(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Invalid email or password."
                });
            }

            return Ok(jwtTokenService.CreateToken(user));
        }
    }

    public sealed record RegisterRequest(
        [Required, EmailAddress, StringLength(254)]
    string Email,

        [Required, MinLength(12)]
    string Password);

    public sealed record LoginRequest(
        [Required, EmailAddress]
    string Email,

        [Required]
    string Password);
}
