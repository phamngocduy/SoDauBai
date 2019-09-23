using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Data.Entity;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Gmail.v1.Data;
using System.Web.Hosting;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao")]
    public class CauHinhController : Controller
    {
        public ActionResult Index()
        {
            using (var db = new SoDauBaiEntities())
            {
                return View(db.CauHinhs.ToList());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string passwd, int khoaSo, string noEmail)
        {
            using (var db = new SoDauBaiEntities())
            {
                var PassWD = db.CauHinhs.Find(CONFIG.ACDM511);
                PassWD.GiaTri = passwd;
                db.Entry(PassWD).State = EntityState.Modified;

                var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO);
                KhoaSo.GiaTri = khoaSo.ToString();
                db.Entry(KhoaSo).State = EntityState.Modified;

                var NoEmail = db.CauHinhs.Find(CONFIG.NO_EMAIL);
                NoEmail.GiaTri = (noEmail ?? "").Trim();
                db.Entry(NoEmail).State = EntityState.Modified;

                db.SaveChanges();
            }
            TempData["Message"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("Index");
        }

        public static void GuiEmail(string from, string to, string subject, string content)
        {
            var password = "AnthonyChauDuyMi";
            using (var db = new SoDauBaiEntities())
            {
                password = db.CauHinhs.Find(CONFIG.ACDM511).GiaTri;
            }
            var credentials = new NetworkCredential("acdm511@gmail.com",
                Encoding.ASCII.GetString(Convert.FromBase64String(password)));

            var mail = new MailMessage()
            {
                From = new MailAddress("acdm511@gmail.com", from),
                Subject = subject,
                Body = content + Environment.NewLine + "Sent from Sổ Đầu Bài. Please do not reply."
            };

            mail.To.Add(new MailAddress(to));

            var client = new SmtpClient()
            {
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Credentials = credentials
            };

            client.Send(mail);
        }

        static string[] Scopes = { GmailService.Scope.GmailSend };
        static string ApplicationName = "SoDauBai";

        public static void SendEmail(string from, string to, string subject, string content)
        {
            using (var db = new SoDauBaiEntities())
            {
                var noEmails = db.CauHinhs.Find(CONFIG.NO_EMAIL).Init().GiaTri ?? "";
                to = String.Join(",", (to ?? "").ToLower().Split(',').Except(noEmails.ToLower().Split(',')));
            }

            UserCredential credential;
            using (var stream =
                new FileStream(Path.Combine(HostingEnvironment.MapPath("~/bin"), "credentials.json"), FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = Path.Combine(HostingEnvironment.MapPath("~/App_Data"), "token.json");
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "acdm511@gmail.com",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            from = from.Trim(); to = to.Trim(); subject = subject.Trim(); content = content.Trim();
            var email = String.Format("From: \"{0}\" <acdm511@gmail.com>\r\nReply-To: {0}\r\nTo: {1}\r\nSubject: =?utf-8?B?{2}?=\r\nContent-Type: text/html; charset=utf-8\r\n\r\n{3}",
                from, to, subject.ToBase64(), String.Join("", content.Split('\n').Select(s => String.Format("<p>{0}</p>", s.Trim()))) + "Sent from Sổ Đầu Bài. Please do not reply.");
            var message = new Message();
            message.Raw = email.ToBase64().Replace("+", "-").Replace("/", "_").Replace("=", "");
            service.Users.Messages.Send(message, "me").Execute();
        }
    }
}