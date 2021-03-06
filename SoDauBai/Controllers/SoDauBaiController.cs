﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;
using System.Globalization;
using System.Threading;
using System.Web.Routing;
using System.Collections.Generic;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class SoDauBaiController : Controller
    {
        private SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult Index(int id)
        {
            var TKB = db.ThoiKhoaBieux.Find(id);
            if (TKB == null)
                return HttpNotFound();
            var model = db.ThoiKhoaBieux.Where(tkb => tkb.MaMH == TKB.MaMH && tkb.HocKy == TKB.HocKy
                            && tkb.NhomTo == TKB.NhomTo && (tkb.ToTH == TKB.ToTH || tkb.ToTH == null));
            ViewBag.TKB = TKB;
            ViewBag.GVs = db.GiangViens.ToList();
            ViewBag.TKBs = model.ToList();
            ViewBag.KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO).GiaTri.ToIntOrDefault(0);
            return View(model.Select(tkb => tkb.SoGhiBais.ToList()).ToList().Merge());
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Thread.CurrentThread.CurrentCulture = new CultureInfo("vi-VN");
        }

        private void Validate(SoGhiBai model)
        {
            if (model.TongSoSV <= 0)
                ModelState.AddModelError("TongSoSV", "TongSoSV < 0");
            if (model.ThoiGianKT <= model.ThoiGianBD)
                ModelState.AddModelError("ThoiGianKT", "ThoiGianKT < ThoiGianBD");
            if (model.Loai != 1 && model.NgayDay > DateTime.Today)
                ModelState.AddModelError("NgayDay", "NgayDay > NgayHomNay");
            var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO).GiaTri.ToIntOrDefault(0);
            if ((DateTime.Today - model.NgayDay).Days > KhoaSo)
                ModelState.AddModelError("NgayDay", "NgayHomNay - NgayDay > " + KhoaSo);
        }

        [TKBAuthorization]
        public ActionResult Create(int id, int? back)
        {
            var tkb = db.ThoiKhoaBieux.Find(id);
            var model = new SoGhiBai
            {
                NgayDay = DateTime.Today,
                ThoiGianBD = CONST.TIET[tkb.TietBD],
                ThoiGianKT = CONST.TIET[tkb.TietBD + tkb.SoTiet - 1].Add(new TimeSpan(0, 45, 0)),
                SoTietDay = tkb.SoTiet,
                MaPhong = tkb.MaPH,
                TongSoSV = tkb.TongSoSV,
                Email = User.Identity.GetUserName(),
                idTKB = tkb.id
            };
            ViewBag.NhanXets = db.NhanXets.ToList();
            ViewBag.Back = back;
            return View(model);
        }

        [HttpPost]
        [TKBAuthorization]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SoGhiBai model, int? back)
        {
            Validate(model);
            if (ModelState.IsValid)
                try
                {
                    model.NgayTao = DateTime.Now;
                    model.NgaySua = DateTime.Now;
                    model.Email = User.Identity.GetUserName();
                    db.SoGhiBais.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Index", new { id = back ?? model.idTKB });
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.GetBaseException().Message);
                }
            ViewBag.NhanXets = db.NhanXets.ToList();
            ViewBag.Back = back;
            return View(model);
        }

        [SDBAuthorization]
        public ActionResult Edit(int id, int? back)
        {
            var model = db.SoGhiBais.Find(id);
            if (model == null)
                return HttpNotFound();
            ViewBag.NhanXets = db.NhanXets.ToList();
            ViewBag.Back = back;
            return View("Update", model);
        }

        [HttpPost]
        [SDBAuthorization]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SoGhiBai model, int? back)
        {
            Validate(model);
            if (ModelState.IsValid)
            {
                model.NgayTao = db.SoGhiBais.AsNoTracking().Single(sdb => sdb.id == model.id).NgayTao;
                model.NgaySua = DateTime.Now;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = model.idTKB });
            }
            ViewBag.NhanXets = db.NhanXets.ToList();
            ViewBag.Back = back;
            return View("Update", model);
        }

        [SDBAuthorization]
        public ActionResult Delete(int id, int? back)
        {
            var model = db.SoGhiBais.Find(id);
            if (model == null)
                return HttpNotFound();
            ViewBag.Back = back;
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SDBAuthorization]
        public ActionResult DeleteConfirmed(int id, int? back)
        {
            var KhoaSo = db.CauHinhs.Find(CONFIG.KHOA_SO).GiaTri.ToIntOrDefault(0);
            var model = db.SoGhiBais.Find(id);
            if ((DateTime.Today - model.NgayDay).Days < KhoaSo)
            {
                db.SoGhiBais.Remove(model);
                db.SaveChanges();
            }
            return RedirectToAction("Index", new { id = back ?? model.idTKB });
        }

        public static int countSoBuoiDay(List<ThoiKhoaBieu> TKB)
        {
            var SDB = TKB.Merge(tkb => tkb.SoGhiBais);
            if (SDB.Count() > 0)
            {
                /*
                var buoiDay = 0;
                var buoiDau = SDB.Min(sdb => sdb.NgayDay);
                var buoiSau = SDB.Max(sdb => sdb.NgayDay);
                if (buoiSau.Subtract(buoiDau).TotalDays < 15 * 7)
                    buoiSau = buoiDau.AddDays(15 * 7);
                if (buoiSau > DateTime.Today) buoiSau = DateTime.Today;

                var tempDau = buoiDau;
                foreach (var tkb in TKB)
                {
                    tempDau = buoiDau.AddDays(-(int)buoiDau.DayOfWeek);
                    tempDau = tempDau.AddDays(CONST.DAY[tkb.ThuKieuSo]);
                    while (tempDau <= buoiSau)
                    {
                        buoiDay++;
                        tempDau = tempDau.AddDays(7);
                    }
                }
                return buoiDay;
                */
            }
            return TKB.Sum(tkb => tkb.TuanKT - tkb.TuanBD + 1);
        }

        public ActionResult ThongKe(int id)
        {
            return View((Index(id) as ViewResult).Model);
        }

        [HttpPost]
        [Authorize(Roles = "DaoTao,GiaoVu")]
        public bool XemDeXuat(int id, bool xem)
        {
            var model = db.SoGhiBais.Find(id);
            if (model != null)
            {
                model.XemDeXuat = xem ? model.XemDeXuat = User.Identity.GetUserName() : null;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            return false;
        }


        public ActionResult Export(int id)
        {
            return View((Index(id) as ViewResult).Model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public EmptyResult Export(string content, string name = "SDB")
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=" + name + ".doc");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-word";
            Response.Output.Write(content);
            Response.Flush();
            Response.End();
            return new EmptyResult();
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
