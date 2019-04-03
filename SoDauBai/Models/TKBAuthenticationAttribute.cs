using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Principal;

namespace SoDauBai.Models
{
    public class TKBAuthenticationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var id = filterContext.RouteData.Values["id"].ToString().ToIntOrDefault(0);
            if (filterContext.HttpContext.User.IsInTKB(id) == false)
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }

    public static class TKBAuthenticationExtension
    {
        public static bool IsInTKB(this IPrincipal user, int id)
        {
            if (!user.Identity.IsAuthenticated)
                return false;
            using (var db = new SoDauBaiEntities())
            {
                var model = db.ThoiKhoaBieux.Find(id);
                if (model == null) return false;
                return db.GiangViens.SingleOrDefault(gv => gv.MaGV == model.MaGV).Init().Email == user.Identity.GetUserName();
            }
        }
    }
}