using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    [Authorize(Roles = "DaoTao,GiaoVu")]
    public class ThongKeController : Controller
    {
        SoDauBaiEntities db = new SoDauBaiEntities();

        private IQueryable<ThoiKhoaBieu> FilterHocKy(IQueryable<ThoiKhoaBieu> TKB, int? hk = null)
        {
            TKB = TKB.Include(tkb => tkb.SoGhiBais);
            if (hk.HasValue)
                return TKB.Where(tkb => tkb.HocKy == hk);
            else
                hk = db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy);
            return TKB.Where(tkb => tkb.HocKy == hk);
        }

        public ActionResult LopDangDay()
        {
            var model = FilterHocKy(db.ThoiKhoaBieux);
            ViewBag.GiangViens = db.GiangViens.ToList();
            var now = DateTime.Now;
            var thu = CONST.THU[(int)now.DayOfWeek];
            model = model.Where(tkb => tkb.ThuKieuSo == thu);
            return View(model.ToList().Where(tkb => CONST.TIET[tkb.TietBD] <= now.TimeOfDay &&
                        now.TimeOfDay <= CONST.TIET[tkb.TietBD + tkb.SoTiet - 1].Add(CONST.TIET[0])));
        }

        private IQueryable<ThoiKhoaBieu> FilterGiaoVu(IQueryable<ThoiKhoaBieu> TKB)
        {
            if (!User.IsInRole("DaoTao") && User.IsInRole("GiaoVu"))
            {
                var email = User.Identity.GetUserName();
                var GV = db.GiaoVus.SingleOrDefault(gv => gv.Email == email);
                var maNganh = GV.Init().MaNganh.Split(',');
                TKB = TKB.Where(tkb => maNganh.Contains(tkb.MaNganh));
            }
            return TKB;
        }

        public ActionResult CacDeXuat()
        {
            var model = from tkb in FilterGiaoVu(FilterHocKy(db.ThoiKhoaBieux))
                        join sdb in db.SoGhiBais on tkb.id equals sdb.idTKB
                        where sdb.DeXuat != null && sdb.DeXuat != ""
                        select sdb;
            ViewBag.GiangViens = db.GiangViens.ToList();
            return View(model.ToList());
        }

        public ActionResult ThongKeChung()
        {
            var hk = this.GetHocKy(db);
            var model = FilterGiaoVu(FilterHocKy(db.ThoiKhoaBieux, hk));
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