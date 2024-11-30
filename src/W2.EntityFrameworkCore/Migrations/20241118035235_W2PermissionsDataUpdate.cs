using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class SeedPermissionsDataUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""W2Permissions"" (""Id"", ""Name"", ""Code"", ""ParentId"", ""CreatorId"", ""CreationTime"")
                VALUES 
                    ('3a1688e3-413f-dcd8-857a-c645fd6413eb', 'Permissions Management', 'Permissions', NULL, NULL, '2024-11-19T07:35:52.981763Z'),
                    ('3a1688e4-8215-01ff-dce3-18208715aaab', 'View List Permissions', 'Permissions.View', '3a1688e3-413f-dcd8-857a-c645fd6413eb', NULL, '2024-11-19T07:35:52.986832Z'),
                    ('3a1688e4-dcfe-c04f-72e7-9d58cd6af4bf', 'Create Permissions', 'Permissions.Create', '3a1688e3-413f-dcd8-857a-c645fd6413eb', NULL, '2024-11-19T07:35:52.986971Z'),
                    ('3a1688e5-0815-bb5b-6c41-7c2865879277', 'Update Permissions', 'Permissions.Update', '3a1688e3-413f-dcd8-857a-c645fd6413eb', NULL, '2024-11-19T07:35:52.986971Z'),
                    ('3a1688e5-2c8a-3f36-701a-6c3a10aa598c', 'Delete Permissions', 'Permissions.Delete', '3a1688e3-413f-dcd8-857a-c645fd6413eb', NULL, '2024-11-19T07:35:52.986971Z');
            ");
}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"W2Permissions\";");
        }
    }
}
