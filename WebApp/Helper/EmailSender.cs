using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using WebApp.Models;

namespace WebApp.Helper
{
    public class EmailSender
    {
        private string FromEmail { get; set; }
        private string FromName { get; set; }
        public string ToEmail { get; set; }
        public string ToCC { get; set; }
        public string MailSubject { get; set; }
        
        private string SMTPHost = "";
        private int SMTPPort = 25;
        private bool SMTPAuth = false;
        private bool SMTPSsl = false;

        public EmailSender()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.FromEmail = db.Settings.Where(x => x.vSettingID == "E0004312-1D18-1237-R868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // From Email
                this.FromName = db.Settings.Where(x => x.vSettingID == "E0005323-9E18-4B37-S868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // From Name
                this.SMTPHost = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // SMTP Host
                this.SMTPPort = Convert.ToInt16(db.Settings.Where(x => x.vSettingID == "E001034C-3J18-8K37-X868-DICEN89JOMEL").FirstOrDefault().vSettingOption); // SMTP Port
                this.SMTPAuth = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "E001234A-1L18-RRR7-Z868-DICEN89JOMEL").FirstOrDefault().vSettingOption); // SMTP Authentication
                this.SMTPSsl = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "E111234A-1T22-ERW7-x868-DICEN88JOMEL").FirstOrDefault().vSettingOption); // SMTP Enable SSL
            }
        }

        public string ComposeMessage(string htmlbody)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(string.Concat(this.FromName, " <", this.FromEmail, ">"));

            // if has Email
            if (!String.IsNullOrWhiteSpace(this.ToEmail))
            {
                foreach (var address in this.ToEmail.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                if (String.IsNullOrEmpty(address.Trim()))  continue;
                mail.To.Add(address.Trim());
                }
            }

            // if has CC
            if (!String.IsNullOrWhiteSpace(this.ToCC))
            {
                foreach (var address in this.ToCC.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (String.IsNullOrEmpty(address.Trim())) continue;
                    mail.CC.Add(address.Trim());
                }
            }

            mail.Subject = this.MailSubject;
            
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
          
            AlternateView plainView = AlternateView.CreateAlternateViewFromString("Please view as HTML-Mail.", System.Text.Encoding.UTF8, "text/plain");
            plainView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlbody, System.Text.Encoding.UTF8, "text/html");            
            htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;
            mail.AlternateViews.Add(plainView);
            mail.AlternateViews.Add(htmlView);

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = SMTPHost;
                smtp.Port = SMTPPort;
                smtp.UseDefaultCredentials = SMTPAuth;
                smtp.EnableSsl = SMTPSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                try
                {
                    smtp.Send(mail);

                    return "Ok";
                }
                catch (Exception ex)
                {                    
                    return ex.InnerException.Message;
                }
            }
        }
        
        // with file as attachment
        public void CustomMessage(string body, AlternateView plainView, AlternateView htmlView, Attachment attachment)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(string.Concat(this.FromName, " <", this.FromEmail, ">"));
            foreach (var address in this.ToEmail.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (String.IsNullOrEmpty(address.Trim()))
                    continue;
                mail.To.Add(address);
            }

            mail.Subject = this.MailSubject;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;

            if(attachment != null)
                mail.Attachments.Add(attachment);

            mail.AlternateViews.Add(plainView);
            mail.AlternateViews.Add(htmlView);
            this.SendSmtpNet(mail);
        }

        // with image as attachment
        public void CustomMessage2(string body, AlternateView plainView, AlternateView htmlView, IEnumerable<CustomVoucher> vouchers)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(string.Concat(this.FromName, " <", this.FromEmail, ">"));
            foreach (var address in this.ToEmail.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (String.IsNullOrEmpty(address.Trim()))
                    continue;
                mail.To.Add(address);
            }

            mail.Subject = this.MailSubject;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.SubjectEncoding = System.Text.Encoding.UTF8;

            foreach (var vc in vouchers)
            {
                Byte[] renderedBytes = Convert.FromBase64String(vc.VoucherBase64);
                System.IO.MemoryStream m = new System.IO.MemoryStream(renderedBytes);
                Attachment attachment = new Attachment(m, vc.VoucherCode + ".jpg");
                if (attachment != null)
                    mail.Attachments.Add(attachment);
            }

            mail.AlternateViews.Add(plainView);
            mail.AlternateViews.Add(htmlView);
            this.SendSmtpNet(mail);
        }

        private void SendSmtpNet(MailMessage mail)
        {
            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Host = SMTPHost;
                smtp.Port = SMTPPort;
                smtp.UseDefaultCredentials = SMTPAuth;
                smtp.EnableSsl = SMTPSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                try
                {
                    smtp.Send(mail);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void SendSmtpMailKit(MimeMessage mail)
        {

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                client.Connect(SMTPHost, SMTPPort, SMTPAuth);

                try
                {
                    client.Send(mail);
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                }
            }
        }

    }
}