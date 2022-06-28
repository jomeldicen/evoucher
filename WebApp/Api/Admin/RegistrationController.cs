using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApp.Models;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Web.Configuration;
using System.Data.SqlClient;
using System;
using System.Linq;
using System.Transactions;
using WebApp.Api.Admin;
using System.Collections.Generic;
using WebApp.Helper;

namespace WebApp.Api
{
    [Authorize]
    [RoutePrefix("api/Registration")]
    public class RegistrationController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public RegistrationController()
        {
        }

        public RegistrationController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }
        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }
            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    return BadRequest();
                }
                return BadRequest(ModelState);
                //GetResponse();
            }
            return null;
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //registration
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(CustomRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities dbcon = new WebAppEntities())
            {
                using (var dbContextTransaction = dbcon.Database.BeginTransaction())
                {
                    try
                    {
                        var isEmailConfirmed = Convert.ToBoolean(dbcon.Settings.Where(x => x.vSettingID == "8196514E-70AA-47F0-8844-3AC3B707F8C5").FirstOrDefault().vSettingOption);
                        var user = new ApplicationUser() { UserName = model.Email, Email = model.Email, EmailConfirmed = isEmailConfirmed };
                        bool isAnyUserExists = dbcon.AspNetUsers.Any();
                        var roleId = "";
                        if (isAnyUserExists)
                        {
                            bool isEmailDuplicate = dbcon.AspNetUsers.Where(x => x.Email == model.Email).Any();
                            if (isEmailDuplicate)
                                return BadRequest("duplicate_email");
                            else
                            {
                                roleId = dbcon.Settings.Where(x => x.vSettingID == "9B55EDC6-2B9C-4869-B5D7-8B2A788DAA12").FirstOrDefault().vSettingOption;
                                if (!dbcon.AspNetRoles.Where(x => x.Id == roleId).Any())
                                    return BadRequest("User Role Not Defined");
                            }
                        }
                        else
                        {
                            roleId = "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03";

                            List<AspNetUsersMenu> anuml = dbcon.AspNetUsersMenus.OrderBy(x => x.iSerialNo).ToList();

                            AspNetRole role = new AspNetRole();
                            role.Id = roleId;
                            role.Name = "Super Admin";
                            role.IndexPage = anuml[0].nvPageUrl;
                            dbcon.AspNetRoles.Add(role);
                            dbcon.SaveChanges();

                            foreach (var mn in anuml)
                            {
                                AspNetUsersMenuPermission anump = new AspNetUsersMenuPermission();
                                anump.vMenuPermissionID = Guid.NewGuid().ToString();
                                anump.Id = role.Id;
                                anump.vMenuID = mn.vMenuID;
                                dbcon.AspNetUsersMenuPermissions.Add(anump);
                                dbcon.SaveChanges();
                            }
                        }

                        AspNetUser asp = new AspNetUser();
                        asp.Id = user.Id;
                        asp.Email = user.Email;
                        asp.EmailConfirmed = user.EmailConfirmed;
                        asp.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                        asp.SecurityStamp = Guid.NewGuid().ToString();
                        asp.PhoneNumber = user.PhoneNumber;
                        asp.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                        asp.TwoFactorEnabled = user.TwoFactorEnabled;
                        asp.LockoutEndDateUtc = user.LockoutEndDateUtc;
                        asp.LockoutEnabled = user.LockoutEnabled;
                        asp.AccessFailedCount = user.AccessFailedCount;
                        asp.UserName = user.Email;
                        asp.Date = DateTime.UtcNow;
                        dbcon.AspNetUsers.Add(asp);
                        dbcon.SaveChanges();

                        var aspuser = dbcon.AspNetUsers.Where(x => x.Email == model.Email).FirstOrDefault();
                        AspNetUserRole anur = new AspNetUserRole();
                        anur.vAspNetUserRolesID = Guid.NewGuid().ToString();
                        anur.UserId = aspuser.Id;
                        anur.RoleId = roleId;
                        dbcon.AspNetUserRoles.Add(anur);
                        dbcon.SaveChanges();

                        dbContextTransaction.Commit();

                        if (!isEmailConfirmed)
                            await ConfirmMailSent(aspuser, isEmailConfirmed);


                        return Ok(isEmailConfirmed);
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest("" + ex.InnerException);
                    }
                }
            }
        }

        [Route("CreateUser")]
        public async Task<IHttpActionResult> CreateUser(CustomUserRegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                if(model.Photo == null) 
                    return BadRequest("photo");
                else
                    return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = model.EmailVerificationDisabled
            };

            using (WebAppEntities dbcon = new WebAppEntities())
            {
                using (var dbContextTransaction = dbcon.Database.BeginTransaction())
                {
                    try
                    {
                        bool isEmailDuplicate = dbcon.AspNetUsers.Where(x => x.Email == model.Email).Any();
                        if (isEmailDuplicate)
                            return BadRequest("duplicate_email");
                        else
                        {
                            AspNetUser asp = new AspNetUser();
                            asp.Id = user.Id;
                            asp.Email = user.Email;
                            asp.EmailConfirmed = user.EmailConfirmed;
                            asp.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                            asp.SecurityStamp = Guid.NewGuid().ToString();
                            asp.PhoneNumber = user.PhoneNumber;
                            asp.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
                            asp.TwoFactorEnabled = user.TwoFactorEnabled;
                            asp.LockoutEndDateUtc = user.LockoutEndDateUtc;
                            asp.LockoutEnabled = user.LockoutEnabled;
                            asp.AccessFailedCount = user.AccessFailedCount;
                            asp.Status = (model.Status == "True") ? true : false;
                            asp.BlockedAccount = (model.BlockedAccount == "True") ? true : false;
                            asp.ResetPassword = (model.ResetPassword == "True") ? true : false;
                            asp.UserName = user.Email;
                            asp.Date = DateTime.UtcNow;
                            dbcon.AspNetUsers.Add(asp);
                            await dbcon.SaveChangesAsync();

                            var aspuser = dbcon.AspNetUsers.Where(x => x.Email == model.Email).FirstOrDefault();

                            AspNetUserRole anur = new AspNetUserRole();
                            anur.vAspNetUserRolesID = Guid.NewGuid().ToString();
                            anur.UserId = aspuser.Id;
                            anur.RoleId = model.RoleId;
                            dbcon.AspNetUserRoles.Add(anur);
                            await dbcon.SaveChangesAsync();

                            bool isImageSaved = new UserController().UploadImage(model.Photo, user.Id);
                            if (isImageSaved)
                            {
                                AspNetUsersProfile asppro = new AspNetUsersProfile();
                                asppro.Id = aspuser.Id;
                                asppro.vFirstName = model.FirstName;
                                asppro.vLastName = model.LastName;
                                asppro.vMiddleName = model.MiddleName;
                                asppro.vCompany = model.Company;
                                asppro.vDepartment = model.Department;
                                asppro.vRank = model.Rank;
                                asppro.vPosition = model.Position;
                                asppro.vPhoto = "/Content/Img/" + aspuser.Id + ".png";
                                dbcon.AspNetUsersProfiles.Add(asppro);
                                dbcon.SaveChanges();
                            }
                            else
                            {
                                dbContextTransaction.Rollback();
                                return BadRequest("Something Wrong!");
                            }

                            dbContextTransaction.Commit();

                            //if (!model.EmailVerificationDisabled)
                            //    await ConfirmMailSent(aspuser, model.EmailVerificationDisabled);

                            return Ok(model.EmailVerificationDisabled);
                        }
                    }
                    catch (Exception ex)
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest(ex.Message);
                    }
                }
            }
        }

        public async Task<IHttpActionResult> ConfirmMailSent(AspNetUser user, bool iec)
        {
            try
            {
                var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                string Domain = string.Empty;
                string callbackUrl = string.Empty;
                using (WebAppEntities db = new WebAppEntities())
                {
                    Domain = db.Settings.Where(x => x.vSettingID == "D000434T-8C18-4W37-N868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // SMTP Host
                    callbackUrl = string.Concat(Domain, "/api/Registration/ConfirmEmail?userId=", user.Id, "&code=", HttpUtility.UrlEncode(code));
                }

                // Email Sending
                EmailSender sendmail = new EmailSender();
                sendmail.MailSubject = "Email Verification";
                sendmail.ToEmail = user.Email;

                string htmlbody = string.Empty;
                using (StreamReader reader = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Views/Htm/Emailconfirmation.htm")))
                {
                    htmlbody = reader.ReadToEnd();
                }
                htmlbody = htmlbody.Replace("{link}", callbackUrl);

                sendmail.ComposeMessage(htmlbody);

                return Ok(iec);
            }
            catch (Exception ex)
            {
                return BadRequest("" + ex);
            }
        }


        //Confirm Mail For Register new User
        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail", Name = "ConfirmEmailRoute")]
        public async Task<IHttpActionResult> ConfirmEmail(string userId = "", string code = "")
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);
            }

            IdentityResult result = await this.UserManager.ConfirmEmailAsync(userId, code);

            var Email = "";

            if (result.Succeeded)
            {
                string ConnectString = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(ConnectString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.AspNetUsers WHERE Id='" + userId + "'", con))
                    {
                        SqlDataReader Reader = cmd.ExecuteReader();
                        while (Reader.Read())
                        {
                            Email = Reader["Email"].ToString();
                        }
                        Reader.Close();
                    }
                }
                string webaddress = WebConfigurationManager.AppSettings["SiteAddress"];
                string url = webaddress + "/Auth/RegistrationSuccessful";

                System.Uri uri = new System.Uri(url);
                return Redirect(uri);
            }
            else
            {
                return GetErrorResult(result);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}