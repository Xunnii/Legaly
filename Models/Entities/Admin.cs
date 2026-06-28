namespace Legaly.Web.Models.Entities;

public class Admin
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;        // NVARCHAR(255) UNIQUE
    public string PasswordHash { get; set; } = string.Empty; // BCrypt hash, cost 12
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
