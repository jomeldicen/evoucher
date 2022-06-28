using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using WebApp.Helper;
using Newtonsoft.Json;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/Settings")]
    public class SettingsController : ApiController
    {
        private string PageUrl = "/Admin/Configuration";
        private string ApiName = "Global Configuration";

        private CustomControl GetPermissionControl()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                var cId = User.Identity.GetUserId();
                var roleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;

                return db.Database.SqlQuery<CustomControl>("EXEC spPermissionControls {0}, {1}", roleId, PageUrl).SingleOrDefault();
            }
        }

        public async Task<IHttpActionResult> Get()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl();

                    var role = await (from rl in db.AspNetRoles
                                      where rl.Id != "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03"
                                      select new
                                      {
                                          rl.Id,
                                          rl.Name
                                      }).OrderBy(x => x.Name).ToListAsync();

                    var settings = db.Settings.ToList();
                    CustomSettings customSettings = new CustomSettings
                    {
                        UserRegister = Convert.ToBoolean(settings.Where(x => x.vSettingID == "82A52FA2-E91F-4195-84FD-8EA32DA2637A").FirstOrDefault().vSettingOption),
                        EmailVerificationDisable = Convert.ToBoolean(settings.Where(x => x.vSettingID == "8196514E-70AA-47F0-8844-3AC3B707F8C5").FirstOrDefault().vSettingOption),
                        UserRole = settings.Where(x => x.vSettingID == "9B55EDC6-2B9C-4869-B5D7-8B2A788DAA12").FirstOrDefault().vSettingOption,
                        RecoverPassword = Convert.ToBoolean(settings.Where(x => x.vSettingID == "95A1ED0B-9645-4E18-9BD1-CAAB4F9F21F5").FirstOrDefault().vSettingOption),
                        ChangePassword = Convert.ToBoolean(settings.Where(x => x.vSettingID == "A55D224B-8C28-4A27-A767-C15C089F26A8").FirstOrDefault().vSettingOption),
                        ChangeProfile = Convert.ToBoolean(settings.Where(x => x.vSettingID == "03665F19-463B-4168-94AE-A27D9857605A").FirstOrDefault().vSettingOption),
                        VoucherThreshold = Convert.ToDecimal(settings.Where(x => x.vSettingID == "B16D224B-1C28-4A37-B767-B15C089JOMEL").FirstOrDefault().vSettingOption),
                        SiteName = settings.Where(x => x.vSettingID == "D0011XX1-2ES1-4B39-1268-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        SiteCode = settings.Where(x => x.vSettingID == "D0012345-3312-4B27-E268-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        SiteOffline = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D001334B-LL33-4BD7-XX68-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        SiteOfflineMessage = settings.Where(x => x.vSettingID == "D001434B-0212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        BodySmallText = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D001XX4B-02VV-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        NavbarSmallText = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D002314B-2212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        SidebarNavSmallText = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D003W34B-1212-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        FooterSmallText = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D004430B-JJH1-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        SidebarNavFlatStyle = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D005434B-1232-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        SidebarNavLegacyStyle = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D006434B-0HH2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        SidebarNavCompact = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D007434B-0GG2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        SidebarNavChildIndent = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D008834B-T112-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        BrandLogoSmallText = Convert.ToBoolean(settings.Where(x => x.vSettingID == "D009934B-02XX-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption),
                        NavbarColorVariants = settings.Where(x => x.vSettingID == "D011434B-22R2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        LinkColorVariants = settings.Where(x => x.vSettingID == "D021434B-12H2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        DarkSidebarVariants = settings.Where(x => x.vSettingID == "D031434B-0GG2-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        LightSidebarVariants = settings.Where(x => x.vSettingID == "D041434B-FF12-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        BrandLogoVariants = settings.Where(x => x.vSettingID == "D051434B-0312-4BE7-H168-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        SiteMetaDescription = settings.Where(x => x.vSettingID == "D001534B-JG18-4B27-G868-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        SiteMetaKeyword = settings.Where(x => x.vSettingID == "D001634B-EW18-4B37-F868-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        Robots = settings.Where(x => x.vSettingID == "D001734B-CS18-4B37-E868-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        SiteAuthor = settings.Where(x => x.vSettingID == "D001831C-1C18-4D37-I868-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        ContentRight = settings.Where(x => x.vSettingID == "D001939B-2CWW-4A37-J868-JOMEL89DICEN").FirstOrDefault().vSettingOption,
                        IPAddress = settings.Where(x => x.vSettingID == "D000234B-EF28-4S37-L868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        SystemEnvironment = settings.Where(x => x.vSettingID == "D000334B-RSWR-4C37-M868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        DefaultIndex = settings.Where(x => x.vSettingID == "D000434T-8C18-4W37-N868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        SecretKey = settings.Where(x => x.vSettingID == "D000412X-8238-5WZ7-B8NN-D1CEN89J0MEL").FirstOrDefault().vSettingOption,
                        ProjectDirectory = settings.Where(x => x.vSettingID == "E000134Y-4A18-4R37-O868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        UploadPath = settings.Where(x => x.vSettingID == "E00023JY-3B18-4X37-P868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        ReportPath = settings.Where(x => x.vSettingID == "E000334B-2C18-4Q37-Q868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        FromEmail = settings.Where(x => x.vSettingID == "E0004312-1D18-1237-R868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        FromName = settings.Where(x => x.vSettingID == "E0005323-9E18-4B37-S868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        ReplyToEmail = settings.Where(x => x.vSettingID == "E000634B-7F18-5B37-T868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        ReplyToName = settings.Where(x => x.vSettingID == "E0007332-6G18-9B37-U868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        Mailer = settings.Where(x => x.vSettingID == "E0008312-5H18-1237-V868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        SMTPHost = settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        SMTPPort = settings.Where(x => x.vSettingID == "E001034C-3J18-8K37-X868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        SMTPSecurity = settings.Where(x => x.vSettingID == "E001134B-2K18-ABC7-Y868-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        SMTPAuthentication = Convert.ToBoolean(settings.Where(x => x.vSettingID == "E001234A-1L18-RRR7-Z868-DICEN89JOMEL").FirstOrDefault().vSettingOption),
                        SMTPEnableSSL = Convert.ToBoolean(settings.Where(x => x.vSettingID == "E111234A-1T22-ERW7-x868-DICEN88JOMEL").FirstOrDefault().vSettingOption),
                        Company = settings.Where(x => x.vSettingID == "F000134R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        CorporateAddress = settings.Where(x => x.vSettingID == "F000234R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        CorporateContactNos = settings.Where(x => x.vSettingID == "F000314R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        CorporateFaxNos = settings.Where(x => x.vSettingID == "F0004Y4R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        CorporateEmail = settings.Where(x => x.vSettingID == "F0005W4R-1D23-1R37-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        CorporateWebsite = settings.Where(x => x.vSettingID == "F0006U4R-1D23-2337-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        FacebookPage = settings.Where(x => x.vSettingID == "F0007D4R-1D23-1R52-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        TwitterPage = settings.Where(x => x.vSettingID == "F0008K4R-1D23-1231-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        InstagramPage = settings.Where(x => x.vSettingID == "F0009O4R-1D23-TT21-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        YoutubePage = settings.Where(x => x.vSettingID == "F000RR4R-1W23-WWWR-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        LinkedinPage = settings.Where(x => x.vSettingID == "F000Q34R-2R23-1R11-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        PinterestPage = settings.Where(x => x.vSettingID == "F000W34R-1E23-1R22-K823-DICEN89JOMEL").FirstOrDefault().vSettingOption,
                        MarketingEmail = settings.Where(x => x.vSettingID == "MKG0W34R-2323-XR22-K823-BKLS89JOMEL").FirstOrDefault().vSettingOption,
                        ExecutiveEmail = settings.Where(x => x.vSettingID == "MKG1X33R-3211-XR22-VBFG-BKLS89JOMEL").FirstOrDefault().vSettingOption,
                        ITEmail = settings.Where(x => x.vSettingID == "ITG1X44R-4411-XR22-VXFG-BKLS19JOMEL").FirstOrDefault().vSettingOption
                    };
                    List<CustomSettings> CustomSettingsList = new List<CustomSettings>();
                    CustomSettingsList.Add(customSettings);

                    var data = new { ROLE = role, SETTINGS = CustomSettingsList, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateSettings")]
        public async Task<IHttpActionResult> UpdateSettings(CustomSettings setting)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var settings = db.Settings.ToList();
                        foreach (var st in settings)
                        {
                            if (st.vSettingID == "82A52FA2-E91F-4195-84FD-8EA32DA2637A") st.vSettingOption = setting.UserRegister.ToString();
                            else if (st.vSettingID == "8196514E-70AA-47F0-8844-3AC3B707F8C5") st.vSettingOption = setting.EmailVerificationDisable.ToString();
                            else if (st.vSettingID == "9B55EDC6-2B9C-4869-B5D7-8B2A788DAA12") st.vSettingOption = setting.UserRole;
                            else if (st.vSettingID == "95A1ED0B-9645-4E18-9BD1-CAAB4F9F21F5") st.vSettingOption = setting.RecoverPassword.ToString();
                            else if (st.vSettingID == "A55D224B-8C28-4A27-A767-C15C089F26A8") st.vSettingOption = setting.ChangePassword.ToString();
                            else if (st.vSettingID == "03665F19-463B-4168-94AE-A27D9857605A") st.vSettingOption = setting.ChangeProfile.ToString();
                            else if (st.vSettingID == "B16D224B-1C28-4A37-B767-B15C089JOMEL") st.vSettingOption = setting.VoucherThreshold.ToString();
                            else if (st.vSettingID == "D000234B-EF28-4S37-L868-DICEN89JOMEL") st.vSettingOption = setting.IPAddress.ToString();
                            else if (st.vSettingID == "D000334B-RSWR-4C37-M868-DICEN89JOMEL") st.vSettingOption = setting.SystemEnvironment.ToString();
                            else if (st.vSettingID == "D000434T-8C18-4W37-N868-DICEN89JOMEL") st.vSettingOption = setting.DefaultIndex.ToString();
                            else if (st.vSettingID == "D000412X-8238-5WZ7-B8NN-D1CEN89J0MEL") st.vSettingOption = setting.SecretKey.ToString();
                            else if (st.vSettingID == "D0011XX1-2ES1-4B39-1268-JOMEL89DICEN") st.vSettingOption = setting.SiteName.ToString();
                            else if (st.vSettingID == "D0012345-3312-4B27-E268-JOMEL89DICEN") st.vSettingOption = setting.SiteCode.ToString();
                            else if (st.vSettingID == "D001334B-LL33-4BD7-XX68-JOMEL89DICEN") st.vSettingOption = setting.SiteOffline.ToString();
                            else if (st.vSettingID == "D001434B-0212-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.SiteOfflineMessage.ToString();
                            else if (st.vSettingID == "D001534B-JG18-4B27-G868-JOMEL89DICEN") st.vSettingOption = setting.SiteMetaDescription.ToString();
                            else if (st.vSettingID == "D001634B-EW18-4B37-F868-JOMEL89DICEN") st.vSettingOption = setting.SiteMetaKeyword.ToString();
                            else if (st.vSettingID == "D001734B-CS18-4B37-E868-JOMEL89DICEN") st.vSettingOption = setting.Robots.ToString();
                            else if (st.vSettingID == "D001831C-1C18-4D37-I868-JOMEL89DICEN") st.vSettingOption = setting.SiteAuthor.ToString();
                            else if (st.vSettingID == "D001939B-2CWW-4A37-J868-JOMEL89DICEN") st.vSettingOption = setting.ContentRight.ToString();
                            else if (st.vSettingID == "D001XX4B-02VV-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.BodySmallText.ToString();
                            else if (st.vSettingID == "D002314B-2212-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.NavbarSmallText.ToString();
                            else if (st.vSettingID == "D003W34B-1212-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.SidebarNavSmallText.ToString();
                            else if (st.vSettingID == "D004430B-JJH1-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.FooterSmallText.ToString();
                            else if (st.vSettingID == "D005434B-1232-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.SidebarNavFlatStyle.ToString();
                            else if (st.vSettingID == "D006434B-0HH2-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.SidebarNavLegacyStyle.ToString();
                            else if (st.vSettingID == "D007434B-0GG2-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.SidebarNavCompact.ToString();
                            else if (st.vSettingID == "D008834B-T112-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.SidebarNavChildIndent.ToString();
                            else if (st.vSettingID == "D009934B-02XX-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.BrandLogoSmallText.ToString();
                            else if (st.vSettingID == "D011434B-22R2-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.NavbarColorVariants.ToString();
                            else if (st.vSettingID == "D021434B-12H2-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.LinkColorVariants.ToString();
                            else if (st.vSettingID == "D031434B-0GG2-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.DarkSidebarVariants.ToString();
                            else if (st.vSettingID == "D041434B-FF12-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.LightSidebarVariants.ToString();
                            else if (st.vSettingID == "D051434B-0312-4BE7-H168-JOMEL89DICEN") st.vSettingOption = setting.BrandLogoVariants.ToString();
                            else if (st.vSettingID == "E000134Y-4A18-4R37-O868-DICEN89JOMEL") st.vSettingOption = setting.ProjectDirectory.ToString();
                            else if (st.vSettingID == "E00023JY-3B18-4X37-P868-DICEN89JOMEL") st.vSettingOption = setting.UploadPath.ToString();
                            else if (st.vSettingID == "E000334B-2C18-4Q37-Q868-DICEN89JOMEL") st.vSettingOption = setting.ReportPath.ToString();
                            else if (st.vSettingID == "E0004312-1D18-1237-R868-DICEN89JOMEL") st.vSettingOption = setting.FromEmail.ToString();
                            else if (st.vSettingID == "E0005323-9E18-4B37-S868-DICEN89JOMEL") st.vSettingOption = setting.FromName.ToString();
                            else if (st.vSettingID == "E000634B-7F18-5B37-T868-DICEN89JOMEL") st.vSettingOption = setting.ReplyToEmail.ToString();
                            else if (st.vSettingID == "E0007332-6G18-9B37-U868-DICEN89JOMEL") st.vSettingOption = setting.ReplyToName.ToString();
                            else if (st.vSettingID == "E0008312-5H18-1237-V868-DICEN89JOMEL") st.vSettingOption = setting.Mailer.ToString();
                            else if (st.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL") st.vSettingOption = setting.SMTPHost.ToString();
                            else if (st.vSettingID == "E001034C-3J18-8K37-X868-DICEN89JOMEL") st.vSettingOption = setting.SMTPPort.ToString();
                            else if (st.vSettingID == "E001134B-2K18-ABC7-Y868-DICEN89JOMEL") st.vSettingOption = setting.SMTPSecurity.ToString();
                            else if (st.vSettingID == "E001234A-1L18-RRR7-Z868-DICEN89JOMEL") st.vSettingOption = setting.SMTPAuthentication.ToString();
                            else if (st.vSettingID == "E111234A-1T22-ERW7-x868-DICEN88JOMEL") st.vSettingOption = setting.SMTPEnableSSL.ToString();
                            else if (st.vSettingID == "F000134R-1D23-1R37-K823-DICEN89JOMEL") st.vSettingOption = setting.Company.ToString();
                            else if (st.vSettingID == "F000234R-1D23-1R37-K823-DICEN89JOMEL") st.vSettingOption = setting.CorporateAddress.ToString();
                            else if (st.vSettingID == "F000314R-1D23-1R37-K823-DICEN89JOMEL") st.vSettingOption = setting.CorporateContactNos.ToString();
                            else if (st.vSettingID == "F0004Y4R-1D23-1R37-K823-DICEN89JOMEL") st.vSettingOption = setting.CorporateFaxNos.ToString();
                            else if (st.vSettingID == "F0005W4R-1D23-1R37-K823-DICEN89JOMEL") st.vSettingOption = setting.CorporateEmail.ToString();
                            else if (st.vSettingID == "F0006U4R-1D23-2337-K823-DICEN89JOMEL") st.vSettingOption = setting.CorporateWebsite.ToString();
                            else if (st.vSettingID == "F0007D4R-1D23-1R52-K823-DICEN89JOMEL") st.vSettingOption = setting.FacebookPage.ToString();
                            else if (st.vSettingID == "F0008K4R-1D23-1231-K823-DICEN89JOMEL") st.vSettingOption = setting.TwitterPage.ToString();
                            else if (st.vSettingID == "F0009O4R-1D23-TT21-K823-DICEN89JOMEL") st.vSettingOption = setting.InstagramPage.ToString();
                            else if (st.vSettingID == "F000Q34R-2R23-1R11-K823-DICEN89JOMEL") st.vSettingOption = setting.LinkedinPage.ToString();
                            else if (st.vSettingID == "F000RR4R-1W23-WWWR-K823-DICEN89JOMEL") st.vSettingOption = setting.YoutubePage.ToString();
                            else if (st.vSettingID == "F000W34R-1E23-1R22-K823-DICEN89JOMEL") st.vSettingOption = setting.PinterestPage.ToString();
                            else if (st.vSettingID == "MKG0W34R-2323-XR22-K823-BKLS89JOMEL") st.vSettingOption = setting.MarketingEmail.ToString();
                            else if (st.vSettingID == "MKG1X33R-3211-XR22-VBFG-BKLS89JOMEL") st.vSettingOption = setting.ExecutiveEmail.ToString();
                            else if (st.vSettingID == "ITG1X44R-4411-XR22-VXFG-BKLS19JOMEL") st.vSettingOption = setting.ITEmail.ToString();

                            db.Entry(st).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(settings);
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