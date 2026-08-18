using Domain.Entities;

namespace Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid[] PatientIds =
    {
        Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Guid.Parse("55555555-5555-5555-5555-555555555555"),
    };

    public static readonly Guid DemoUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    // BCrypt.Net.BCrypt.HashPassword("demo")
    public const string DemoUserPasswordHash = "$2a$11$lKXu3UJut3gYYXQB3wuoWO4eZwxIHEfJahCOUXS2cIDd6eYk.xxVi";

    public static Patient[] Patients() => new[]
    {
        new Patient { Id = PatientIds[0], Name = "王小明" },
        new Patient { Id = PatientIds[1], Name = "陳美麗" },
        new Patient { Id = PatientIds[2], Name = "林大同" },
        new Patient { Id = PatientIds[3], Name = "張淑芬" },
        new Patient { Id = PatientIds[4], Name = "李國強" },
    };

    public static User[] Users() => new[]
    {
        new User { Id = DemoUserId, Username = "demo", PasswordHash = DemoUserPasswordHash },
    };
}
