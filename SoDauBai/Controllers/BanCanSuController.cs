﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class BanCanSuController : Controller
    {
        public JsonResult Search(string term)
        {
            return Json(db.AspNetUsers.Select(u => u.Email).ToArray().Select(s => s.ToLower())
                .Where(e => e.EndsWith("@vanlanguni.vn") && e.Contains(term.ToLower())).ToArray(),
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult Index(int id)
        {
            return Json(db.BanCanSus.Where(bcs => bcs.idTKB == id).Select(bcs => bcs.Email).ToArray(),
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [TKBAuthorization]
        public string Insert(int id, string email)
        {
            try
            {
                db.BanCanSus.Add(new BanCanSu
                {
                    idTKB = id,
                    Email = email
                });
                db.SaveChanges();
                return String.Empty;
            }
            catch (Exception e)
            {
                return e.GetBaseException().Message;
            }
        }

        [HttpPost]
        [TKBAuthorization]
        public string Remove(int id, string email)
        {
            try
            {
                foreach (var item in db.BanCanSus.Where(bcs => bcs.idTKB == id && bcs.Email == email))
                    db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return String.Empty;
            }
            catch (Exception e)
            {
                return e.GetBaseException().Message;
            }
        }

        /// For students to write notes
        SoDauBaiEntities db = new SoDauBaiEntities();

        [BCSAuthorization]
        public ActionResult Create(int id)
        {
            return View(new SoGhiChu { idTKB = id, GioBD = DateTime.Now.TimeOfDay });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SoGhiChu model)
        {
            if (User.IsInBCS(model.idTKB))
            {
                model.NgayGhi = DateTime.Today;
                ValidateModel(model);
                if (ModelState.IsValid)
                {
                    if (model.id == 0)
                        model.NgayGhi = DateTime.Now;
                    model.NgaySua = DateTime.Now;
                    model.Email = User.Identity.GetUserName();
                    db.SoGhiChus.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index", "SoDauBai", new { id = model.idTKB });
                }
            }
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}