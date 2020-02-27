using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Principal;

namespace SoDauBai.Models
{
    public class TKBAuthorizationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var id = filterContext.RouteData.Values["id"].ToString().ToIntOrDefault(0);
            if (filterContext.HttpContext.User.IsInTKB(id) == false)
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }

    public static class TKBAuthorizationExtension
    {
        public static bool IsInTKB(this IPrincipal user, int id)
        {
            if (!user.Identity.IsAuthenticated)
                return false;
            using (var db = new SoDauBaiEntities())
            {
                var model = db.ThoiKhoaBieux.Find(id);
                if (model == null) return false;
                var maGVs = model.PhuGiangs.Select(pg => pg.MaGV).Union(new string[] { model.MaGV }).ToArray();
                var email = user.Identity.GetUserName().Trim().ToLower();
                return db.GiangViens.Where(gv => maGVs.Contains(gv.MaGV))
                    .Count(gv => gv.Email.Trim().ToLower() == email) > 0;
            }
        }
    }
}