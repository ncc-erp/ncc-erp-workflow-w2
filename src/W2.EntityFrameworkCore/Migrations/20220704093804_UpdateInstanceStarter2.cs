using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class UpdateInstanceStarter2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Properties",
                table: "WorkflowInstanceStarters");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Properties",
                table: "WorkflowInstanceStarters",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
