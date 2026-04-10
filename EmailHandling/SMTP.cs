using System;
using System.Net.Mail;
using System.Net;
using System.Web.Configuration;

namespace EmailHandling
{
    public static class SMTP
    {
        private static SmtpClient client = null;
        private static MailMessage mail = null;
        public static string OwnerName;
        public static string OwnerEmail;

        static SMTP()
        {
            OwnerName = WebConfigurationManager.AppSettings["SMTPOwnerName"];
            OwnerEmail = WebConfigurationManager.AppSettings["SMTPOwnerEmail"];

            client = new SmtpClient(WebConfigurationManager.AppSettings["SMTPHost"],
                                    int.Parse(WebConfigurationManager.AppSettings["SMTPPort"]))
            {
                Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["SMTPOwnerEmail"],
                                                    WebConfigurationManager.AppSettings["SMTPOwnerPassword"]),

                EnableSsl = bool.Parse(WebConfigurationManager.AppSettings["SMTPEnableSSL"]),
                Timeout = int.Parse(WebConfigurationManager.AppSettings["SMTPTimeOut"])
            };

            mail = new MailMessage
            {
                From = new MailAddress(OwnerEmail, OwnerName, System.Text.Encoding.UTF8),
                SubjectEncoding = System.Text.Encoding.UTF8,
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
        }

        public static void SendEmail(string toName, string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(
                WebConfigurationManager.AppSettings["SMTPHost"],
                int.Parse(WebConfigurationManager.AppSettings["SMTPPort"])))
            {
                client.Credentials = new NetworkCredential(
                    WebConfigurationManager.AppSettings["SMTPOwnerEmail"],
                    WebConfigurationManager.AppSettings["SMTPOwnerPassword"]);

                client.EnableSsl = bool.Parse(WebConfigurationManager.AppSettings["SMTPEnableSSL"]);
                client.Timeout = int.Parse(WebConfigurationManager.AppSettings["SMTPTimeOut"]);

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(OwnerEmail, OwnerName);
                    mail.To.Add(new MailAddress(toEmail, toName));
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    client.Send(mail);
                }
            }
        }
    }
}