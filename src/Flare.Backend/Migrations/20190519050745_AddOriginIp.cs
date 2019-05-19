using Microsoft.EntityFrameworkCore.Migrations;

namespace Flare.Backend.Migrations
{
    public partial class AddOriginIp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "origin_ip",
                table: "servers",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "origin_ip",
                table: "servers");
        }
    }
}
