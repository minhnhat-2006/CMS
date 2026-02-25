using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CMS.Migrations
{
    /// <inheritdoc />
    public partial class AddTable_ChuyenMuc_For_News : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChuyenMucId",
                table: "SidebarItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChuyenMucId",
                table: "NavigationMenus",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChuyenMucId",
                table: "ContentPages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChuyenMucs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuyenMucs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SidebarItems_ChuyenMucId",
                table: "SidebarItems",
                column: "ChuyenMucId");

            migrationBuilder.CreateIndex(
                name: "IX_NavigationMenus_ChuyenMucId",
                table: "NavigationMenus",
                column: "ChuyenMucId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPages_ChuyenMucId",
                table: "ContentPages",
                column: "ChuyenMucId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentPages_ChuyenMucs_ChuyenMucId",
                table: "ContentPages",
                column: "ChuyenMucId",
                principalTable: "ChuyenMucs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NavigationMenus_ChuyenMucs_ChuyenMucId",
                table: "NavigationMenus",
                column: "ChuyenMucId",
                principalTable: "ChuyenMucs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SidebarItems_ChuyenMucs_ChuyenMucId",
                table: "SidebarItems",
                column: "ChuyenMucId",
                principalTable: "ChuyenMucs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentPages_ChuyenMucs_ChuyenMucId",
                table: "ContentPages");

            migrationBuilder.DropForeignKey(
                name: "FK_NavigationMenus_ChuyenMucs_ChuyenMucId",
                table: "NavigationMenus");

            migrationBuilder.DropForeignKey(
                name: "FK_SidebarItems_ChuyenMucs_ChuyenMucId",
                table: "SidebarItems");

            migrationBuilder.DropTable(
                name: "ChuyenMucs");

            migrationBuilder.DropIndex(
                name: "IX_SidebarItems_ChuyenMucId",
                table: "SidebarItems");

            migrationBuilder.DropIndex(
                name: "IX_NavigationMenus_ChuyenMucId",
                table: "NavigationMenus");

            migrationBuilder.DropIndex(
                name: "IX_ContentPages_ChuyenMucId",
                table: "ContentPages");

            migrationBuilder.DropColumn(
                name: "ChuyenMucId",
                table: "SidebarItems");

            migrationBuilder.DropColumn(
                name: "ChuyenMucId",
                table: "NavigationMenus");

            migrationBuilder.DropColumn(
                name: "ChuyenMucId",
                table: "ContentPages");
        }
    }
}
