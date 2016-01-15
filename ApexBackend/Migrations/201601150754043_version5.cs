namespace ApexBackend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class version5 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Messages", "SeenByDoctor", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Messages", "SeenByDoctor");
        }
    }
}
