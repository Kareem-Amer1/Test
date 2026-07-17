namespace HireExam.Contracts.Users;

public sealed record HrUserResponse(
    string Id,
    string Email,
    string FullName,
    bool IsActive,
    DateTime CreatedAt);

public sealed record CreateHrUserRequest(string Email, string Password, string FullName);
