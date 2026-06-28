using System.Globalization;
using Legaly.Web.Data;
using Legaly.Web.Models.Entities;
using Legaly.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Legaly.Web.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;

    public DocumentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateAsync(DocumentFormViewModel vm)
    {
        var template = await _context.Templates.OrderBy(t => t.Id).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("Tidak ada template tersedia.");

        var nomorSurat = await GenerateNomorSuratAsync(vm.TanggalPerjanjian);
        var generatedContent = AutoFill(template.Content, vm, nomorSurat);

        var document = new Document
        {
            TemplateId        = template.Id,
            NomorSurat        = nomorSurat,
            NamaPenjual       = vm.NamaPenjual,
            NikPenjual        = vm.NikPenjual,
            AlamatPenjual     = vm.AlamatPenjual,
            NamaPembeli       = vm.NamaPembeli,
            NikPembeli        = vm.NikPembeli,
            AlamatPembeli     = vm.AlamatPembeli,
            MerkMotor         = vm.MerkMotor,
            TipeMotor         = vm.TipeMotor,
            TahunMotor        = vm.TahunMotor,
            WarnaMotor        = vm.WarnaMotor,
            NomorPolisi       = vm.NomorPolisi,
            NomorBPKB         = vm.NomorBPKB,
            NomorSTNK         = vm.NomorSTNK,
            Harga             = vm.Harga,
            TanggalPerjanjian = vm.TanggalPerjanjian,
            GeneratedContent  = generatedContent,
            CreatedAt         = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    public async Task<Document?> GetByIdAsync(int id)
        => await _context.Documents.Include(d => d.Template).FirstOrDefaultAsync(d => d.Id == id);

    public string AutoFill(string templateContent, DocumentFormViewModel form, string nomorSurat)
    {
        var culture = new CultureInfo("id-ID");
        return templateContent
            .Replace("{{NamaPenjual}}",       form.NamaPenjual)
            .Replace("{{NikPenjual}}",        form.NikPenjual)
            .Replace("{{AlamatPenjual}}",     form.AlamatPenjual)
            .Replace("{{NamaPembeli}}",       form.NamaPembeli)
            .Replace("{{NikPembeli}}",        form.NikPembeli)
            .Replace("{{AlamatPembeli}}",     form.AlamatPembeli)
            .Replace("{{MerkMotor}}",         form.MerkMotor)
            .Replace("{{TipeMotor}}",         form.TipeMotor)
            .Replace("{{TahunMotor}}",        form.TahunMotor.ToString())
            .Replace("{{WarnaMotor}}",        form.WarnaMotor)
            .Replace("{{NomorPolisi}}",       form.NomorPolisi)
            .Replace("{{NomorBPKB}}",         form.NomorBPKB)
            .Replace("{{NomorSTNK}}",         form.NomorSTNK)
            .Replace("{{Harga}}",             form.Harga.ToString("C0", culture))
            .Replace("{{TanggalPerjanjian}}", form.TanggalPerjanjian.ToString("dd MMMM yyyy", culture))
            .Replace("{{NomorSurat}}",        nomorSurat);
    }

    public async Task<string> GenerateNomorSuratAsync(DateOnly tanggal)
    {
        var count = await _context.Documents
            .Where(d => d.TanggalPerjanjian.Month == tanggal.Month
                     && d.TanggalPerjanjian.Year  == tanggal.Year)
            .CountAsync();

        return $"{(count + 1):D3}/LEGALY/JBM/{tanggal.Month:D2}/{tanggal.Year}";
    }
}
