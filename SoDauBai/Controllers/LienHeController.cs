using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class LienHeController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index()
        {
            return View(db.LienHes.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LienHe model)
        {
            model.on = DateTime.Now;
            model.from = User.Identity.GetUserName();
            if (ModelState.IsValid)
            {
                db.LienHes.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Update(int id)
        {
            var model = db.LienHes.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(LienHe model)
        {
            var lienHe = db.LienHes.Find(model.id);
            if (!String.IsNullOrEmpty(lienHe.Answer))
                return HttpNotFound();
            lienHe.on = DateTime.Now;
            lienHe.from = User.Identity.GetUserName();
            lienHe.Question = model.Question;
            if (ModelState.IsValid)
            {
                db.Entry(lienHe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize(Roles = "HoTro")]
        public ActionResult Answer(int id)
        {
            var model = db.LienHes.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [Authorize(Roles = "HoTro")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Answer(LienHe model)
        {
            var lienHe = db.LienHes.Find(model.id);
            lienHe.at = DateTime.Now;
            lienHe.to = User.Identity.GetUserName();
            lienHe.Answer = model.Answer;
            if (ModelState.IsValid)
            {
                db.Entry(lienHe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [Authorize(Roles = "HoTro")]
        public ActionResult Delete(int id)
        {
            var model = db.LienHes.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [Authorize(Roles = "HoTro")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.LienHes.Find(id);
            db.LienHes.Remove(model);
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
