using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class UpdateW2Settings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
            name: "Value",
            table: "W2Settings",
            type: "jsonb USING \"Value\"::jsonb",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "W2Settings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }
    }
}
