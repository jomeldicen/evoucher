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

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/TemplateCondition")]
    public class TemplateConditionController : ApiController
    {
        private string PageUrl = "/Admin/TemplateCondition";
        private string ApiName = "TemplateCondition";
        private ImageHelper ImgHelper = new ImageHelper();

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

        [Route("GetTemplateCondition")]
        public async Task<IHttpActionResult> GetTemplateCondition([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    int value1 = Convert.ToInt16(param.value1);

                    // Get Templates
                    var template = db.Templates.Where(x => x.Published == true).Select(x => new { x.Id, x.Name }).ToList();

                    IEnumerable<CustomTemplateCondition> source = null;
                    source = await (from tc in db.TemplateConditions
                                    select new CustomTemplateCondition
                                    {
                                        Id = tc.Id,
                                        TemplateID = tc.Template.Id,
                                        TemplateName = tc.Template.Name,
                                        Description = tc.Description,
                                        Published = tc.Published.ToString(),
                                        Amount = tc.Amount,
                                        Amount2 = tc.Amount2,
                                        TemplatePath = tc.TemplatePath,
                                        Guidelines = tc.Guidelines,
                                        Condition = tc.Condition,
                                        //VoucherBase64 = tc.VoucherBase64,
                                        IsCustomTemplate = tc.IsCustomTemplate.ToString(),
                                        isChecked = false,
                                        ModifiedByPK = tc.ModifiedByPK,
                                        ModifiedDate = tc.ModifiedDate,
                                        CreatedByPK = tc.CreatedByPK,
                                        CreatedDate = tc.CreatedDate,
                                    }).ToListAsync();

                    if (value1 != 0)
                        source = source.Where(x => x.TemplateID == value1).ToList();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Description.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomTemplateCondition).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), TemplateConditionLIST = sourcePaged, CONTROLS = permissionCtrl, TemplateLIST = template };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(CustomTemplateCondition data)
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
                        var cd = db.TemplateConditions.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.TemplateID, x.Description, x.Amount, x.Amount2, x.Condition, x.TemplatePath, x.Guidelines, Published = x.Published.ToString(), IsCustomTemplate = x.IsCustomTemplate.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update TemplateCondition SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
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

        [Route("SaveTemplateCondition")]
        public async Task<IHttpActionResult> SaveTemplateCondition(CustomTemplateCondition data)
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
                        var user = db.AspNetUsersProfiles.Where(x => x.Id == cId).Select(x => new { Name = x.vFirstName + " " + x.vLastName }).FirstOrDefault();

                        VoucherController voucher = new VoucherController();
                        string UrlLink = "This is a sample Voucher."; 
                        
                        // Default Datasets for Report
                        IEnumerable<CustomVoucher> ds = db.Vouchers.Where(x => x.Id == 0).Select(x => new CustomVoucher { Recipient = x.Recipient, VoucherCode = x.VoucherCode, Amount = x.Amount, VoucherBase64 = x.VoucherBase64, UrlLink = x.UrlLink, UniqueID = x.UniqueID }).AsEnumerable();
                        
                        TemplateCondition temp = new TemplateCondition();
                        temp.Id = data.Id;
                        temp.TemplateID = data.TemplateID;
                        temp.Description = data.Description;
                        temp.Amount = data.Amount;
                        temp.Amount2 = data.Amount2;
                        temp.Condition = data.Condition;
                        temp.TemplatePath = data.TemplatePath;
                        temp.Guidelines = data.Guidelines;
                        temp.VoucherBase64 = voucher.GenerateVoucher(data.TemplateID, user.Name, "123456", data.Amount, temp, ImgHelper.GenerateQRCode(UrlLink), ds);
                        temp.Published = (data.Published == "True") ? true : false;
                        temp.IsCustomTemplate = (data.IsCustomTemplate == "True") ? true : false;
                        temp.ModifiedByPK = cId;
                        temp.ModifiedDate = DateTime.Now;

                        if (temp.Id == "" || temp.Id == "0")
                        {
                            nwe = true;
                            temp.Id = Guid.NewGuid().ToString();
                            temp.CreatedByPK = cId;
                            temp.CreatedDate = DateTime.Now;
                            db.TemplateConditions.Add(temp);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            temp.CreatedByPK = data.CreatedByPK;
                            temp.CreatedDate = data.CreatedDate;
                            db.Entry(temp).State = EntityState.Modified;
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

        [Route("RemoveRecords")]
        public async Task<IHttpActionResult> RemoveRecords(CustomTemplateCondition data)
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
                        var cd = db.TemplateConditions.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.TemplateID, x.Description, x.Amount, x.Amount2, x.Condition, x.TemplatePath, x.Guidelines, Published = x.Published.ToString(), IsCustomTemplate = x.IsCustomTemplate.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM TemplateCondition WHERE Id = {0}";
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

        [Route("RemoveData")]
        public IHttpActionResult RemoveData(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var cd = db.TemplateConditions.Where(x => x.Id == ID).Select(x => new { x.Id, x.TemplateID, x.Description, x.Amount, x.Amount2, x.TemplatePath, x.Condition, x.Guidelines, Published = x.Published.ToString() }).SingleOrDefault();

                        db.TemplateConditions.RemoveRange(db.TemplateConditions.Where(x => x.Id == ID));
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
    }
}