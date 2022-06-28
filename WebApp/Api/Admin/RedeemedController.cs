using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using WebApp.Helper;
using System.IO;

namespace WebApp.Api.Admin
{
    [RoutePrefix("api/Redeemed")]
    public class RedeemedController : ApiController
    {
        private string PublicIP = "";
        private string SecretKey = "";
        private string MarketingEmail = "";

        private RedeemedController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.PublicIP = db.Settings.Where(x => x.vSettingID == "D000434T-8C18-4W37-N868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                this.SecretKey = db.Settings.Where(x => x.vSettingID == "D000412X-8238-5WZ7-B8NN-D1CEN89J0MEL").FirstOrDefault().vSettingOption;
                this.MarketingEmail = db.Settings.Where(x => x.vSettingID == "MKG0W34R-2323-XR22-K823-BKLS89JOMEL").FirstOrDefault().vSettingOption;
            }
        }

        [Route("GetMerchant")]
        public async Task<IHttpActionResult> GetMerchant(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    { 
                        // Check Voucher if exist
                        var voucher = await db.Vouchers
                                            .Where(x => x.UniqueID == ID && x.WithDiscrepancy == false)
                                            .Select(x => 
                                                    new { x.Id, x.VoucherCode, x.Amount,
                                                        x.UniqueID, x.VoucherStatus, x.Recipient,
                                                        TemplateID = x.TemplateCondition.Template.Id,
                                                        PropertyID = x.TemplateCondition.Template.PropertyID,
                                                        PropertyName = x.TemplateCondition.Template.Property.Name,
                                                        PropertyLocation = x.TemplateCondition.Template.Property.Location,
                                                        PropertyImage = x.TemplateCondition.Template.Property.PropertyImage                                                        
                                                    })
                                            .FirstOrDefaultAsync();
                        if (voucher == null)
                            return BadRequest("Voucher code doesn't exist");

                        if (voucher.VoucherStatus == "Registered")
                            return BadRequest("Voucher code was already validated");

                        // Get the list of merchants if any
                        var merchants = await db.Merchants
                                            .Where(x => x.PropertyID == voucher.PropertyID)
                                            .Select(x => new { x.Id, x.Name, x.MerchantImage, x.Email }).ToListAsync();

                        var data = new { VOUCHERINFO = voucher, MerchantList = merchants, MerchantCount = merchants.Count };
                        return Ok(data);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        [Route("GetVoucherData")]
        public async Task<IHttpActionResult> GetVoucherData(string ID, int MerchantID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Check Voucher if exist
                        var voucher = db.Vouchers.Where(x => x.UniqueID == ID && x.WithDiscrepancy == false).Select(x => new { x.Id, x.VoucherCode, x.Amount, x.UniqueID, x.VoucherStatus, x.WithDiscrepancy, x.Recipient, PropertyName = x.TemplateCondition.Template.Property.Name }).FirstOrDefault();
                        if (voucher == null)
                            return BadRequest("Voucher code doesn't exist");

                        var merchant = await db.Merchants
                                        .Where(x => x.Id == MerchantID)
                                        .Select(x => new { x.Id, MerchantName = x.Name, x.Email }).FirstOrDefaultAsync();

                        if (voucher.VoucherStatus == "Registered")
                            return BadRequest("Voucher code was already validated");

                        Recipient rc = new Recipient();

                        if (rc.Id == 0)
                        {
                            DateTime dt = DateTime.Now;
                            rc.UniqueID = voucher.UniqueID;
                            rc.ProcessDate = dt;

                            // Send Email Notification if Voucher has been redeemed.
                            EmailSender sendmail = new EmailSender();
                            sendmail.MailSubject = "Successful Registration";

                            string emailTempalte = "~/Views/Htm/confirmation.htm";
                            // Update Voucher if successfully claimed
                            if (merchant != null)
                            {
                                sendmail.ToEmail = merchant.Email.Replace("\n", "");
                                sendmail.ToCC = MarketingEmail.Replace("\n", "");

                                if(voucher.PropertyName != merchant.MerchantName)
                                    emailTempalte = "~/Views/Htm/confirmation2.htm";

                                var sql = "Update Voucher SET ClaimedDate = GETDATE(), VoucherStatus = {1}, MerchantID = {2} WHERE Id = {0}";
                                await db.Database.ExecuteSqlCommandAsync(sql, voucher.Id, "Registered", MerchantID);
                            }
                            else
                            {
                                sendmail.ToEmail = MarketingEmail.Replace("\n", "");

                                var sql = "Update Voucher SET ClaimedDate = GETDATE(), VoucherStatus = {1} WHERE Id = {0}";
                                await db.Database.ExecuteSqlCommandAsync(sql, voucher.Id, "Registered");
                            }

                            string htmlbody = string.Empty;
                            string listVoucher = string.Empty;

                            using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(emailTempalte)))
                                htmlbody = reader.ReadToEnd();

                            htmlbody = htmlbody.Replace("{Recipient}", voucher.Recipient);
                            htmlbody = htmlbody.Replace("{VoucherCode}", voucher.VoucherCode);
                            htmlbody = htmlbody.Replace("{PropertyName}", voucher.PropertyName);
                            htmlbody = (merchant != null) ? htmlbody.Replace("{MerchantName}", merchant.MerchantName) : htmlbody;
                            htmlbody = htmlbody.Replace("{Amount}", voucher.Amount.ToString("#,##0.00"));

                            string mailMessage = sendmail.ComposeMessage(htmlbody);

                            rc.IsEmailSent = (mailMessage == "Ok") ? true : false;
                            rc.EmailDetail = htmlbody;
                            rc.EmailSentTo = (merchant != null) ? merchant.Email : MarketingEmail;
                            rc.EmailSentDate = rc.ProcessDate;
                            rc.EmailErrorLog = mailMessage;
                            db.Recipients.Add(rc);
                            await db.SaveChangesAsync();
                            dbContextTransaction.Commit();
                        }

                        var data = new { VoucherInfo = voucher, MerchantInfo = merchant };
                        return Ok(data);
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