using Legaly.Web.Models.Entities;
using Legaly.Web.Models.ViewModels;

namespace Legaly.Web.Services;

public interface IDocumentService
{
    Task<Document> CreateAsync(DocumentFormViewModel vm);
    Task<Document?> GetByIdAsync(int id);
    string AutoFill(string templateContent, DocumentFormViewModel form, string nomorSurat);
    Task<string> GenerateNomorSuratAsync(DateOnly tanggal);
}
