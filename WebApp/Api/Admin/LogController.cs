using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using WebApp.Models;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/Log")]
    public class LogController : ApiController
    {
        private string PageUrl = "/Admin/Log";
        private string ApiName = "Log";

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

        [Route("GetLoginHistory")]
        public async Task<IHttpActionResult> GetLoginHistory([FromUri] FilterModel param)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var permissionCtrl = this.GetPermissionControl(param.PageUrl);

                    var loginList = await (from anu in db.AspNetUsers
                                           join anulh in db.AspNetUsersLoginHistories on anu.Id equals anulh.Id
                                           join anup in db.AspNetUsersProfiles on anu.Id equals anup.Id into joined
                                           from anup in joined.DefaultIfEmpty()
                                           select new
                                           {
                                               anu.Id,
                                               Name = anup.vFirstName + " " + anup.vLastName,
                                               anu.Email,
                                               Login = anulh.dLogIn,
                                               Logout = anulh.dLogOut,
                                               IP = anulh.nvIPAddress,
                                               LoginDate = DbFunctions.TruncateTime(anulh.dLogIn)
                                           }).OrderByDescending(x=>x.Login).ToListAsync();
                    var totalLogin = loginList.Count();
                    var highestLogin = (from ll in loginList
                                        group ll.Id by ll.Id into l
                                        select new
                                        {
                                            Id = l.Key,
                                            Count = l.ToList().Count(),
                                            loginList.Where(x => x.Id == l.Key).FirstOrDefault().Name
                                        }).OrderByDescending(x => x.Count).FirstOrDefault();

                    var date = DateTime.Now.Date;
                    var todayTotalLogin = loginList.Where(x => x.LoginDate == date).Count();

                    var data = new { LOGINLIST = loginList, TOTALLOGIN = totalLogin, HIGHESTLOGINBY = highestLogin.Name, HIGHESTLOGIN = highestLogin.Count, TODAYTOTALLOGIN = todayTotalLogin };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }

        [Route("GetPageVisited")]
        public async Task<IHttpActionResult> GetPageVisited()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var visitedList = await (from anu in db.AspNetUsers
                                           join anupv in db.AspNetUsersPageVisiteds on anu.Id equals anupv.Id
                                           join anup in db.AspNetUsersProfiles on anu.Id equals anup.Id into joined
                                           from anup in joined.DefaultIfEmpty()
                                           select new
                                           {
                                               anu.Id,
                                               Name = anup.vFirstName + " " + anup.vLastName,
                                               anu.Email,
                                               PageName = anupv.nvPageName,
                                               VisitedDate = anupv.dDateVisited,
                                               IP = anupv.nvIPAddress,
                                               Date = DbFunctions.TruncateTime(anupv.dDateVisited)
                                           }).OrderByDescending(x => x.VisitedDate).ToListAsync();

                    var totalVisit = visitedList.Count();

                    var highestVisit = (from vl in visitedList
                                        group vl.PageName by vl.PageName into l
                                        select new
                                        {
                                            PageName = l.Key,
                                            Count = l.ToList().Count()
                                        }).OrderByDescending(x => x.Count).FirstOrDefault();

                    var highestVisitedBy = (from vl in visitedList
                                            group vl.Id by vl.Id into l
                                            select new
                                            {
                                                Id = l.Key,
                                                Count = l.ToList().Count(),
                                                visitedList.Where(x => x.Id == l.Key).FirstOrDefault().Name
                                            }).OrderByDescending(x => x.Count).FirstOrDefault();

                    var date = DateTime.Now.Date;
                    var todayTotalVisit = visitedList.Where(x => x.Date == date).Count();

                    var data = new { VISITEDLIST = visitedList, TOTALVISIT = totalVisit, HIGHESTVISITEDPAGE = highestVisit.PageName, HIGHESTVISITEDBY = highestVisitedBy.Name, TODAYTOTALVISIT = todayTotalVisit };
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