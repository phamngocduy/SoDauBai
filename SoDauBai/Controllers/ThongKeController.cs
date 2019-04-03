using System;
using System.Collections.Generic;
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
            var hk = db.ThoiKhoaBieux.Select(tkb => tkb.HocKy).Max();
            var model = db.ThoiKhoaBieux.Include("SoGhiBais").Where(tkb => tkb.HocKy == hk);
            ViewBag.GiangViens = db.GiangViens.ToList();
            var now = DateTime.Now;
            var thu = CONST.THU[(int)now.DayOfWeek];
            model = model.Where(tkb => tkb.ThuKieuSo == thu);
            return View(model.ToList().Where(tkb => CONST.TIET[tkb.TietBD] <= now.TimeOfDay &&
                        now.TimeOfDay <= CONST.TIET[tkb.TietBD + tkb.SoTiet - 1]));
        }

        public ActionResult ThongKeChung()
        {
            var hk = db.ThoiKhoaBieux.Select(tkb => tkb.HocKy).Max();
            var model = db.ThoiKhoaBieux.Include("SoGhiBais").Where(tkb => tkb.HocKy == hk);
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