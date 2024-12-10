using QuanLyVeTau.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanLyVeTau
{
    public class CustomRoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string _role;

        public CustomRoleAuthorizeAttribute(string role)
        {
            _role = role;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var session = filterContext.HttpContext.Session; 
            var username = session["Email"] as string;
            if (string.IsNullOrEmpty(username))
            {
                filterContext.Result = new RedirectResult("/QuanTri/DangNhap");
                return; 
            }
            var roleProvider = new CustomRoleProvider();
            var roles = roleProvider.GetRolesForUser(username);

            if (_role == "*")
            {
                base.OnAuthorization(filterContext);
                return;
            }

            if (roles == null || !_role.Contains(roles[0]))
            {
                filterContext.Result = new RedirectResult("/Authenticate/AccessDenied");
            }

            base.OnAuthorization(filterContext);
        }

    }
}