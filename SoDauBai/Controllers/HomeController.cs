using System;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using SoDauBai.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

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
            if (Session["ExternalLoginInfo"] is ExternalLoginInfo)
            {
                var loginInfo = Session["ExternalLoginInfo"] as ExternalLoginInfo;
                if (loginInfo?.Email != email)
                {
                    var user = db.AspNetUsers.SingleOrDefault(u => u.Email == email);
                    if (user != null)
                    {
                        user.UserName = user.Email = loginInfo.Email;
                        db.Entry(user).State = EntityState.Modified;
                    }

                    var giangVien = db.GiangViens.SingleOrDefault(gv => gv.Email == email);
                    if (giangVien != null)
                    {
                        giangVien.Email = loginInfo.Email;
                        db.Entry(giangVien).State = EntityState.Modified;
                    }
                    var giaoVu = db.GiaoVus.SingleOrDefault(gv => gv.Email == email);
                    if (giaoVu != null)
                    {
                        giaoVu.Email = loginInfo.Email;
                        db.Entry(giaoVu).State = EntityState.Modified;
                    }

                    var banCanSu = db.BanCanSus.Where(bcs => bcs.Email == email);
                    banCanSu.ForEach(bcs => bcs.Email = loginInfo.Email);
                    banCanSu.ForEach(bcs => db.Entry(bcs).State = EntityState.Modified);

                    var guiEmail = db.GuiEmails.Where(gm => gm.Email == email || gm.FromTo == email);
                    guiEmail.ForEach(gm =>
                    {
                        if (gm.Email == email) gm.Email = loginInfo.Email;
                        if (gm.FromTo == email) gm.FromTo = loginInfo.Email;
                        db.Entry(gm).State = EntityState.Modified;
                    });

                    var lienHe = db.LienHes.Where(lh => lh.from == email || lh.to == email);
                    lienHe.ForEach(lh =>
                    {
                        if (lh.from == email) lh.from = loginInfo.Email;
                        if (lh.to == email) lh.to = loginInfo.Email;
                        db.Entry(lh).State = EntityState.Modified;
                    });

                    var phongDayBu = db.PhongDayBus.Where(pdb => pdb.email1 == email || pdb.email2 == email);
                    phongDayBu.ForEach(pdb =>
                    {
                        if (pdb.email1 == email) pdb.email1 = loginInfo.Email;
                        if (pdb.email2 == email) pdb.email2 = loginInfo.Email;
                        db.Entry(pdb).State = EntityState.Modified;
                    });

                    var soGhiBai = db.SoGhiBais.Where(sgb => sgb.Email == email);
                    soGhiBai.ForEach(sgb =>
                    {
                        sgb.Email = loginInfo.Email;
                        db.Entry(sgb).State = EntityState.Modified;
                    });

                    var soGhiChu = db.SoGhiChus.Where(sgc => sgc.Email == email);
                    soGhiChu.ForEach(sgc =>
                    {
                        sgc.Email = loginInfo.Email;
                        db.Entry(sgc).State = EntityState.Modified;
                    });

                    var troGiang = db.TroGiangs.Where(tg => tg.Email.Contains(email));
                    troGiang.ForEach(tg =>
                    {
                        tg.Email = tg.Email.Replace(email, loginInfo.Email);
                        db.Entry(tg).State = EntityState.Modified;
                    });

                    db.SaveChanges();
                    email = loginInfo.Email;
                    ViewBag.Message = "Successfully transfer to email: " + email;
                    HttpContext.GetOwinContext().Get<ApplicationSignInManager>()
                        .ExternalSignIn(loginInfo, isPersistent: false);
                }
            }

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