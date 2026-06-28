using Legaly.Web.Models.ViewModels;
using Legaly.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Legaly.Web.Controllers;

[Authorize]
[Route("template")]
public class TemplateController : Controller
{
    private readonly ITemplateService _templateService;

    public TemplateController(ITemplateService templateService)
    {
        _templateService = templateService;
    }

    // GET /template
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var templates = await _templateService.GetAllAsync();
        return View(templates);
    }

    // GET /template/create
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new TemplateFormViewModel());
    }

    // POST /template/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _templateService.CreateAsync(model);
        TempData["Success"] = "Template berhasil dibuat.";
        return RedirectToAction(nameof(Index));
    }

    // GET /template/edit/{id}
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null)
            return NotFound();

        var model = new TemplateFormViewModel
        {
            Name    = template.Name,
            Content = template.Content
        };
        return View(model);
    }

    // POST /template/edit/{id}
    [HttpPost("edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var template = await _templateService.GetByIdAsync(id);
        if (template == null)
            return NotFound();

        await _templateService.UpdateAsync(id, model);
        TempData["Success"] = "Template berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    // POST /template/delete/{id}
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var template = await _templateService.GetByIdAsync(id);
        if (template == null)
            return NotFound();

        var deleted = await _templateService.DeleteAsync(id);
        if (deleted)
            TempData["Success"] = "Template berhasil dihapus.";
        else
            TempData["Error"] = "Template tidak dapat dihapus karena masih direferensikan oleh dokumen.";

        return RedirectToAction(nameof(Index));
    }
}
