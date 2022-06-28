using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/Dashboard")]
    public class DashboardController : ApiController
    {
        private string PageUrl = "/Admin/Dashboard";
        private string ApiName = "Dashboard";

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

                    var userlist = await (db.AspNetUsers).ToListAsync();
                    var totalUser = userlist.Count();
                    var activeUser = userlist.Where(x => x.EmailConfirmed == true).Count();
                    var totalLogin = await (db.AspNetUsersLoginHistories).CountAsync();
                    var totalPageVisit = await (db.AspNetUsersPageVisiteds).CountAsync();
                    var roles = await (db.AspNetRoles).ToListAsync();
                    var pages = await (db.AspNetUsersPageVisiteds.GroupBy(x=>x.nvPageName).OrderByDescending(x=>x.Count())).ToListAsync();

                    var RWUDataList = new List<dynamic>();
                    var rwuLabels = new List<string>();
                    var rwuData = new List<int>();
                    var rwuBackgroundColor = new List<string>();
                    var random = new Random();
                    foreach(var role in roles)
                    {
                        rwuLabels.Add(role.Name);
                        var userCount = db.AspNetUserRoles.Where(x => x.RoleId == role.Id).Count();
                        rwuData.Add(userCount);
                        var color = string.Format("#{0:X6}", random.Next(0x1000000));
                        rwuBackgroundColor.Add(color);
                    }
                    RWUDataList.Add(rwuLabels);
                    RWUDataList.Add(rwuData);
                    RWUDataList.Add(rwuBackgroundColor);

                    var TRUDataList = new List<dynamic>();
                    var truLabels = new List<string>();
                    var truData = new List<int>();
                    var lhData = new List<List<int>>();
                    DateTime today = DateTime.Now;
                    for(var i = 5; i >= 0; i--)
                    {
                        var predate = today.AddMonths(-i);
                        var month = predate.ToString("MMM");
                        var userCount = (from ur in userlist
                                         where ur.Date.Month == predate.Month && ur.Date.Year == predate.Year
                                         select ur).Count();

                        var countList = new List<int>();
                        var days = DateTime.DaysInMonth(predate.Year, predate.Month);
                        for (var j = 1; j <= days; j++)
                        {
                            var loginCount = (from lh in db.AspNetUsersLoginHistories
                                              where lh.dLogIn.Month == predate.Month && lh.dLogIn.Year == predate.Year && lh.dLogIn.Day == j
                                              select lh).Count();
                            countList.Add(loginCount);
                            if (i == 0 && today.Day == j)
                                break;
                        }

                        lhData.Add(countList);
                        truLabels.Add(month);
                        truData.Add(userCount);
                    }
                    TRUDataList.Add(truLabels);
                    TRUDataList.Add(truData);

                    var TPVDataList = new List<dynamic>();
                    var tpvLabels = new List<string>();
                    var tpvData = new List<int>();
                    foreach(var p in pages)
                    {
                        tpvLabels.Add(p.Key.Substring(1));
                        tpvData.Add(p.Count());
                    }
                    TPVDataList.Add(tpvLabels);
                    TPVDataList.Add(tpvData);

                    var data = new { TOTALUSER = totalUser, ACTIVEUSER = activeUser, TOTALLOGIN = totalLogin, TOTALPAGEVISIT = totalPageVisit, RWUDATA = RWUDataList, TRUDATA = TRUDataList, LHDATA = lhData, TPVDATA = TPVDataList };
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