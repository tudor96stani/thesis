namespace Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bookborrowingandlending : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UsersBooks", "Lent", c => c.Boolean(nullable: false));
            AddColumn("dbo.UsersBooks", "LendedToId", c => c.String(maxLength: 128));
            CreateIndex("dbo.UsersBooks", "LendedToId");
            AddForeignKey("dbo.UsersBooks", "LendedToId", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UsersBooks", "LendedToId", "dbo.AspNetUsers");
            DropIndex("dbo.UsersBooks", new[] { "LendedToId" });
            DropColumn("dbo.UsersBooks", "LendedToId");
            DropColumn("dbo.UsersBooks", "Lent");
        }
    }
}
