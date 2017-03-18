namespace DTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFourNewModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DocumentUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DocumentId = c.Int(nullable: false),
                        UserId = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        CompanyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserGroups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false),
                        GroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserPrivilages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CanAddDocument = c.Int(nullable: false),
                        CanDeleteDocument = c.Int(nullable: false),
                        CanTagDocument = c.Int(nullable: false),
                        UserId = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.Companies", "UserId", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "UserId", c => c.String());
            DropTable("dbo.UserPrivilages");
            DropTable("dbo.UserGroups");
            DropTable("dbo.Groups");
            DropTable("dbo.DocumentUsers");
        }
    }
}
