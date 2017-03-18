namespace DTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeUserIdtoStringInTagAndRestrictedModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Restricteds", "UserId", c => c.String(nullable: false));
            AlterColumn("dbo.Tags", "UserId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Tags", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.Restricteds", "UserId", c => c.String());
        }
    }
}
