using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.IO;
using System.Collections.Generic;
using WebApp.Helper;
using Newtonsoft.Json;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/User")]
    public class UserController : ApiController
    {
        private string PageUrl = "/Admin/Users";
        private string ApiName = "Users";

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

        public async Task<IHttpActionResult> Get([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var Id = db.AspNetUserRoles.Where(x => x.RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03").FirstOrDefault().UserId;
                    Id = (User.Identity.GetUserId() == Id) ? null : Id;
                    var roles = await (from role in db.AspNetRoles
                                       where role.Id != ((Id == null) ? null : "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03")
                                       select new
                                         {
                                             role.Id,
                                             role.Name
                                         }).OrderBy(x => x.Name).ToListAsync();

                    var ranks = await (from op in db.Options
                                       where op.Published == true && op.OptionGroup == "Rank"
                                       select new
                                       {
                                           id = op.Id,
                                           label = op.Description
                                       }).OrderBy(x => x.id).ToListAsync();

                    var departments = await (from op in db.Options
                                       where op.Published == true && op.OptionGroup == "Department"
                                       select new
                                       {
                                           id = op.Id,
                                           label = op.Description
                                       }).OrderBy(x => x.id).ToListAsync();

                    IEnumerable<CustomUsers> source = null;
                    source = await (from usr in db.AspNetUsers
                                    where usr.Id != Id
                                    select new CustomUsers
                                    {
                                        Id = usr.Id,
                                        Email = usr.Email,
                                        RoleName = usr.AspNetUserRoles.FirstOrDefault().AspNetRole.Name,
                                        FullName = usr.AspNetUsersProfile.vFirstName + " " + usr.AspNetUsersProfile.vLastName,
                                        Photo = usr.AspNetUsersProfile.vPhoto,
                                        Company = usr.AspNetUsersProfile.vCompany,
                                        Department = usr.AspNetUsersProfile.vDepartment,
                                        Position = usr.AspNetUsersProfile.vPosition,
                                        Rank = usr.AspNetUsersProfile.vRank,
                                        BlockedAccount = (usr.BlockedAccount) ? "True" : "False",
                                        Status = (usr.Status) ? "True" : "False",
                                        ResetPassword = (usr.ResetPassword) ? "True" : "False",
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.FullName.ToLower().Contains(param.search) || x.RoleName.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomUsers).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), USER = sourcePaged, ROLE = roles, RANK = ranks, DEPARTMENT = departments, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetSingleUser")]
        public async Task<IHttpActionResult> GetSingleUser()
        {
            var userId = User.Identity.GetUserId();
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var loginList = await (from anu in db.AspNetUsers
                                           join anulh in db.AspNetUsersLoginHistories on anu.Id equals anulh.Id
                                           join anup in db.AspNetUsersProfiles on anu.Id equals anup.Id into joined
                                           from anup in joined.DefaultIfEmpty()
                                           where anu.Id == userId
                                           select new
                                           {
                                               Login = anulh.dLogIn,
                                               Logout = anulh.dLogOut,
                                               IP = anulh.nvIPAddress
                                           }).OrderByDescending(x => x.Login).Take(10).ToListAsync();

                    var visitedList = await (from anu in db.AspNetUsers
                                             join anupv in db.AspNetUsersPageVisiteds on anu.Id equals anupv.Id
                                             join anup in db.AspNetUsersProfiles on anu.Id equals anup.Id into joined
                                             from anup in joined.DefaultIfEmpty()
                                             where anu.Id == userId
                                             select new
                                             {
                                                 PageName = anupv.nvPageName,
                                                 VisitedDate = anupv.dDateVisited,
                                                 IP = anupv.nvIPAddress,
                                             }).OrderByDescending(x => x.VisitedDate).Take(10).ToListAsync();

                    var lastLogin = loginList[0].Login;
                    var user = await (from usr in db.AspNetUsers
                                      where usr.Id == userId
                                      select new
                                      {
                                          usr.Id,
                                          usr.Email,
                                          RoleId = usr.AspNetUserRoles.FirstOrDefault().AspNetRole.Id,
                                          RoleName = usr.AspNetUserRoles.FirstOrDefault().AspNetRole.Name,
                                          FirstName = usr.AspNetUsersProfile.vFirstName,
                                          LastName = usr.AspNetUsersProfile.vLastName,
                                          MiddleName = usr.AspNetUsersProfile.vMiddleName,
                                          FullName = usr.AspNetUsersProfile.vFirstName + " " + usr.AspNetUsersProfile.vLastName,
                                          Company = usr.AspNetUsersProfile.vCompany,
                                          Department = usr.AspNetUsersProfile.vDepartment,
                                          Rank = usr.AspNetUsersProfile.vRank,
                                          Position = usr.AspNetUsersProfile.vPosition,
                                          usr.PhoneNumber,
                                          BlockedAccount = (usr.BlockedAccount) ? "True" : "False",
                                          Status = (usr.Status) ? "True" : "False",
                                          ResetPassword = (usr.ResetPassword) ? "True" : "False",
                                          Photo = usr.AspNetUsersProfile.vPhoto,
                                          LastLogin = lastLogin,
                                          TotalLogin = loginList.Count,
                                          TotalPageVisited = visitedList.Count
                                      }).ToListAsync();

                    var Id = db.AspNetUserRoles.Where(x => x.RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03").FirstOrDefault().UserId;
                    Id = (userId == Id) ? null : Id;
                    var roles = await (from role in db.AspNetRoles
                                       where role.Id != ((Id == null) ? null : "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03")
                                       select new
                                       {
                                           role.Id,
                                           role.Name
                                       }).OrderBy(x => x.Name).ToListAsync();

                    var data = new { USER = user, ROLES = roles, LOGINLIST = loginList, VISITEDLIST = visitedList };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetUserById")]
        public async Task<IHttpActionResult> GetUserById(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var getData = await (from usr in db.AspNetUsers
                                         where usr.Id == ID
                                         select new
                                         {
                                             usr.Id,
                                             usr.Email,
                                             RoleId = usr.AspNetUserRoles.Where(x=>x.UserId == usr.Id).FirstOrDefault().RoleId,
                                             FirstName = usr.AspNetUsersProfile.vFirstName,
                                             LastName = usr.AspNetUsersProfile.vLastName,
                                             MiddleName = usr.AspNetUsersProfile.vMiddleName,
                                             usr.PhoneNumber,
                                             BlockedAccount = (usr.BlockedAccount) ? "True" : "False",
                                             Status = (usr.Status) ? "True" : "False",
                                             ResetPassword = (usr.ResetPassword) ? "True" : "False",
                                             Photo = usr.AspNetUsersProfile.vPhoto,
                                             Rank = usr.AspNetUsersProfile.vRank,
                                             Department = usr.AspNetUsersProfile.vDepartment,
                                             Position = usr.AspNetUsersProfile.vPosition,
                                             Company = usr.AspNetUsersProfile.vCompany
                                         }).ToListAsync();
                    var data = new { USER = getData };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateProfile")]
        public async Task<IHttpActionResult> UpdateProfile(UpdateUserModel data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                bool isAllowed = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "03665F19-463B-4168-94AE-A27D9857605A").FirstOrDefault().vSettingOption);
                if (isAllowed)
                {
                    using (var dbContextTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            bool isEmailDuplicate = db.AspNetUsers.Where(x => x.Email == data.Email && x.Id != data.Id).Any();
                            if (isEmailDuplicate)
                                return BadRequest("duplicate_email");
                           
                            AspNetUser anu = db.AspNetUsers.Where(x => x.Id == data.Id).FirstOrDefault();
                            anu.Email = data.Email;
                            anu.UserName = data.Email;
                            anu.PhoneNumber = data.PhoneNumber;
                            anu.Status = (data.Status == "True") ? true : false;
                            anu.BlockedAccount = (data.BlockedAccount == "True") ? true : false;
                            anu.ResetPassword = (data.ResetPassword == "True") ? true : false;
                            db.Entry(anu).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            AspNetUserRole anur = db.AspNetUserRoles.Where(x => x.UserId == data.Id).FirstOrDefault();
                            anur.RoleId = data.RoleId;
                            db.Entry(anur).State = EntityState.Modified;
                            await db.SaveChangesAsync();

                            bool isImageSaved = UploadImage(data.Photo, data.Id);
                            if (isImageSaved)
                            {
                                AspNetUsersProfile anupro = db.AspNetUsersProfiles.Where(x => x.Id == data.Id).FirstOrDefault();
                                if (anupro == null)
                                {
                                    AspNetUsersProfile anup = new AspNetUsersProfile();
                                    anup.Id = data.Id;
                                    anup.vFirstName = data.FirstName;
                                    anup.vLastName = data.LastName;
                                    anup.vMiddleName = data.MiddleName;
                                    anup.vRank = data.Rank;
                                    anup.vPosition = data.Position;
                                    anup.vCompany = data.Company;
                                    anup.vDepartment = data.Department;
                                    anup.vPhoto = "/Content/Img/" + data.Id + ".png";
                                    db.AspNetUsersProfiles.Add(anup);
                                }
                                else
                                {
                                    anupro.Id = data.Id;
                                    anupro.vFirstName = data.FirstName;
                                    anupro.vLastName = data.LastName;
                                    anupro.vMiddleName = data.MiddleName;
                                    anupro.vRank = data.Rank;
                                    anupro.vPosition = data.Position;
                                    anupro.vCompany = data.Company;
                                    anupro.vDepartment = data.Department;
                                    anupro.vPhoto = "/Content/Img/" + data.Id + ".png";
                                    db.Entry(anupro).State = EntityState.Modified;
                                }
                                await db.SaveChangesAsync();
                            }
                            else
                            {
                                var isDelete = DeleteImage(data.Id);
                                dbContextTransaction.Rollback();
                                return BadRequest("Something Wrong!");
                            }
                           
                            dbContextTransaction.Commit();

                            // ---------------- Start Transaction Activity Logs ------------------ //
                            AuditTrail log = new AuditTrail();
                            log.EventType = "UPDATE";
                            log.Description = "Update " + this.ApiName;
                            log.PageUrl = this.PageUrl;
                            log.ObjectType = this.GetType().Name;
                            log.EventName = this.ApiName;
                            log.ContentDetail = JsonConvert.SerializeObject(data);
                            log.SaveTransactionLogs();
                            // ---------------- End Transaction Activity Logs -------------------- //

                            return Ok();
                        }
                        catch (Exception ex)
                        {
                            var isDelete = DeleteImage(data.Id);
                            dbContextTransaction.Rollback();
                            return BadRequest(ex.Message);
                        }
                    }
                }
                else
                    return BadRequest("Change Profile Not Allowed");
            }
        }

        [Route("UpdateUser")]
        public async Task<IHttpActionResult> UpdateUser(UpdateUserModel data)
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
                        bool isEmailDuplicate = db.AspNetUsers.Where(x => x.Email == data.Email && x.Id != data.Id).Any();
                        if (isEmailDuplicate)
                            return BadRequest("duplicate_email");
                            
                        AspNetUser anu = db.AspNetUsers.Where(x => x.Id == data.Id).FirstOrDefault();
                        anu.Email = data.Email;
                        anu.UserName = data.Email;
                        anu.PhoneNumber = data.PhoneNumber;
                        anu.Status = (data.Status == "True") ? true : false;
                        anu.BlockedAccount = (data.BlockedAccount == "True") ? true : false;
                        anu.ResetPassword = (data.ResetPassword == "True") ? true : false;
                        db.Entry(anu).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        AspNetUserRole anur = db.AspNetUserRoles.Where(x => x.UserId == data.Id).FirstOrDefault();
                        anur.RoleId = data.RoleId;
                        db.Entry(anur).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        bool isImageSaved = UploadImage(data.Photo, data.Id);
                        if (isImageSaved)
                        {
                            AspNetUsersProfile anup = db.AspNetUsersProfiles.Where(x => x.Id == data.Id).FirstOrDefault();
                            anup.vFirstName = data.FirstName;
                            anup.vLastName = data.LastName;
                            anup.vMiddleName = data.MiddleName;
                            anup.vRank = data.Rank;
                            anup.vPosition = data.Position;
                            anup.vCompany = data.Company;
                            anup.vDepartment = data.Department;
                            anup.vPhoto = "/Content/Img/" + data.Id + ".png";
                            db.Entry(anup).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            dbContextTransaction.Rollback();
                            return BadRequest("Something Wrong!");
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = "Update " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(data);
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

        [Route("RemoveData")]
        public IHttpActionResult RemoveData(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                if (ID != User.Identity.GetUserId())
                {
                    using (var dbContextTransaction = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var cd = db.AspNetUsers.Where(x => x.Id == ID).Select(x => new { x.Id, x.Email, x.AspNetUsersProfile.vFirstName, x.AspNetUsersProfile.vLastName }).SingleOrDefault();

                            if (db.AspNetUserRoles.Where(x => x.UserId == ID).FirstOrDefault().RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03")
                                return BadRequest("Super Admin can't be delete");

                            db.AspNetUsersLoginHistories.RemoveRange(db.AspNetUsersLoginHistories.Where(x => x.Id == ID));
                            db.SaveChanges();

                            db.AspNetUsersPageVisiteds.RemoveRange(db.AspNetUsersPageVisiteds.Where(x => x.Id == ID));
                            db.SaveChanges();

                            db.AspNetUserRoles.RemoveRange(db.AspNetUserRoles.Where(x => x.UserId == ID));
                            db.SaveChanges();

                            db.AspNetUsersProfiles.RemoveRange(db.AspNetUsersProfiles.Where(x => x.Id == ID));
                            db.SaveChanges();

                            db.AspNetUsers.RemoveRange(db.AspNetUsers.Where(x => x.Id == ID));
                            db.SaveChanges();

                            bool isImageDeleted = DeleteImage(ID);
                            if (!isImageDeleted)
                            {
                                dbContextTransaction.Rollback();
                                return BadRequest("Image Can't Delete.");
                            }

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
                else
                    return BadRequest("Self");
            }
        }

        public bool UploadImage(string photo, string name)
        {
            try
            {
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Img/");
                string imageName = name + ".png";
                string imgPath = Path.Combine(sPath, imageName);
                if (!Directory.Exists(sPath))
                {
                    Directory.CreateDirectory(sPath); 
                }
                var imgStr = photo.Split(',')[1];
                byte[] imageBytes = Convert.FromBase64String(imgStr);
                File.WriteAllBytes(imgPath, imageBytes);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool DeleteImage(string name)
        {
            try
            {
                string sPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/Img/");
                File.Delete(sPath + name + ".png");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}