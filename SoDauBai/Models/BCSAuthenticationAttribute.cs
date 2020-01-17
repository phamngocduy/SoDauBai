using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Principal;

namespace SoDauBai.Models
{
    public class BCSAuthorizationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var id = filterContext.RouteData.Values["id"].ToString().ToIntOrDefault(0);
            if (filterContext.HttpContext.User.IsInBCS(id) == false)
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }

    public static class BCSAuthorizationExtension
    {
        public static bool IsInBCS(this IPrincipal user, int id)
        {
            if (!user.Identity.IsAuthenticated)
                return false;
            using (var db = new SoDauBaiEntities())
            {
                var model = db.ThoiKhoaBieux.Find(id);
                if (model == null) return false;
                var email = user.Identity.GetUserName();
                return model.BanCanSus.Count(bcs => bcs.Email == email) > 0;
            }
        }
    }
}