using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using DTS.Models;
using Microsoft.AspNet.Identity;

namespace DTS.Controllers
{
    [RoutePrefix("api/Company")]
    [Authorize]


    public class CompanyController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // UserAcl Defined
        private int Admin = 0;
        private int Manager = 1;
        private int Normal = 2;



        // GET: api/Company/5
        [ResponseType(typeof(Company))]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetCompany(int id)
        {
            var UserId = User.Identity.GetUserId();

            Company company = await db.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        // PUT: api/Company/5
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutCompany(int id, Company company)
        {

            var UserId = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == UserId);



            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == UserId).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }

            company.Id = id;               // add Id to the Company

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != company.Id)
            {
                return BadRequest();
            }

            db.Entry(company).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(company);
        }



        // POST: api/Company
        [ResponseType(typeof(Company))]
        [Route("")]
        public async Task<IHttpActionResult> PostCompany(Company company)
        {
            var UserId = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == UserId);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == UserId).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }



            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            company.UserId = UserId;    // add the userId to the company User
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            // add companyId to the User Model
            aUser.CompanyId = company.Id;
            await db.SaveChangesAsync();


            return Ok(company);
        }

        // DELETE: api/Company/5
        [ResponseType(typeof(Company))]
        [Route("")]
        public async Task<IHttpActionResult> DeleteCompany(int id)
        {
            var UserId = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == UserId);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == UserId).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }


            // delete all the document of the company

            // delete all the user of the company

            Company company = await db.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            db.Companies.Remove(company);
            await db.SaveChangesAsync();

            return Ok(company);
        }



        [Route("{id:int}/Users")]
        public async Task<IHttpActionResult> GetUserOfTheCompany(int id)
        {
            var UserId = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == UserId);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == UserId).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }



            if (!CompanyExists(id))
            {
                return BadRequest("Company is not Exist");
            }



            var UserList = await (from u in db.Users
                                  join up in db.UserProfiles on u.Id equals up.UserId
                                  join uacl in db.UserAcls on u.Id equals uacl.UserId
                                  where u.CompanyId == id
                                  orderby uacl.UserType descending
                                  select new
                                  {
                                      u.Id,
                                      u.UserName,
                                      u.Email,
                                      u.CompanyId,
                                      up.FirstName,
                                      up.LastName,
                                      uacl.UserType,
                                  }).ToListAsync();
            return Ok(UserList);
        }

        [Route("{id:int}/Documents")]
        public async Task<IHttpActionResult> GetDocumentsOfTheCompany(int id)
        {

            var UserId = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == UserId);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == UserId).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }


            if (!CompanyExists(id))
            {
                return BadRequest("Company is not Exist");
            }

            var DocumentList = await (from c in db.Companies
                                      join d in db.Documents on c.Id equals d.CompanyId
                                      where c.Id == id
                                      select new
                                      {
                                          d.UserId,
                                          d.CompanyId,
                                          d.FileId,
                                          d.DocumentACLId,
                                          d.DocType,
                                          d.Name,
                                      }).ToListAsync();
            return Ok(DocumentList);
        }

        [Route("{CompanyId}/Groups")]
        // GET: api/Company/{CompanyId}/Groups
        public async Task<IHttpActionResult> GetAllGroupsOfCompany(int CompanyId)
        {

            List<Group> groups = await db.Groups.Where(x => x.CompanyId == CompanyId).ToListAsync();
            return Ok(groups);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CompanyExists(int id)
        {
            return db.Companies.Count(e => e.Id == id) > 0;
        }
    }
}