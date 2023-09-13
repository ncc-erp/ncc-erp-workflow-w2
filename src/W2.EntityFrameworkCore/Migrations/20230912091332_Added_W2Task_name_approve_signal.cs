using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class Added_W2Task_name_approve_signal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApproveSignal",
                table: "W2Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "W2Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectSignal",
                table: "W2Tasks",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproveSignal",
                table: "W2Tasks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "W2Tasks");

            migrationBuilder.DropColumn(
                name: "RejectSignal",
                table: "W2Tasks");
        }
    }
}
