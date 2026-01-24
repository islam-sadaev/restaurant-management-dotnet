namespace Restaurant.Configuration.MailService
{
    public interface IEmailSender
    {
        void SendEmailAsync(string ontvanger,string onderwerp,string body);
    }
}
