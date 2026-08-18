namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Patient? Patient { get; set; }
}
