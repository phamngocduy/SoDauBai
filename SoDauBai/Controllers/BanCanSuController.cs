using System;
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
        public ActionResult Create(SoGhiChu model, bool create = true)
        {
            if (User.IsInBCS(model.idTKB))
            {
                if (ModelState.IsValid)
                {
                    if (create)
                        model.NgayGhi = DateTime.Now;
                    model.NgaySua = DateTime.Now;
                    model.Email = User.Identity.GetUserName();
                    if (create)
                        db.SoGhiChus.Add(model);
                    else // same method for create & edit
                    {
                        var soGhiChu = db.SoGhiChus.Find(model.id);
                        var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO).GiaTri.ToIntOrDefault(0);
                        if ((DateTime.Today - soGhiChu.NgayGhi).Days > KhoaSo)
                            ModelState.AddModelError("", "NgayHomNay - NgayGhi > " + KhoaSo);
                        if (ModelState.IsValid)
                        {
                            soGhiChu.NgaySua = model.NgaySua;
                            soGhiChu.GioBD = model.GioBD;
                            soGhiChu.GioKT = model.GioKT;
                            soGhiChu.DanhGia = model.DanhGia;
                            soGhiChu.NoiDung = model.NoiDung;
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index", "SoDauBai", new { id = model.idTKB });
                }
            }
            return View("Create", model);
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            var model = db.SoGhiChus.Find(id);
            if (model != null && model.Email == User.Identity.Name)
                return View("Create", model);
            return HttpNotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(SoGhiChu model)
        {
            var soGhiChu = db.SoGhiChus.Find(model.id);
            if (soGhiChu != null && soGhiChu.Email == User.Identity.Name)
                return Create(model, false);
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var model = db.SoGhiChus.Find(id);
            if (model != null && model.Email == User.Identity.Name)
                return View(model);
            return HttpNotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO).GiaTri.ToIntOrDefault(0);
            var model = db.SoGhiChus.Find(id);
            if ((DateTime.Today - model.NgayGhi).Days < KhoaSo && model.Email == User.Identity.Name)
            {
                db.SoGhiChus.Remove(model);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "SoDauBai", new { id = model.idTKB });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}