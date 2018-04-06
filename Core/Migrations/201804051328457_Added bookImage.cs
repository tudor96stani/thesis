namespace Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedbookImage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BookImages",
                c => new
                    {
                        BookImageId = c.Guid(nullable: false),
                        Content = c.Binary(),
                        BookId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.BookImageId)
                .ForeignKey("dbo.Books", t => t.BookImageId)
                .Index(t => t.BookImageId);
            
            AddColumn("dbo.Books", "BookImageId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BookImages", "BookImageId", "dbo.Books");
            DropIndex("dbo.BookImages", new[] { "BookImageId" });
            DropColumn("dbo.Books", "BookImageId");
            DropTable("dbo.BookImages");
        }
    }
}
