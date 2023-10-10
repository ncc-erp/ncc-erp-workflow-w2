using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class Added_OtherActionSignals_Tasks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherActionSignal",
                table: "W2Tasks");

            migrationBuilder.AddColumn<List<string>>(
                name: "OtherActionSignals",
                table: "W2Tasks",
                type: "text[]",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherActionSignals",
                table: "W2Tasks");

            migrationBuilder.AddColumn<string>(
                name: "OtherActionSignal",
                table: "W2Tasks",
                type: "text",
                nullable: true);
        }
    }
}
