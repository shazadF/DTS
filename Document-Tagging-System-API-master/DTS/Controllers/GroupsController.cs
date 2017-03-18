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
    [Authorize]
    [RoutePrefix("api/Group")]

    public class GroupsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //User AclType
        private int Admin = 0;
        private int Manager = 1;
        private int Normal = 2;






        [Route("{id}/Users")]
        // GET: api/Groups/id
        public async Task<IHttpActionResult> GetAllUsersOfTheGroup(int id)
        {
            var groups = await (from u in db.Users
                                join ug in db.UserGroups on u.Id equals ug.UserId
                                join g in db.Groups on ug.GroupId equals g.Id
                                where g.Id == id
                                select new
                                {
                                    u.Id,
                                    u.UserName,
                                    u.Email,
                                    u.CompanyId,

                                }).ToListAsync();


            return Ok(groups);
        }





        [Route("{id}")]
        // GET: api/Groups/5
        [ResponseType(typeof(Group))]
        public async Task<IHttpActionResult> GetGroup(int id)
        {
            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
        }
        [Route("{id}")]
        // PUT: api/Groups/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutGroup(int id, Group group)
        {

            var userIdToCheckAdmin = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == userIdToCheckAdmin);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == userIdToCheckAdmin).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != group.Id)
            {
                return BadRequest();
            }

            db.Entry(group).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        [Route("")]
        // POST: api/Groups
        [ResponseType(typeof(Group))]
        public async Task<IHttpActionResult> PostGroup(Group group)
        {
            var userIdToCheckAdmin = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == userIdToCheckAdmin);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == userIdToCheckAdmin).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Groups.Add(group);
            await db.SaveChangesAsync();

            return Ok(group);
        }
        [Route("{id}")]
        // DELETE: api/Groups/5
        [ResponseType(typeof(Group))]
        public async Task<IHttpActionResult> DeleteGroup(int id)
        {
            var userIdToCheckAdmin = User.Identity.GetUserId();

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == userIdToCheckAdmin).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }

            Group group = await db.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound();
            }

            db.Groups.Remove(group);
            await db.SaveChangesAsync();

            return Ok(group);
        }



        [Route("{id:int}/User/{userIdToAddIntoGroup}")]
        // POST: api/Groups/3/User
        public async Task<IHttpActionResult> PostMemberIntoGroup(int id, string userIdToAddIntoGroup)
        {

            var userIdToCheckAdmin = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == userIdToCheckAdmin);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == userIdToCheckAdmin).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }

            //Check if Group Exists
            var aGroup = await db.Groups.FindAsync(id);

            if (aGroup == null)
            {
                return BadRequest("Group is not exist");
            }

            //check the user is already member of this group  
            var aUserGroup = await
                db.UserGroups.Where(x => x.UserId == userIdToAddIntoGroup && x.GroupId == id).SingleOrDefaultAsync();
            if (aUserGroup != null)
            {
                return BadRequest("User is already a member of this group");
            }


            // Add User to the Group 
            UserGroup userGroup = new UserGroup();
            userGroup.UserId = userIdToAddIntoGroup;
            userGroup.GroupId = id;

            db.UserGroups.Add(userGroup);
            await db.SaveChangesAsync();


            return Ok(userGroup);
        }


        [Route("{id:int}/User/{userIdToAddIntoGroup}")]
        // Delete: api/Groups/3/member
        public async Task<IHttpActionResult> DeleteMemberFromGroup(int id, string userIdToAddIntoGroup)
        {

            var userIdToCheckAdmin = User.Identity.GetUserId();
            var aUser = await db.Users.FirstAsync(x => x.Id == userIdToCheckAdmin);

            //Check the User is a Admin User
            var aUserAcl = await db.UserAcls.Where(x => x.UserId == userIdToCheckAdmin).SingleOrDefaultAsync();
            if (aUserAcl.UserType != Admin)
            {
                return BadRequest("User is not a Admin User");
            }

            //Check if Group Exists
            var aGroup = await db.Groups.FindAsync(id);

            if (aGroup == null)
            {
                return BadRequest("Group is not exist");
            }

            //check the user is not a member of this group  
            var aUserGroup = await
                db.UserGroups.Where(x => x.UserId == userIdToAddIntoGroup && x.GroupId == id).SingleOrDefaultAsync();
            if (aUserGroup == null)
            {
                return BadRequest("User is not a member of this group");
            }




            // Delete the User From the Group  

            db.UserGroups.Remove(aUserGroup);
            await db.SaveChangesAsync();


            return Ok(aUserGroup);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GroupExists(int id)
        {
            return db.Groups.Count(e => e.Id == id) > 0;
        }
    }
}