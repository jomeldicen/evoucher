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
    [RoutePrefix("api/Merchant")]
    public class MerchantController : ApiController
    {
        private string PageUrl = "/Admin/Merchant";
        private string ApiName = "Merchant";
        private string UploadPath = "";

        private MerchantController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.UploadPath = db.Settings.Where(x => x.vSettingID == "E00023JY-3B18-4X37-P868-DICEN89JOMEL").FirstOrDefault().vSettingOption; // Upload Path
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

        [Route("GetMerchant")]
        public async Task<IHttpActionResult> GetMerchant([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);


                    var propertys = db.Properties.Where(x => x.Published == true).Select(x => new { x.Id, x.Name }).OrderBy(x => x.Name).ToList();
                    if (propertys == null)
                        return BadRequest("Property maintenance is not properly set.");


                    IEnumerable<CustomMerchant> source = null;
                    source = await (from me in db.Merchants
                                    select new CustomMerchant
                                    {
                                        Id = me.Id,
                                        PropertyID = me.PropertyID,
                                        PropertyName = me.Property.Name,
                                        Name = me.Name,
                                        ContactPerson = me.ContactPerson,
                                        Mobile = me.Mobile,
                                        Email = me.Email,
                                        Published = me.Published.ToString(),
                                        MerchantImage = me.MerchantImage,
                                        Section = me.Section,
                                        OldImage = me.MerchantImage,
                                        isChecked = false,
                                        ModifiedByPK = me.ModifiedByPK,
                                        ModifiedDate = me.ModifiedDate,
                                        CreatedByPK = me.CreatedByPK,
                                        CreatedDate = me.CreatedDate,
                                    }).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomMerchant).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), MerchantLIST = sourcePaged, PropertyLIST = propertys, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomMerchant data)
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
                        var cd = db.Merchants.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update Merchant SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
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

        [Route("SaveMerchant")]
        public async Task<IHttpActionResult> SaveMerchant(CustomMerchant data)
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

                        bool isMerchantnExists = db.Merchants.Where(x => x.Name == data.Name && x.Id != data.Id).Any();
                        if (isMerchantnExists)
                            return BadRequest("Record already exists");

                        if (String.IsNullOrEmpty(data.MerchantImage))
                            return BadRequest("Image is required!");

                        Merchant mer = new Merchant();
                        mer.Id = data.Id;
                        mer.PropertyID = data.PropertyID;
                        mer.Name = data.Name;
                        mer.Email = data.Email;
                        mer.ContactPerson = data.ContactPerson;
                        mer.Section = data.Section;
                        mer.Mobile = data.Mobile;
                        mer.Published = (data.Published == "True") ? true : false;
                        mer.MerchantImage = data.MerchantImage;
                        mer.ModifiedByPK = cId;
                        mer.ModifiedDate = DateTime.Now;

                        // Image Uploading Process
                        string filename = "";
                        if (!string.IsNullOrWhiteSpace(data.MerchantImage))
                        {
                            // if Old Image and Current Image is the same then do nothing
                            if (!data.MerchantImage.Equals(data.OldImage))
                            {
                                filename = data.Name.RemoveSpecialCharacters().AppendTimeStamp();
                                filename = new ImageHelper().UploadImage(this.UploadPath, data.MerchantImage, filename);
                                if (string.IsNullOrWhiteSpace(filename))
                                    return BadRequest("Error occured upon uploading image");
                                else
                                {
                                    mer.MerchantImage = filename;

                                    // Old image removing process
                                    if (!string.IsNullOrWhiteSpace(data.OldImage))
                                    {
                                        string sPath = System.Web.Hosting.HostingEnvironment.MapPath(data.OldImage);
                                        File.Delete(sPath);
                                    }
                                }
                            }
                        }

                        if (mer.Id == 0)
                        {
                            nwe = true;
                            mer.CreatedByPK = cId;
                            mer.CreatedDate = DateTime.Now;
                            db.Merchants.Add(mer);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            mer.CreatedByPK = data.CreatedByPK;
                            mer.CreatedDate = data.CreatedDate;
                            db.Entry(mer).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // check if exisitingdata in db is aligned on data in ui
                        //List<int> existingMerchants = db.MerchantProperties.Where(x => x.MerchantID == data.Id).Select(x => x.PropertyID).ToList();
                        //List<int> currentMerchants = data.OptionIDs.Select(x => x.id).ToList();
                        //bool isEqual = Enumerable.SequenceEqual(existingMerchants.OrderBy(e => e), currentMerchants.OrderBy(e => e));

                        //if (!isEqual)
                        //{
                        //    db.MerchantProperties.RemoveRange(db.MerchantProperties.Where(x => x.MerchantID == data.Id));
                        //    await db.SaveChangesAsync();

                        //    // loop selected controls
                        //    foreach (var op in data.OptionIDs)
                        //    {
                        //        MerchantProperty mt = new MerchantProperty();
                        //        mt.Id = Guid.NewGuid().ToString();
                        //        mt.MerchantID = mer.Id;
                        //        mt.PropertyID = op.id;
                        //        db.MerchantProperties.Add(mt);
                        //        await db.SaveChangesAsync();
                        //    }
                        //}

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
                        var cd = db.Merchants.Where(x => x.Id == ID).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).SingleOrDefault();

                        db.Merchants.RemoveRange(db.Merchants.Where(x => x.Id == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(CustomMerchant data)
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
                        var cd = db.Merchants.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM Merchant WHERE Id = {0}";
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