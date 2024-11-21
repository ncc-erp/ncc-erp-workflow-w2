using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace W2.Migrations
{
    public partial class SeedPermissionsData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                IF NOT EXISTS (SELECT 1 FROM ""W2Permissions"") THEN
                INSERT INTO ""W2Permissions"" (""Id"", ""Name"", ""Code"", ""ParentId"", ""CreatorId"", ""CreationTime"")
                VALUES 
                    ('cb3433ea-e362-4181-85e8-25c0b6de4219', 'Requests', 'WorkflowInstances', NULL, NULL, '2024-11-19T07:35:52.981453Z'),
                    ('7458fe46-1fe0-4091-b6b5-bc2fedc35ca9', 'View List Requests', 'WorkflowInstances.View', 'cb3433ea-e362-4181-85e8-25c0b6de4219', NULL, '2024-11-19T07:35:52.985780Z'),
                    ('7673eb7f-142d-4f00-8adf-4dccd4132916', 'Create Requests', 'WorkflowInstances.Create', 'cb3433ea-e362-4181-85e8-25c0b6de4219', NULL, '2024-11-19T07:35:52.986039Z'),
                    ('f3654bcb-e323-4d44-bf01-49e122045ed1', 'Cancel Requests', 'WorkflowInstances.Cancel', 'cb3433ea-e362-4181-85e8-25c0b6de4219', NULL, '2024-11-19T07:35:52.986158Z'),
                    ('0dc259ca-bf21-4b32-89a3-9de4ef114e18', 'View All Requests', 'WorkflowInstances.ViewAll', 'cb3433ea-e362-4181-85e8-25c0b6de4219', NULL, '2024-11-19T07:35:52.985911Z'),

                    -- Children Permissions for Roles
                    ('bb367208-d345-4182-8854-57a6cdf1ae9e', 'Roles Management', 'Roles', NULL, NULL, '2024-11-19T07:35:52.982008Z'),
                    ('54e8008c-6d2d-4c00-873f-39c33c3ed705', 'View List Roles', 'Roles.View', 'bb367208-d345-4182-8854-57a6cdf1ae9e', NULL, '2024-11-19T07:35:52.987715Z'),
                    ('5dbcbd58-ac15-48a8-86d2-853c96c05359', 'Update Role', 'Roles.Update', 'bb367208-d345-4182-8854-57a6cdf1ae9e', NULL, '2024-11-19T07:35:52.988002Z'),
                    ('743f4fb1-b57b-4ff8-a1cd-9a9bb16ec5f3', 'Create Role', 'Roles.Create', 'bb367208-d345-4182-8854-57a6cdf1ae9e', NULL, '2024-11-19T07:35:52.987866Z'),
                    ('e117cd9c-2cde-4ec6-8d51-1fc37b0a282a', 'Delete User On Role', 'Roles.DeleteUserOnRole', 'bb367208-d345-4182-8854-57a6cdf1ae9e', NULL, '2024-11-19T07:35:52.987866Z'),
                    ('e18dd7ad-ecc1-4ef6-9574-efa55f4fcd18', 'Delete Role', 'Roles.Delete', 'bb367208-d345-4182-8854-57a6cdf1ae9e', NULL, '2024-11-19T07:35:52.987866Z'),

                    -- Children Permissions for Tasks
                    ('b8c527cf-4f12-44d2-8d8e-ca176364b9ce', 'Tasks Management', 'Tasks', NULL, NULL, '2024-11-19T07:35:52.981659Z'),
                    ('149a06d0-0847-4f44-9d4f-00a0f16a1ada', 'Assign Task', 'Tasks.Assign', 'b8c527cf-4f12-44d2-8d8e-ca176364b9ce', NULL, '2024-11-19T07:35:52.986700Z'),
                    ('24cf5b06-7f8b-4608-b3d6-d15d75c93b21', 'View List Tasks', 'Tasks.View', 'b8c527cf-4f12-44d2-8d8e-ca176364b9ce', NULL, '2024-11-19T07:35:52.986298Z'),
                    ('3b28e3bc-edd7-40f0-b708-76b53023365f', 'Update Task Status', 'Tasks.UpdateStatus', 'b8c527cf-4f12-44d2-8d8e-ca176364b9ce', NULL, '2024-11-19T07:35:52.986557Z'),
                    ('d983c04c-1730-4870-a600-ba238b854945', 'View All Tasks', 'Tasks.ViewAll', 'b8c527cf-4f12-44d2-8d8e-ca176364b9ce', NULL, '2024-11-19T07:35:52.986430Z'),

                    -- Children Permissions for Users
                    ('d33e6bbe-fb32-43c6-85f1-ad195539428f', 'Users Management', 'Users', NULL, NULL, '2024-11-19T07:35:52.981763Z'),
                    ('3df1151a-bccd-41ff-9ffc-e9646e681417', 'View List Users', 'Users.View', 'd33e6bbe-fb32-43c6-85f1-ad195539428f', NULL, '2024-11-19T07:35:52.986832Z'),
                    ('7d3d4799-4188-4f8e-ae6e-efeab1bee239', 'Update User', 'Users.Update', 'd33e6bbe-fb32-43c6-85f1-ad195539428f', NULL, '2024-11-19T07:35:52.986971Z'),

                    -- Children Permissions for Settings
                    ('55fe7d90-7cb2-4f92-8385-ec834c774cbd', 'Settings Management', 'Settings', NULL, NULL, '2024-11-19T07:35:52.981898Z'),
                    ('129f79f2-be33-4df1-9a2f-972bc3e71500', 'View List Settings', 'Settings.View', '55fe7d90-7cb2-4f92-8385-ec834c774cbd', NULL, '2024-11-19T07:35:52.987104Z'),
                    ('536d29fd-2c17-4012-951d-ebac875053f7', 'Delete Setting', 'Settings.Delete', '55fe7d90-7cb2-4f92-8385-ec834c774cbd', NULL, '2024-11-19T07:35:52.987565Z'),
                    ('fde1430d-5100-4077-aa94-b85b54a422ca', 'Create Setting', 'Settings.Create', '55fe7d90-7cb2-4f92-8385-ec834c774cbd', NULL, '2024-11-19T07:35:52.987235Z'),
                    ('6d3213be-f7d6-4675-a452-5396f200be01', 'Update Setting', 'Settings.Update', '55fe7d90-7cb2-4f92-8385-ec834c774cbd', NULL, '2024-11-19T07:35:52.987426Z'),

                    -- Children Permissions for WorkflowDefinitions
                    ('6734d713-cd5c-4c9d-880f-dc0e3955d8b1', 'Request Templates', 'WorkflowDefinitions', NULL, NULL, '2024-11-19T07:35:52.973059Z'),
                    ('643d855c-8846-4586-aeb4-3299a72e2813', 'View List Request Templates', 'WorkflowDefinitions.View', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985121Z'),
                    ('1714e44b-8020-45af-8b66-34aa4066f51d', 'Define Input Request Templates', 'WorkflowDefinitions.DefineInput', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985121Z'),
                    ('652b036b-fc7f-4305-8d8d-1b646f88224c', 'Import Request Templates', 'WorkflowDefinitions.Import', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985121Z'),
                    ('6dc377be-198f-4a81-869d-b1c8666d2706', 'Edit Request Templates', 'WorkflowDefinitions.Edit', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985892Z'),
                    ('4e4e9032-19f0-49a9-8c88-93da17864104', 'Create Request Templates', 'WorkflowDefinitions.Create', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985389Z'),
                    ('497286cc-3b83-4b32-81e7-1925895fdcc4', 'Update Request Templates Status', 'WorkflowDefinitions.UpdateStatus', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985389Z'),
                    ('7a16ffd2-b817-41d9-8555-fc9645f279b2', 'Delete Request Templates', 'WorkflowDefinitions.Delete', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985389Z');
                END IF;
                END $$;
            ");
            migrationBuilder.Sql(@"
                UPDATE ""AbpRoles""
                SET ""Permissions"" = '[
                    {
                    ""Id"": ""6734d713-cd5c-4c9d-880f-dc0e3955d8b1"",
                    ""Code"": ""WorkflowDefinitions"",
                    ""Name"": ""Request Templates"",
                    ""Children"": [
                        {
                        ""Id"": ""1714e44b-8020-45af-8b66-34aa4066f51d"",
                        ""Code"": ""WorkflowDefinitions.DefineInput"",
                        ""Name"": ""Define Input Request Templates"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859133Z""
                        },
                        {
                        ""Id"": ""6dc377be-198f-4a81-869d-b1c8666d2706"",
                        ""Code"": ""WorkflowDefinitions.Edit"",
                        ""Name"": ""Edit Request Templates"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859259Z""
                        },
                        {
                        ""Id"": ""497286cc-3b83-4b32-81e7-1925895fdcc4"",
                        ""Code"": ""WorkflowDefinitions.UpdateStatus"",
                        ""Name"": ""Update Request Templates Status"",
                        ""CreationTime"": ""2024-11-08T06:19:05.858818Z""
                        },
                        {
                        ""Id"": ""4e4e9032-19f0-49a9-8c88-93da17864104"",
                        ""Code"": ""WorkflowDefinitions.Create"",
                        ""Name"": ""Create Request Templates"",
                        ""CreationTime"": ""2024-11-08T06:19:05.858337Z""
                        },
                        {
                        ""Id"": ""652b036b-fc7f-4305-8d8d-1b646f88224c"",
                        ""Code"": ""WorkflowDefinitions.Import"",
                        ""Name"": ""Import Request Templates"",
                        ""CreationTime"": ""2024-11-08T06:19:05.858607Z""
                        },
                        {
                        ""Id"": ""7a16ffd2-b817-41d9-8555-fc9645f279b2"",
                        ""Code"": ""WorkflowDefinitions.Delete"",
                        ""Name"": ""Delete Request Templates"",
                        ""CreationTime"": ""2024-11-08T06:19:05.858983Z""
                        },
                        {
                        ""Id"": ""ff21a143-ec09-4a99-a2b5-72d312efd9bf"",
                        ""Code"": ""WorkflowDefinitions.View"",
                        ""Name"": ""View List Request Templates"",
                        ""CreationTime"": ""2024-11-08T06:19:05.855199Z""
                        }
                    ],
                    ""CreationTime"": ""2024-11-08T06:19:05.827253Z""
                    },
                    {
                    ""Id"": ""cb3433ea-e362-4181-85e8-25c0b6de4219"",
                    ""Code"": ""WorkflowInstances"",
                    ""Name"": ""Request"",
                    ""Children"": [
                        {
                        ""Id"": ""0dc259ca-bf21-4b32-89a3-9de4ef114e18"",
                        ""Code"": ""WorkflowInstances.ViewAll"",
                        ""Name"": ""View All Request"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859557Z""
                        },
                        {
                        ""Id"": ""7673eb7f-142d-4f00-8adf-4dccd4132916"",
                        ""Code"": ""WorkflowInstances.Create"",
                        ""Name"": ""Create Request"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859688Z""
                        },
                        {
                        ""Id"": ""f3654bcb-e323-4d44-bf01-49e122045ed1"",
                        ""Code"": ""WorkflowInstances.Cancel"",
                        ""Name"": ""Cancel Request"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859815Z""
                        },
                        {
                        ""Id"": ""7458fe46-1fe0-4091-b6b5-bc2fedc35ca9"",
                        ""Code"": ""WorkflowInstances.View"",
                        ""Name"": ""View Request"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859369Z""
                        }
                    ],
                    ""CreationTime"": ""2024-11-08T06:19:05.844228Z""
                    },
                    {
                    ""Id"": ""55fe7d90-7cb2-4f92-8385-ec834c774cbd"",
                    ""Code"": ""Settings"",
                    ""Name"": ""Settings Management"",
                    ""Children"": [
                        {
                        ""Id"": ""129f79f2-be33-4df1-9a2f-972bc3e71500"",
                        ""Code"": ""Settings.View"",
                        ""Name"": ""View List Settings"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860743Z""
                        },
                        {
                        ""Id"": ""536d29fd-2c17-4012-951d-ebac875053f7"",
                        ""Code"": ""Settings.Delete"",
                        ""Name"": ""Delete Setting"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861165Z""
                        },
                        {
                        ""Id"": ""6d3213be-f7d6-4675-a452-5396f200be01"",
                        ""Code"": ""Settings.Update"",
                        ""Name"": ""Update Setting"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861032Z""
                        },
                        {
                        ""Id"": ""fde1430d-5100-4077-aa94-b85b54a422ca"",
                        ""Code"": ""Settings.Create"",
                        ""Name"": ""Create Setting"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860876Z""
                        }
                    ],
                    ""CreationTime"": ""2024-11-08T06:19:05.844805Z""
                    },
                    {
                    ""Id"": ""b8c527cf-4f12-44d2-8d8e-ca176364b9ce"",
                    ""Code"": ""Tasks"",
                    ""Name"": ""Tasks Management"",
                    ""Children"": [
                        {
                        ""Id"": ""24cf5b06-7f8b-4608-b3d6-d15d75c93b21"",
                        ""Code"": ""Tasks.View"",
                        ""Name"": ""View Tasks"",
                        ""CreationTime"": ""2024-11-08T06:19:05.859959Z""
                        },
                        {
                        ""Id"": ""149a06d0-0847-4f44-9d4f-00a0f16a1ada"",
                        ""Code"": ""Tasks.Assign"",
                        ""Name"": ""Assign Task"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860349Z""
                        },
                        {
                        ""Id"": ""3b28e3bc-edd7-40f0-b708-76b53023365f"",
                        ""Code"": ""Tasks.UpdateStatus"",
                        ""Name"": ""Update Task Status"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860198Z""
                        },
                        {
                        ""Id"": ""d983c04c-1730-4870-a600-ba238b854945"",
                        ""Code"": ""Tasks.ViewAll"",
                        ""Name"": ""View All Tasks"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860081Z""
                        }
                    ],
                    ""CreationTime"": ""2024-11-08T06:19:05.844595Z""
                    },
                    {
                    ""Id"": ""bb367208-d345-4182-8854-57a6cdf1ae9e"",
                    ""Code"": ""Roles"",
                    ""Name"": ""Roles Management"",
                    ""Children"": [
                        {
                        ""Id"": ""5dbcbd58-ac15-48a8-86d2-853c96c05359"",
                        ""Code"": ""Roles.Update"",
                        ""Name"": ""Update Role"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861565Z""
                        },
                        {
                        ""Id"": ""743f4fb1-b57b-4ff8-a1cd-9a9bb16ec5f3"",
                        ""Code"": ""Roles.Create"",
                        ""Name"": ""Create Role"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861446Z""
                        },
                        {
                        ""Id"": ""54e8008c-6d2d-4c00-873f-39c33c3ed705"",
                        ""Code"": ""Roles.View"",
                        ""Name"": ""View List Roles"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861291Z""
                        },
                        {
                        ""Id"": ""e18dd7ad-ecc1-4ef6-9574-efa55f4fcd18"",
                        ""Code"": ""Roles.Delete"",
                        ""Name"": ""Delete Roles"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861291Z""
                        },
                        {
                        ""Id"": ""e117cd9c-2cde-4ec6-8d51-1fc37b0a282a"",
                        ""Code"": ""Roles.DeleteUserOnRole"",
                        ""Name"": ""Delete User On Role"",
                        ""CreationTime"": ""2024-11-08T06:19:05.861291Z""
                        }
                    ],
                    ""CreationTime"": ""2024-11-08T06:19:05.845056Z""
                    },
                    {
                    ""Id"": ""d33e6bbe-fb32-43c6-85f1-ad195539428f"",
                    ""Code"": ""Users"",
                    ""Name"": ""Users Management"",
                    ""Children"": [
                        {
                        ""Id"": ""3df1151a-bccd-41ff-9ffc-e9646e681417"",
                        ""Code"": ""Users.View"",
                        ""Name"": ""View List Users"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860478Z""
                        },
                        {
                        ""Id"": ""7d3d4799-4188-4f8e-ae6e-efeab1bee239"",
                        ""Code"": ""Users.Update"",
                        ""Name"": ""Update User"",
                        ""CreationTime"": ""2024-11-08T06:19:05.860601Z""
                        }
                    ],
                    ""CreationTime"": ""2024-11-08T06:19:05.844716Z""
                    }
                ]'
                WHERE ""Name"" = 'admin';   
                ");

}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"W2Permissions\";");
            migrationBuilder.Sql(@"UPDATE ""AbpRoles"" 
                            SET ""Permissions"" = '[]'
                            WHERE ""Name"" = 'admin';");
        }
    }
}
