namespace ApexBackend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class version4 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MoodRatings",
                c => new
                    {
                        MoodRatingId = c.Int(nullable: false, identity: true),
                        Rating = c.Single(nullable: false),
                        DateMillis = c.Long(nullable: false),
                        PatientId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.MoodRatingId)
                .ForeignKey("dbo.Patients", t => t.PatientId, cascadeDelete: true)
                .Index(t => t.PatientId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MoodRatings", "PatientId", "dbo.Patients");
            DropIndex("dbo.MoodRatings", new[] { "PatientId" });
            DropTable("dbo.MoodRatings");
        }
    }
}
