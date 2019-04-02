﻿using System;
using System.Data.Entity;
using System.Linq;
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
                model.NgayTao = DateTime.Now;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = model.idTKB });
            }
            return View("Update", model);
        }

        public ActionResult Delete(int id)
        {
            var model = db.SoGhiBais.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
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
