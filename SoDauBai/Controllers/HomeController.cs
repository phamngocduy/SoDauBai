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
        public ActionResult Index()
        {
            var hk = db.ThoiKhoaBieux.Max(tkb => tkb.HocKy);
            var email = User.Identity.GetUserName();
            var model = from tkb in db.ThoiKhoaBieux
                        join gv in db.GiangViens on tkb.MaGV equals gv.MaGV
                        where gv.Email == email && tkb.HocKy == hk select tkb;
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