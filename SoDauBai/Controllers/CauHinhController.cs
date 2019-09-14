using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Data.Entity;

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
        public ActionResult Index(int khoaSo)
        {
            using (var db = new SoDauBaiEntities())
            {
                var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO);
                KhoaSo.GiaTri = khoaSo.ToString();
                db.Entry(KhoaSo).State = EntityState.Modified;
                db.SaveChanges();
            }
            TempData["Message"] = "Cập nhật thông tin thành công.";
            return RedirectToAction("Index");
        }

        public static void SendEmail(string from, string to, string subject, string content)
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
    }
}