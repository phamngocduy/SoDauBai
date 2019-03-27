using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SoDauBai.Models;
using ExcelDataReader;
using System.Collections.Generic;

namespace SoDauBai.Controllers
{
    public class ThoiKhoaBieuController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index()
        {
            return View(db.ThoiKhoaBieux.ToList());
        }

        [ActionName("Template_TKB")]
        public ActionResult Template()
        {
            return File(Server.MapPath("~/App_Data/Template_TKB.xls"), "application/vnd.ms-excel");
        }

        class ExcelRow : ThoiKhoaBieu
        {
            public string RawData { get; set; }
            public string ErrorMsg { get; set; }
        }

        [HttpPost]
        public ActionResult Upload()
        {
            var list = new List<ExcelRow>();
            using (var reader = ExcelReaderFactory.CreateReader(Request.Files[0].InputStream))
            {
                reader.Read();
                while (reader.Read())
                {
                    var row = new ExcelRow();
                    var MaMH = reader.GetString(0);
                    if (string.IsNullOrEmpty(MaMH))
                        break;
                    var TenMH = reader.GetString(1);
                }
            }
            return View();
        }

        // POST: ThoiKhoaBieu/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,MaMH,TenMH,SoTinChi,NhomTo,ToTH,TenToHop,MaNganh,MaLop,TenLop,TongSoSV,ThuKieuSo,TietBD,SoTiet,MaGV,MaPH")] ThoiKhoaBieu thoiKhoaBieu)
        {
            if (ModelState.IsValid)
            {
                db.ThoiKhoaBieux.Add(thoiKhoaBieu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(thoiKhoaBieu);
        }

        // GET: ThoiKhoaBieu/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThoiKhoaBieu thoiKhoaBieu = db.ThoiKhoaBieux.Find(id);
            if (thoiKhoaBieu == null)
            {
                return HttpNotFound();
            }
            return View(thoiKhoaBieu);
        }

        // POST: ThoiKhoaBieu/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,MaMH,TenMH,SoTinChi,NhomTo,ToTH,TenToHop,MaNganh,MaLop,TenLop,TongSoSV,ThuKieuSo,TietBD,SoTiet,MaGV,MaPH")] ThoiKhoaBieu thoiKhoaBieu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(thoiKhoaBieu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(thoiKhoaBieu);
        }

        // GET: ThoiKhoaBieu/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ThoiKhoaBieu thoiKhoaBieu = db.ThoiKhoaBieux.Find(id);
            if (thoiKhoaBieu == null)
            {
                return HttpNotFound();
            }
            return View(thoiKhoaBieu);
        }

        // POST: ThoiKhoaBieu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ThoiKhoaBieu thoiKhoaBieu = db.ThoiKhoaBieux.Find(id);
            db.ThoiKhoaBieux.Remove(thoiKhoaBieu);
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
