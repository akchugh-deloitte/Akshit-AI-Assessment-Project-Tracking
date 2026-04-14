using Microsoft.AspNetCore.Mvc;
using ServiceApi.API.DTOs;
using ServiceApi.API.Services;

namespace ServiceApi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Login and receive a JWT token</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _auth.LoginAsync(request);
        if (response is null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(response);
    }

    /// <summary>Register a new user (Admin only in production; open for demo)</summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _auth.RegisterAsync(request);
        if (!result.Success)
        {
            return result.Failure switch
            {
                RegisterFailure.UsernameTaken => Conflict(new { message = "Username already taken" }),
                RegisterFailure.EmailTaken => Conflict(new { message = "Email already registered" }),
                _ => BadRequest()
            };
        }

        return CreatedAtAction(nameof(Login), result.Response!);
    }
}
