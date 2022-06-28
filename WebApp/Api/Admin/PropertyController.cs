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
    [RoutePrefix("api/Property")]
    public class PropertyController : ApiController
    {
        private string PageUrl = "/Admin/Property";
        private string ApiName = "Property";
        private string UploadPath = "";

        private PropertyController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.UploadPath = db.Settings.Where(x => x.vSettingID == "E00023JY-3B18-4X37-P868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // Property
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

        [Route("GetProperty")]
        public async Task<IHttpActionResult> GetProperty([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    IEnumerable<CustomProperty> source = null;
                    source = await (from pr in db.Properties
                                    select new CustomProperty
                                    {
                                        Id = pr.Id,
                                        Name = pr.Name,
                                        Description = pr.Description,
                                        Location = pr.Location,
                                        ContactPerson = pr.ContactPerson,
                                        Mobile = pr.Mobile,
                                        Email = pr.Email,
                                        Published = pr.Published.ToString(),
                                        PropertyImage = pr.PropertyImage,
                                        OldImage = pr.PropertyImage,
                                        isChecked = false,
                                        ModifiedByPK = pr.ModifiedByPK,
                                        ModifiedDate = pr.ModifiedDate,
                                        CreatedByPK = pr.CreatedByPK,
                                        CreatedDate = pr.CreatedDate,
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomProperty).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), PropertyLIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomProperty data)
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
                        var cd = db.Properties.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update Property SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.Id, data.Published, User.Identity.GetUserId(), DateTime.Now);
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "UPDATE";
                        log.Description = (data.Published == "True") ? "Activate list of " + this.ApiName : "Deactivate list of " + this.ApiName;
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

        [Route("SaveProperty")]
        public async Task<IHttpActionResult> SaveProperty(CustomProperty data)
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

                        bool isPropertynExists = db.Properties.Where(x => x.Name == data.Name && x.Id != data.Id).Any();
                        if (isPropertynExists)
                            return BadRequest("Record already exists");

                        if (String.IsNullOrEmpty(data.PropertyImage))
                            return BadRequest("Image is required!");

                        Property pro = new Property();
                        pro.Id = data.Id;
                        pro.Name = data.Name;
                        pro.Description = data.Description;
                        pro.Email = data.Email;
                        pro.ContactPerson = data.ContactPerson;
                        pro.Mobile = data.Mobile;
                        pro.Location = data.Location;
                        pro.PropertyImage = data.PropertyImage;
                        pro.Published = (data.Published == "True") ? true : false;
                        pro.ModifiedByPK = cId;
                        pro.ModifiedDate = DateTime.Now;

                        // Image Uploading Process
                        string filename = "";
                        if (!string.IsNullOrWhiteSpace(data.PropertyImage))
                        {
                            // if Old Image and Current Image is the same then do nothing
                            if (!data.PropertyImage.Equals(data.OldImage))
                            {
                                filename = data.Name.RemoveSpecialCharacters().AppendTimeStamp();
                                filename = new ImageHelper().UploadImage(this.UploadPath, data.PropertyImage, filename);
                                if (string.IsNullOrWhiteSpace(filename))
                                    return BadRequest("Error occured upon uploading image");
                                else
                                {
                                    pro.PropertyImage = filename;

                                    // Old image removing process
                                    if (!string.IsNullOrWhiteSpace(data.OldImage))
                                    {
                                        string sPath = System.Web.Hosting.HostingEnvironment.MapPath(data.OldImage);
                                        File.Delete(sPath);
                                    }
                                }
                            }
                        }

                        if (pro.Id == 0)
                        {
                            nwe = true;
                            pro.CreatedByPK = cId;
                            pro.CreatedDate = DateTime.Now;
                            db.Properties.Add(pro);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            pro.CreatedByPK = data.CreatedByPK;
                            pro.CreatedDate = data.CreatedDate;
                            db.Entry(pro).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create " + this.ApiName : "Update " + this.ApiName;
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
        public IHttpActionResult RemoveData(int ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.Properties.Where(x => x.Id == ID).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).SingleOrDefault();

                        db.Properties.RemoveRange(db.Properties.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomProperty data)
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
                        var cd = db.Properties.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM Property WHERE Id = {0}";
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