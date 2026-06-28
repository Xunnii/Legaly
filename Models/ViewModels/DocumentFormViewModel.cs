using System.ComponentModel.DataAnnotations;

namespace Legaly.Web.Models.ViewModels;

public class DocumentFormViewModel
{
    [Required]
    [StringLength(255)]
    public string NamaPenjual { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "NIK harus 16 digit angka")]
    public string NikPenjual { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string AlamatPenjual { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string NamaPembeli { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "NIK harus 16 digit angka")]
    public string NikPembeli { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string AlamatPembeli { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string MerkMotor { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TipeMotor { get; set; } = string.Empty;

    [Required]
    [Range(1900, 9999)]
    public int TahunMotor { get; set; }

    [Required]
    [StringLength(50)]
    public string WarnaMotor { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string NomorPolisi { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string NomorBPKB { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string NomorSTNK { get; set; } = string.Empty;

    [Required]
    [Range(1, 999999999999.99)]
    public decimal Harga { get; set; }

    [Required]
    public DateOnly TanggalPerjanjian { get; set; }

    [Required]
    [StringLength(255)]
    public string TempatPerjanjian { get; set; } = string.Empty;
}
