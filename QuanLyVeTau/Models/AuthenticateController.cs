using System.Web.Mvc;

namespace QuanLyVeTau.Models
{
    public class AuthenticateController : Controller
    {
        public ActionResult AccessDenied()
        {
            return View();
        }
    }
}
