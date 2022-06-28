using WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace WebApp.Controllers
{
    public class CustomAuthorize : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // If they are authorized, handle accordingly
            if (this.AuthorizeCore(filterContext.HttpContext))
            {
                base.OnAuthorization(filterContext);
            }
            else
            {
                using (WebAppEntities db = new WebAppEntities())
                {
                    string webaddress = db.Settings.Where(x => x.vSettingID == "D000234B-EF28-4S37-L868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                    // Otherwise redirect to your specific authorized area
                    filterContext.Result = new RedirectResult(webaddress);
                }
            }
        }
    }

    public class SettingActionFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                // Site Settings
                filterContext.Controller.ViewBag.PageTitle = dbcon.Settings.Where(x => x.vSettingID == "D0011XX1-2ES1-4B39-1268-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.SiteCode = dbcon.Settings.Where(x => x.vSettingID == "D0012345-3312-4B27-E268-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.MetaDescription = dbcon.Settings.Where(x => x.vSettingID == "D001534B-JG18-4B27-G868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.MetaKeyword = dbcon.Settings.Where(x => x.vSettingID == "D001634B-EW18-4B37-F868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.SiteAuthor = dbcon.Settings.Where(x => x.vSettingID == "D001831C-1C18-4D37-I868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.SiteRobots = dbcon.Settings.Where(x => x.vSettingID == "D001734B-CS18-4B37-E868-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.Company = dbcon.Settings.Where(x => x.vSettingID == "F000134R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.CorporateEmail = dbcon.Settings.Where(x => x.vSettingID == "F0006U4R-1D23-2337-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption;

                // Theme Settings
                filterContext.Controller.ViewBag.BodySmallText = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D001XX4B-02VV-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.NavbarSmallText = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D002314B-2212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.SidebarSmallText = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D003W34B-1212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.FooterSmallText = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D004430B-JJH1-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.SidebarNavFlatStyle = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D005434B-1232-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.SidebarNavLegacyStyle = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D006434B-0HH2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.SidebarNavCompact = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D007434B-0GG2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.SidebarNavChildIndent = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D008834B-T112-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";
                filterContext.Controller.ViewBag.BrandLogoSmallText = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "D009934B-02XX-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption) ? "text-sm" : "";

                filterContext.Controller.ViewBag.NavbarColorVariant = dbcon.Settings.Where(x => x.vSettingID == "D011434B-22R2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.LinkColorVariant = dbcon.Settings.Where(x => x.vSettingID == "D021434B-12H2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.DarkSidebarVariant = dbcon.Settings.Where(x => x.vSettingID == "D031434B-0GG2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.LightSidebarVariant = dbcon.Settings.Where(x => x.vSettingID == "D041434B-FF12-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
                filterContext.Controller.ViewBag.BrandLogoVariant = dbcon.Settings.Where(x => x.vSettingID == "D051434B-0312-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption;
            }
        }
    }

    public class AdminController : Controller
    {
        string webaddress = "";
        private string RedirectAction { get; set; }

        public AdminController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.webaddress = db.Settings.Where(x => x.vSettingID == "D000234B-EF28-4S37-L868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
            }
        }

        [CustomAuthorize]
        public bool isAuthorized(string action)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                var currentUserId = User.Identity.GetUserId();
                if (currentUserId == null)
                    return false;
                else
                {
                    var roleId = dbcon.AspNetUserRoles.Where(x => x.UserId == currentUserId).FirstOrDefault().RoleId;
                    var url = dbcon.AspNetRoles.Where(x => x.Id == roleId).FirstOrDefault().IndexPage;
                    RedirectAction = url.Split('/')[2];
                    action = "/Admin/" + action;
                    bool isUserAuthorized = dbcon.AspNetUsersMenuPermissions.Where(x => x.AspNetUsersMenu.nvPageUrl == action && x.Id == roleId).Any();
                    if (isUserAuthorized)
                    {
                        VisitCount();
                        return true;
                    }
                    else
                        return false;
                }
            }
        }

        public string GetModuleName()
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                try
                {
                    var action = string.Concat("/Admin/", ControllerContext.RouteData.Values["action"].ToString());
                    var menu = dbcon.AspNetUsersMenus.Where(x => x.nvPageUrl == action).FirstOrDefault();
                    if (menu != null) return menu.nvMenuName;

                    return "";
                }
                catch (Exception )
                {
                    throw;
                }
            }
        }

        [CustomAuthorize]
        public void VisitCount()
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                using (var dbContextTransaction = dbcon.Database.BeginTransaction())
                {
                    try
                    {
                        var path = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
                        string ip = "";
                        System.Web.HttpContext cont = System.Web.HttpContext.Current;
                        string ipAddress = cont.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                        if (!string.IsNullOrEmpty(ipAddress))
                        {
                            string[] addresses = ipAddress.Split(',');
                            if (addresses.Length != 0)
                            {
                                ip = addresses[0];
                            }
                        }
                        ip = cont.Request.ServerVariables["REMOTE_ADDR"];
                        AspNetUsersPageVisited anupv = new AspNetUsersPageVisited();
                        anupv.vPageVisitedID = Guid.NewGuid().ToString();
                        anupv.Id = User.Identity.GetUserId();
                        anupv.nvPageName = path;
                        anupv.dDateVisited = DateTime.UtcNow;
                        anupv.nvIPAddress = ip;
                        dbcon.AspNetUsersPageVisiteds.Add(anupv);
                        dbcon.SaveChanges();
                        dbContextTransaction.Commit();
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                    }
                }
            }
        }

        public bool isAllowedBySettings(string Id)
        {
            using (WebAppEntities dbcon = new WebAppEntities())
            {
                return Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == Id).FirstOrDefault().vSettingOption);
            }
        }

        protected string LinkRedirect(string addr, int df)
        {
            if(string.IsNullOrEmpty(addr))
            {
                if(df == 1)
                    addr = "/Auth/Login";
                else
                    addr = "/BadRequest";
            } 
            return addr;
        }

        /**** Start Dashboard ****/
        public ActionResult Index()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Index";

            if (isAuthorized("Index"))
                return View();
            else
                return Redirect(LinkRedirect(webaddress, 1));
        }
        /**** End Transactions ****/

        /**** Start User Management ****/
        public ActionResult Menu()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Menu";

            if (isAuthorized("Menu"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Users()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Users";

            if (isAuthorized("Users"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult CreateUser()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/CreateUser";

            if (isAuthorized("CreateUser"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Role()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Role";

            if (isAuthorized("Role"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult AdminCustomMenu()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/AdminCustomMenu";

            if (isAuthorized("AdminCustomMenu"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        [CustomAuthorize]
        public new ActionResult Profile()
        {
            VisitCount();
            ViewBag.PageUrl = "/Admin/Profile";

            return View();
        }
        /**** End User Management ****/

        /**** Start Logs ****/
        public ActionResult LogDashboard()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/LogDashboard";

            if (isAuthorized("LogDashboard"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult PendingEmailVerification()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/PendingEmailVerification";

            if (isAuthorized("PendingEmailVerification"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
       
        public ActionResult LoginHistory()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/LoginHistory";

            if (isAuthorized("LoginHistory"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult PageVisited()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/PageVisited";

            if (isAuthorized("PageVisited"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));

        }
        /**** End Logs ****/

        /**** Start Transaction ****/
        public ActionResult Voucher()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Voucher";

            if (isAuthorized("Voucher"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult VerificationMasterlist()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/VerificationMasterlist";

            if (isAuthorized("VerificationMasterlist"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** End Transaction ****/

        /**** Start Administration ****/
        public ActionResult Property()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Property";

            if (isAuthorized("Property"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Merchant()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Merchant";

            if (isAuthorized("Merchant"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult Template()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Template";

            if (isAuthorized("Template"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        public ActionResult TemplateCondition()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/TemplateCondition";

            if (isAuthorized("TemplateCondition"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }

        //public ActionResult Restaurant()
        //{
        //    ViewBag.Title = this.GetModuleName();
        //    ViewBag.PageUrl = "/Admin/Restaurant";

        //    if (isAuthorized("Restaurant"))
        //        return View();
        //    else
        //        return Redirect(LinkRedirect(webaddress, 1));
        //}

        public ActionResult Option()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Option";

            if (isAuthorized("Option"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** Start Administration ****/

        /**** Start Settings ****/
        public ActionResult Configuration()
        {
            ViewBag.Title = this.GetModuleName();
            ViewBag.PageUrl = "/Admin/Configuration";

            if (isAuthorized("Configuration"))
                return View();
            else
                return RedirectToAction(LinkRedirect(RedirectAction, 2));
        }
        /**** End Settings ****/

        public ActionResult BadRequest()
        {
            return Redirect("/Auth/Login");
        }

    }
}
