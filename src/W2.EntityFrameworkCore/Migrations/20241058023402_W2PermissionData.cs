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
                INSERT INTO ""W2Permissions"" (""Id"", ""Name"", ""Code"", ""ParentId"", ""CreatorId"", ""CreationTime"")
                VALUES 
                    ('353896e7-9516-4313-a5ce-3b8bec18a279', 'Requests', 'WorkflowInstances', NULL, NULL, '2024-11-19T07:35:52.981453Z'),
                    ('377ad1fd-b7ef-4849-92d5-b945a9d2ca0f', 'View List Requests', 'WorkflowInstances.View', '353896e7-9516-4313-a5ce-3b8bec18a279', NULL, '2024-11-19T07:35:52.985780Z'),
                    ('9cc23baf-b5cb-4e4a-874f-defcf806ae20', 'Create Requests', 'WorkflowInstances.Create', '353896e7-9516-4313-a5ce-3b8bec18a279', NULL, '2024-11-19T07:35:52.986039Z'),
                    ('be869468-5117-4619-9a3c-eb26ddb78970', 'Cancel Requests', 'WorkflowInstances.Cancel', '353896e7-9516-4313-a5ce-3b8bec18a279', NULL, '2024-11-19T07:35:52.986158Z'),
                    ('9980cba3-072d-4fc9-8576-197597b01ab1', 'View All Requests', 'WorkflowInstances.ViewAll', '353896e7-9516-4313-a5ce-3b8bec18a279', NULL, '2024-11-19T07:35:52.985911Z'),

                    -- Children Permissions for Roles
                    ('6990fbaf-5a00-40cf-a3a1-13b85d1f6099', 'Roles Management', 'Roles', NULL, NULL, '2024-11-19T07:35:52.982008Z'),
                    ('54e8008c-6d2d-4c00-873f-39c33c3ed705', 'View List Roles', 'Roles.View', '6990fbaf-5a00-40cf-a3a1-13b85d1f6099', NULL, '2024-11-19T07:35:52.987715Z'),
                    ('8eb0cb9b-7cab-4d38-9163-a9a4cc8ea7e0', 'Update Role', 'Roles.Update', '6990fbaf-5a00-40cf-a3a1-13b85d1f6099', NULL, '2024-11-19T07:35:52.988002Z'),
                    ('9c41b0c6-fcae-49c0-94d1-146f392fda3f', 'Create Role', 'Roles.Create', '6990fbaf-5a00-40cf-a3a1-13b85d1f6099', NULL, '2024-11-19T07:35:52.987866Z'),

                    -- Children Permissions for Tasks
                    ('881263f8-95d7-4410-88e5-d3873f99b53e', 'Tasks Management', 'Tasks', NULL, NULL, '2024-11-19T07:35:52.981659Z'),
                    ('034d0ba5-be32-4e84-8ee7-09b7a809bb4c', 'Assign Task', 'Tasks.Assign', '881263f8-95d7-4410-88e5-d3873f99b53e', NULL, '2024-11-19T07:35:52.986700Z'),
                    ('33fc5382-9f93-41d6-acc7-751e239b7e4f', 'View List Tasks', 'Tasks.View', '881263f8-95d7-4410-88e5-d3873f99b53e', NULL, '2024-11-19T07:35:52.986298Z'),
                    ('6c131f7f-0295-417d-8472-b792ec8250ee', 'Update Task Status', 'Tasks.UpdateStatus', '881263f8-95d7-4410-88e5-d3873f99b53e', NULL, '2024-11-19T07:35:52.986557Z'),
                    ('cdfae99c-e8f7-4a0a-9802-401cb61bc4c7', 'View All Tasks', 'Tasks.ViewAll', '881263f8-95d7-4410-88e5-d3873f99b53e', NULL, '2024-11-19T07:35:52.986430Z'),

                    -- Children Permissions for Users
                    ('d51e853a-4d25-4121-bec1-c4a235b3db3e', 'Users Management', 'Users', NULL, NULL, '2024-11-19T07:35:52.981763Z'),
                    ('3686a277-2718-4f19-b061-b59ac015f786', 'View List Users', 'Users.View', 'd51e853a-4d25-4121-bec1-c4a235b3db3e', NULL, '2024-11-19T07:35:52.986832Z'),
                    ('bf6a0114-08a8-49ec-99d2-b12b35ae71ae', 'Update User', 'Users.Update', 'd51e853a-4d25-4121-bec1-c4a235b3db3e', NULL, '2024-11-19T07:35:52.986971Z'),

                    -- Children Permissions for Settings
                    ('e8d8e99d-d6e4-4199-823e-67bef350aa1a', 'Settings Management', 'Settings', NULL, NULL, '2024-11-19T07:35:52.981898Z'),
                    ('0865f67b-f075-4107-9044-a9c7ea9112be', 'View List Settings', 'Settings.View', 'e8d8e99d-d6e4-4199-823e-67bef350aa1a', NULL, '2024-11-19T07:35:52.987104Z'),
                    ('43f96ebf-ebce-4927-894f-14db8232b7e6', 'Delete Setting', 'Settings.Delete', 'e8d8e99d-d6e4-4199-823e-67bef350aa1a', NULL, '2024-11-19T07:35:52.987565Z'),
                    ('46c5ce9f-3c2e-4462-aeea-fed83c9fd1cc', 'Create Setting', 'Settings.Create', 'e8d8e99d-d6e4-4199-823e-67bef350aa1a', NULL, '2024-11-19T07:35:52.987235Z'),
                    ('79931735-d83f-4799-b1df-36932923ba33', 'Update Setting', 'Settings.Update', 'e8d8e99d-d6e4-4199-823e-67bef350aa1a', NULL, '2024-11-19T07:35:52.987426Z'),

                    -- Children Permissions for WorkflowDefinitions
                    ('6734d713-cd5c-4c9d-880f-dc0e3955d8b1', 'Request Templates', 'WorkflowDefinitions', NULL, NULL, '2024-11-19T07:35:52.973059Z'),
                    ('643d855c-8846-4586-aeb4-3299a72e2813', 'Import Request Templates', 'WorkflowDefinitions.Import', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985121Z'),
                    ('6dc377be-198f-4a81-869d-b1c8666d2706', 'Edit Request Templates', 'WorkflowDefinitions.Edit', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985892Z'),
                    ('9cefeb05-62cf-4c58-8157-5f6c82fbe9e1', 'Create Request Templates', 'WorkflowDefinitions.Create', '6734d713-cd5c-4c9d-880f-dc0e3955d8b1', NULL, '2024-11-19T07:35:52.985389Z');
            ");
            migrationBuilder.Sql(@"
                UPDATE ""AbpRoles""
                SET ""Permissions"" = '[
                    {
                    ""Id"": ""319dca97-f851-4f60-960c-3b2d201f1787"",
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
                        ""Id"": ""1ce2613a-9369-4521-8a7d-0a5f1d94d515"",
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
                        ""Id"": ""ee42c1e5-6c8a-4366-bce6-9a95a7a51514"",
                        ""Code"": ""Roles.View"",
                        ""Name"": ""View List Roles"",
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
