using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Transactions;
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
                using (var scope = new TransactionScope())
                {
                    db.LienHes.Add(model);
                    db.SaveChanges();

                    UrlHelper url = new UrlHelper(Request.RequestContext);
                    var to = String.Join(",", db.AspNetRoles.FirstOrDefault(r => r.Name == "HoTro").Init().AspNetUsers.Select(u => u.Email).ToArray());
                    CauHinhController.SendEmail(model.from, to, "[SĐB] Liên hệ / Báo lỗi", model.Question + '\n' + url.Action("Index", "LienHe", null, Request.Url.Scheme));

                    scope.Complete();
                    return RedirectToAction("Index");
                }
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
                using (var scope = new TransactionScope())
                {
                    db.Entry(lienHe).State = EntityState.Modified;
                    db.SaveChanges();

                    UrlHelper url = new UrlHelper(Request.RequestContext);
                    var question = String.Join("\n", lienHe.Question.Split('\n').Select(q => "<pre>" + q + "</pre>"));
                    question = "________________________________________\n" + question;
                    CauHinhController.SendEmail(lienHe.to, lienHe.from, "[SĐB] V/v Liên hệ / Báo lỗi", model.Answer + '\n' + question + '\n' + url.Action("Index", "LienHe", null, Request.Url.Scheme));

                    scope.Complete();
                    return RedirectToAction("Index");
                }
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
