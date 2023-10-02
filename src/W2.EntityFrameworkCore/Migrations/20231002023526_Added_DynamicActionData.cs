using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class Added_DynamicActionData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DynamicActionData",
                table: "W2Tasks",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DynamicActionData",
                table: "W2Tasks");
        }
    }
}
