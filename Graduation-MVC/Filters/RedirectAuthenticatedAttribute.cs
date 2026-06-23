using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Graduation_MVC.Filters
{
    /// <summary>
    /// Action filter that redirects authenticated users to the admin dashboard.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RedirectAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var user = context.HttpContext.User;
            if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
            {
                // Redirect authenticated users to the Admin Dashboard
                context.Result = new RedirectToActionResult("Index", "Dashboard", new { area = "Admin" });
            }

            base.OnActionExecuting(context);
        }
    }
}
