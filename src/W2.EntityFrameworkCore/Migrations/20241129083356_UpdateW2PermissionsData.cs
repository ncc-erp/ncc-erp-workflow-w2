using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class UpdateW2PermissionsData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO ""W2Permissions"" (""Id"", ""Name"", ""Code"", ""ParentId"", ""CreatorId"", ""CreationTime"")
                VALUES 
                    ('b261fbba-fb5a-46cf-9b96-d6839646e6d0', 'WFH Report', 'WFHReport', NULL, NULL, '2024-11-19T07:35:52.981763Z'),
                    ('488b0485-932b-43be-9765-ade9742ac2be', 'View List WFH Report', 'WFHReport.View', 'b261fbba-fb5a-46cf-9b96-d6839646e6d0', NULL, '2024-11-19T07:35:52.986832Z')
                ON CONFLICT (""Id"")
                DO NOTHING;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""W2Permissions"" 
                WHERE ""Id"" IN ('b261fbba-fb5a-46cf-9b96-d6839646e6d0', '488b0485-932b-43be-9765-ade9742ac2be');
            ");
        }
    }
}
