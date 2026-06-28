using Legaly.Web.Models.Entities;
using Legaly.Web.Models.ViewModels;

namespace Legaly.Web.Services;

public interface ITemplateService
{
    Task<List<Template>> GetAllAsync();
    Task<Template?> GetByIdAsync(int id);
    Task<Template> CreateAsync(TemplateFormViewModel vm);
    Task<Template> UpdateAsync(int id, TemplateFormViewModel vm);
    Task<bool> DeleteAsync(int id); // false if referenced by documents
}
