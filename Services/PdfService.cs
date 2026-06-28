using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Legaly.Web.Services;

public class PdfService : IPdfService
{
    public byte[] GeneratePdf(Legaly.Web.Models.Entities.Document document)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Item().Text(document.NomorSurat).Bold().AlignCenter();
                    col.Item().PaddingTop(10).Column(innerCol =>
                    {
                        foreach (var line in document.GeneratedContent.Split('\n'))
                        {
                            innerCol.Item().Text(line.TrimEnd('\r'));
                        }
                    });
                });
            });
        }).GeneratePdf();
    }

    public string BuildFileName(string nomorPolisi, int id)
    {
        var sanitized = Regex.Replace(nomorPolisi.Replace(" ", "-"), @"[^a-zA-Z0-9\-]", "");
        return $"Surat-Perjanjian-Jual-Beli-{sanitized}-{id}.pdf";
    }
}
