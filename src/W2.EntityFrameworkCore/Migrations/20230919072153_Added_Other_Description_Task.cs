using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class Added_Other_Description_Task : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "W2Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherActionSignal",
                table: "W2Tasks",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "W2Tasks");

            migrationBuilder.DropColumn(
                name: "OtherActionSignal",
                table: "W2Tasks");
        }
    }
}
