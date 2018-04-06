namespace Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class bookcover : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Books", "BookImageId");
            DropColumn("dbo.BookImages", "BookId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BookImages", "BookId", c => c.Guid(nullable: false));
            AddColumn("dbo.Books", "BookImageId", c => c.Guid(nullable: false));
        }
    }
}
