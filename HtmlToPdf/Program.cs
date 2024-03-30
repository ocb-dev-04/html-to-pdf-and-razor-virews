using System.Text;

using Microsoft.AspNetCore.Mvc;

using Razor.Templating.Core;
using QuestPDF.Infrastructure;

using HtmlToPdf.Models;
using HtmlToPdf.Factory;
using FluentEmail.Core;
using HtmlToPdf.Settings;
using FluentEmail.Core.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddRazorTemplating();
services.AddSingleton<InvoiceFactory>();
services.AddSingleton<PdfGenerateService>();

// fluent email start area
services.AddOptions<MailSettings>()
    .BindConfiguration(nameof(MailSettings))
    .ValidateDataAnnotations();

MailSettings? settings = configuration.GetSection(nameof(MailSettings)).Get<MailSettings>();
if (settings is null)
    throw new ArgumentException($"{nameof(MailSettings)} can't be null");

services.AddFluentEmail(settings.DefaultFromEmail)
    .AddRazorRenderer()
    .AddSmtpSender(settings.Host, settings.Port, settings.UserName, settings.Password);
// fluent email end area

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
        [FromServices] PdfGenerateService pdfGenerateService,
        [FromServices] IFluentEmail fluentEmail,
        IOptions<MailSettings> mailSettings,
        CancellationToken cancellationToken) =>
    {
        Invoice invoice = invoiceFactory.Create();

        string html = await razorTemplateEngine.RenderAsync("Views/InvoiceReport.cshtml", invoice);
        byte[] pdf = pdfGenerateService.GeneratePdf(html);

        var list = new List<Attachment>(); ;
        Attachment attach = new()
        {
            ContentType = "application/pdf",
            Data = new MemoryStream(pdf),
            Filename = $"invoice_{invoice.Number}_{invoice.IssuedDate}.pdf"
        };
        list.Add(attach);

        EmailMetadata emailMetadata = new("devflutternet@gmail.com", "Cliente X", string.Empty, string.Empty, "Welcome and confirmation code", html, invoice, list.ToArray());

        MailSettings mailSettingsValue = mailSettings.Value;
        IFluentEmail fluent = fluentEmail.SetFrom(mailSettingsValue.DefaultFromEmail, mailSettingsValue.DefaultFromName)
            .Body(emailMetadata.Body);

        if (string.IsNullOrEmpty(emailMetadata.ToName))
            fluent.To(emailMetadata.To);
        else
            fluent.To(emailMetadata.To, emailMetadata.ToName);

        if (!string.IsNullOrEmpty(emailMetadata.Subject))
            fluent.Subject(emailMetadata.Subject);

        if (!string.IsNullOrEmpty(emailMetadata.CC))
            fluent.CC(emailMetadata.CC);

        if (!string.IsNullOrEmpty(emailMetadata.Template) && emailMetadata.TemplateModel is not null)
            fluent.UsingTemplate(emailMetadata.Template, emailMetadata.TemplateModel);

        if (emailMetadata.AttachmentList.Any())
            foreach (var item in emailMetadata.AttachmentList)
                fluent.Attach(item);

        await fluent.SendAsync(cancellationToken);

        return Results.File(pdf, "application/pdf", $"invoice_{invoice.Number}_{invoice.IssuedDate}.pdf");
    });

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public sealed record EmailMetadata(
    string To,
    string ToName,
    string Body,
    string CC,
    string Subject,
    string Template,
    object? TemplateModel,
    Attachment[] AttachmentList);
