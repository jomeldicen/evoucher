using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;
using System.Data.Entity;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/PendingEmailVerification")]
    public class PendingEmailVerificationController : ApiController
    {
        public async Task<IHttpActionResult> Get()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var getData = await (from bs in db.AspNetUsers
                                         where bs.EmailConfirmed == false
                                         select new
                                         {
                                             bs.Id,
                                             bs.Email,
                                             bs.EmailConfirmed,
                                             bs.UserName,
                                             uType = bs.AspNetUserRoles.Where(x => x.UserId == bs.Id).FirstOrDefault().AspNetRole.Name
                                         }).ToListAsync();

                    var data = new { PEVLIST = getData };
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