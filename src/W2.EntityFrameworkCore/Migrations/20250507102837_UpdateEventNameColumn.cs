using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class UpdateEventNameColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventName",
                table: "W2Webhooks");

            migrationBuilder.AddColumn<string>(
                name: "EventNames",
                table: "W2Webhooks",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'[]'::jsonb"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventNames",
                table: "W2Webhooks");

            migrationBuilder.AddColumn<string[]>(
                name: "EventName",
                table: "W2Webhooks",
                type: "jsonb",
                maxLength: 250,
                nullable: false,
                defaultValueSql: "'[]'::jsonb"
            );
        }
    }
}
