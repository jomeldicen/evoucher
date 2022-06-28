using WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        public bool isAllowedBySettings(string Id)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                return Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == Id).FirstOrDefault().vSettingOption);
            }
        }

        /**** Start Admin Accounts ****/
        public ActionResult Register()
        {
            if (isAllowedBySettings("82A52FA2-E91F-4195-84FD-8EA32DA2637A"))
                return View();
            else
                return Redirect("~/Auth/Login");
        } 

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult RecoverPassword()
        {
            return View();
        }

        public ActionResult CreateNewPassword()
        {
            return View();
        }

        public ActionResult RegistrationSuccessful()
        {
            return View();
        }
        /**** End Admin Accounts  ****/

        public ActionResult BadRequest()
        {
            return View();
        }
    }
}
