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
        public ActionResult Index(int khoaSo, string noEmail, string pCNTT)
        {
            using (var db = new SoDauBaiEntities())
            {
                var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO);
                KhoaSo.GiaTri = khoaSo.ToString();
                db.Entry(KhoaSo).State = EntityState.Modified;

                var NoEmail = db.CauHinhs.Find(CONFIG.NO_EMAIL);
                NoEmail.GiaTri = (noEmail ?? "").Trim();
                db.Entry(NoEmail).State = EntityState.Modified;

                var PCNTT = db.CauHinhs.Find(CONFIG.EP_CNTT);
                PCNTT.GiaTri = (pCNTT ?? "").Trim();
                db.Entry(PCNTT).State = EntityState.Modified;

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
                password = db.CauHinhs.Find(CONFIG.PASS_WD).GiaTri;
            }
            var credentials = new NetworkCredential(CONFIG.ACDM511,
                Encoding.ASCII.GetString(Convert.FromBase64String(password)));

            var mail = new MailMessage()
            {
                From = new MailAddress(CONFIG.ACDM511, from),
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

        public string TestEmail()
        {
            SendEmail(User.Identity.Name, CONFIG.ACDM511, "Test sending email", "<h1>Hello World</h1>");
            return "OK";
        }

        static string[] Scopes = { GmailService.Scope.GmailSend };
        static string ApplicationName = "SoDauBai";

        public static void SendEmail(string from, string to, string subject, string content, string cc = null)
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
                    CONFIG.ACDM511,
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
            cc = cc != null ? String.Format("Cc: {0}\r\n", cc) : null;
            var email = String.Format("From: \"{0}\" <" + CONFIG.ACDM511 + ">\r\nReply-To: {0}\r\nTo: {1}\r\n{4}Subject: =?utf-8?B?{2}?=\r\nContent-Type: text/html; charset=utf-8\r\n\r\n{3}",
                from, to, subject.ToBase64(), String.Join("", content.Split('\n').Select(s => String.Format("<p>{0}</p>", s.Trim()))) + "Sent from Sổ Đầu Bài. Please do not reply.", cc);
            var message = new Message();
            message.Raw = email.ToBase64().Replace("+", "-").Replace("/", "_").Replace("=", "");
            service.Users.Messages.Send(message, "me").Execute();
        }
    }
}