using System.Text;

using Microsoft.AspNetCore.Mvc;

using Razor.Templating.Core;
using QuestPDF.Infrastructure;

using HtmlToPdf.Models;
using HtmlToPdf.Factory;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorTemplating();
builder.Services.AddSingleton<InvoiceFactory>();
builder.Services.AddSingleton<PdfGenerateService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet(
    "invoice-report-as-html",
    async (
        [FromServices] InvoiceFactory invoiceFactory,
        [FromServices] IRazorTemplateEngine razorTemplateEngine,
        [FromServices] PdfGenerateService pdfGenerateService) =>
    {
        Invoice invoice = invoiceFactory.Create();
        string html = await razorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoice);

        return Results.Content(html, "text/html", Encoding.UTF8);
    });

app.MapGet(
    "invoice-report-as-pdf",
    async (
        [FromServices] InvoiceFactory invoiceFactory,
        [FromServices] IRazorTemplateEngine razorTemplateEngine,
        [FromServices] PdfGenerateService pdfGenerateService) =>
    {
        Invoice invoice = invoiceFactory.Create();

        string html = await razorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoice);
        byte[] pdf = pdfGenerateService.GeneratePdf(html);

        return Results.File(pdf, "application/pdf", $"invoice_{invoice.Number}_{invoice.IssuedDate}.pdf");
    });

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();