namespace Monetra.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

