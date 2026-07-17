namespace HireExam.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, long ExpiresIn);

public sealed record UserResponse(string Id, string Email, string FullName, string Role, DateTime CreatedAt);

public sealed record UpdateProfileRequest(string FullName);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
