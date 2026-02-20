using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class RenameSidebarColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "SidebarItems");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "SidebarItems");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "SidebarItems",
                newName: "LinkUrl");

            migrationBuilder.RenameColumn(
                name: "HtmlContent",
                table: "SidebarItems",
                newName: "Content");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LinkUrl",
                table: "SidebarItems",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "SidebarItems",
                newName: "HtmlContent");

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "SidebarItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "SidebarItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
