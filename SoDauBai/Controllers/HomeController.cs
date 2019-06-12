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
            var model = (from tkb in db.ThoiKhoaBieux
                        join gv in db.GiangViens on tkb.MaGV equals gv.MaGV
                        where gv.Email == email && tkb.HocKy == id
                        select tkb).ToList();
            var code1 = model.Select(tkb => tkb.MaMH).Distinct().ToArray();
            var code2 = model.Select(tkb => tkb.NhomTo).Distinct().ToArray();
            model.AddRange(db.ThoiKhoaBieux.Where(tkb => code1.Contains(tkb.MaMH) && code2.Contains(tkb.NhomTo) && tkb.HocKy == id).ToList());
            ViewBag.HocKy = new SelectList(db.ThoiKhoaBieux.Select(tkb => tkb.HocKy).Distinct().OrderByDescending(hk => hk), (byte)id.Value);
            return View(model.Distinct());
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