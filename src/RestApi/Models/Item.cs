namespace RestApi.Models;

public class Item
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ItemRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
