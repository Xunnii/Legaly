namespace Legaly.Web.Models.Entities;

public class Document
{
    public int Id { get; set; }
    public int TemplateId { get; set; }
    public Template Template { get; set; } = null!;
    public string NomorSurat { get; set; } = string.Empty;       // UNIQUE
    public string NamaPenjual { get; set; } = string.Empty;
    public string NikPenjual { get; set; } = string.Empty;
    public string AlamatPenjual { get; set; } = string.Empty;
    public string NamaPembeli { get; set; } = string.Empty;
    public string NikPembeli { get; set; } = string.Empty;
    public string AlamatPembeli { get; set; } = string.Empty;
    public string MerkMotor { get; set; } = string.Empty;
    public string TipeMotor { get; set; } = string.Empty;
    public int TahunMotor { get; set; }
    public string WarnaMotor { get; set; } = string.Empty;
    public string NomorPolisi { get; set; } = string.Empty;
    public string NomorBPKB { get; set; } = string.Empty;
    public string NomorSTNK { get; set; } = string.Empty;
    public decimal Harga { get; set; }                           // DECIMAL(18,2)
    public DateOnly TanggalPerjanjian { get; set; }
    public string GeneratedContent { get; set; } = string.Empty; // NVARCHAR(MAX)
    public DateTime CreatedAt { get; set; }
}
