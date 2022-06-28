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
using System.IO;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/VerificationMasterlist")]
    public class VerificationMasterlistController : ApiController
    {
        private string PageUrl = "/Admin/VerificationMasterlist";

        private string SecretKey = "";

        private VerificationMasterlistController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.SecretKey = db.Settings.Where(x => x.vSettingID == "D000412X-8238-5WZ7-B8NN-D1CEN89J0MEL").FirstOrDefault().vSettingOption;
            }
        }

        private CustomControl GetPermissionControl()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        [Route("GetVerificationMasterlist")]
        public async Task<IHttpActionResult> GetVerificationMasterlist([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl();

                    int value1 = Convert.ToInt16(param.value1);
                    // Get Templates
                    var template = db.Templates.Where(x => x.Published == true).Select(x => new { x.Id, x.Name }).ToList();

                    IEnumerable<CustomRecipient> source = null;
                    source = await (from rc in db.Recipients
                                    join vc in db.Vouchers on rc.UniqueID equals vc.UniqueID
                                    select new CustomRecipient
                                    {
                                        Id = rc.Id,
                                        TemplateID = vc.TemplateCondition.TemplateID,
                                        TemplateName = vc.TemplateCondition.Template.Name,
                                        PropertyName = vc.TemplateCondition.Template.Property.Name,
                                        PropertyLocation = vc.TemplateCondition.Template.Property.Location,
                                        MerchantName = vc.Merchant.Name,
                                        UniqueID = rc.UniqueID,
                                        Recipient = vc.Recipient,
                                        VoucherCode = vc.VoucherCode,
                                        Amount = vc.Amount,
                                        ProcessDate = rc.ProcessDate,
                                        IsEmailSent = (rc.IsEmailSent) ? "Success" : "Failed",
                                        EmailSentDate = rc.EmailSentDate,
                                        EmailResentDate = rc.EmailResentDate,
                                        EmailDetail = rc.EmailDetail,
                                        EmailErrorLog = rc.EmailErrorLog
                                    }).ToListAsync();

                    if (value1 != 0)
                        source = source.Where(x=> x.TemplateID == value1).ToList();
                    
                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Recipient.ToLower().Contains(param.search) || x.VoucherCode.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomRecipient).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), RECIPIENTLIST = sourcePaged, CONTROLS = permissionCtrl, TemplateLIST = template };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("ReSendEmail")]
        public async Task<IHttpActionResult> ReSendEmail(CustomRecipient data)
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
                        string ExecutiveEmail = db.Settings.Where(x => x.vSettingID == "MKG1X33R-3211-XR22-VBFG-BKLS89JOMEL").FirstOrDefault().vSettingOption;

                        var source = (from rc in db.Recipients
                                        join vc in db.Vouchers on rc.UniqueID equals vc.UniqueID
                                        where rc.Id == data.Id
                                        select new CustomRecipient
                                        {
                                            Id = rc.Id,
                                           VoucherCode = vc.VoucherCode,
                                           Amount = vc.Amount
                                        }).FirstOrDefault();

                        if (source == null)
                            return BadRequest("Not Found!");

                        EmailSender sendmail = new EmailSender();
                        sendmail.MailSubject = "Your Registration is successful";
                        sendmail.ToEmail = ExecutiveEmail.Replace("\n", "");

                        string htmlbody = string.Empty;
                        string listVoucher = string.Empty;
                        using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Views/Htm/confirmation.htm")))
                        {
                            htmlbody = reader.ReadToEnd();
                        }
                        htmlbody = htmlbody.Replace("{VoucherCode}", source.VoucherCode);
                        htmlbody = htmlbody.Replace("{Amount}", source.Amount.ToString("#,##0.00"));

                        string mailMessage = sendmail.ComposeMessage(htmlbody);

                        // Update Voucher if successfully claimed
                        var sql = "Update Recipient SET isEmailSent = {1}, EmailDetail = {2}, EmailResentDate = {3}, EmailErrorLog = {4} WHERE Id = {0}";
                        await db.Database.ExecuteSqlCommandAsync(sql, source.Id, (mailMessage == "Ok")? true : false, htmlbody, DateTime.Now, mailMessage);
                        dbContextTransaction.Commit();

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