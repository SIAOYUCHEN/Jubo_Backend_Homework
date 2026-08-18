using Domain.Entities;

namespace Infrastructure.Persistence;

public static class SeedData
{
    public static readonly Guid[] PatientIds =
    {
        Guid.Parse("b2cc2f38-22ec-4236-be7e-08445071a3d2"),
        Guid.Parse("9a36db19-860d-40cf-8277-d4c53ca99bd0"),
        Guid.Parse("6891fed6-bfb8-478e-8993-abfd66ed859e"),
        Guid.Parse("aff37c0c-9146-4329-92c8-f63202ccc1de"),
        Guid.Parse("d2836813-d635-412e-ade8-ef3958e3cd39"),
    };

    public static readonly Guid DemoUserId = Guid.Parse("1220680d-0d0b-423f-a4ff-4653a15b77f6");

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
