using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class adddefinationtoworkflowstarter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkflowDefinitionId",
                table: "WorkflowInstanceStarters",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkflowDefinitionVersionId",
                table: "WorkflowInstanceStarters",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionId",
                table: "WorkflowInstanceStarters");

            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionVersionId",
                table: "WorkflowInstanceStarters");
        }
    }
}
