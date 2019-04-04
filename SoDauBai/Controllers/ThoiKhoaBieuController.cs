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
    [Authorize(Roles = "DaoTao")]
    public class ThoiKhoaBieuController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index(int? id)
        {
            id = id.HasValue ? id : db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy);
            var model = db.ThoiKhoaBieux.Where(tkb => tkb.HocKy == id);
            ViewBag.HocKy = new SelectList(db.ThoiKhoaBieux.Select(tkb => tkb.HocKy).Distinct().OrderByDescending(hk => hk), (byte)id.Value);
            return View(model.ToList());
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
            var rand = new Random(Environment.TickCount);
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
                        if (row.SoTinChi < 1 || row.SoTinChi > 6)
                            throw new Exception("SoTinChi không trong khoảng [1-6]!");
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
                        if (NganhHoc.SingleOrDefault(nh => nh.MaNganh == row.MaNganh) == null)
                        {
                            db.NganhHocs.Add(new NganhHoc
                            {
                                MaNganh = row.MaNganh,
                                TenNganh = reader.GetString(i)
                            });
                            db.SaveChanges();
                        }
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
                        if (row.ThuKieuSo < 2 || row.ThuKieuSo > 8)
                            throw new Exception("ThuKieuSo không trong khoảng [2-8]!");
                        i++;
                        row.TietBD = byte.Parse(reader.GetValue(i).ToString());
                        if (row.TietBD < 0 || row.TietBD > 15)
                            throw new Exception("TietBD không trong khoảng [1-15]!");
                        i++;
                        row.SoTiet = byte.Parse(reader.GetValue(i).ToString());
                        if (row.SoTiet < 0 || row.SoTiet > 6)
                            throw new Exception("SoTiet không trong khoảng [1-6]!");
                        i++;
                        row.MaGV = reader.GetString(i) ?? "";
                        if (row.MaGV.Length > 10)
                            throw new Exception("MaGV dài hơn 10 ký tự!");
                        i++;
                        if (GiangVien.SingleOrDefault(gv => gv.MaGV == row.MaGV) == null)
                        {
                            db.GiangViens.Add(new GiangVien
                            {
                                MaGV = row.MaGV,
                                HoTen = reader.GetString(i),
                                Email = String.Format("ACDM{0}@gmail.com", rand.Next())
                            });
                            db.SaveChanges();
                        }
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
        public ActionResult Create(byte hocKy)
        {
            var TKB = db.ThoiKhoaBieux.AsNoTracking().Where(tkb => tkb.HocKy == hocKy);
            foreach (var model in TempData["ThoiKhoaBieu"] as List<ThoiKhoaBieu>)
            {
                model.HocKy = hocKy;
                var old = TKB.SingleOrDefault(tkb => tkb.MaMH == model.MaMH &&
                    tkb.NhomTo == model.NhomTo && tkb.ToTH == model.ToTH && tkb.TenToHop == model.TenToHop);
                if (old != null)
                {
                    model.id = old.id;
                    db.Entry(model).State = EntityState.Modified;
                }
                else
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
                    return RedirectToAction("Index", new { id = model.HocKy });
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
            return RedirectToAction("Index", new { id = model.HocKy });
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
