$(function () {
    var l = abp.localization.getResource('W2');
    var dataTable = $("#WorkflowInstancesTable").DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            searching: false,
            paging: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(w2.workflowInstances.workflowInstance.list),
            columnDefs: [
                {
                    title: l('WorkflowInstance:DefinitionDisplayName'),
                    data: "workflowDefinitionDisplayName",
                    render: function (row, type, val) {
                        return `<a href="/WorkflowInstances/Designer?id=${val.id}">${row}</a>`;
                    }
                },
                {
                    title: l('WorkflowInstance:CreatedAt'),
                    data: "createdAt",
                    dataFormat: "datetime"
                },
                {
                    title: l('WorkflowInstance:LastExecutedAt'),
                    data: "lastExecutedAt",
                    dataFormat: "datetime"
                },
                {
                    title: l('Status'),
                    data: "status"
                },
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Cancel'),
                                action: function (data) {
                                    w2.workflowInstances.workflowInstance
                                        .cancel(data.record.id)
                                        .then(function () {
                                            dataTable.ajax.reload();
                                            abp.notify.success(l('WorkflowInstance:SuccessfullyCancelled'));
                                        });
                                },
                                confirmMessage: function () {
                                    return l('WorkflowInstance:CancelConfirmationMessage');
                                },
                                visible: abp.auth.isGranted("WorkflowManagement.WorkflowInstances.Create")
                            },
                            {
                                text: l('Delete'),
                                action: function (data) {
                                    w2.workflowInstances.workflowInstance
                                        .delete(data.record.id)
                                        .then(function () {
                                            dataTable.ajax.reload();
                                            abp.notify.success(l('SuccessfullyDeleted'));
                                        });
                                },
                                confirmMessage: function () {
                                    return l('WorkflowInstance:DeleteConfirmationMessage');
                                },
                                visible: abp.auth.isGranted("WorkflowManagement.WorkflowInstances.Create")
                            }
                        ]
                    }
                }
            ]
        })
    );
});