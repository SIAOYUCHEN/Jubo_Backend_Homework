namespace Application.Orders.Dtos;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
