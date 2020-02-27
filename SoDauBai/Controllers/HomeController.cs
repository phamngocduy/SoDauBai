using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    public class HomeController : Controller
    {
        SoDauBaiEntities db = new SoDauBaiEntities();

        public void SetHocKy(byte hk)
        {
            Session[CONST.HocKy] = hk;
        }

        [Authorize]
        public ActionResult Index()
        {
            var hk = this.GetHocKy(db);
            var email = User.Identity.GetUserName();
            var MaMH_NhomTo = (from tkb in db.ThoiKhoaBieux join gv in db.GiangViens on tkb.MaGV equals gv.MaGV
                            where tkb.HocKy == hk && (gv.Email == email || tkb.BanCanSus.Count(bcs => bcs.Email == email) > 0)
                            select tkb) // next for PhuGiang
                            .Union(from pg in db.PhuGiangs join gv in db.GiangViens on pg.MaGV equals gv.MaGV
                                where gv.Email == email && pg.ThoiKhoaBieu.HocKy == hk select pg.ThoiKhoaBieu)
                            .Select(tkb => tkb.MaMH + "_" + tkb.NhomTo).ToList();
            var model = db.ThoiKhoaBieux.Where(tkb => MaMH_NhomTo.Contains(tkb.MaMH + "_" + tkb.NhomTo) && tkb.HocKy == hk);
            ViewBag.GVs = db.GiangViens.ToList();
            return View(model.ToList());
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