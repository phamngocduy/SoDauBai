using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Web.Routing;
using System.Threading;
using System.Globalization;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class PhongDayBuController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index1(int id)
        {
            var model = db.PhongDayBus.Where(p => p.idTKB == id);
            ViewBag.TKB = db.ThoiKhoaBieux.Find(id);
            return View("Index", model.ToList());
        }

        [Authorize(Roles = "DaoTao")]
        public ActionResult Index2(int id)
        {
            var model = db.PhongDayBus;
            return View("Index", model.ToList());
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
        }

        [TKBAuthentication]
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
        [TKBAuthentication]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int id, PhongDayBu model)
        {
            model.idTKB = id;
            model.Date = DateTime.Now;
            model.email1 = User.Identity.GetUserName();
            if (ModelState.IsValid)
            {
                db.PhongDayBus.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index1", new { id = id });
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

        [Authorize(Roles = "DaoTao")]
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
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index1", new { id = model.idTKB });
            }
            return View(phong);
        }

        [HttpPost]
        [Authorize(Roles = "DaoTao")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit2(PhongDayBu phong)
        {
            var model = db.PhongDayBus.Find(phong.id);
            if (ModelState.IsValid)
            {
                model.MaPH = phong.MaPH;
                model.TietBD = phong.TietBD;
                model.NgayDay = phong.NgayDay;
                model.GhiChu2 = phong.GhiChu2;
                model.status = DateTime.Now;
                model.email2 = User.Identity.GetUserName();
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index2", new { id = model.idTKB });
            }
            return View(phong);
        }

        public ActionResult Delete(int id)
        {
            var model = db.PhongDayBus.Find(id);
            if (model == null || !User.IsInTKB(model.idTKB))
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.PhongDayBus.Find(id);
            if (User.IsInTKB(model.idTKB))
            {
                db.PhongDayBus.Remove(model);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { id = model.idTKB });
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
