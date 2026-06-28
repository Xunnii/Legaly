using System.ComponentModel.DataAnnotations;

namespace Legaly.Web.Models.ViewModels;

public class TemplateFormViewModel
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;
}
