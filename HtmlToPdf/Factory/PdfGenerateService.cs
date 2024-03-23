using QuestPDF.Fluent;
using HTMLQuestPDF.Extensions;

using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HtmlToPdf.Factory;

internal sealed class PdfGenerateService
{
    public byte[] GeneratePdf(string htmlTemplate)
        => Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);

                page.Content()
                    .Column(col => col.Item()
                    .HTML(handle => handle.SetHtml(htmlTemplate)));

                page.Footer()
                    .AlignCenter()
                    .Text(t =>
                    {
                        t.Span("Default Footer");
                        t.EmptyLine();
                        t.CurrentPageNumber();
                    });

            });
        }).GeneratePdf();
}

