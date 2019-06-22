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

        [Authorize]
        public ActionResult Index(int? id)
        {
            id = id.HasValue ? id : db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy);
            var email = User.Identity.GetUserName();
            var MaMH_NhomTo = (from tkb in db.ThoiKhoaBieux
                            join gv in db.GiangViens on tkb.MaGV equals gv.MaGV
                            where gv.Email == email && tkb.HocKy == id
                            select tkb.MaMH + "_" + tkb.NhomTo).ToList();
            var model = db.ThoiKhoaBieux.Where(tkb => MaMH_NhomTo.Contains(tkb.MaMH + "_" + tkb.NhomTo) && tkb.HocKy == id);
            ViewBag.HocKy = new SelectList(db.ThoiKhoaBieux.Select(tkb => tkb.HocKy).Distinct().OrderByDescending(hk => hk), (byte)id.Value);
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