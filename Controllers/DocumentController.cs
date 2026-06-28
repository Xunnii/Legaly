using Legaly.Web.Models.ViewModels;
using Legaly.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Legaly.Web.Controllers;

[Route("document")]
public class DocumentController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        IDocumentService documentService,
        IPdfService pdfService,
        ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _pdfService = pdfService;
        _logger = logger;
    }

    // GET /document/create
    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new DocumentFormViewModel());
    }

    // POST /document/create
    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DocumentFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var document = await _documentService.CreateAsync(model);
            return RedirectToAction(nameof(Preview), new { id = document.Id });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("template", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(ex, "Tidak ada template tersedia saat membuat dokumen.");
            return View("Error", new Legaly.Web.Models.ErrorViewModel
            {
                Message = "Template tidak tersedia. Hubungi administrator."
            });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DbUpdateException saat menyimpan dokumen (kemungkinan NomorSurat duplikat).");
            return View("Error", new Legaly.Web.Models.ErrorViewModel
            {
                Message = "Terjadi kesalahan saat menyimpan dokumen. Silakan coba lagi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tidak terduga saat membuat dokumen.");
            return View("Error", new Legaly.Web.Models.ErrorViewModel
            {
                Message = "Terjadi kesalahan. Silakan coba lagi."
            });
        }
    }

    // GET /document/preview/{id}
    [HttpGet("preview/{id}")]
    public async Task<IActionResult> Preview(int id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document == null)
            return NotFound();

        var vm = new PreviewViewModel
        {
            Id               = document.Id,
            NomorSurat       = document.NomorSurat,
            GeneratedContent = document.GeneratedContent,
            NomorPolisi      = document.NomorPolisi
        };

        return View(vm);
    }

    // GET /document/download/{id}
    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document == null)
            return NotFound();

        try
        {
            var pdfBytes = _pdfService.GeneratePdf(document);
            var fileName = _pdfService.BuildFileName(document.NomorPolisi, document.Id);

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saat menghasilkan PDF untuk dokumen {Id}.", id);
            return View("Error", new Legaly.Web.Models.ErrorViewModel
            {
                Message = "Terjadi kesalahan saat menghasilkan PDF. Silakan coba lagi."
            });
        }
    }
}
