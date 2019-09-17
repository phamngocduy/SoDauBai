using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Security.Principal;

namespace SoDauBai.Models
{
    public class SDBAuthorizationAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var id = filterContext.RouteData.Values["id"].ToString().ToIntOrDefault(0);
            if (filterContext.HttpContext.User.IsInSDB(id) == false)
                filterContext.Result = new HttpUnauthorizedResult();
        }
    }

    public static class SDBAuthorizationExtension
    {
        public static bool IsInSDB(this IPrincipal user, int id)
        {
            if (!user.Identity.IsAuthenticated)
                return false;
            using (var db = new SoDauBaiEntities())
            {
                var model = db.SoGhiBais.Find(id);
                if (model == null) return false;
                return model.Email.Trim().ToLower() == user.Identity.GetUserName().Trim().ToLower();
            }
        }
    }
}