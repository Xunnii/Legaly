using Legaly.Web.Data;
using Legaly.Web.Models.Entities;
using Legaly.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Legaly.Web.Services;

public class TemplateService : ITemplateService
{
    private readonly ApplicationDbContext _context;

    public TemplateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Template>> GetAllAsync()
        => await _context.Templates.OrderBy(t => t.Id).ToListAsync();

    public async Task<Template?> GetByIdAsync(int id)
        => await _context.Templates.FindAsync(id);

    public async Task<Template> CreateAsync(TemplateFormViewModel vm)
    {
        var template = new Template
        {
            Name      = vm.Name,
            Content   = vm.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Templates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<Template> UpdateAsync(int id, TemplateFormViewModel vm)
    {
        var template = await _context.Templates.FindAsync(id)
            ?? throw new InvalidOperationException($"Template with id {id} not found.");
        template.Name      = vm.Name;
        template.Content   = vm.Content;
        template.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hasDocuments = await _context.Documents.AnyAsync(d => d.TemplateId == id);
        if (hasDocuments) return false;

        var template = await _context.Templates.FindAsync(id);
        if (template == null) return false;

        _context.Templates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }
}
