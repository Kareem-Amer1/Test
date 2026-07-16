namespace HireExam.Infrastructure.Seed;

public sealed class SeedOptions
{
    public const string Section = "Seed";
    public string SuperAdminEmail { get; set; } = "admin@hireexam.local";
    public string SuperAdminPassword { get; set; } = "Admin@12345";
    public string SuperAdminFullName { get; set; } = "Super Admin";
}
