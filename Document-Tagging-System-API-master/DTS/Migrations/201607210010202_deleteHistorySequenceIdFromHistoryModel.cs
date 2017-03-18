namespace DTS.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteHistorySequenceIdFromHistoryModel : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Histories", "HistorySequenceId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Histories", "HistorySequenceId", c => c.Int(nullable: false));
        }
    }
}
