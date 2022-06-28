using WebApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Collections.Generic;
using Microsoft.Reporting.WebForms;
using System.Net.Mail;
using WebApp.Helper;
using System.IO;

namespace WebApp.Controllers
{
    public class ReportController : Controller
    {
        string webaddress = System.Web.Configuration.WebConfigurationManager.AppSettings["SiteAddress"];
        private string RedirectAction { get; set; }

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

        public ActionResult ExportVoucher(string type)
        {
            string[] arr1 = { "word", "excel" };
            if (!arr1.Contains(type))
                return Redirect("/Home/BadRequest");

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.WithDiscrepancy == false).Select(x => new CustomVoucher { VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();
                    // object is used to load and export a report
                    LocalReport localReport = new LocalReport();

                    // Validation on voucher.  x value greater that amount use voucher design 2 else voucher design 1
                    localReport.ReportPath = @"Reports/Rdlc/VoucherList.rdlc";
                    localReport.DataSources.Clear();
                    localReport.DataSources.Add(new ReportDataSource("DSVouchers", ds));

                    string reportType = (type == "word") ? "WORD" : "EXCEL";
                    string fileName = (type == "word") ? "VoucherMasterList.doc" : "VoucherMasterList.xls";
                    string mimeType;
                    string encoding;
                    string fileNameExtension;
                    Warning[] warnings;
                    string[] streams;
                    byte[] renderedBytes;

                    // Render the report to bytes
                    renderedBytes = localReport.Render(reportType, null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                    localReport.Dispose();

                    return File(renderedBytes, mimeType, fileName);
                }
            }
        }

        public ActionResult ViewVoucher(string uniqueid)
        {
            ViewBag.VoucherBase64 = "";
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    ViewBag.VoucherBase64 = db.Vouchers.Where(x => x.UniqueID == uniqueid).SingleOrDefault().VoucherBase64;
                }
            }

            return View("~/Views/Admin/VoucherImage.cshtml");
        }

        public ActionResult PreviewVoucher(string conditionid)
        {
            ViewBag.VoucherBase64 = "";
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    ViewBag.VoucherBase64 = db.TemplateConditions.Where(x => x.Id == conditionid).SingleOrDefault().VoucherBase64;
                }
            }

            return View("~/Views/Admin/VoucherImage.cshtml");
        }

        public ActionResult ExportReport()
        {
        
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {

                    string SecretKey = db.Settings.Where(x => x.vSettingID == "D000412X-8238-5WZ7-B8NN-D1CEN89J0MEL").FirstOrDefault().vSettingOption;

                    IEnumerable<CustomRecipient> ds = null;
                    ds = (from rc in db.Recipients
                          join vc in db.Vouchers on rc.UniqueID equals vc.UniqueID
                          select new CustomRecipient
                          {
                              Id = rc.Id,
                              UniqueID = rc.UniqueID,
                              Recipient = vc.Recipient,
                              VoucherCode = vc.VoucherCode,
                              Amount = vc.Amount,
                              ProcessDate = rc.ProcessDate,
                              IsEmailSent = (rc.IsEmailSent) ? "Success" : "Failed",
                              EmailSentDate = rc.EmailSentDate,
                              EmailResentDate = rc.EmailResentDate,
                              TemplateName = vc.TemplateCondition.Template.Name
                          }).AsEnumerable();

                    
                    // object is used to load and export a report
                    LocalReport localReport = new LocalReport();

                    // Validation on voucher.  x value greater that amount use voucher design 2 else voucher design 1
                    localReport.ReportPath = @"Reports/Rdlc/RecipientList.rdlc";
                    localReport.DataSources.Clear();
                    localReport.DataSources.Add(new ReportDataSource("DSRecipients", ds));

                    string reportType = "EXCEL";
                    string fileName = "RecipientMasterList.xls";
                    string mimeType;
                    string encoding;
                    string fileNameExtension;
                    Warning[] warnings;
                    string[] streams;
                    byte[] renderedBytes;

                    // Render the report to bytes
                    renderedBytes = localReport.Render(reportType, null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                    localReport.Dispose();

                    return File(renderedBytes, mimeType, fileName);
                }
            }
        }

        public ActionResult ConfirmationVoucher(string ID)
        {
            try
            {
                using (WebAppEntities db = new WebAppEntities())
                {
                    using (var dbContextTransaction = db.Database.BeginTransaction())
                    {
                        string deviceInfo =
                          "<DeviceInfo>" +
                          "  <OutputFormat>JPEG</OutputFormat>" +
                          "  <PageWidth>7in</PageWidth>" +
                          "  <PageHeight>2.6in</PageHeight>" +
                          "  <MarginTop>0in</MarginTop>" +
                          "  <MarginLeft>0in</MarginLeft>" +
                          "  <MarginRight>0in</MarginRight>" +
                          "  <MarginBottom>0in</MarginBottom>" +
                          "</DeviceInfo>";

                        IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.Id == 0).Select(x => new CustomVoucher { VoucherCode = x.VoucherCode, PropertyName = x.TemplateCondition.Template.Property.Name, MerchantName = x.Merchant.Name }).AsEnumerable();
                        var info = db.Vouchers.Where(x => x.UniqueID == ID && x.VoucherStatus == "Registered").Select(x => new { x.Recipient, x.VoucherCode, PropertyName = x.TemplateCondition.Template.Property.Name, MerchantName = x.Merchant.Name, x.Amount, x.UniqueID }).SingleOrDefault();
                        if (info != null)
                        {
                            // object is used to load and export a report
                            LocalReport localReport = new LocalReport();
                            // Parameter Initializing
                            ReportParameter[] parameters = new ReportParameter[5];
                            parameters[0] = new ReportParameter("VoucherCode", info.VoucherCode);
                            parameters[1] = new ReportParameter("Amount", Convert.ToDecimal(info.Amount).ToString("#,##0.00"));
                            parameters[2] = new ReportParameter("PropertyName", info.PropertyName);
                            parameters[3] = new ReportParameter("MerchantName", String.IsNullOrEmpty(info.MerchantName)? "" : info.MerchantName);
                            parameters[4] = new ReportParameter("Recipient", info.Recipient);

                            // Validation on voucher.  x value greater that amount use voucher design 2 else voucher design 1
                            localReport.ReportPath = @"Reports/Rdlc/Confirmation.rdlc";
                            localReport.DataSources.Clear();
                            localReport.DataSources.Add(new ReportDataSource("DSConfirmation", ds));
                            localReport.SetParameters(parameters);

                            string reportType = "Image";
                            string mimeType;
                            string encoding;
                            string fileNameExtension;
                            Warning[] warnings;

                            string[] streams;

                            byte[] renderedBytes;
                            //Render the report

                            renderedBytes = localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                            localReport.Dispose();
                            return File(renderedBytes, "image/jpeg", "e-voucherConfirmation.jpg");
                        }

                        return Redirect("/home/fdrl");
                    }
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        //public ActionResult EmailVoucher()
        //{
        //    using (WebAppEntities db = new WebAppEntities())
        //    {
        //        using (var dbContextTransaction = db.Database.BeginTransaction())
        //        {
        //            IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.WithDiscrepancy == false).Select(x => new CustomVoucher { VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();
        //            // object is used to load and export a report
        //            LocalReport localReport = new LocalReport();

        //            // Validation on voucher.  x value greater that amount use voucher design 2 else voucher design 1
        //            localReport.ReportPath = @"Reports/Rdlc/VoucherList.rdlc";
        //            localReport.DataSources.Clear();
        //            localReport.DataSources.Add(new ReportDataSource("DSVouchers", ds));

        //            string reportType = "WORD";
        //            string fileName = "VoucherMasterList.doc";
        //            string mimeType;
        //            string encoding;
        //            string fileNameExtension;
        //            Warning[] warnings;
        //            string[] streams;
        //            byte[] renderedBytes;

        //            // Render the report to bytes
        //            renderedBytes = localReport.Render(reportType, null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
        //            localReport.Dispose();


        //            // ---------------- Send Email ------------------ //
        //            EmailSender sendmail = new EmailSender();
        //            sendmail.MailSubject = "Gift Voucher(s) Master File";
        //            sendmail.ToEmail = "Jomel Dicen <jpdicen@federalland.ph>";
        //            string htmlbody = string.Empty;
        //            string listVoucher = string.Empty;
        //            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Views/Htm/voucherlist.htm")))
        //            {
        //                htmlbody = reader.ReadToEnd();
        //            }

        //            AlternateView plainView = AlternateView.CreateAlternateViewFromString("Please view as HTML-Mail.", System.Text.Encoding.UTF8, "text/plain");
        //            plainView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

        //            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlbody, System.Text.Encoding.UTF8, "text/html");
        //            htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

        //            System.IO.MemoryStream m = new System.IO.MemoryStream(renderedBytes);
        //            Attachment attachment = new Attachment(m, fileName);

        //            sendmail.CustomMessage(htmlbody, plainView, htmlView, attachment);
        //            // ---------------- Send Email ------------------ //
        //        }
        //    }

        //    return View();
        //}
    }
}
