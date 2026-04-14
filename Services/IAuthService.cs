using ServiceApi.API.DTOs;

namespace ServiceApi.API.Services;

public enum RegisterFailure
{
    None,
    UsernameTaken,
    EmailTaken
}

public record RegisterResult(bool Success, RegisterFailure Failure, LoginResponse? Response);

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
}
