using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Collections;
using System.Collections.Generic;
using WebApp.Helper;
using Newtonsoft.Json;
using MimeKit;
using QRCoder;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Mime;
using Microsoft.Reporting.WebForms;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Net.Mail;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/Voucher")]
    public class VoucherController : ApiController
    {
        private string PageUrl = "/Admin/Voucher";
        private string ApiName = "Voucher";

        public string Domain = "";
        private decimal AmountThreshold = 0;
        private string SecretKey = "";
        private string SMTPHost = "";
        private string MktgEmail = "";
        private string ITGEmail = "";
        private ImageHelper ImgHelper = new ImageHelper();
        public Dictionary<string, Func<decimal, decimal, decimal, bool>> operators = new Dictionary<string, Func<decimal, decimal, decimal, bool>>();

        public VoucherController()
        {
            operators.Add("equal", (a, b, c) => a == b);
            operators.Add("greater than equal", (a, b, c) => a >= b);
            operators.Add("less than equal", (a, b, c) => a <= b);
            operators.Add("greater than", (a, b, c) => a > b);
            operators.Add("less than", (a, b, c) => a < b);
            operators.Add("not equal", (a, b, c) => a != b);
            operators.Add("between", (a, b, c) => c >= a && c <= b);

            using (WebAppEntities db = new WebAppEntities()) 
            {
                this.Domain = db.Settings.Where(x => x.vSettingID == "D000434T-8C18-4W37-N868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // SMTP Host
                this.AmountThreshold = Convert.ToDecimal(db.Settings.Where(x => x.vSettingID == "B16D224B-1C28-4A37-B767-B15C089JOMEL").FirstOrDefault().vSettingOption); // Amount indicator when to change template
                this.SecretKey = db.Settings.Where(x => x.vSettingID == "D000412X-8238-5WZ7-B8NN-D1CEN89J0MEL").FirstOrDefault().vSettingOption; // Secret Key to decrypt data
                this.SMTPHost = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // SMTP Host
                this.MktgEmail = db.Settings.Where(x => x.vSettingID == "MKG0W34R-2323-XR22-K823-BKLS89JOMEL").FirstOrDefault().vSettingOption; // Email where to send generated voucher
                this.ITGEmail = db.Settings.Where(x => x.vSettingID == "ITG1X44R-4411-XR22-VXFG-BKLS19JOMEL").FirstOrDefault().vSettingOption; // Email where to send voucher with discrepancy
            }
        }

        private CustomControl GetPermissionControl(string PageUrl)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.PageUrl = PageUrl;
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        [Route("GetVoucher")]
        public async Task<IHttpActionResult> GetVoucher([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    int value1 = Convert.ToInt16(param.value1);

                    var template = db.Templates.Where(x => x.Published == true).Select(x => new { Id = x.Id.ToString(), x.Name }).OrderBy(x => x.Name).ToList();
                    if(template == null)
                        return BadRequest("Template maintenance is not properly set.");

                    IEnumerable<CustomVoucher> source = null;
                    source = await (from vc in db.Vouchers
                                    select new CustomVoucher
                                    {
                                        Id = vc.Id,
                                        MerchantID = vc.MerchantID,
                                        MerchantName = vc.Merchant.Name,
                                        PropertyName = vc.TemplateCondition.Template.Property.Name,
                                        PropertyLocation = vc.TemplateCondition.Template.Property.Location,
                                        TemplateID = vc.TemplateCondition.TemplateID,
                                        TemplateName = vc.TemplateCondition.Template.Name,
                                        TempCondID = vc.TempCondID,
                                        UniqueID = vc.UniqueID,
                                        Recipient = vc.Recipient,
                                        EventName = vc.EventName,
                                        VoucherCode = vc.VoucherCode,
                                        Amount = vc.Amount,
                                        WithDiscrepancy = vc.WithDiscrepancy.ToString(),
                                        VoucherStatus = vc.VoucherStatus,
                                        Remarks = vc.Remarks,
                                        UrlLink = vc.UrlLink,
                                        isChecked = false,
                                        ModifiedByPK = vc.ModifiedByPK,
                                        ModifiedDate = vc.ModifiedDate,
                                        CreatedByPK = vc.CreatedByPK,
                                        CreatedDate = vc.CreatedDate
                                    }).ToListAsync();

                    if (value1 != 0)
                        source = source.Where(x => x.TemplateID == value1).ToList();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Recipient.ToLower().Contains(param.search) || x.VoucherCode.ToLower().Contains(param.search) || x.EventName.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomVoucher).GetProperty(param.sortby);
                    switch (param.reverse)
                    {
                        case true:
                            source = source.OrderByDescending(s => sortby.GetValue(s, null));
                            break;
                        case false:
                            source = source.OrderBy(s => sortby.GetValue(s, null));
                            break;
                    }

                    // paging
                    var sourcePaged = source.Skip((param.page - 1) * param.itemsPerPage).Take(param.itemsPerPage);

                    var data = new { COUNT = source.Count(), VOUCHERLIST = sourcePaged, CONTROLS = permissionCtrl, TEMPLATELIST = template };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomVoucher data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.Vouchers.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.EventName, x.VoucherCode, x.Amount, x.VoucherBase64, WithDiscrepancy = x.WithDiscrepancy.ToString() }).ToList();

                        string htmlbody = string.Empty;
                        string listVoucher = string.Empty;
                        using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Views/Htm/discrepancy.htm")))
                        {
                            htmlbody = reader.ReadToEnd();
                        }

                        bool mailNotif = false;

                        foreach (var ds in data.dsList)
                        {
                            var voucher = db.Vouchers.Where(x => x.Id == ds.Id).SingleOrDefault();
                            if(voucher != null)
                            {
                                if(voucher.Id == ds.Id && voucher.WithDiscrepancy == false)
                                {
                                    mailNotif = true;
                                    listVoucher += "<li>Voucher Code: <strong>" + voucher.VoucherCode + "</strong> with amount of: <strong>Php " + voucher.Amount + "</strong></li>";
                                }
                                else
                                {
                                    continue;
                                }
                            }                                
                          
                            var sql = "Update Voucher SET WithDiscrepancy = {1}, Remarks = {2}, ModifiedByPK = {3}, ModifiedDate = {4} WHERE Id = {0} AND VoucherStatus = 'For Booking Registration'";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.WithDiscrepancy, data.Remarks, User.Identity.GetUserId(), DateTime.Now);
                        }

                        dbContextTransaction.Commit();
                        
                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = (data.WithDiscrepancy == "True") ? "Validate list of " + this.ApiName : "Validate list of " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //
                        
                        // ---------------- Send Email ------------------ //
                        if(mailNotif)
                        {
                            EmailSender sendmail = new EmailSender();
                            sendmail.MailSubject = "Gift Voucher with Discrepancy";
                            sendmail.ToEmail = this.ITGEmail;

                            htmlbody = htmlbody.Replace("{Remarks}", data.Remarks);
                            htmlbody = htmlbody.Replace("{Vouchers}", listVoucher);
                            sendmail.ComposeMessage(htmlbody);
                        }
                        // ---------------- Send Email ------------------ //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("SaveVoucher")]
        public async Task<IHttpActionResult> SaveVoucher(CustomVoucher data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();
                        string stampid = Guid.NewGuid().ToString().Replace("-", "");

                        // Check Voucher if exist
                        bool isVoucherExists = db.Vouchers.Where(x => x.Id != data.Id && x.VoucherCode == data.VoucherCode).Any();
                        if (isVoucherExists)
                            return BadRequest("Exists");

                        // Check Recipient Name Length
                        if (data.Recipient.Length > 45)
                            return BadRequest("Name exceed maximumn length of 45 characters");

                        // Get Template Conditions based on selection
                        var tempCond = db.TemplateConditions.Where(x => x.TemplateID == data.TemplateID).ToList();
                        if (tempCond.Count == 0)
                            return BadRequest("Template Maintenance is not properly set");

                        // Check if parameter met condition
                        bool isValid = false;
                        TemplateCondition tmpcond = new TemplateCondition();
                        foreach (var cond in tempCond)
                        {
                            var oprtr = operators[cond.Condition];
                            if (cond.Condition == "between")
                            {
                                if (oprtr(cond.Amount, cond.Amount2, data.Amount))
                                {
                                    tmpcond = cond;
                                    isValid = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (oprtr(data.Amount, cond.Amount, 0))
                                {
                                    tmpcond = cond;
                                    isValid = true;
                                    break;
                                }
                            }
                        }
                        
                        // Condition
                        if (isValid)
                        {
                            // Default Datasets for Report
                            IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.Id == 0).Select(x => new CustomVoucher { VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();

                            Voucher vc = new Voucher();

                            vc.Id = data.Id;
                            vc.GenerationID = stampid;
                            vc.UniqueID = (vc.Id == 0) ? Guid.NewGuid().ToString().Replace("-", "") : data.UniqueID;
                            vc.MerchantID = data.MerchantID;
                            vc.TempCondID = tmpcond.Id;
                            vc.EventName = data.EventName;
                            vc.Recipient = data.Recipient;
                            vc.VoucherCode = data.VoucherCode;
                            vc.Amount = data.Amount;
                            vc.UrlLink = string.Concat(this.Domain, "/home/index/", vc.UniqueID);
                            vc.VoucherBase64 = this.GenerateVoucher(data.TemplateID, vc.Recipient, vc.VoucherCode, vc.Amount, tmpcond, ImgHelper.GenerateQRCode(vc.UrlLink), ds);
                            vc.VoucherStatus = (vc.Id == 0) ? "For Booking Registration" : data.VoucherStatus;
                            vc.WithDiscrepancy = false;
                            vc.Remarks = data.Remarks;
                            vc.ModifiedByPK = cId;
                            vc.ModifiedDate = DateTime.Now;
                            if (vc.Id == 0)
                            {
                                nwe = true;
                                vc.CreatedByPK = cId;
                                vc.CreatedDate = DateTime.Now;
                                db.Vouchers.Add(vc);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                vc.CreatedByPK = data.CreatedByPK;
                                vc.CreatedDate = data.CreatedDate;
                                db.Entry(vc).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                            }

                            dbContextTransaction.Commit();

                            // ---------------- Start Send Email ------------------ //
                            SendEmail(stampid);
                            // ---------------- End Send Email ------------------ //

                            // ---------------- Start Transaction Activity Logs ------------------ //
                            AuditTrail log = new AuditTrail();
                            log.EventType = (nwe) ? "CREATE" : "UPDATE";
                            log.Description = (nwe) ? "Create LOV " + this.ApiName : "Update LOV " + this.ApiName;
                            log.PageUrl = this.PageUrl;
                            log.ObjectType = this.GetType().Name;
                            log.EventName = this.ApiName;
                            log.ContentDetail = JsonConvert.SerializeObject(data);
                            log.SaveTransactionLogs();
                            // ---------------- End Transaction Activity Logs -------------------- //

                            return Ok();
                        }
                        else
                            return BadRequest("Amount is not valid");
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("UploadVoucher")]
        public async Task<IHttpActionResult> UploadVoucher(List<CustomVoucher> data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        int count = 0;
                        int count2 = 0;
                        var cId = User.Identity.GetUserId();
                        string stampid = Guid.NewGuid().ToString().Replace("-", "");

                        if (data == null)
                            return BadRequest("Excel doesn't have data");
                        
                        if(data.Count > 10)
                            return BadRequest("Excel file exceed the maximum rows");

                        // Get Template Conditions based on selection
                        int tempid = data.FirstOrDefault().TemplateID;
                        var tempCond = db.TemplateConditions.Where(x => x.TemplateID == tempid).ToList();
                        if (tempCond.Count == 0)
                            return BadRequest("Template Maintenance is not properly set");

                        // Default Datasets for Report
                        IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.Id == 0).Select(x => new CustomVoucher { Recipient = x.Recipient, VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();

                        string invalidVoucher = "";
                        foreach (var res in data)
                        {
                            // Check Voucher if exist
                            bool isVoucherExists = db.Vouchers.Where(x => x.Id != res.Id && x.VoucherCode == res.VoucherCode).Any();
                            if (isVoucherExists)
                            {
                                invalidVoucher += res.VoucherCode + ",";
                                count2++;
                                continue;
                            }

                            // Check Recipient Name Length
                            if(res.Recipient.Length > 45)
                            {
                                invalidVoucher += res.VoucherCode + ",";
                                count2++;
                                continue;
                            }

                            // Check if parameter met condition
                            bool isValid = false;
                            TemplateCondition tmpcond = new TemplateCondition();
                            foreach (var cond in tempCond)
                            {
                                var oprtr = operators[cond.Condition];
                                if (cond.Condition == "between")
                                {
                                    if (oprtr(cond.Amount, cond.Amount2, res.Amount))
                                    {
                                        tmpcond = cond;
                                        isValid = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (oprtr(res.Amount, cond.Amount, 0))
                                    {
                                        tmpcond = cond;
                                        isValid = true;
                                        break;
                                    }
                                }
                            }

                            if (isValid)
                            {
                                Voucher vc = new Voucher();

                                vc.Id = res.Id;
                                vc.GenerationID = stampid;
                                vc.UniqueID = Guid.NewGuid().ToString().Replace("-", "");
                                vc.TempCondID = tmpcond.Id;
                                vc.Recipient = res.Recipient;
                                vc.EventName = res.EventName;
                                vc.VoucherCode = res.VoucherCode;
                                vc.Amount = res.Amount;
                                vc.UrlLink = string.Concat(this.Domain, "/home/index/", vc.UniqueID);
                                vc.VoucherBase64 = this.GenerateVoucher(tempid, vc.Recipient, vc.VoucherCode, vc.Amount, tmpcond, ImgHelper.GenerateQRCode(vc.UrlLink), ds);
                                vc.VoucherStatus = "For Booking Registration";
                                vc.WithDiscrepancy = false;
                                vc.Remarks = "";
                                vc.ModifiedByPK = cId;
                                vc.ModifiedDate = DateTime.Now;
                                vc.CreatedByPK = cId;
                                vc.CreatedDate = DateTime.Now;
                                db.Vouchers.Add(vc);
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                invalidVoucher += res.VoucherCode + ",";
                                count2++;
                                continue;
                            }
                            count++;
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Send Email ------------------ //
                        if (count != 0)
                            SendEmail2(stampid);
                        // ---------------- End Send Email ------------------ //

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create LOV " + this.ApiName : "Update LOV " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(data);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        //if(count != data.Count)
                        //    return BadRequest(count2 + " out of " + data.Count + " Voucher Code already exist");                       

                        string messageNotif = count + " out of " + data.Count + " record(s) was successfuly uploaded.";

                        if (count2 > 0)
                        {
                            invalidVoucher = invalidVoucher.Remove(invalidVoucher.Length - 1, 1);
                            messageNotif += " " + count2 + " items(s) found invalid (" + invalidVoucher + ")";
                        }

                        return Ok(messageNotif);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        // Voucher Generation through the use of RDLC and convert Base64 string
        public string GenerateVoucher(int templateID, string recipient, string VoucherCode, decimal Amount, TemplateCondition tmpcond, string qrcode, IEnumerable<CustomVoucher> ds)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var templateInfo = db.Templates.Where(x => x.Published == true && x.Id == templateID).FirstOrDefault();
                    if (templateInfo == null)
                        return "";

                    string deviceInfo =
                                   "<DeviceInfo>" +
                                   "  <OutputFormat>JPEG</OutputFormat>" +
                                   "  <PageWidth>10in</PageWidth>" +
                                   "  <PageHeight>7.33in</PageHeight>" +
                                   "  <MarginTop>0in</MarginTop>" +
                                   "  <MarginLeft>0in</MarginLeft>" +
                                   "  <MarginRight>0in</MarginRight>" +
                                   "  <MarginBottom>0in</MarginBottom>" +
                                   "  <DpiX>300</DpiX>" +
                                   "  <DpiY>300</DpiY>" +
                                   "</DeviceInfo>";

                    // Locate Property Logo
                    // string propertyLogo = !String.IsNullOrEmpty(templateInfo.Property.PropertyImage)? ImgHelper.GenerateBase64Str(templateInfo.Property.PropertyImage.Trim()) : "";
                    string propertyLogo = "";

                    // object is used to load and export a report
                    LocalReport localReport = new LocalReport();

                    string formattedAmt = Convert.ToDecimal(Amount).ToString("#,##0.00");
                    string guidelines = string.IsNullOrWhiteSpace(tmpcond.Guidelines) ? "" : tmpcond.Guidelines;
                    guidelines = guidelines.Replace("{{Amount}}", formattedAmt);

                    // Parameter Initializing
                    ReportParameter[] parameters = new ReportParameter[7];
                    parameters[0] = new ReportParameter("VoucherCode", VoucherCode);
                    parameters[1] = new ReportParameter("Amount", formattedAmt);
                    parameters[2] = new ReportParameter("QRCode", qrcode, true);
                    parameters[3] = new ReportParameter("Recipient", recipient, true);
                    parameters[4] = new ReportParameter("Guidelines", guidelines);
                    parameters[5] = new ReportParameter("PropertyName", templateInfo.Property.Name);
                    parameters[6] = new ReportParameter("PropertyLogo", propertyLogo);

                    localReport.ReportPath = @"Reports/Rdlc/templates/" + tmpcond.TemplatePath;
                    localReport.DataSources.Clear();
                    localReport.DataSources.Add(new ReportDataSource("DSVouchers", ds));
                    localReport.SetParameters(parameters);

                    string reportType = "Image";
                    string mimeType;
                    string encoding;
                    string fileNameExtension;
                    Warning[] warnings;
                    string[] streams;
                    byte[] renderedBytes;

                    // Render the report to bytes
                    renderedBytes = localReport.Render(reportType, deviceInfo, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                    localReport.Dispose();
                    // Convert bytes to base64string and return value
                    return Convert.ToBase64String(renderedBytes);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        private void SendEmail(string stampid)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    // Default Datasets for Report
                    IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.GenerationID == stampid && x.WithDiscrepancy == false).Select(x => new CustomVoucher { Recipient = x.Recipient, VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();
                    // object is used to load and export a report
                    LocalReport localReport = new LocalReport();

                    // Validation on voucher.  x value greater that amount use voucher design 2 else voucher design 1
                    localReport.ReportPath = @"Reports/Rdlc/VoucherList.rdlc";
                    localReport.DataSources.Clear();
                    localReport.DataSources.Add(new ReportDataSource("DSVouchers", ds));

                    string reportType = "WORD";
                    string fileName = "VoucherMasterList.doc";
                    string mimeType;
                    string encoding;
                    string fileNameExtension;
                    Warning[] warnings;
                    string[] streams;
                    byte[] renderedBytes;

                    // Render the report to bytes
                    renderedBytes = localReport.Render(reportType, null, out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
                    localReport.Dispose();

                    var cid = User.Identity.GetUserId();
                    var email = db.AspNetUsers.Where(x => x.Id == cid).Select(x => x.Email).SingleOrDefault();

                    // ---------------- Send Email ------------------ //
                    EmailSender sendmail = new EmailSender();
                    sendmail.MailSubject = "Gift Voucher(s) Master File";
                    sendmail.ToEmail = email;
                    string htmlbody = string.Empty;
                    string listVoucher = string.Empty;
                    using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Views/Htm/voucherlist.htm")))
                    {
                        htmlbody = reader.ReadToEnd();
                    }

                    AlternateView plainView = AlternateView.CreateAlternateViewFromString("Please view as HTML-Mail.", System.Text.Encoding.UTF8, "text/plain");
                    plainView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlbody, System.Text.Encoding.UTF8, "text/html");
                    htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

                    System.IO.MemoryStream m = new System.IO.MemoryStream(renderedBytes);
                    Attachment attachment = new Attachment(m, fileName);

                    sendmail.CustomMessage(htmlbody, plainView, htmlView, attachment);
                }
            }            
        }

        private void SendEmail2(string stampid)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    // Default Datasets for Report
                    IEnumerable<CustomVoucher> vouchers = db.Vouchers.Where(x => x.GenerationID == stampid && x.WithDiscrepancy == false).Select(x => new CustomVoucher { Recipient = x.Recipient, VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();
                    if(vouchers.Count() > 0)
                    {
                        
                        var cid = User.Identity.GetUserId();
                        var email = db.AspNetUsers.Where(x => x.Id == cid).Select(x => x.Email).SingleOrDefault();

                        // ---------------- Send Email ------------------ //
                        EmailSender sendmail = new EmailSender();
                        sendmail.MailSubject = "Gift Voucher(s) Master File";
                        sendmail.ToEmail = email;
                        string htmlbody = string.Empty;
                        string listVoucher = string.Empty;
                        using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Views/Htm/voucherlist.htm")))
                        {
                            htmlbody = reader.ReadToEnd();
                        }

                        AlternateView plainView = AlternateView.CreateAlternateViewFromString("Please view as HTML-Mail.", System.Text.Encoding.UTF8, "text/plain");
                        plainView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

                        AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlbody, System.Text.Encoding.UTF8, "text/html");
                        htmlView.TransferEncoding = System.Net.Mime.TransferEncoding.QuotedPrintable;

                      
                        sendmail.CustomMessage2(htmlbody, plainView, htmlView, vouchers);
                                             
                    }                    
                }
            }
        }

        [Route("RemoveData")]
        public IHttpActionResult RemoveData(int ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.Vouchers.Where(x => x.Id == ID).Select(x => new { x.Id, x.EventName, x.VoucherCode, x.Amount, x.VoucherBase64, WithDiscrepancy = x.WithDiscrepancy.ToString() }).SingleOrDefault();

                        db.Vouchers.RemoveRange(db.Vouchers.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete single " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("RemoveRecords")]
        public async Task<IHttpActionResult> RemoveRecords(CustomVoucher data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = db.Vouchers.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.EventName, x.VoucherCode, x.VoucherBase64, x.Amount, WithDiscrepancy = x.WithDiscrepancy.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM Voucher WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete list of " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }
    }
}