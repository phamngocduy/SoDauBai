using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SoDauBai.Models;

namespace SoDauBai.Controllers
{
    [Authorize]
    public class BanCanSuController : Controller
    {
        public JsonResult Search(string term)
        {
            using (var db = new SoDauBaiEntities())
            {
                return Json(db.AspNetUsers.Select(u => u.Email).ToArray().Select(s => s.ToLower())
                    .Where(e => e.EndsWith("@vanlanguni.vn") && e.Contains(term.ToLower())).ToArray(),
                    JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Index(int id)
        {
            using (var db = new SoDauBaiEntities())
            {
                return Json(db.BanCanSus.Where(bcs => bcs.idTKB == id).Select(bcs => bcs.Email).ToArray(),
                    JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [TKBAuthorization]
        public string Create(int id, string email)
        {
            using (var db = new SoDauBaiEntities())
            try
            {
                db.BanCanSus.Add(new BanCanSu
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
        public string Delete(int id, string email)
        {
            using (var db = new SoDauBaiEntities())
            try
            {
                foreach (var item in db.BanCanSus.Where(bcs => bcs.idTKB == id && bcs.Email == email))
                    db.Entry(item).State = System.Data.Entity.EntityState.Deleted;
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