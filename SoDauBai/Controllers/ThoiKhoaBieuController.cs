using System;
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

        [HttpPost]
        public ActionResult Upload()
        {
            var list = new List<ThoiKhoaBieu>();
            using (var reader = ExcelReaderFactory.CreateReader(Request.Files[0].InputStream))
            {
                reader.Read();
                while (reader.Read())
                {
                    var row = new ThoiKhoaBieu();
                    try
                    {
                        int i = 0;
                        row.MaMH = reader.GetString(i) ?? "";
                        if (string.IsNullOrEmpty(row.MaMH))
                            break;
                        if (row.MaMH.Length > 10)
                            throw new Exception("MaMH dài hơn 10 ký tự!");
                        i++;
                        row.TenMH = reader.GetString(i) ?? "";
                        i++;
                        row.SoTinChi = byte.Parse(reader.GetValue(i).ToString());
                        if (row.SoTinChi < 1 || row.SoTinChi > 5)
                            throw new Exception("SoTinChi không trong khoảng [1-5]!");
                        i++;
                        row.NhomTo = reader.GetString(i) ?? "";
                        i++;
                        row.ToTH = reader.GetString(i) ?? "";
                        i++;
                        row.TenToHop = reader.GetString(i) ?? "";
                        i++;
                        row.MaNganh = reader.GetString(i) ?? "";
                        if (row.MaNganh.Length > 10)
                            throw new Exception("MaNganh dài hơn 10 ký tự!");
                        i++;
                        row.MaLop = reader.GetString(i) ?? "";
                        i++;
                        row.TenLop = reader.GetString(i) ?? "";
                        i++;
                        row.TongSoSV = short.Parse(reader.GetValue(i).ToString());
                        if (row.TongSoSV < 1)
                            throw new Exception("Lớp không có đủ sinh viên!");
                        i++;
                        row.ThuKieuSo = byte.Parse(reader.GetValue(i).ToString());
                        if (row.ThuKieuSo < 2 || row.ThuKieuSo > 7)
                            throw new Exception("ThuKieuSo không trong khoảng [2-7]!");
                        i++;
                        row.TietBD = byte.Parse(reader.GetValue(i).ToString());
                        if (row.TietBD < 0 || row.TietBD > 12)
                            throw new Exception("TietBD không trong khoảng [1-12]!");
                        i++;
                        row.SoTiet = byte.Parse(reader.GetValue(i).ToString());
                        if (row.SoTiet < 0 || row.SoTiet > 5)
                            throw new Exception("SoTiet không trong khoảng [1-5]!");
                        i++;
                        row.MaGV = reader.GetString(i) ?? "";
                        if (row.MaGV.Length > 10)
                            throw new Exception("MaGV dài hơn 10 ký tự!");
                        i++;
                        row.MaPH = reader.GetString(i) ?? "";
                    }
                    catch (Exception e)
                    {
                        row.MaMH = null;
                        row.TenMH = e.Message;
                    }
                    list.Add(row);
                }
            }
            return View(list);
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
