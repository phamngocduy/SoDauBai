using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Transactions;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao,GiaoVu")]
    public class ThongKeController : Controller
    {
        SoDauBaiEntities db = new SoDauBaiEntities();

        private IQueryable<ThoiKhoaBieu> FilterHocKy(IQueryable<ThoiKhoaBieu> TKB, int? hk = null)
        {
            TKB = TKB.Include(tkb => tkb.SoGhiBais);
            if (hk.HasValue)
                return TKB.Where(tkb => tkb.HocKy == hk);
            else
                hk = db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy);
            return TKB.Where(tkb => tkb.HocKy == hk);
        }

        public ActionResult LopDangDay()
        {
            var model = FilterGiaoVu(FilterHocKy(db.ThoiKhoaBieux));
            ViewBag.GiangViens = db.GiangViens.ToList();
            var now = DateTime.Now;
            var thu = CONST.THU[(int)now.DayOfWeek];
            model = model.Where(tkb => tkb.ThuKieuSo == thu);
            return View(model.ToList().Where(tkb => CONST.TIET[tkb.TietBD] <= now.TimeOfDay &&
                        now.TimeOfDay <= CONST.TIET[tkb.TietBD + tkb.SoTiet - 1].Add(CONST.TIET[0])));
        }

        private IQueryable<ThoiKhoaBieu> FilterGiaoVu(IQueryable<ThoiKhoaBieu> TKB)
        {
            if (!User.IsInRole("DaoTao") && User.IsInRole("GiaoVu"))
            {
                var email = User.Identity.GetUserName();
                var GV = db.GiaoVus.SingleOrDefault(gv => gv.Email == email);
                var maNganh = GV.Init().MaNganh.Split(',');
                TKB = TKB.Where(tkb => maNganh.Contains(tkb.MaNganh));
            }
            return TKB;
        }

        public ActionResult CacDeXuat()
        {
            var model = from tkb in FilterGiaoVu(FilterHocKy(db.ThoiKhoaBieux))
                        join sdb in db.SoGhiBais on tkb.id equals sdb.idTKB
                        where sdb.DeXuat != null && sdb.DeXuat != ""
                        select sdb;
            ViewBag.GiangViens = db.GiangViens.ToList();
            return View(model.ToList());
        }

        public ActionResult ThongKeChung()
        {
            var hk = this.GetHocKy(db);
            var model = FilterGiaoVu(FilterHocKy(db.ThoiKhoaBieux, hk));
            ViewBag.GiangViens = db.GiangViens.ToList();
            ViewBag.GuiEmails = db.GuiEmails.Where(e => e.Loai == EMAILS.GhiSo).ToList();
            return View(model.ToList());
        }

        public ActionResult GuiMailNhacNho(int id)
        {
            ViewBag.TKB = id;
            var tkb = db.ThoiKhoaBieux.Find(id);
            var email = db.GiangViens.Single(gv => gv.MaGV == tkb.MaGV).Email;
            ViewBag.GuiEmails = db.GuiEmails.Where(e => e.Loai == EMAILS.GhiSo && e.Email == email && e.Tag == id).ToList();
            return View(email as object);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuiMailNhacNho(int id, string subject, string content)
        {
            UrlHelper url = new UrlHelper(Request.RequestContext);
            var tkb = db.ThoiKhoaBieux.Find(id);
            var email = db.GiangViens.Single(gv => gv.MaGV == tkb.MaGV).Email;

            subject = "[SĐB] " + subject;
            content = content + "\n" + url.Action("Index", "SoDauBai", new { id = tkb.id }, Request.Url.Scheme);
            using (var scope = new TransactionScope())
            {
                db.GuiEmails.Add(new GuiEmail
                {
                    Tag = tkb.id,
                    Loai = EMAILS.GhiSo,
                    Ngay = DateTime.Now,
                    MaGV = tkb.MaGV,
                    Email = email,
                    FromTo = User.Identity.GetUserName(),
                    TieuDe = subject,
                    NoiDung = content
                });
                db.SaveChanges();

                CauHinhController.SendEmail(User.Identity.GetUserName(), email, subject, content);

                scope.Complete();
            }

            return RedirectToAction("ThongKeChung");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}