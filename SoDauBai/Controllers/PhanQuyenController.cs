using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao")]
    public class PhanQuyenController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index(string id)
        {
            return View(db.AspNetUsers.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string id, string userName)
        {
            var role = db.AspNetRoles.SingleOrDefault(r => r.Name == id);
            if (role == null) return HttpNotFound();
            var user = db.AspNetUsers.SingleOrDefault(u => u.UserName == userName);
            if (user == null) return HttpNotFound();

            role.AspNetUsers.Add(user);
            user.AspNetRoles.Add(role);
            db.Entry(role).State = EntityState.Modified;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new { id = id });
        }

        public ActionResult Delete(string id, string email)
        {
            var model = db.AspNetUsers.SingleOrDefault(u => u.UserName == email);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id, string userName)
        {
            var role = db.AspNetRoles.SingleOrDefault(r => r.Name == id);
            if (role == null) return HttpNotFound();
            var user = db.AspNetUsers.SingleOrDefault(u => u.UserName == userName);
            if (user == null) return HttpNotFound();

            role.AspNetUsers.Remove(user);
            user.AspNetRoles.Remove(role);
            db.Entry(role).State = EntityState.Modified;
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index", new { id = id });
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
