using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class PhuGiangController : Controller
    {
        public JsonResult Search(string term)
        {
            using (var db = new SoDauBaiEntities())
            {
                term = term.ToLower().Replace(" ", "");
                return Json(db.GiangViens.Select(gv => new { gv.MaGV, maGV = gv.MaGV.ToLower(), gv.HoTen, hoTen = gv.HoTen.ToLower() }).ToArray()
                    .Where(gv => gv.maGV.Replace(" ", "").Contains(term) || gv.hoTen.Replace(" ", "").Contains(term)).ToArray(),
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Index(int id)
        {
            using (var db = new SoDauBaiEntities())
            {
                return Json((from pg in db.PhuGiangs join gv in db.GiangViens on pg.MaGV equals gv.MaGV
                             where pg.idTKB == id select new { pg.MaGV, gv.HoTen }).ToArray(),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [TKBAuthorization]
        public string Create(int id, string maGV)
        {
            using (var db = new SoDauBaiEntities())
            try
            {
                db.PhuGiangs.Add(new PhuGiang
                {
                    idTKB = id,
                    MaGV = maGV
                });
                db.SaveChanges();
                return String.Empty;
            }
            catch (Exception e)
            {
                return e.GetBaseException().Message;
            }
        }

        [HttpPost]
        [TKBAuthorization]
        public string Delete(int id, string maGV)
        {
            using (var db = new SoDauBaiEntities())
            try
            {
                foreach (var item in db.PhuGiangs.Where(pg => pg.idTKB == id && pg.MaGV == maGV))
                    db.Entry(item).State = EntityState.Deleted;
                db.SaveChanges();
                return String.Empty;
            }
            catch (Exception e)
            {
                return e.GetBaseException().Message;
            }
        }
    }
}