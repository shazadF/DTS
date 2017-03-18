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
using DTS.Models.DTO;
using Microsoft.AspNet.Identity;

namespace DTS.Controllers
{
    [Authorize]
    [RoutePrefix("api/tag")]
    public class TagController : ApiController
    {

        //User AclType
        private int Admin = 0;
        private int Manager = 1;
        private int Normal = 2;


        private ApplicationDbContext db = new ApplicationDbContext();



        // GET: api/Tag/5
        [ResponseType(typeof(Tag))]
        [Route("{id}")]
        public async Task<IHttpActionResult> GetTag(int id)
        {
            Tag tag = await db.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            return Ok(tag);
        }

        // PUT: api/Tag/5
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutTag(int id, Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tag.Id)
            {
                return BadRequest();
            }


            db.Entry(tag).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TagExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            //Edit history info
            History aHistory = await db.Histories.Where(x => x.DocumentId == tag.DocumentId).SingleOrDefaultAsync();
            //aHistory.AddedOn = DateTime.Now;
            aHistory.EditedOn = DateTime.Now;
            //aHistory.DocumentId = document.Id;
            //db.Histories.Add(aHistory);
            await db.SaveChangesAsync();


            //Add History Sequence Info
            HistorySequence aHistorySequence = new HistorySequence();

            aHistorySequence.HistoryId = aHistory.Id;
            aHistorySequence.Message = "tag " + id + " is changed";
            aHistorySequence.Date = aHistory.EditedOn;
            aHistorySequence.UserId = User.Identity.GetUserId();
            db.HistorySequences.Add(aHistorySequence);
            await db.SaveChangesAsync();





            return Ok(tag);
        }




        // POST api/Tag
        [ResponseType(typeof(TagDTO))]
        [Route("")]

        public async Task<IHttpActionResult> PostTag(TagDTO tag)
        {
            Tag aTag = new Tag();
            //History aHistory = new History();
            HistorySequence aHistorySequence = new HistorySequence();
            SequenceACL aSequenceAcl = new SequenceACL();
            bool updatePresentAccess = false;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            //Check the doc is exist
            Document document = await db.Documents.FindAsync(tag.DocumentId);
            if (document == null)
            {
                return BadRequest("Document is not exist");
            }


            // check the document acl
            DocumentACL documentAcl = await db.DocumentAcls.Where(x => x.DocumentId == tag.DocumentId).SingleOrDefaultAsync();
            if (documentAcl.DocAclType == 0)
            {
                if (document.UserId != tag.UserId)
                {
                    return BadRequest("This document is private and this user is not authorized to tag this document");
                }
                else
                {
                    aTag.DocumentId = tag.DocumentId;
                    aTag.TagContent = tag.TagContent;
                    aTag.TagColor = tag.TagColor;
                    aTag.UserId = tag.UserId;
                    db.Tags.Add(aTag);
                }
            }

            else if (documentAcl.DocAclType == 1)
            {
                var CompanyUser = await
                    db.Users.Where(x => x.CompanyId == document.CompanyId && x.Id == tag.UserId).SingleOrDefaultAsync();

                if (CompanyUser == null)
                {
                    return BadRequest("This User is not belongs to this company");
                }
                else
                {
                    aTag.DocumentId = tag.DocumentId;
                    aTag.TagContent = tag.TagContent;
                    aTag.TagColor = tag.TagColor;
                    aTag.UserId = tag.UserId;
                    db.Tags.Add(aTag);
                }
            }

            else if (documentAcl.DocAclType == 3)
            {
                Restricted aRestricted = await db.Restricteds.Where(x => x.DocAclId == documentAcl.Id).SingleOrDefaultAsync();
                List<string> tagList = aRestricted.List.Split(',').ToList();
                //Check the User is in the restricted list
                DocumentUser aDocumentUser =
                    await db.DocumentUsers.Where(x => x.UserId == tag.UserId).SingleOrDefaultAsync();
                if (aDocumentUser == null)
                {
                    return BadRequest("The Document is restriced and this user is not authorize to use it ");
                }

                else
                {
                    aTag.DocumentId = tag.DocumentId;
                    aTag.TagContent = tag.TagContent;
                    aTag.TagColor = tag.TagColor;
                    aTag.UserId = tag.UserId;
                    db.Tags.Add(aTag);
                }
            }

            else if (documentAcl.DocAclType == 2)
            {
                bool canTag = false;

                aSequenceAcl = await db.SequenceAcls.Where(x => x.DocAclId == documentAcl.Id).SingleOrDefaultAsync();
                List<string> tagList = aSequenceAcl.Sequence.Split(',').ToList();

                //
                var presentUserInList = tagList[aSequenceAcl.PresentAccess];
                var len = presentUserInList.Length;


                if (len != 36 && presentUserInList[0] == 'g')
                {
                    var groupId = Convert.ToInt64(presentUserInList.Substring(1));

                    // check the user is in the group
                    var userIsIngroup = await (from ug in db.UserGroups
                                               join u in db.Users on ug.UserId equals u.Id
                                               where ug.GroupId == groupId && ug.UserId == tag.UserId
                                               select new
                                               {
                                                   Id = u.Id,
                                               }).SingleOrDefaultAsync();

                    if (userIsIngroup != null)
                    {
                        var userPrivilageforTag = await
                            db.UserPrivilages.Where(x => x.UserId == userIsIngroup.Id).SingleOrDefaultAsync();

                        if (userPrivilageforTag.CanTagDocument == 1)
                        {
                            canTag = true;
                        }
                    }
                    else
                    {
                        return BadRequest("User is not in authorize to tag the document");
                    }


                }

                else
                {
                    int index = tagList.IndexOf(tag.UserId);
                    if (index == -1)
                    {
                        return
                            BadRequest("The Document is Sequencial-restriced and this user is not authorize to use it ");
                    }
                    else if (tagList.Count - 1 < aSequenceAcl.PresentAccess)
                    {
                        return
                            BadRequest(
                                "The Document is Sequencial-restriced. All the User has already tagged this document.");
                    }
                    else if (tagList[aSequenceAcl.PresentAccess] != tag.UserId)
                    {
                        return
                            BadRequest(
                                "The Document is Sequencial-restriced. User trun to tag the document is not come.");
                    }
                    else
                    {
                        canTag = true;
                    }
                }

                if (canTag)
                {
                    aTag.DocumentId = tag.DocumentId;
                    aTag.TagContent = tag.TagContent;
                    aTag.TagColor = tag.TagColor;
                    aTag.UserId = tag.UserId;
                    db.Tags.Add(aTag);
                    updatePresentAccess = true;
                }

            }




            await db.SaveChangesAsync();

            //Edit history info
            History aHistory = await db.Histories.Where(x => x.DocumentId == tag.DocumentId).SingleOrDefaultAsync();
            //aHistory.AddedOn = DateTime.Now;
            aHistory.EditedOn = DateTime.Now;
            //aHistory.DocumentId = document.Id;
            //db.Histories.Add(aHistory);
            await db.SaveChangesAsync();

            //Add History Sequence Info

            aHistorySequence.HistoryId = aHistory.Id;
            aHistorySequence.Message = tag.Message;
            aHistorySequence.Date = aHistory.EditedOn;
            aHistorySequence.UserId = User.Identity.GetUserId();
            db.HistorySequences.Add(aHistorySequence);
            await db.SaveChangesAsync();


            //Update the present Access if it is a Sequencial Restricted Document
            if (updatePresentAccess)
            {
                aSequenceAcl.PresentAccess += 1;
            }
            await db.SaveChangesAsync();
            return Ok(aTag);
        }
        // DELETE: api/Tag/5
        [Route("{id}")]
        [ResponseType(typeof(Tag))]
        public async Task<IHttpActionResult> DeleteTag(int id)
        {
            Tag tag = await db.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            db.Tags.Remove(tag);
            await db.SaveChangesAsync();

            //Edit history info
            History aHistory = await db.Histories.Where(x => x.DocumentId == tag.DocumentId).SingleOrDefaultAsync();
            //aHistory.AddedOn = DateTime.Now;
            aHistory.EditedOn = DateTime.Now;
            //aHistory.DocumentId = document.Id;
            //db.Histories.Add(aHistory);
            await db.SaveChangesAsync();


            //Add History Sequence Info
            HistorySequence aHistorySequence = new HistorySequence();

            aHistorySequence.HistoryId = aHistory.Id;
            aHistorySequence.Message = "tag " + id + " is changed";
            aHistorySequence.Date = aHistory.EditedOn;
            aHistorySequence.UserId = User.Identity.GetUserId();
            db.HistorySequences.Add(aHistorySequence);
            await db.SaveChangesAsync();




            return Ok(tag);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TagExists(int id)
        {
            return db.Tags.Count(e => e.Id == id) > 0;
        }
    }
}