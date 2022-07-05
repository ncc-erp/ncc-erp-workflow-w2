using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class UpdateInstanceStarter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkflowDefinitionId",
                table: "WorkflowInstanceStarters",
                newName: "WorkflowInstanceId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstanceStarters_WorkflowDefinitionId",
                table: "WorkflowInstanceStarters",
                newName: "IX_WorkflowInstanceStarters_WorkflowInstanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WorkflowInstanceId",
                table: "WorkflowInstanceStarters",
                newName: "WorkflowDefinitionId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkflowInstanceStarters_WorkflowInstanceId",
                table: "WorkflowInstanceStarters",
                newName: "IX_WorkflowInstanceStarters_WorkflowDefinitionId");
        }
    }
}
