namespace Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Renamedcolumns : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.UsersBooks", name: "BorrowerId", newName: "BorrowedFromId");
            RenameColumn(table: "dbo.UsersBooks", name: "LendedToId", newName: "LentToId");
            RenameIndex(table: "dbo.UsersBooks", name: "IX_BorrowerId", newName: "IX_BorrowedFromId");
            RenameIndex(table: "dbo.UsersBooks", name: "IX_LendedToId", newName: "IX_LentToId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.UsersBooks", name: "IX_LentToId", newName: "IX_LendedToId");
            RenameIndex(table: "dbo.UsersBooks", name: "IX_BorrowedFromId", newName: "IX_BorrowerId");
            RenameColumn(table: "dbo.UsersBooks", name: "LentToId", newName: "LendedToId");
            RenameColumn(table: "dbo.UsersBooks", name: "BorrowedFromId", newName: "BorrowerId");
        }
    }
}
