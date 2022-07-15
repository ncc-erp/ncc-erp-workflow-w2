using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class AddWorkflowInstanceInput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "WorkflowInstanceStarters");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "WorkflowCustomInputDefinitions");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "WorkflowCustomInputDefinitions");

            migrationBuilder.RenameColumn(
                name: "ExtraProperties",
                table: "WorkflowInstanceStarters",
                newName: "Input");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Input",
                table: "WorkflowInstanceStarters",
                newName: "ExtraProperties");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "WorkflowInstanceStarters",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "WorkflowCustomInputDefinitions",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "WorkflowCustomInputDefinitions",
                type: "text",
                nullable: true);
        }
    }
}
