using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class Update_WorkflowCustomInputDefinitions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           migrationBuilder.AddColumn<string>(
           name: "Settings",
           table: "WorkflowCustomInputDefinitions",
           type: "text",
           nullable: true,
           defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
            name: "Settings",
            table: "WorkflowCustomInputDefinitions");
        }
    }
}
