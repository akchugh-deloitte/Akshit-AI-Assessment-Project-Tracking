using BCrypt.Net;
using ServiceApi.API.DTOs;
using ServiceApi.API.Models;
using ServiceApi.API.Repositories;

namespace ServiceApi.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _users.GetByUsernameAsync(request.Username, ct);
        if (user == null) return null;

        var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!ok) return null;

        var token = _tokens.GenerateToken(user);
        return new LoginResponse(token, user.Username, user.Role);
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (await _users.ExistsByUsernameAsync(request.Username, ct))
            return new RegisterResult(false, RegisterFailure.UsernameTaken, null);

        if (await _users.ExistsByEmailAsync(request.Email, ct))
            return new RegisterResult(false, RegisterFailure.EmailTaken, null);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role == "Admin" ? "Admin" : "Member",
            CreatedOn = DateTime.UtcNow
        };

        await _users.AddAsync(user, ct);
        await _users.SaveChangesAsync(ct);

        var token = _tokens.GenerateToken(user);
        var response = new LoginResponse(token, user.Username, user.Role);
        return new RegisterResult(true, RegisterFailure.None, response);
    }
}
