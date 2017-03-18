namespace DTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeRequiredInUserIdInComapanyModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Companies", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "UserId", c => c.String(nullable: false));
        }
    }
}
