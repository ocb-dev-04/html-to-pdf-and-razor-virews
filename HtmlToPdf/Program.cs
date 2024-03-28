using Microsoft.AspNetCore.Mvc;

using Razor.Templating.Core;
using QuestPDF.Infrastructure;
using HtmlToPdf.Models;
using HtmlToPdf.Factory;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorTemplating();
builder.Services.AddSingleton<InvoiceFactory>();
builder.Services.AddSingleton<PdfGenerateService>();
builder.Services.AddSingleton<IRazorEngineService>(provider =>
{
    var config = new TemplateServiceConfiguration();
    return RazorEngineService.Create(config);
});

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