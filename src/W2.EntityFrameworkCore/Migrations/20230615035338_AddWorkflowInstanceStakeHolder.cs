using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class AddWorkflowInstanceStakeHolder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FinalStatus",
                table: "WorkflowInstanceStarters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StateName = table.Column<string>(type: "text", nullable: true),
                    WorkflowInstanceStarterId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceStates_WorkflowInstanceStarters_WorkflowIns~",
                        column: x => x.WorkflowInstanceStarterId,
                        principalTable: "WorkflowInstanceStarters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowInstanceStakeHolders",
                columns: table => new
                {
                    WorkflowInstanceStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowInstanceStakeHolders", x => new { x.UserId, x.WorkflowInstanceStateId });
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceStakeHolders_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowInstanceStakeHolders_WorkflowInstanceStates_Workflo~",
                        column: x => x.WorkflowInstanceStateId,
                        principalTable: "WorkflowInstanceStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceStakeHolders_WorkflowInstanceStateId",
                table: "WorkflowInstanceStakeHolders",
                column: "WorkflowInstanceStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowInstanceStates_WorkflowInstanceStarterId",
                table: "WorkflowInstanceStates",
                column: "WorkflowInstanceStarterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkflowInstanceStakeHolders");

            migrationBuilder.DropTable(
                name: "WorkflowInstanceStates");

            migrationBuilder.DropColumn(
                name: "FinalStatus",
                table: "WorkflowInstanceStarters");
        }
    }
}
