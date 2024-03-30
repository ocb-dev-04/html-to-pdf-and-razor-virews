using System.ComponentModel.DataAnnotations;

namespace HtmlToPdf.Settings;

public class MailSettings
{

    [Required]
    public string DefaultFromEmail { get; set; }
    [Required]
    public string DefaultFromName { get; set; }
    [Required]
    public string Host { get; set; }
    [Required]
    public int Port { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Password { get; set; }
}

