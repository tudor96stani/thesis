namespace Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedAuthorBirtdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Authors", "FirstName", c => c.String());
            AlterColumn("dbo.Authors", "LastName", c => c.String());
            DropColumn("dbo.Authors", "Birthdate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Authors", "Birthdate", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Authors", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.Authors", "FirstName", c => c.String(nullable: false));
        }
    }
}
