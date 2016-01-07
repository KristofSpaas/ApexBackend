namespace ApexBackend.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class version3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AspNetUserRoles", "FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId");
            DropForeignKey("dbo.AspNetUserRoles", "FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId");

            AddForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
        }
    }
}
