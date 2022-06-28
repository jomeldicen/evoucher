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
using Newtonsoft.Json;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/ChangeLog")]
    public class ChangeLogController : ApiController
    {
        private string PageUrl = "/Admin/ChangeLog";

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

        [Route("GetChangeLog")]
        public async Task<IHttpActionResult> GetChangeLog([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    //multiple Filters
                    var a = JsonConvert.DeserializeObject<Dictionary<string, string>>(param.multiplesearch[0]);
                    string ObjectType = null;

                    if (a.ToList().Count() == 2)
                    {
                        foreach (KeyValuePair<string, string> i in a.ToList())
                        {
                            if (i.Key == "ObjectType" && !string.IsNullOrWhiteSpace(i.Value))
                            {
                                if (i.Value != "") ObjectType = i.Value.ToString();
                            }
                        }
                    }

                    var userID = User.Identity.GetUserId();
                    IEnumerable<CustomChangeLog> source = null;
                    source = await (from log in db.ChangeLogs
                                    where log.ObjectType == ObjectType
                                    select new CustomChangeLog
                                    {
                                        Id = log.Id,
                                        vMenuID = log.vMenuID,
                                        EventType = log.EventType,
                                        EventName = log.EventName,
                                        Description = log.Description,
                                        ObjectType = log.ObjectType,
                                        CreatedByPK = log.CreatedByPK,
                                        CreatedDate = log.CreatedDate,
                                        UserName = string.Concat(log.AspNetUser.AspNetUsersProfile.vLastName, ", ", log.AspNetUser.AspNetUsersProfile.vFirstName)
                                    }).OrderBy(x => x.CreatedDate).ToListAsync();

                    // searching
                    if (!string.IsNullOrWhiteSpace(param.search))
                    {
                        param.search = param.search.ToLower();
                        source = source.Where(x => x.EventName.ToLower().Contains(param.search) || x.ObjectType.ToLower().Contains(param.search));
                    }

                    // sorting
                    var sortby = typeof(CustomChangeLog).GetProperty(param.sortby);
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

                    var data = new { COUNT = source.Count(), CHANGELOGLIST = sourcePaged,  };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }
    }
}