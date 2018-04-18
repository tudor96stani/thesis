namespace Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedBorrowRequests : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BorrowRequests",
                c => new
                    {
                        BookId = c.Guid(nullable: false),
                        BorrowerId = c.String(nullable: false, maxLength: 128),
                        LenderId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.BookId, t.BorrowerId, t.LenderId });
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BorrowRequests");
        }
    }
}
