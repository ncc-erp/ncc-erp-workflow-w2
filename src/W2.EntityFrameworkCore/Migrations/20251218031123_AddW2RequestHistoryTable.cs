using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class AddW2RequestHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "W2RequestHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowInstanceId = table.Column<string>(type: "text", nullable: false),
                    WorkflowDefinitionId = table.Column<string>(type: "text", nullable: true),
                    WorkflowInstanceStarterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RequestType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W2RequestHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_W2RequestHistories_Email_Date",
                table: "W2RequestHistories",
                columns: new[] { "Email", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_W2RequestHistories_Status",
                table: "W2RequestHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_W2RequestHistories_WorkflowInstanceStarterId",
                table: "W2RequestHistories",
                column: "WorkflowInstanceStarterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "W2RequestHistories");
        }
    }
}
