using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Transactions;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao")]
    public class GiaoVuController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index()
        {
            return View(db.GiaoVus.ToList());
        }

        public ActionResult Create()
        {
            ViewBag.Users = db.AspNetUsers.ToList();
            ViewBag.Nganh = db.ThoiKhoaBieux.Select(tkb => tkb.MaNganh).Distinct().ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GiaoVu model)
        {
            model.MaNganh = String.Join(",",Request.Form.AllKeys.Where(k => k.StartsWith("#"))
                .Where(k => Request.Form[k].StartsWith("true")).Select(k => k.Substring(1)));
            if (ModelState.IsValid)
                using (var scope = new TransactionScope())
                {
                    var role = db.AspNetRoles.SingleOrDefault(r => r.Name == "GiaoVu");
                    if (role == null) return HttpNotFound();
                    var user = db.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
                    if (user == null) return HttpNotFound();

                    role.AspNetUsers.Add(user);
                    user.AspNetRoles.Add(role);
                    db.Entry(role).State = EntityState.Modified;
                    db.Entry(user).State = EntityState.Modified;

                    db.GiaoVus.Add(model);
                    db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("Index");
                }
            ViewBag.Users = db.AspNetUsers.ToList();
            ViewBag.Nganh = db.ThoiKhoaBieux.Select(tkb => tkb.MaNganh).Distinct().ToList();
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = db.GiaoVus.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            ViewBag.Users = db.AspNetUsers.ToList();
            ViewBag.Nganh = db.ThoiKhoaBieux.Select(tkb => tkb.MaNganh).Distinct().ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GiaoVu model)
        {
            model.MaNganh = String.Join(",", Request.Form.AllKeys.Where(k => k.StartsWith("#"))
                .Where(k => Request.Form[k].StartsWith("true")).Select(k => k.Substring(1)));
            if (ModelState.IsValid)
                using (var scope = new TransactionScope())
                {
                    var role = db.AspNetRoles.SingleOrDefault(r => r.Name == "GiaoVu");
                    if (role == null) return HttpNotFound();
                    var user = db.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
                    if (user == null) return HttpNotFound();

                    role.AspNetUsers.Add(user);
                    user.AspNetRoles.Add(role);
                    db.Entry(role).State = EntityState.Modified;
                    db.Entry(user).State = EntityState.Modified;

                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    scope.Complete();
                    return RedirectToAction("Index");
                }
            ViewBag.Users = db.AspNetUsers.ToList();
            ViewBag.Nganh = db.ThoiKhoaBieux.Select(tkb => tkb.MaNganh).Distinct().ToList();
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var model = db.GiaoVus.Find(id);
            if (model == null)
            {
                return HttpNotFound();
            }
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var scope = new TransactionScope())
            {
                GiaoVu model = db.GiaoVus.Find(id);
                var role = db.AspNetRoles.SingleOrDefault(r => r.Name == "GiaoVu");
                if (role == null) return HttpNotFound();
                var user = db.AspNetUsers.SingleOrDefault(u => u.UserName == model.Email);
                if (user == null) return HttpNotFound();

                role.AspNetUsers.Remove(user);
                user.AspNetRoles.Remove(role);
                db.Entry(role).State = EntityState.Modified;
                db.Entry(user).State = EntityState.Modified;

                db.GiaoVus.Remove(model);
                db.SaveChanges();
                scope.Complete();
            }
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
