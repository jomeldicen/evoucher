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
    [RoutePrefix("api/Role")]
    public class RoleController : ApiController
    {
        private string PageUrl = "/Admin/Role";
        private string ApiName = "Role";

        private List<Menu> tMenu { get; set; }
        private List<Menu> MenuListBySerial { get; set; }

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

        public async Task<IHttpActionResult> Get()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    //var permissionCtrl = this.GetPermissionControl();

                    var cId = User.Identity.GetUserId();
                    var RoleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;
                    var Role = (RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03") ? null : "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03";
                    int i = 0;
                    var getData = await (from role in db.AspNetRoles
                                         where role.Id != Role
                                         select new
                                         {
                                             sl = (role.Id == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03") ? i : i + 1,
                                             role.Id,
                                             role.Name,
                                             role.IndexPage,
                                             totalUser = role.AspNetUserRoles.Where(x => x.RoleId == role.Id).Count()
                                         }).OrderBy(x => x.sl).ToListAsync();

                    var data = new { ROLE = getData };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetRole")]
        public async Task<IHttpActionResult> GetRole([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var cId = User.Identity.GetUserId();
                    var RoleId = db.AspNetUserRoles.Where(x => x.UserId == cId).FirstOrDefault().RoleId;
                    var Role = (RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03") ? null : "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03";
                    int i = 0;

                    IEnumerable<Role> source = null;
                    source = await (from role in db.AspNetRoles
                                    where role.Id != Role
                                    select new Role
                                    {
                                        sl = (role.Id == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03") ? i : i + 1,
                                        Id = role.Id,
                                        Name = role.Name,
                                        IndexPage = role.IndexPage,
                                        totalUser = role.AspNetUserRoles.Where(x => x.RoleId == role.Id).Count(),
                                        Published = role.Published.ToString(),
                                        isChecked = false,
                                        ModifiedByPK = role.ModifiedByPK,
                                        ModifiedDate = role.ModifiedDate,
                                        CreatedByPK = role.CreatedByPK,
                                        CreatedDate = role.CreatedDate
                                    }).OrderBy(x => x.sl).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.Name.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(Role).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), ROLELIST = sourcePaged, CONTROLS = permissionCtrl };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        // record list for Menu Module
        [Route("GetMenu")]
        public async Task<IHttpActionResult> GetMenu(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    //var permissionCtrl = this.GetPermissionControl();

                    IEnumerable <Menu> source = null;
                    source = await (from mn in db.AspNetUsersMenus
                                    join pm in db.AspNetUsersMenuPermissions.Where(x => x.Id == ID) on mn.vMenuID equals pm.vMenuID
                                    into UserPermission
                                    from pm in UserPermission.DefaultIfEmpty()
                                    where mn.Published == true
                                    select new Menu
                                    {
                                        vMenuID = mn.vMenuID,
                                        vParentMenuID = mn.vParentMenuID,
                                        iSerialNo = mn.iSerialNo,
                                        nvMenuName = mn.nvMenuName,
                                        nvPageUrl = mn.nvPageUrl,
                                        nvFabIcon = mn.nvFabIcon,
                                        PrefixCode = mn.PrefixCode,
                                        Published = mn.Published.ToString(),
                                        ControlIDs = (from w in db.AspNetUsersMenuControls 
                                                     join o in db.Options on w.OptionID equals o.Id 
                                                     where w.vMenuID == mn.vMenuID 
                                                     select new IdLabelList { id = w.OptionID, label = o.Description }).Distinct().ToList(),
                                        OptionIDs = (from x in db.AspNetUsersMenus
                                                      join y in db.AspNetUsersMenuPermissions on x.vMenuID equals y.vMenuID
                                                      join z in db.AspNetUsersMenuPermissionControls on y.vMenuPermissionID equals z.vMenuPermissionID
                                                      where x.vMenuID == mn.vMenuID && y.Id == ID
                                                      select new IdList { id = z.OptionID }).Distinct().ToList(),
                                        isChecked = pm.vMenuID != null? true : false,
                                        //wAccess = (db.AspNetUsersMenuPermissions.Where(x => x.vMenuID == mn.vMenuID && x.Id == ID).Count() > 0)? db.AspNetUsersMenuPermissions.Where(x => x.vMenuID == mn.vMenuID && x.Id == ID).FirstOrDefault().wView : false,
                                        ModifiedByPK = mn.ModifiedByPK,
                                        ModifiedDate = mn.ModifiedDate,
                                        CreatedByPK = mn.CreatedByPK,
                                        CreatedDate = mn.CreatedDate
                                    }).OrderBy(x => x.iSerialNo).ToListAsync();

                    // parent menu list
                    var pMenu = source.Where(x => x.vParentMenuID == null).OrderBy(x => x.iSerialNo).ToList();
                    tMenu = source.Where(x => x.vParentMenuID != null).OrderBy(x => x.iSerialNo).ToList();

                    List<Menu> Menu = new List<Menu>();
                    MenuListBySerial = new List<Menu>();
                    foreach (var pm in pMenu)
                    {
                        pm.NameWithParent = pm.nvMenuName;
                        MenuListBySerial.Add(pm);
                        Menu mn = new Menu();
                        GetAllMenu(pm, tMenu.ToList());
                        Menu.Add(mn);
                    }
                    var parent = MenuListBySerial.Where(x => x.nvPageUrl == "#").ToList();
                    var Index = MenuListBySerial.Where(x => x.nvPageUrl != "#").ToList();

                    var data = new { MENULIST = MenuListBySerial, PARENT = parent, INDEX = Index };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        private void GetAllMenu(Menu pMenu, List<Menu> menuList)
        {
            List<Menu> childList = new List<Menu>();
            List<Menu> tempList = new List<Menu>();
            foreach (var ml in menuList)
            {
                if (pMenu.vMenuID == ml.vParentMenuID)
                {
                    ml.NameWithParent = ((pMenu.NameWithParent == null) ? pMenu.nvMenuName : pMenu.NameWithParent) + " >> " + ml.nvMenuName;
                    childList.Add(ml);
                    tMenu.Remove(ml);
                }
                else
                    tempList.Add(ml);
            }
            foreach (var cl in childList)
            {
                MenuListBySerial.Add(cl);
                GetAllMenu(cl, tempList);
            }
        }


        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(Role data)
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
                        var cd = db.AspNetRoles.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.IndexPage, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update AspNetRoles SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE Id = {0}";
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

        [Route("SaveRole")]
        public async Task<IHttpActionResult> SaveRole(CustomRoles role)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();

                        bool isRoleExists = db.AspNetRoles.Where(x => x.Id != role.Role.Id && x.Name == role.Role.Name).Any();
                        if (isRoleExists)
                            return BadRequest("Exists");

                        AspNetRole rl = new AspNetRole();

                        rl.Id = role.Role.Id;
                        rl.Name = role.Role.Name;
                        rl.IndexPage = role.Role.IndexPage;
                        rl.Published = (role.Role.Published == "True") ? true : false;
                        rl.ModifiedByPK = cId;
                        rl.ModifiedDate = DateTime.Now;

                        if (rl.Id == null)
                        {
                            nwe = true;
                            rl.CreatedByPK = cId;
                            rl.CreatedDate = DateTime.Now;
                            rl.Id = Guid.NewGuid().ToString();
                            db.AspNetRoles.Add(rl);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            rl.CreatedByPK = role.Role.CreatedByPK;
                            rl.CreatedDate = role.Role.CreatedDate;
                            db.Entry(rl).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // clear Menu Permissions table and Permission conrols table before new set of entry
                        db.AspNetUsersMenuPermissionControls.RemoveRange(db.AspNetUsersMenuPermissionControls.Where(x => x.vRoleID == rl.Id));
                        await db.SaveChangesAsync();

                        db.AspNetUsersMenuPermissions.RemoveRange(db.AspNetUsersMenuPermissions.Where(x => x.Id == rl.Id));
                        await db.SaveChangesAsync();

                        // saving wAccess Menus/Modules on table
                        foreach (var mn in role.MenuList.Where(x => x.isChecked == true))
                        {
                            AspNetUsersMenuPermission mp = new AspNetUsersMenuPermission();
                            mp.vMenuPermissionID = Guid.NewGuid().ToString();
                            mp.Id = rl.Id;
                            mp.vMenuID = mn.vMenuID;
                            db.AspNetUsersMenuPermissions.Add(mp);
                            await db.SaveChangesAsync();

                            if (mn.OptionIDs.Count > 0)
                            { 
                                // saving menu controls per permission on table
                                foreach (var op in mn.OptionIDs)
                                {
                                    AspNetUsersMenuPermissionControl pc = new AspNetUsersMenuPermissionControl();
                                    pc.vPermissionControlID = Guid.NewGuid().ToString();
                                    pc.vMenuPermissionID = mp.vMenuPermissionID;
                                    pc.vRoleID = rl.Id;
                                    pc.OptionID = op.id;
                                    db.AspNetUsersMenuPermissionControls.Add(pc);
                                    await db.SaveChangesAsync();
                                }
                            }
                        }

                        dbContextTransaction.Commit();

                        //----------------Start Transaction Activity Logs------------------ //
                        var cd = db.AspNetRoles.Where(x => x.Id == rl.Id).Select(x => new { x.Id, x.Name, x.IndexPage, Published = x.Published.ToString() }).SingleOrDefault();

                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create " + this.ApiName : "Update " + this.ApiName;
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        //----------------End Transaction Activity Logs-------------------- //

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

        [Route("GetRoleById")]
        public async Task<IHttpActionResult> GetRoleById(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var getData = await (from role in db.AspNetRoles
                                            where role.Id == ID
                                            select new
                                            {
                                                role.Id,
                                                role.Name,
                                                role.IndexPage
                                            }).ToListAsync();
                    var menuPer = await (from anump in db.AspNetUsersMenuPermissions
                                            where anump.Id == ID
                                            select new
                                            {
                                                anump.vMenuID,
                                                anump.wView,
                                                anump.wAdd,
                                                anump.wEdit,
                                                anump.wDelete,
                                                anump.wExtract,
                                                anump.wPrint,
                                                anump.wDownload
                                            }).ToListAsync();
                    var data = new { ROLE = getData, MENUPER = menuPer };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
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
                        var result = db.AspNetUserRoles.Where(x => x.RoleId == ID).Count();
                        if(result > 0)
                            return BadRequest("Unable to delete role with exisisting user(s)");

                        var cd = db.AspNetRoles.Where(x => x.Id == ID).Select(x => new { x.Id, x.Name, x.IndexPage, Published = x.Published.ToString() }).SingleOrDefault();

                        db.AspNetUsersMenuPermissionControls.RemoveRange(db.AspNetUsersMenuPermissionControls.Where(x => x.vRoleID == ID));
                        db.SaveChanges();

                        db.AspNetUsersMenuPermissions.RemoveRange(db.AspNetUsersMenuPermissions.Where(x => x.Id == ID));
                        db.SaveChanges();

                        db.AspNetRoles.RemoveRange(db.AspNetRoles.Where(x => x.Id == ID));
                        db.SaveChanges();

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete single " + this.ApiName + " and related Permissions";
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
        public async Task<IHttpActionResult> RemoveRecords(Role data)
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
                        int count = 0;
                        bool flag = false;
                        var ids = data.dsList.Select(o => o.Id).ToArray();
                        var cd = await db.AspNetRoles.Where(x => ids.Contains(x.Id)).Select(x => new { x.Id, x.Name, x.IndexPage, Published = x.Published.ToString() }).ToListAsync();

                        foreach (var ds in data.dsList)
                        {
                            var result = db.AspNetUserRoles.Where(x => x.RoleId == ds.Id).Count();
                            if (result > 0)
                            {
                                flag = true;
                                continue;
                            } else
                            {
                                count++;
                                db.AspNetUsersMenuPermissionControls.RemoveRange(db.AspNetUsersMenuPermissionControls.Where(x => x.vRoleID == ds.Id));
                                db.SaveChanges();

                                db.AspNetUsersMenuPermissions.RemoveRange(db.AspNetUsersMenuPermissions.Where(x => x.Id == ds.Id));
                                db.SaveChanges();

                                db.AspNetRoles.RemoveRange(db.AspNetRoles.Where(x => x.Id == ds.Id));
                                db.SaveChanges();
                            }
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        AuditTrail log = new AuditTrail();
                        log.EventType = "DELETE";
                        log.Description = "Delete list of " + this.ApiName + " and related Permissions";
                        log.PageUrl = this.PageUrl;
                        log.ObjectType = this.GetType().Name;
                        log.EventName = this.ApiName;
                        log.ContentDetail = JsonConvert.SerializeObject(cd);
                        log.SaveTransactionLogs();
                        // ---------------- End Transaction Activity Logs -------------------- //

                        if(flag)
                            return BadRequest("Unable to delete role with exisisting user(s)");

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