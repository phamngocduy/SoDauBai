using System;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Transactions;
using Microsoft.AspNet.Identity;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class TroGiangController : Controller
    {
        SoDauBaiEntities db = new SoDauBaiEntities();

        public JsonResult Index(int id)
        {
            return Json(db.TroGiangs.Where(tg => tg.idTKB == id).Select(tg => tg.Email).ToArray(),
                JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [TKBAuthorization]
        public string Insert(int id, string email)
        {
            try
            {
                db.TroGiangs.Add(new TroGiang
                {
                    idTKB = id,
                    Email = email
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
        public string Remove(int id, string email)
        {
            try
            {
                foreach (var item in db.TroGiangs.Where(tg => tg.idTKB == id && tg.Email == email))
                    db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return String.Empty;
            }
            catch (Exception e)
            {
                return e.GetBaseException().Message;
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}