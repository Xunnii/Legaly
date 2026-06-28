namespace Legaly.Web.Models.Entities;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;    // NVARCHAR(255)
    public string Content { get; set; } = string.Empty; // NVARCHAR(MAX), berisi {{placeholder}}
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
