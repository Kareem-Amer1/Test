namespace VoiceFlowStudio.Contracts.Auth;

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);

public sealed record AuthTokensResponse(string AccessToken, string RefreshToken, long ExpiresIn);

public sealed record UserResponse(string Id, string Email, string Role);
