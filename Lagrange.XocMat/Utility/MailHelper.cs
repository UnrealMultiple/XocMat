using System.Net;
using System.Net.Mail;
using System.Text;

namespace Lagrange.XocMat.Utility;

public class MailHelper(string Host, string Password) : IDisposable
{
    public string Host { get; } = Host;

    public string Password { get; } = Password;

    private readonly SmtpClient Client = new(Host);

    private readonly MailMessage Mail = new();

    public static MailHelper Builder(string host, string password) => new(host, password);

    public MailHelper SetTile(string title)
    {
        Mail.Subject = title;
        Mail.SubjectEncoding = Encoding.UTF8;
        return this;
    }

    public MailHelper SetBody(string body)
    {
        Mail.Body = body;
        Mail.BodyEncoding = Encoding.UTF8;
        return this;
    }

    public MailHelper AddTarget(string target)
    {
        Mail.To.Add(target);
        return this;
    }

    public MailHelper AddAttachment(string path)
    {
        var attach = new Attachment(path);
        var disposition = attach.ContentDisposition!;
        disposition.CreationDate = File.GetCreationTime(path);
        disposition.ModificationDate = File.GetLastWriteTime(path);
        disposition.ReadDate = File.GetLastAccessTime(path);
        Mail.Attachments.Add(attach);
        return this;
    }

    public MailHelper SetSender(string sender)
    {
        Mail.From = new(sender);
        return this;
    }

    public MailHelper EnableHtmlBody(bool enable)
    {
        Mail.IsBodyHtml = enable;
        return this;
    }

    public MailHelper Send()
    {
        Client.DeliveryMethod = SmtpDeliveryMethod.Network;
        Mail.BodyEncoding = Encoding.UTF8;
        Client.UseDefaultCredentials = false;
        Client.Credentials = new NetworkCredential(Mail.From?.Address, Password);
        Client.Send(Mail);
        return this;
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}
