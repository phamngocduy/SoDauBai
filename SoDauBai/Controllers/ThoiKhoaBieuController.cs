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
            var NganhHoc = db.NganhHocs.ToList();
            var GiangVien = db.GiangViens.ToList();
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
                        if (NganhHoc.SingleOrDefault(nh => nh.MaNganh == row.MaNganh) == null)
                            throw new Exception("Không có MaNganh trong hệ thống!");
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
                        if (GiangVien.SingleOrDefault(gv => gv.MaGV == row.MaGV) == null)
                            throw new Exception("Không có MaGV trong hệ thống!");
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

            TempData["ThoiKhoaBieu"] = list;
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(byte HocKy)
        {
            foreach (var model in TempData["ThoiKhoaBieu"] as List<ThoiKhoaBieu>)
            {
                model.HocKy = HocKy;
                db.ThoiKhoaBieux.Add(model);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = db.ThoiKhoaBieux.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ThoiKhoaBieu model)
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
            var model = db.ThoiKhoaBieux.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var model = db.ThoiKhoaBieux.Find(id);
            db.ThoiKhoaBieux.Remove(model);
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
