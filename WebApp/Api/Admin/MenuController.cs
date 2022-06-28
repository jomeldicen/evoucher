using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using WebApp.Helper;
using Newtonsoft.Json;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/Menu")]
    public class MenuController : ApiController
    {
        private string PageUrl = "/Admin/Menu";
        private string ApiName = "Module";

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

        // for Sidebar Menu population
        [Route("GetSideMenu")]
        public async Task<IHttpActionResult> GetMenuList()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var roleId = db.AspNetUserRoles.Where(x => x.UserId == userId).FirstOrDefault().RoleId;
                    var menuData = await (from mn in db.AspNetUsersMenus
                                          join anump in db.AspNetUsersMenuPermissions on mn.vMenuID equals anump.vMenuID
                                          where anump.Id == roleId
                                          select new
                                          {
                                              mn.vMenuID,
                                              mn.vParentMenuID,
                                              mn.iSerialNo,
                                              mn.nvMenuName,
                                              mn.nvPageUrl,
                                              mn.nvFabIcon,
                                              mn.PrefixCode
                                          }).OrderBy(x => x.iSerialNo).ToListAsync();
                    
                    List<Menu> menuList = new List<Menu>();
                    foreach(var mn in menuData)
                    {
                        Menu menu = new Menu();
                        menu.vMenuID = mn.vMenuID;
                        menu.vParentMenuID = mn.vParentMenuID;
                        menu.iSerialNo = mn.iSerialNo;
                        menu.nvMenuName = mn.nvMenuName;
                        menu.nvPageUrl = mn.nvPageUrl;
                        menu.nvFabIcon = mn.nvFabIcon;
                        menu.PrefixCode = mn.PrefixCode;
                        menu.Child = new List<Menu>();
                        menuList.Add(menu);
                    }
                    
                    var pMenu = menuList.Where(x => x.vParentMenuID == null).OrderBy(x => x.iSerialNo).ToList();
                    tMenu = menuList.Where(x => x.vParentMenuID != null).OrderBy(x => x.iSerialNo).ToList();

                    List<Menu> Menu = new List<Menu>();
                    foreach (var pm in pMenu)
                    {
                        Menu mn = new Menu();
                        mn = GetMenuHierarchy(pm, tMenu.ToList());
                        Menu.Add(mn);
                    }

                    var data = new { MENU = Menu };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        private Menu GetMenuHierarchy(Menu pMenu, List<Menu> menuList)
        {
            List<Menu> childList = new List<Menu>();
            List<Menu> tempList = new List<Menu>();
            foreach (var ml in menuList)
            {
                if (pMenu.vMenuID == ml.vParentMenuID)
                {
                    childList.Add(ml);
                    tMenu.Remove(ml);
                }
                else
                    tempList.Add(ml);
            }
            foreach(var cl in childList)
            {
                Menu child = new Menu();
                child = GetMenuHierarchy(cl, tempList);
                pMenu.Child.Add(child);
            }

            return pMenu;
        }

        // record list for Menu Module
        [Route("GetMenu")]
        public async Task<IHttpActionResult> GetMenu()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    //var permissionCtrl = this.GetPermissionControl();

                    var menuData = await (from mn in db.AspNetUsersMenus
                                          select new
                                          {
                                              mn.vMenuID,
                                              mn.vParentMenuID,
                                              mn.iSerialNo,
                                              mn.nvMenuName,
                                              mn.nvPageUrl,
                                              mn.nvFabIcon,
                                              mn.PrefixCode,
                                          }).OrderBy(x => x.iSerialNo).ToListAsync();

                    List<Menu> menuList = new List<Menu>();
                    foreach (var mn in menuData)
                    {
                        Menu menu = new Menu();
                        menu.vMenuID = mn.vMenuID;
                        menu.vParentMenuID = mn.vParentMenuID;
                        menu.iSerialNo = mn.iSerialNo;
                        menu.nvMenuName = mn.nvMenuName;
                        menu.nvPageUrl = mn.nvPageUrl;
                        menu.nvFabIcon = mn.nvFabIcon;
                        menu.PrefixCode = mn.PrefixCode;
                        menu.Child = new List<Menu>();
                        menuList.Add(menu);
                    }

                    // parent menu list
                    var pMenu = menuList.Where(x => x.vParentMenuID == null).OrderBy(x => x.iSerialNo).ToList();
                    tMenu = menuList.Where(x => x.vParentMenuID != null).OrderBy(x => x.iSerialNo).ToList();

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

        // record list for Menu Module
        [Route("GetMenus")]
        public async Task<IHttpActionResult> GetMenus([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var options = await (from op in db.Options
                                        where op.Published == true && op.OptionGroup == "Button Conrols"
                                        select new
                                        {
                                            id = op.Id,
                                            label = op.Description
                                        }).OrderBy(x => x.id).ToListAsync();

                    IEnumerable<Menu> source = null;
                    source = await (from mn in db.AspNetUsersMenus
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
                                    isChecked = false,
                                    OptionIDs = (from w in db.AspNetUsersMenuControls where w.vMenuID == mn.vMenuID select new IdList { id = w.OptionID }).Distinct().ToList(),
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

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.nvMenuName.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(Menu).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), MENULIST = sourcePaged, OPTIONS = options, PARENT = parent, CONTROLS = permissionCtrl };
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
        public async Task<IHttpActionResult> UpdateStatus(Menu data)
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
                        var ids = data.dsList.Select(o => o.vMenuID).ToArray();
                        var cd = db.AspNetUsersMenus.Where(x => ids.Contains(x.vMenuID)).Select(x => new { x.vMenuID, x.nvMenuName, x.iSerialNo, x.nvFabIcon, x.vParentMenuID, x.nvPageUrl, x.PrefixCode, Published = x.Published.ToString()}).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "Update AspNetUsersMenu SET Published = {1}, ModifiedByPK = {2}, ModifiedDate = {3} WHERE vMenuID = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.vMenuID, data.Published, User.Identity.GetUserId(), DateTime.Now);
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

        [Route("SaveMenu")]
        public async Task<IHttpActionResult> SaveMenu(Menu data)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);           

            if (data.OptionIDs.Count == 0 && data.nvPageUrl != "#")
                return BadRequest("Please first make a selection from controls.");

            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        bool nwe = false;
                        var cId = User.Identity.GetUserId();

                        bool isSerialExists = db.AspNetUsersMenus.Where(x => x.iSerialNo == data.iSerialNo && x.vParentMenuID == data.vParentMenuID && x.vMenuID != data.vMenuID).Any();
                        if(isSerialExists)
                            return BadRequest("Serial No. Exists");

                        db.AspNetUsersMenuControls.RemoveRange(db.AspNetUsersMenuControls.Where(x => x.vMenuID == data.vMenuID));
                        await db.SaveChangesAsync();

                        AspNetUsersMenu um = new AspNetUsersMenu();

                        um.vMenuID = data.vMenuID;
                        um.nvMenuName = data.nvMenuName;
                        um.iSerialNo = data.iSerialNo;
                        um.nvFabIcon = data.nvFabIcon;
                        um.vParentMenuID = data.vParentMenuID;
                        um.nvPageUrl = data.nvPageUrl;
                        um.PrefixCode = data.PrefixCode;
                        um.Published = (data.Published == "True") ? true : false;
                        um.ModifiedByPK = cId;
                        um.ModifiedDate = DateTime.Now;
                        if (um.vMenuID == null)
                        {
                            nwe = true;
                            um.CreatedByPK = cId;
                            um.CreatedDate = DateTime.Now;
                            um.vMenuID = Guid.NewGuid().ToString();
                            db.AspNetUsersMenus.Add(um);
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            um.CreatedByPK = data.CreatedByPK;
                            um.CreatedDate = data.CreatedDate;
                            db.Entry(um).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        // loop selected controls
                        foreach (var op in data.OptionIDs)
                        {
                            AspNetUsersMenuControl uc = new AspNetUsersMenuControl();
                            uc.vMenuControlID = Guid.NewGuid().ToString();
                            uc.vMenuID = um.vMenuID;
                            uc.OptionID = op.id;
                            db.AspNetUsersMenuControls.Add(uc);
                            await db.SaveChangesAsync();
                        }

                        dbContextTransaction.Commit();

                        // ---------------- Start Transaction Activity Logs ------------------ //
                        var cd = db.AspNetUsersMenus.Where(x => x.vMenuID == um.vMenuID).Select(x => new { x.vMenuID, x.nvMenuName, x.iSerialNo, x.nvFabIcon, x.vParentMenuID, x.nvPageUrl, x.PrefixCode, Published = x.Published.ToString() }).SingleOrDefault();

                        AuditTrail log = new AuditTrail();
                        log.EventType = (nwe) ? "CREATE" : "UPDATE";
                        log.Description = (nwe) ? "Create " + this.ApiName : "Update " + this.ApiName;
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
                        var cd = db.AspNetUsersMenus.Where(x => x.vMenuID == ID).Select(x => new { x.vMenuID, x.nvMenuName, x.iSerialNo, x.nvFabIcon, x.vParentMenuID, x.nvPageUrl, x.PrefixCode, Published = x.Published.ToString() }).SingleOrDefault();

                        db.AspNetUsersMenuControls.RemoveRange(db.AspNetUsersMenuControls.Where(x => x.vMenuID == ID));
                        db.SaveChanges();

                        db.AspNetUsersMenuPermissions.RemoveRange(db.AspNetUsersMenuPermissions.Where(x => x.vMenuID == ID));
                        db.SaveChanges();

                        db.AspNetUsersMenus.RemoveRange(db.AspNetUsersMenus.Where(x => x.vMenuID == ID));
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
        public async Task<IHttpActionResult> RemoveRecords(Menu data)
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
                        var ids = data.dsList.Select(o => o.vMenuID).ToArray();
                        var cd = db.AspNetUsersMenus.Where(x => ids.Contains(x.vMenuID)).Select(x => new { x.vMenuID, x.nvMenuName, x.iSerialNo, x.nvFabIcon, x.vParentMenuID, x.nvPageUrl, x.PrefixCode, Published = x.Published.ToString() }).ToList();

                        foreach (var ds in data.dsList)
                        {
                            var sql = "DELETE FROM AspNetUsersMenuControls WHERE vMenuID = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.vMenuID);

                            sql = "DELETE FROM AspNetUsersMenuPermission WHERE vMenuID = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.vMenuID);

                            sql = "DELETE FROM AspNetUsersMenu WHERE vMenuID = {0}";
                            await db.Database.ExecuteSqlCommandAsync(sql, ds.vMenuID);
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