namespace DTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAllModeltoDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Address = c.String(),
                        Phone = c.String(),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DocumentACLs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        DocAclType = c.Int(nullable: false),
                        IsSequential = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Documents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        UserId = c.String(),
                        CompanyId = c.Int(nullable: false),
                        FileId = c.Int(nullable: false),
                        DocumentACLId = c.Int(nullable: false),
                        DocType = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Files",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        Content = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Histories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AddedOn = c.DateTime(nullable: false),
                        EditedOn = c.DateTime(nullable: false),
                        DocumentId = c.Int(nullable: false),
                        HistorySequenceId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.HistorySequences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HistoryId = c.Int(nullable: false),
                        Message = c.String(),
                        Date = c.DateTime(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ToWhom = c.String(),
                        Date = c.DateTime(nullable: false),
                        IsViewed = c.Boolean(nullable: false),
                        DocumentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Restricteds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocAclId = c.Int(nullable: false),
                        UserId = c.String(),
                        List = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SequenceACLs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocAclId = c.Int(nullable: false),
                        Sequence = c.String(),
                        PresentAccess = c.Int(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        TagContent = c.String(),
                        TagColor = c.String(),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserACLs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserType = c.Int(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserACLs");
            DropTable("dbo.Tags");
            DropTable("dbo.SequenceACLs");
            DropTable("dbo.Restricteds");
            DropTable("dbo.Notifications");
            DropTable("dbo.HistorySequences");
            DropTable("dbo.Histories");
            DropTable("dbo.Files");
            DropTable("dbo.Documents");
            DropTable("dbo.DocumentACLs");
            DropTable("dbo.Companies");
        }
    }
}
