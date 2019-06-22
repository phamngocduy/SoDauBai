using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;
using System.Globalization;
using System.Threading;
using System.Web.Routing;
using System.Collections.Generic;

namespace SoDauBai.Controllers
{
    public class SoDauBaiController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index(int id)
        {
            var TKB = db.ThoiKhoaBieux.Find(id);
            if (TKB == null)
                return HttpNotFound();
            var model = db.ThoiKhoaBieux.Where(tkb => tkb.MaMH == TKB.MaMH && tkb.HocKy == TKB.HocKy
                            && tkb.NhomTo == TKB.NhomTo && (tkb.ToTH == TKB.ToTH || tkb.ToTH == null));
            ViewBag.TKB = TKB;
            ViewBag.GV = db.GiangViens.FirstOrDefault(gv => gv.MaGV == TKB.MaGV);
            return View(model.Select(tkb => tkb.SoGhiBais.ToList()).ToList().Merge());
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
        }

        private void Validate(SoGhiBai model)
        {
            if (model.TongSoSV <= 0)
                ModelState.AddModelError("TongSoSV", "TongSoSV < 0");
            if (model.ThoiGianKT <= model.ThoiGianBD)
                ModelState.AddModelError("ThoiGianKT", "ThoiGianKT < ThoiGianBD");
        }

        [TKBAuthorization]
        public ActionResult Create(int id)
        {
            var tkb = db.ThoiKhoaBieux.Find(id);
            var model = new SoGhiBai
            {
                NgayDay = DateTime.Today,
                ThoiGianBD = CONST.TIET[tkb.TietBD],
                ThoiGianKT = CONST.TIET[tkb.TietBD + tkb.SoTiet - 1].Add(new TimeSpan(0, 45, 0)),
                SoTietDay = tkb.SoTiet,
                MaPhong = tkb.MaPH,
                TongSoSV = tkb.TongSoSV,
                Email = User.Identity.GetUserName(),
                idTKB = tkb.id
            };
            ViewBag.NhanXets = db.NhanXets.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [TKBAuthorization]
        public ActionResult Create(SoGhiBai model)
        {
            Validate(model);
            if (ModelState.IsValid)
                try
                {
                    model.NgayTao = DateTime.Now;
                    model.Email = User.Identity.GetUserName();
                    db.SoGhiBais.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { id = model.idTKB });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }
            ViewBag.NhanXets = db.NhanXets.ToList();
            return View(model);
        }

        [SDBAuthorization]
        public ActionResult Edit(int id)
        {
            var model = db.SoGhiBais.Find(id);
            if (model == null)
                return HttpNotFound();
            ViewBag.NhanXets = db.NhanXets.ToList();
            return View("Update", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SDBAuthorization]
        public ActionResult Edit(SoGhiBai model)
        {
            Validate(model);
            if (ModelState.IsValid)
            {
                model.NgayTao = DateTime.Now;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = model.idTKB });
            }
            ViewBag.NhanXets = db.NhanXets.ToList();
            return View("Update", model);
        }

        [SDBAuthorization]
        public ActionResult Delete(int id)
        {
            var model = db.SoGhiBais.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SDBAuthorization]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.SoGhiBais.Find(id);
            db.SoGhiBais.Remove(model);
            db.SaveChanges();
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
