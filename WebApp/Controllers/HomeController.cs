using WebApp.Models;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string ID)
        {
            if(String.IsNullOrEmpty(ID))
                return Redirect("/home/fdrl");

            ViewBag.CurrentYear = DateTime.Now.Year;
            ViewBag.UniqueID = ID;
            return View();
        }

        public ActionResult RegistrationSuccessful()
        {
            ViewBag.CurrentYear = DateTime.Now.Year;
            return View();
        }

        public ActionResult fdrl()
        {
            ViewBag.CurrentYear = DateTime.Now.Year;
            return View();
        }

        public ActionResult RedeemSelection()
        {
            ViewBag.CurrentYear = DateTime.Now.Year;
            return View();
        }

        public ActionResult BadRequest()
        {
            return View();
        }
    }
}
