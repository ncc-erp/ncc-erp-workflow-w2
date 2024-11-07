using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class W2Roles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomPermissions",
                table: "AbpUsers",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'[]'");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE \"AbpUsers\" " +
                "SET \"Discriminator\" = 'W2CustomIdentityUser'");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpUserRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE \"AbpUserRoles\" " +
                "SET \"Discriminator\" = 'W2CustomIdentityUserRole'");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpRoles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Permissions",
                table: "AbpRoles",
                type: "jsonb",
                nullable: true,
                defaultValueSql: "'[]'");

            migrationBuilder.Sql(
                "UPDATE \"AbpRoles\" " +
                "SET \"Discriminator\" = 'W2CustomIdentityRole'");

            migrationBuilder.CreateTable(
                name: "W2Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_W2Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_W2Permissions_W2Permissions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "W2Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_W2Permissions_ParentId",
                table: "W2Permissions",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "W2Permissions");

            migrationBuilder.DropColumn(
                name: "CustomPermissions",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpUserRoles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpRoles");

            migrationBuilder.DropColumn(
                name: "Permissions",
                table: "AbpRoles");
        }
    }
}
