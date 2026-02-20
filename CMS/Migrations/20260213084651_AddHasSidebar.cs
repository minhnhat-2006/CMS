using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class AddHasSidebar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SidebarItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LinkUrl",
                table: "SidebarItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "SidebarItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "SidebarItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "SidebarItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MenuId",
                table: "SidebarItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "SidebarItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContentPageId",
                table: "NavigationMenus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "NavigationMenus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ContentPages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "ContentPages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

           /* migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "ContentPages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");*/

            migrationBuilder.AddColumn<bool>(
                name: "HasSidebar",
                table: "ContentPages",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    TenDangNhap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.TenDangNhap);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SidebarItems_ContentPageId",
                table: "SidebarItems",
                column: "ContentPageId");

            migrationBuilder.CreateIndex(
                name: "IX_SidebarItems_MenuId",
                table: "SidebarItems",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationMenus_ContentPageId",
                table: "NavigationMenus",
                column: "ContentPageId");

            migrationBuilder.AddForeignKey(
                name: "FK_NavigationMenus_ContentPages_ContentPageId",
                table: "NavigationMenus",
                column: "ContentPageId",
                principalTable: "ContentPages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SidebarItems_ContentPages_ContentPageId",
                table: "SidebarItems",
                column: "ContentPageId",
                principalTable: "ContentPages",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SidebarItems_NavigationMenus_MenuId",
                table: "SidebarItems",
                column: "MenuId",
                principalTable: "NavigationMenus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NavigationMenus_ContentPages_ContentPageId",
                table: "NavigationMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_SidebarItems_ContentPages_ContentPageId",
                table: "SidebarItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SidebarItems_NavigationMenus_MenuId",
                table: "SidebarItems");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_SidebarItems_ContentPageId",
                table: "SidebarItems");

            migrationBuilder.DropIndex(
                name: "IX_SidebarItems_MenuId",
                table: "SidebarItems");

            migrationBuilder.DropIndex(
                name: "IX_NavigationMenus_ContentPageId",
                table: "NavigationMenus");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "SidebarItems");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "SidebarItems");

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "SidebarItems");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "SidebarItems");

            migrationBuilder.DropColumn(
                name: "ContentPageId",
                table: "NavigationMenus");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "NavigationMenus");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ContentPages");

            migrationBuilder.DropColumn(
                name: "HasSidebar",
                table: "ContentPages");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SidebarItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LinkUrl",
                table: "SidebarItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "SidebarItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ContentPages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "ContentPages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
