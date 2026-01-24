using System.Net.Mail;
using MailKit.Net.Smtp;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;
namespace Restaurant.Configuration.MailService
{
    public class EmailSender : IEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailSender(IConfiguration configuration)
        {
            _smtpServer = configuration.GetValue<string>("SmtpSettings:SmtpServer","");
            _smtpPort = configuration.GetValue<int>("SmtpSettings:SmtpPort",0);
            _smtpUsername = configuration.GetValue<string>("SmtpSettings:SmtpUsername","");
            _smtpPassword = configuration.GetValue<string>("SmtpSettings:SmtpPassword","");
        }


        public void SendEmailAsync(string ontvanger, string onderwerp, string body) {

           var verstuurder = "project.CarpMinds@outlook.com";
           var message = new MailMessage();
            message.From = new MailAddress(verstuurder);
            message.To.Add(new MailAddress(ontvanger));
            message.Subject = onderwerp;
            message.Body = body;
            using(var client = new SmtpClient())
            {
                client.Connect(_smtpServer, _smtpPort);
                client.Authenticate(_smtpUsername, _smtpPassword);

                try
                {
                    var result = client.Send((MimeKit.MimeMessage)message);
                    client.Disconnect(true);
                }
                catch (Exception e) { }

            }           
           
        }
    }
}
