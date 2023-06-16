$(function () {
    var l = abp.localization.getResource('W2');

    var getFilter = function () {
        return {
            StakeHolder: $('#input_stakeholder').val(),
            Status: $("#input_status").val(),
            WorkflowDefinitionId: $("#input_workflowDefinitionId").val()
        };
    };

    var dataTable = $("#WorkflowInstancesTable").DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            searching: false,
            paging: true,
            sorting: true,
            scrollX: true,
            order: [[4, 'desc']],
            ajax: abp.libs.datatables.createAjax(w2.workflowInstances.workflowInstance.list, getFilter),
            columnDefs: [
                {
                    title: l('WorkflowInstance:DefinitionDisplayName'),
                    data: "workflowDefinitionDisplayName",
                    render: function (row, type, val) {
                        return `<a href="/WorkflowInstances/Designer?id=${val.id}">${row}</a>`;
                    },
                    orderable: false
                },
                {
                    title: l('WorkflowInstance:RequestUser'),
                    data: "userRequestName",
                    orderable: false
                },
                {
                    title: l('WorkflowInstance:CurrentState'),
                    data: "currentStates",
                    render: function (row, type, val) {
                        return row.length > 0 ? row.join('<br>') : 'None';
                    },
                    orderable: false
                },
                {
                    title: l('WorkflowInstance:StakeHolders'),
                    data: "stakeHolders",
                    render: function (row, type, val) {
                        return row.length > 0 ? row.join('<br>') : 'None';
                    },
                    orderable: false
                },
                {
                    title: l('WorkflowInstance:CreatedAt'),
                    data: "createdAt",
                    dataFormat: "datetime",
                    orderable: true
                },
                {
                    title: l('WorkflowInstance:LastExecutedAt'),
                    data: "lastExecutedAt",
                    dataFormat: "datetime",
                    orderable: true
                },
                {
                    title: l('Status'),
                    data: "status",
                    orderable: false
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

    $("#input_stakeholder").change(function () {
        dataTable.ajax.reload();
    });

    $("#input_workflowDefinitionId").change(function () {
        dataTable.ajax.reload();
    });

    $("#input_status").change(function () {
        dataTable.ajax.reload();
    });
});