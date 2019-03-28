using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    public class SoDauBaiController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index(int id)
        {
            var model = db.ThoiKhoaBieux.Find(id);
            if (model == null)
                return HttpNotFound();
            ViewBag.TKB = model;
            return View(model.SoGhiBais.ToList());
        }

        private TimeSpan[] TIET =
        {
            new TimeSpan(),
            new TimeSpan(7, 0, 0),
            new TimeSpan(7, 50, 0),
            new TimeSpan(8, 40, 0),
            new TimeSpan(9, 35, 0),
            new TimeSpan(10, 25, 0),
            new TimeSpan(11, 15, 0),
            new TimeSpan(13, 0, 0),
            new TimeSpan(13, 50, 0),
            new TimeSpan(14, 40, 0),
            new TimeSpan(15, 35, 0),
            new TimeSpan(16, 25, 0),
            new TimeSpan(17, 15, 0)
        };

        public ActionResult Create(int id)
        {
            var tkb = db.ThoiKhoaBieux.Find(id);
            var model = new SoGhiBai
            {
                NgayDay = DateTime.Today,
                ThoiGianBD = TIET[tkb.TietBD],
                ThoiGianKT = TIET[tkb.TietBD + tkb.SoTiet - 1].Add(new TimeSpan(0, 45, 0)),
                SoTietDay = tkb.SoTiet,
                MaPhong = tkb.MaPH,
                TongSoSV = tkb.TongSoSV,
                Email = User.Identity.GetUserName(),
                idTKB = tkb.id
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SoGhiBai model)
        {
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

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = db.SoGhiBais.Find(id);
            if (model == null)
                return HttpNotFound();
            return View("Update", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SoGhiBai model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = model.idTKB });
            }
            return View("Update", model);
        }

        // GET: SoDauBai/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SoGhiBai soDauBai = db.SoGhiBais.Find(id);
            if (soDauBai == null)
            {
                return HttpNotFound();
            }
            return View(soDauBai);
        }

        // POST: SoDauBai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SoGhiBai soDauBai = db.SoGhiBais.Find(id);
            db.SoGhiBais.Remove(soDauBai);
            db.SaveChanges();
            return RedirectToAction("Index");
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
