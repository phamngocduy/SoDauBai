using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Web.Routing;
using System.Threading;
using System.Transactions;
using System.Globalization;
using Microsoft.AspNet.Identity;
using System.Security.Principal;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class PhongDayBuController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        private static IQueryable<PhongDayBu> FilterGiaoVu(IQueryable<PhongDayBu> Phong, IPrincipal user, SoDauBaiEntities db)
        {
            if (!user.IsInRole("DaoTao") && user.IsInRole("GiaoVu"))
            {
                var email = user.Identity.GetUserName();
                var GV = db.GiaoVus.SingleOrDefault(gv => gv.Email == email);
                var maNganh = (GV.Init().MaNganh ?? "").Split(',');
                Phong = Phong.Where(p => maNganh.Contains(p.ThoiKhoaBieu.MaNganh));
            }
            var hk = Extension.GetHocKy(null, db); // filter by HK
            Phong = Phong.Where(p => p.ThoiKhoaBieu.HocKy == hk);
            return Phong;
        }

        public static int countPhongDayBu(int id)
        {
            using (var db = new SoDauBaiEntities())
            return db.PhongDayBus.Count(p => p.idTKB == id && p.status.HasValue && DateTime.Today < p.NgayDay);
        }

        public ActionResult Index1(int id)
        {
            var model = db.PhongDayBus.Where(p => p.idTKB == id);
            ViewBag.TKB = db.ThoiKhoaBieux.Find(id);
            ViewBag.GV = db.GiangViens.ToList();
            return View("Index", model.ToList());
        }

        public static int countPhongDayBu(IPrincipal user)
        {
            using (var db = new SoDauBaiEntities())
            return FilterGiaoVu(db.PhongDayBus, user, db).Count(p => !p.status.HasValue);
        }

        [Authorize(Roles = "DaoTao,GiaoVu")]
        public ActionResult Index2(bool showAll = false)
        {
            var model = FilterGiaoVu(db.PhongDayBus, User, db)
                .Where(p => showAll || !p.status.HasValue);
            ViewBag.GV = db.GiangViens.ToList();
            return View("Index", model.ToList());
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
        }

        [TKBAuthorization]
        public ActionResult Create(int id)
        {
            var model = new PhongDayBu
            {
                idTKB = id,
                NgayBD = DateTime.Today.AddDays(7)
            };
            return View(model);
        }

        [HttpPost]
        [TKBAuthorization]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, PhongDayBu model)
        {
            model.idTKB = id;
            model.Date = DateTime.Now;
            model.email1 = User.Identity.GetUserName();
            var tkb = db.ThoiKhoaBieux.Find(id);
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope())
                {
                    model.MaPH = tkb.MaPH;
                    model.TietBD = tkb.TietBD;
                    db.PhongDayBus.Add(model);
                    db.SaveChanges();

                    UrlHelper url = new UrlHelper(Request.RequestContext);
                    var course = String.Format("Môn {0}, thứ {1} tiết {2} - {3}", tkb.TenMH, tkb.ThuKieuSo, tkb.TietBD, tkb.TietBD + tkb.SoTiet - 1);
                    var to = String.Join(",", db.GiaoVus.ToList().Where(gv => gv.MaNganh.Split(',').Contains(tkb.MaNganh)).Select(gv => gv.Email).ToArray());
                    if (String.IsNullOrWhiteSpace(to)) to = "p.dt@vanlanguni.edu.vn";
                    var cc = db.CauHinhs.Find(CONFIG.EP_CNTT).GiaTri;
                    cc = model.PhongMay && !String.IsNullOrEmpty(cc) ? cc : null;
                    CauHinhController.SendEmail(model.email1, to, "[SĐB] Đặt phòng dạy bù", course + '\n' + model.GhiChu1 + '\n' + url.Action("Index2", "PhongDayBu", null, Request.Url.Scheme), cc);

                    scope.Complete();
                    return RedirectToAction("Index1", new { id = id });
                }
            }

            return View(model);
        }

        public ActionResult Edit1(int id)
        {
            var model = db.PhongDayBus.Find(id);
            if (model == null || !User.IsInTKB(model.idTKB))
                return HttpNotFound();
            return View(model);
        }

        [Authorize(Roles = "DaoTao,GiaoVu")]
        public ActionResult Edit2(int id)
        {
            var model = db.PhongDayBus.Find(id);
            if (model == null)
                return HttpNotFound();
            if (model.NgayDay == DateTime.MinValue)
                model.NgayDay = model.NgayBD;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit1(PhongDayBu phong)
        {
            var model = db.PhongDayBus.Find(phong.id);
            if (ModelState.IsValid && User.IsInTKB(model.idTKB))
            {
                model.NgayBD = phong.NgayBD;
                model.GhiChu1 = phong.GhiChu1;
                model.email1 = User.Identity.GetUserName();
                model.status = null;
                model.PhongMay = phong.PhongMay;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index1", new { id = model.idTKB });
            }
            return View(phong);
        }

        [HttpPost]
        [Authorize(Roles = "DaoTao,GiaoVu")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit2(PhongDayBu phong)
        {
            var model = db.PhongDayBus.Find(phong.id);
            if (ModelState.IsValid)
            {
                using (var scope = new TransactionScope())
                {
                    model.MaPH = phong.MaPH;
                    model.TietBD = phong.TietBD;
                    model.NgayDay = phong.NgayDay;
                    model.GhiChu2 = phong.GhiChu2;
                    model.status = DateTime.Now;
                    model.email2 = User.Identity.GetUserName();
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    UrlHelper url = new UrlHelper(Request.RequestContext);
                    var ghiChu1 = String.Join("\n", model.GhiChu1.Split('\n').Select(q => "<pre>" + q + "</pre>"));
                    ghiChu1 = "________________________________________\n" + ghiChu1;
                    var ghiChu2 = String.Format("Ngày <mark>{0}</mark> Phòng <mark>{1}</mark>, từ Tiết <mark>{2}</mark>", model.NgayDay.ToString("dd/MM/yyyy"), model.MaPH, model.TietBD);
                    CauHinhController.SendEmail(model.email2, model.email1, "[SĐB] V/v Đặt phòng dạy bù", model.GhiChu2 + '\n' + ghiChu2 + '\n' + ghiChu1 + '\n' + url.Action("Index1", "PhongDayBu", new { id = model.idTKB }, Request.Url.Scheme));

                    scope.Complete();
                    return RedirectToAction("Index2", new { id = model.idTKB });
                }
            }
            return View(phong);
        }

        public ActionResult Delete(int id)
        {
            var model = db.PhongDayBus.Find(id);
            if (model == null || !(User.IsInTKB(model.idTKB) || User.IsInRoles("DaoTao,GiaoVu")))
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.PhongDayBus.Find(id);
            if (User.IsInTKB(model.idTKB) || User.IsInRoles("DaoTao,GiaoVu"))
            {
                db.PhongDayBus.Remove(model);
                db.SaveChanges();
            }
            if (User.IsInTKB(model.idTKB))
                return RedirectToAction("Index1", new { id = model.idTKB });
            else return RedirectToAction("Index2"); // User.IsInRole("DaoTao,GiaoVu")
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
