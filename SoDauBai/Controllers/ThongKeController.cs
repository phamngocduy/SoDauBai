using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    public class ThongKeController : Controller
    {
        SoDauBaiEntities db = new SoDauBaiEntities();

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