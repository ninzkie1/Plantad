using System.Web.Mvc;
using System.Web.Routing;

namespace MoralesFiFthCRUD
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // sa route for Admin/User
            routes.MapRoute(
                name: "AdminUser",
                url: "Admin/User",
                defaults: new { controller = "Admin", action = "User" }
            );

          
            routes.MapRoute(
                name: "AdminUserEdit",
                url: "Admin/UserEdit/{memberId}",
                defaults: new { controller = "Admin", action = "UserEdit" }
            );

            // sa Default route
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Shop", id = UrlParameter.Optional }
            );
        }
    }
}