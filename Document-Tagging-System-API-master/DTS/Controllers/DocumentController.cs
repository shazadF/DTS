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
using System.Web.UI;
using DTS.Models;
using DTS.Models.DTO;
using Microsoft.AspNet.Identity;

namespace DTS.Controllers
{
    [Authorize]
    [RoutePrefix("api/Document")]
    public class DocumentController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        //User AclType
        private int Admin = 0;
        private int Manager = 1;
        private int Normal = 2;

        private int aclSequencialRestricted = 2;
        private int aclRestricted = 3;



        // GET api/Document/5
        [ResponseType(typeof(DocumentAllInfoDTO))]
        [Route("{id:int}")]
        public async Task<IHttpActionResult> GetDocument(int id)
        {


            var document = await db.Documents.FindAsync(id);
            var documentAcl = await db.DocumentAcls.Where(x => x.DocumentId == id).SingleOrDefaultAsync();
            var file = await db.Files.Where(x => x.DocumentId == id).SingleOrDefaultAsync();

            List<Tag> tags = await (from d in db.Documents
                                    join t in db.Tags on d.Id equals t.DocumentId
                                    where d.Id == id
                                    select t).ToListAsync();

            History aHistory = await db.Histories.Where(x => x.DocumentId == id).SingleOrDefaultAsync();

            List<HistorySequence> HistorySequences = await (from h in db.Histories
                                                            join hs in db.HistorySequences on h.Id equals hs.HistoryId
                                                            where h.Id == aHistory.Id
                                                            select hs).ToListAsync();

            if (document == null)
            {
                return NotFound();
            }



            DocumentAllInfoDTO documentInfo = new DocumentAllInfoDTO();
            documentInfo.Name = document.Name;
            documentInfo.Id = document.Id;
            documentInfo.UserId = document.UserId;
            documentInfo.CompanyId = document.CompanyId;
            documentInfo.DocType = document.DocType;
            documentInfo.FileId = document.FileId;
            documentInfo.Content = file.Content;
            documentInfo.DocumentACLType = documentAcl.DocAclType;

            documentInfo.Tags = tags;
            documentInfo.History = aHistory;
            documentInfo.HistorySequences = HistorySequences;



            return Ok(documentInfo);
        }


        [Route("{id}/UpdateFile")]
        public async Task<IHttpActionResult> putUpdateFileContent(int id, File file)
        {


            // 
            var UserId = User.Identity.GetUserId();
            var aDocumentforValidation = await db.Documents.FindAsync(id);

            if (aDocumentforValidation.UserId != UserId)
            {
                return BadRequest("User is not the owner of the document");
            }



            Document aDocument = await db.Documents.FindAsync(id);
            if (aDocument.UserId != UserId)
            {
                return BadRequest("User is not the owner of the document");
            }

            // Get the File 
            File aFile = await db.Files.Where(x => x.DocumentId == id).SingleOrDefaultAsync();

            // Update the File
            aFile.Content = file.Content;
            await db.SaveChangesAsync();


            // History and History Sequence change korte hobe. 

            return Ok(aFile);
        }



        // PUT: api/Document/5
        [ResponseType(typeof(void))]
        [Route("{id}")]
        public async Task<IHttpActionResult> PutDocument(int id, Document document)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            // User is not the owner of the document
            var UserId = User.Identity.GetUserId();
            if (document.UserId != UserId)
            {
                return BadRequest("User is not the owner of the document");
            }

            Document aDocument = await db.Documents.FindAsync(id);
            // Only document Name and Type can be changed
            aDocument.Name = document.Name;
            aDocument.DocType = document.DocType;
            db.Entry(aDocument).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
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

        // POST api/Document
        [ResponseType(typeof(DocumentDTO))]
        [Route("")]
        public async Task<IHttpActionResult> PostDocument(DocumentDTO document)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check the User have the Privelage to add Document
            var userIdToCheckPrivilage = User.Identity.GetUserId();
            var userPrivilage = await db.UserPrivilages.FirstAsync(x => x.UserId == userIdToCheckPrivilage);

            if (userPrivilage.CanAddDocument != 1)
            {
                return BadRequest("User doens't have the Permission to add Document");
            }


            // UserId and CompanyId must be correct
            var UserId = User.Identity.GetUserId();
            var aUser = await db.Users.Where(x => x.Id == UserId).SingleOrDefaultAsync();
            document.UserId = aUser.Id;
            document.CompanyId = aUser.CompanyId;

            //Save Document    
            Document aDocument = new Document();
            aDocument.UserId = document.UserId;
            aDocument.CompanyId = document.CompanyId;
            aDocument.DocType = document.DocType;
            aDocument.Name = document.Name;

            db.Documents.Add(aDocument);
            await db.SaveChangesAsync();


            //Save File
            File aFile = new File();
            aFile.Content = document.FileContent;
            aFile.DocumentId = aDocument.Id;

            db.Files.Add(aFile);
            await db.SaveChangesAsync();


            //Save History
            History aHistory = new History();
            aHistory.AddedOn = DateTime.Now;
            aHistory.EditedOn = DateTime.Now;
            aHistory.DocumentId = aDocument.Id;

            db.Histories.Add(aHistory);
            await db.SaveChangesAsync();


            // Save DocumentAcl
            DocumentACL aDocumentAcl = new DocumentACL();
            aDocumentAcl.DocumentId = aDocument.Id;
            if (document.DocumentACLType != 0)
            {
                document.DocumentACLType = 0;
            }
            aDocumentAcl.DocAclType = document.DocumentACLType;        // By Default Private

            db.DocumentAcls.Add(aDocumentAcl);
            await db.SaveChangesAsync();


            //Save HistorySequence
            HistorySequence aHistorySequence = new HistorySequence();
            aHistorySequence.HistoryId = aHistory.Id;
            aHistorySequence.Message = aDocument.Name + " file is Created.";
            aHistorySequence.Date = aHistory.AddedOn;
            aHistorySequence.UserId = document.UserId;

            db.HistorySequences.Add(aHistorySequence);
            await db.SaveChangesAsync();

            //Add FileId to the Document
            aDocument.FileId = aFile.Id;
            aDocument.DocumentACLId = aDocumentAcl.Id;
            await db.SaveChangesAsync();



            return Ok(aDocument);
        }



        // DELETE: api/Document/5
        [ResponseType(typeof(Document))]
        public async Task<IHttpActionResult> DeleteDocument(int id)
        {

            // Check the User have the Privelage to Delete Document
            var userIdToCheckPrivilage = User.Identity.GetUserId();
            var userPrivilage = await db.UserPrivilages.FirstAsync(x => x.UserId == userIdToCheckPrivilage);

            if (userPrivilage.CanDeleteDocument != 1)
            {
                return BadRequest("User doens't have the Permission to add Document");
            }

            //delete Document
            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            //Delete File



            db.Documents.Remove(document);
            await db.SaveChangesAsync();

            return Ok(document);
        }

        [Route("{id:int}/tags")]
        public async Task<IHttpActionResult> GetDocumentTags(int id)
        {


            var document = await (from d in db.Documents
                                  join t in db.Tags on d.Id equals t.DocumentId into gj
                                  from tagInfo in gj.DefaultIfEmpty()
                                  where d.Id == id
                                  select tagInfo).ToListAsync();

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }

        [Route("{id:int}/histories")]
        public async Task<IHttpActionResult> GetDocumentHistories(int id)
        {


            var document = await (from d in db.Documents
                                  join h in db.Histories on d.Id equals h.DocumentId
                                  join hs in db.HistorySequences on h.Id equals hs.HistoryId
                                  where d.Id == id
                                  select new
                                  {
                                      h.Id,
                                      h.AddedOn,
                                      h.EditedOn,
                                      h.DocumentId,
                                      History = hs,
                                  }).ToListAsync();

            if (document == null)
            {
                return NotFound();
            }

            return Ok(document);
        }



        [Route("{id}/CreateRestricted")]
        public async Task<IHttpActionResult> PostCreateRestrictedSequence(int id, Restricted restricted)
        {
            //Check the user is a Admin User
            var userId = User.Identity.GetUserId();
            UserACL userAclForAdmin = await db.UserAcls.Where(x => x.UserId == userId).SingleOrDefaultAsync();
            if (userAclForAdmin.UserType != Admin)
            {
                return BadRequest("Only Admin User can Update User ACL");
            }



            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }





            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Check Restricted List is already created.
            var restrictedListToCheck = await
                db.Restricteds.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();
            if (restrictedListToCheck != null)
            {
                return BadRequest("Restricted list is already Exist.");
            }

            db.Restricteds.Add(restricted);
            await db.SaveChangesAsync();


            // change the DocAclType in DocumentAcl
            DocumentACL aDocumentAcl = await db.DocumentAcls.Where(x => x.DocumentId == id).SingleOrDefaultAsync();
            aDocumentAcl.DocAclType = 3;
            await db.SaveChangesAsync();


            // Check Document have a Sequencial sequence.
            var SequencialListToCheck = await
                db.SequenceAcls.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();
            if (SequencialListToCheck != null)
            {
                db.SequenceAcls.Remove(SequencialListToCheck);
                await db.SaveChangesAsync();
            }


            //Delete the Document from the DocumentUser Table...
            db.DocumentUsers.RemoveRange(db.DocumentUsers.Where(x => x.DocumentId == id));
            await db.SaveChangesAsync();


            // Add user To the DocumentUser Model
            List<string> userInTagList = new List<string>();
            userInTagList = restricted.List.Split(',').ToList();
            foreach (var userInList in userInTagList)
            {
                var len = userInList.Length;
                if (len != 36 && userInList[0] == 'g')
                {
                    var groupId = Convert.ToInt64(userInList.Substring(1));
                    //Check the Group is Exist
                    var groupIsExist = await db.Groups.FindAsync(groupId);

                    // Get all the user of the Group
                    var groupUsers = await (from u in db.Users
                                            join ug in db.UserGroups on u.Id equals ug.UserId
                                            join g in db.Groups on ug.GroupId equals g.Id
                                            where g.Id == groupId
                                            select new
                                            {
                                                u.Id,
                                                u.UserName,
                                                u.Email,
                                                u.CompanyId,

                                            }).ToListAsync();

                    // Add User to the Document

                    foreach (var userInGroup in groupUsers)
                    {
                        //check the user is already member of this Document  
                        var aDocumentUser = await
                            db.DocumentUsers.Where(x => x.UserId == userInGroup.Id && x.DocumentId == document.Id).SingleOrDefaultAsync();
                        if (aDocumentUser == null)
                        {
                            DocumentUser documentUser = new DocumentUser();
                            documentUser.UserId = userInGroup.Id;
                            documentUser.DocumentId = document.Id;

                            db.DocumentUsers.Add(documentUser);
                            await db.SaveChangesAsync();
                        }

                    }

                }
                else
                {
                    //check the user is already member of this Document  
                    var aDocumentUser = await
                        db.DocumentUsers.Where(x => x.UserId == userInList && x.DocumentId == document.Id).SingleOrDefaultAsync();
                    if (aDocumentUser == null)
                    {
                        DocumentUser documentUser = new DocumentUser();
                        documentUser.UserId = userInList;
                        documentUser.DocumentId = document.Id;

                        db.DocumentUsers.Add(documentUser);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return Ok(restricted);
        }



        [Route("{id}/GetRestricted")]
        public async Task<IHttpActionResult> GetRestrictedSequence(int id)
        {
            //Check the user is a Admin User
            var userId = User.Identity.GetUserId();
            UserACL userAclForAdmin = await db.UserAcls.Where(x => x.UserId == userId).SingleOrDefaultAsync();
            if (userAclForAdmin.UserType != Admin)
            {
                return BadRequest("Only Admin User can get Restricted list");
            }

            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Get the RestrictedList
            Restricted aRestricted = await
                db.Restricteds.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();

            return Ok(aRestricted);

        }



        [Route("{id}/UpdateRestricted")]
        public async Task<IHttpActionResult> PutCreateRestrictedSequence(int id, Restricted restricted)
        {

            //Check the user is a Admin User
            var userId = User.Identity.GetUserId();
            UserACL userAclForAdmin = await db.UserAcls.Where(x => x.UserId == userId).SingleOrDefaultAsync();
            if (userAclForAdmin.UserType != Admin)
            {
                return BadRequest("Only Admin User can Update User ACL");
            }

            //Check DocAcl and UserId is Given
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }



            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }




            // only List is updated
            DocumentACL aDocumentAcl = await db.DocumentAcls.Where(x => x.DocumentId == id).SingleOrDefaultAsync();

            Restricted aRestricted =
                await db.Restricteds.Where(x => x.DocAclId == aDocumentAcl.Id).SingleOrDefaultAsync();


            aRestricted.List = restricted.List;

            //db.Restricteds.Add(aRestricted);
            await db.SaveChangesAsync();


            // change the DocAclType in DocumentAcl

            aDocumentAcl.DocAclType = 3;
            await db.SaveChangesAsync();

            //Delete the Document from the DocumentUser Table...
            db.DocumentUsers.RemoveRange(db.DocumentUsers.Where(x => x.DocumentId == id));
            await db.SaveChangesAsync();


            // Add User to the Document


            List<string> userInTagList = aRestricted.List.Split(',').ToList();
            foreach (var userInList in userInTagList)
            {
                var len = userInList.Length;
                if (len != 36 && userInList[0] == 'g')
                {
                    var groupId = Convert.ToInt64(userInList.Substring(1));
                    //Check the Group is Exist
                    var groupIsExist = await db.Groups.FindAsync(groupId);

                    // Get all the user of the Group
                    var groupUsers = await (from u in db.Users
                                            join ug in db.UserGroups on u.Id equals ug.UserId
                                            join g in db.Groups on ug.GroupId equals g.Id
                                            where g.Id == groupId
                                            select new
                                            {
                                                u.Id,
                                                u.UserName,
                                                u.Email,
                                                u.CompanyId,

                                            }).ToListAsync();

                    // Add User to the Document

                    foreach (var userInGroup in groupUsers)
                    {
                        //check the user is already member of this Document  
                        var aDocumentUser = await
                            db.DocumentUsers.Where(x => x.UserId == userInGroup.Id && x.DocumentId == document.Id).SingleOrDefaultAsync();
                        if (aDocumentUser == null)
                        {
                            DocumentUser documentUser = new DocumentUser();
                            documentUser.UserId = userInGroup.Id;
                            documentUser.DocumentId = document.Id;

                            db.DocumentUsers.Add(documentUser);
                            await db.SaveChangesAsync();
                        }

                    }

                }
                else
                {
                    //check the user is already member of this Document  
                    var aDocumentUser = await
                        db.DocumentUsers.Where(x => x.UserId == userInList && x.DocumentId == document.Id).SingleOrDefaultAsync();
                    if (aDocumentUser == null)
                    {
                        DocumentUser documentUser = new DocumentUser();
                        documentUser.UserId = userInList;
                        documentUser.DocumentId = document.Id;

                        db.DocumentUsers.Add(documentUser);
                        await db.SaveChangesAsync();
                    }
                }
            }



            return Ok(aRestricted);
        }



        [Route("{id}/CreateSequencial")]
        public async Task<IHttpActionResult> PostCreateSequencialRestricted(int id, SequenceACL sequence)
        {
            //Check the user is a Admin User
            var userId = User.Identity.GetUserId();
            UserACL userAclForAdmin = await db.UserAcls.Where(x => x.UserId == userId).SingleOrDefaultAsync();
            if (userAclForAdmin.UserType != Admin)
            {
                return BadRequest("Only Admin User can Create Sequencial Restricted");
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }



            // Check Sequencial List is already created.
            var sequencialListToCheck = await
                db.SequenceAcls.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();
            if (sequencialListToCheck != null)
            {
                return BadRequest("Sequencial list is already Exist.");
            }

            db.SequenceAcls.Add(sequence);
            await db.SaveChangesAsync();


            // change the DocAclType in DocumentAcl
            DocumentACL aDocumentAcl = await db.DocumentAcls.Where(x => x.DocumentId == id).SingleOrDefaultAsync();
            aDocumentAcl.DocAclType = 2;
            await db.SaveChangesAsync();



            // Check Document have a restricted sequence.
            var restrictedListToCheck = await
                db.Restricteds.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();
            if (restrictedListToCheck != null)
            {
                db.Restricteds.Remove(restrictedListToCheck);
                await db.SaveChangesAsync();
            }




            //Delete the Document from the DocumentUser Table...
            db.DocumentUsers.RemoveRange(db.DocumentUsers.Where(x => x.DocumentId == id));
            await db.SaveChangesAsync();



            // Add user To the DocumentUser Model
            List<string> userInTagList = new List<string>();
            userInTagList = sequence.Sequence.Split(',').ToList();
            foreach (var userInList in userInTagList)
            {
                var len = userInList.Length;
                if (len != 36 && userInList[0] == 'g')
                {
                    var groupId = Convert.ToInt64(userInList.Substring(1));
                    //Check the Group is Exist
                    var groupIsExist = await db.Groups.FindAsync(groupId);

                    // Get all the user of the Group
                    var groupUsers = await (from u in db.Users
                                            join ug in db.UserGroups on u.Id equals ug.UserId
                                            join g in db.Groups on ug.GroupId equals g.Id
                                            where g.Id == groupId
                                            select new
                                            {
                                                u.Id,
                                                u.UserName,
                                                u.Email,
                                                u.CompanyId,

                                            }).ToListAsync();

                    // Add User to the Document

                    foreach (var userInGroup in groupUsers)
                    {
                        //check the user is already member of this Document  
                        var aDocumentUser = await
                            db.DocumentUsers.Where(x => x.UserId == userInGroup.Id && x.DocumentId == document.Id).SingleOrDefaultAsync();
                        if (aDocumentUser == null)
                        {
                            DocumentUser documentUser = new DocumentUser();
                            documentUser.UserId = userInGroup.Id;
                            documentUser.DocumentId = document.Id;

                            db.DocumentUsers.Add(documentUser);
                            await db.SaveChangesAsync();
                        }

                    }

                }
                else
                {
                    //check the user is already member of this Document  
                    var aDocumentUser = await
                        db.DocumentUsers.Where(x => x.UserId == userInList && x.DocumentId == document.Id).SingleOrDefaultAsync();
                    if (aDocumentUser == null)
                    {
                        DocumentUser documentUser = new DocumentUser();
                        documentUser.UserId = userInList;
                        documentUser.DocumentId = document.Id;

                        db.DocumentUsers.Add(documentUser);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return Ok(sequence);
        }


        [Route("{id}/UpdateSequencial")]
        public async Task<IHttpActionResult> PutUpdateSequencialRestricted(int id, SequenceACL sequence)
        {
            //Check the user is a Admin User
            var userId = User.Identity.GetUserId();
            UserACL userAclForAdmin = await db.UserAcls.Where(x => x.UserId == userId).SingleOrDefaultAsync();
            if (userAclForAdmin.UserType != Admin)
            {
                return BadRequest("Only Admin User can Update Sequencial Restricted");
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }



            // Check Sequencial List is already created.
            var sequencialListToCheck = await
                db.SequenceAcls.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();
            if (sequencialListToCheck != null)
            {
                return BadRequest("Sequencial list is already Exist.");
            }


            // only sequence and present access is updated
            DocumentACL aDocumentAcl = await db.DocumentAcls.Where(x => x.DocumentId == id).SingleOrDefaultAsync();

            SequenceACL aSequenceAcl =
                await db.SequenceAcls.Where(x => x.DocAclId == aDocumentAcl.Id).SingleOrDefaultAsync();

            aSequenceAcl.Sequence = sequence.Sequence;
            aSequenceAcl.PresentAccess = sequence.PresentAccess;

            await db.SaveChangesAsync();


            // change the DocAclType in DocumentAcl

            aDocumentAcl.DocAclType = 2;
            await db.SaveChangesAsync();



            //// Check Document have a restricted sequence.
            //var restrictedListToCheck = await
            //    db.Restricteds.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();
            //if (restrictedListToCheck != null)
            //{
            //    db.Restricteds.Remove(restrictedListToCheck);
            //    await db.SaveChangesAsync();
            //}




            //Delete the Document from the DocumentUser Table...
            db.DocumentUsers.RemoveRange(db.DocumentUsers.Where(x => x.DocumentId == id));
            await db.SaveChangesAsync();



            // Add user To the DocumentUser Model
            List<string> userInTagList = sequence.Sequence.Split(',').ToList();
            foreach (var userInList in userInTagList)
            {
                var len = userInList.Length;
                if (len != 36 && userInList[0] == 'g')
                {
                    var groupId = Convert.ToInt64(userInList.Substring(1));

                    //Check the Group is Exist
                    var groupIsExist = db.Groups.FindAsync(groupId);

                    // Get all the user of the Group
                    var groupUsers = await (from u in db.Users
                                            join ug in db.UserGroups on u.Id equals ug.UserId
                                            join g in db.Groups on ug.GroupId equals g.Id
                                            where g.Id == groupId
                                            select new
                                            {
                                                u.Id,
                                                u.UserName,
                                                u.Email,
                                                u.CompanyId,

                                            }).ToListAsync();

                    // Add User to the Document

                    foreach (var userInGroup in groupUsers)
                    {
                        //check the user is already member of this Document  
                        var aDocumentUser = await
                            db.DocumentUsers.Where(x => x.UserId == userInGroup.Id && x.DocumentId == document.Id).SingleOrDefaultAsync();
                        if (aDocumentUser == null)
                        {
                            DocumentUser documentUser = new DocumentUser();
                            documentUser.UserId = userInGroup.Id;
                            documentUser.DocumentId = document.Id;

                            db.DocumentUsers.Add(documentUser);
                            await db.SaveChangesAsync();
                        }

                    }

                }
                else
                {
                    //check the user is already member of this Document  
                    var aDocumentUser = await
                        db.DocumentUsers.Where(x => x.UserId == userInList && x.DocumentId == document.Id).SingleOrDefaultAsync();
                    if (aDocumentUser == null)
                    {
                        DocumentUser documentUser = new DocumentUser();
                        documentUser.UserId = userInList;
                        documentUser.DocumentId = document.Id;

                        db.DocumentUsers.Add(documentUser);
                        await db.SaveChangesAsync();
                    }
                }
            }

            return Ok(sequence);
        }


        [Route("{id}/GetSeqencial")]
        public async Task<IHttpActionResult> GetSequencialRestricted(int id)
        {
            //Check the user is a Admin User
            var userId = User.Identity.GetUserId();
            UserACL userAclForAdmin = await db.UserAcls.Where(x => x.UserId == userId).SingleOrDefaultAsync();
            if (userAclForAdmin.UserType != Admin)
            {
                return BadRequest("Only Admin User can get Sequencial list");
            }

            Document document = await db.Documents.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Get the SequencialList
            SequenceACL sequence = await
                db.SequenceAcls.Where(x => x.DocAclId == document.DocumentACLId).SingleOrDefaultAsync();

            return Ok(sequence);

        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool DocumentExists(int id)
        {
            return db.Documents.Count(e => e.Id == id) > 0;
        }
    }
}