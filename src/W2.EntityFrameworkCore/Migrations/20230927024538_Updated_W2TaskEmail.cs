using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class Updated_W2TaskEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailTo",
                table: "W2Tasks");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "W2TaskEmail");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "W2TaskEmail");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "W2TaskEmail");

            migrationBuilder.DropColumn(
                name: "EmailTo",
                table: "W2TaskEmail");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "W2TaskEmail",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskId",
                table: "W2TaskEmail",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "W2TaskEmail");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "W2TaskEmail");

            migrationBuilder.AddColumn<string>(
                name: "EmailTo",
                table: "W2Tasks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Author",
                table: "W2TaskEmail",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "W2TaskEmail",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "W2TaskEmail",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "EmailTo",
                table: "W2TaskEmail",
                type: "text[]",
                nullable: true);
        }
    }
}
