using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SoDauBai.Models;
using ExcelDataReader;
using System.Data.Entity.Validation;
using System.Collections.Generic;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao")]
    public class ThoiKhoaBieuController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index()
        {
            var hk = this.GetHocKy(db);
            var model = db.ThoiKhoaBieux.Where(tkb => tkb.HocKy == hk);
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
                        try
                        {
                            row.MaMH = reader.GetText(i);
                        }
                        catch (Exception)
                        {
                            break;
                        }
                        if (string.IsNullOrEmpty(row.MaMH))
                            break;
                        if (row.MaMH.Length > 10)
                            throw new Exception("MaMH dài hơn 10 ký tự!");
                        i++;
                        row.TenMH = reader.GetText(i);
                        i++;
                        row.SoTinChi = reader.GetText(i).Parse<byte>("SoTinChi không phải là số!");
                        if (row.SoTinChi < 1)
                            throw new Exception("SoTinChi không trong khoảng [1-?]!");
                        i++;
                        row.NhomTo = reader.GetText(i);
                        i++;
                        row.ToTH = reader.GetText(i);
                        i++;
                        row.TenToHop = reader.GetText(i);
                        i++;
                        row.MaNganh = reader.GetText(i);
                        if (row.MaNganh.Length > 10)
                            throw new Exception("MaNganh dài hơn 10 ký tự!");
                        i++;
                        if (db.NganhHocs.SingleOrDefault(nh => nh.MaNganh == row.MaNganh) == null)
                        {
                            db.NganhHocs.Add(new NganhHoc
                            {
                                MaNganh = row.MaNganh,
                                TenNganh = reader.GetText(i)
                            });
                            db.SaveChanges();
                        }
                        i++;
                        row.MaLop = reader.GetText(i);
                        i++;
                        row.TenLop = reader.GetText(i);
                        i++;
                        row.TongSoSV = reader.GetText(i).Parse<short>(1);
                        if (row.TongSoSV < 1)
                            throw new Exception("Lớp không có đủ sinh viên!");
                        i++;
                        row.ThuKieuSo = reader.GetText(i).Parse<byte>("ThuKieuSo không phải là số!");
                        if (row.ThuKieuSo < 2 || row.ThuKieuSo > 8)
                            throw new Exception("ThuKieuSo không trong khoảng [2-8]!");
                        i++;
                        row.TietBD = reader.GetText(i).Parse<byte>("TietBD không phải là số!");
                        if (row.TietBD < 0 || row.TietBD > 15)
                            throw new Exception("TietBD không trong khoảng [1-15]!");
                        i++;
                        row.SoTiet = reader.GetText(i).Parse<byte>("SoTiet không phải là số!");
                        if (row.SoTiet < 0 || row.SoTiet > 12)
                            throw new Exception("SoTiet không trong khoảng [1-12]!");
                        i++;
                        row.MaGV = reader.GetText(i);
                        if (row.MaGV.Length > 10)
                            throw new Exception("MaGV dài hơn 10 ký tự!");
                        i++;
                        if (db.GiangViens.SingleOrDefault(gv => gv.MaGV == row.MaGV) == null)
                        {
                            db.GiangViens.Add(new GiangVien
                            {
                                MaGV = row.MaGV,
                                HoTen = reader.GetText(i),
                                Email = String.Format("ACDM{0}@gmail.com", rand.Next())
                            });
                            db.SaveChanges();
                        }
                        i++;
                        row.MaPH = reader.GetText(i);
                        i++;
                        row.TuanBD = reader.GetText(i).Parse<byte>(1);
                        i++;
                        row.TuanKT = reader.GetText(i).Parse<byte>(15);
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
                var old = TKB.SingleOrDefault(tkb => tkb.HocKy == hocKy && tkb.MaMH == model.MaMH &&
                    (tkb.NhomTo == model.NhomTo || ((tkb.NhomTo == null || tkb.NhomTo == "") && (model.NhomTo == null || model.NhomTo == ""))) &&
                    (tkb.ToTH == model.ToTH || ((tkb.ToTH == null || tkb.ToTH == "") && (model.ToTH == null || model.ToTH == ""))) &&
                    (tkb.TenToHop == model.TenToHop || ((tkb.TenToHop == null || tkb.TenToHop == "") && (model.TenToHop == null || model.TenToHop == ""))));
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
                catch (DbEntityValidationException e)
                {
                    ModelState.AddModelError("",
                        (e.EntityValidationErrors.Count() > 0 && e.EntityValidationErrors.First().ValidationErrors.Count > 0)
                        ? e.EntityValidationErrors.First().ValidationErrors.First().ErrorMessage : e.Message);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }
            return View(model);
        }

        private void ValidateTKB(ThoiKhoaBieu model)
        {
            if (model.TongSoSV < 1)
                ModelState.AddModelErrorFor(m => model.TongSoSV, "Lớp không có đủ sinh viên!");
            if (model.ThuKieuSo < 2 || model.ThuKieuSo > 8)
                ModelState.AddModelErrorFor(m => model.ThuKieuSo, "ThuKieuSo không trong khoảng [2-8]!");
            if (model.TietBD < 0 || model.TietBD > 15)
                ModelState.AddModelErrorFor(m => model.TietBD, "TietBD không trong khoảng [1-15]!");
            if (model.SoTiet < 0 || model.SoTiet > 6)
                ModelState.AddModelErrorFor(m => model.SoTiet, "SoTiet không trong khoảng [1-6]!");
        }

        [OverrideAuthorization]
        public ActionResult Update(int id)
        {
            var model = db.ThoiKhoaBieux.Find(id);
            if (model == null)
                return HttpNotFound();
            return View(model);
        }

        [HttpPost]
        [OverrideAuthorization]
        [ValidateAntiForgeryToken]
        [TKBAuthorization]
        public ActionResult Update(ThoiKhoaBieu model)
        {
            ValidateTKB(model);
            if (ModelState.IsValid)
                try
                {
                    var tkb = db.ThoiKhoaBieux.Find(model.id);
                    tkb.MaLop = model.MaLop;
                    tkb.TenLop = model.TenLop;
                    tkb.TongSoSV = model.TongSoSV;
                    tkb.ThuKieuSo = model.ThuKieuSo;
                    tkb.TietBD = model.TietBD;
                    tkb.SoTiet = model.SoTiet;
                    tkb.TuanBD = model.TuanBD;
                    tkb.TuanKT = model.TuanKT;
                    tkb.MaPH = model.MaPH;

                    db.Entry(tkb).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "SoDauBai", new { model.id });
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
