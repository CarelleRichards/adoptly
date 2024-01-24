namespace Adoptly.Web.Models;

public class SmtpConfig
{
    public string AdoptlyEmailAddress { get; set; }
    public string EmailServiceApiRoute { get; set; }
    public string ServiceApiRoute { get; set; }
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
    public string SmtpUserName { get; set; }
    public string SmtpPass { get; set; }
}