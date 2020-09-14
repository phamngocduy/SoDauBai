using System;
using System.IO;
using System.Text;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao")]
    public class GiangVienController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index()
        {
            return View(db.GiangViens.ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GiangVien model)
        {
            if (ModelState.IsValid)
                try
                {
                    db.GiangViens.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var model = db.GiangViens.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GiangVien model)
        {
            if (ModelState.IsValid)
                try
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var model = db.GiangViens.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.GiangViens.Find(id);
            db.GiangViens.Remove(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public FileResult Download()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.Unicode))
            {
                writer.WriteLine("MaGV\tHocVi\tHoTen\tEmail");
                foreach (var gv in db.GiangViens)
                    writer.WriteLine("\"=\"\"{0}\"\"\"\t\"{1}\"\t\"{2}\"\t\"{3}\"",
                        gv.MaGV, gv.HocVi, gv.HoTen, gv.Email);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream.GetBuffer(), "text/x-csv", "GiangVien.csv");
            }
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
