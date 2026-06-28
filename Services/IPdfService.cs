using Legaly.Web.Models.Entities;

namespace Legaly.Web.Services;

public interface IPdfService
{
    byte[] GeneratePdf(Document document);
    string BuildFileName(string nomorPolisi, int id);
}
