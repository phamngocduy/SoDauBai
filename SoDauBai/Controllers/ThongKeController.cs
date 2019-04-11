﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao")]
    public class ThongKeController : Controller
    {
        SoDauBaiEntities db = new SoDauBaiEntities();

        public ActionResult LopDangDay()
        {
            var hk = db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy);
            var model = db.ThoiKhoaBieux.Include(tkb => tkb.SoGhiBais).Where(tkb => tkb.HocKy == hk);
            ViewBag.GiangViens = db.GiangViens.ToList();
            var now = DateTime.Now;
            var thu = CONST.THU[(int)now.DayOfWeek];
            model = model.Where(tkb => tkb.ThuKieuSo == thu);
            return View(model.ToList().Where(tkb => CONST.TIET[tkb.TietBD] <= now.TimeOfDay &&
                        now.TimeOfDay <= CONST.TIET[tkb.TietBD + tkb.SoTiet - 1].Add(CONST.TIET[0])));
        }

        public ActionResult ThongKeChung()
        {
            var hk = db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy);
            var model = db.ThoiKhoaBieux.Include(tkb => tkb.SoGhiBais).Where(tkb => tkb.HocKy == hk);
            ViewBag.GiangViens = db.GiangViens.ToList();
            return View(model.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}