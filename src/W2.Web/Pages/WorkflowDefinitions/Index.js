$(function () {
    var l = abp.localization.getResource('W2');

    var newWorkflowInstanceModal = new abp.ModalManager(abp.appPath + 'WorkflowDefinitions/NewWorkflowInstanceModal');

    var defineWorkflowInputModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'WorkflowDefinitions/DefineWorkflowInputModal',
        scriptUrl: '/Pages/WorkflowDefinitions/DefineWorkflowInputModal.js',
        modalClass: 'DefineWorkflowInputModal'
    });

    var createWorkflowDefinitionModal = new abp.ModalManager(abp.appPath + 'WorkflowDefinitions/CreateModal');

    var dataTable = $('#WorkflowDefinitionsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            searching: false,
            paging: false,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(w2.workflowDefinitions.workflowDefinition.listAll),
            columnDefs: [
                {
                    title: l('WorkflowDefinition:Name'),
                    data: "name"
                },
                {
                    title: l('WorkflowDefinition:DisplayName'),
                    data: "displayName",
                },
                {
                    title: l('WorkflowDefinition:Version'),
                    data: "version"
                },
                {
                    title: l('WorkflowDefinition:IsPublished'),
                    data: "isPublished",
                },
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('WorkflowDefinition:NewWorkflowInstance'),
                                action: function (data) {
                                    newWorkflowInstanceModal.open({
                                        workflowDefinitionId: data.record.definitionId,
                                        propertiesDefinitionJson: data.record.inputDefinition
                                            ? JSON.stringify(data.record.inputDefinition.propertyDefinitions)
                                            : null
                                    });
                                },
                                visible: abp.auth.isGranted("WorkflowManagement.WorkflowInstances.Create")
                            },
                            {
                                text: l('WorkflowDefinition:DefineInput'),
                                action: function (data) {
                                    defineWorkflowInputModal.open({
                                        workflowDefinitionId: data.record.definitionId
                                    });
                                },
                                visible: function (record) {
                                    return abp.auth.isGranted("WorkflowManagement.WorkflowDefinitions.Design");
                                }
                            },
                            {
                                text: l('WorkflowDefinition:OpenDesigner'),
                                action: function (data) {
                                    window.open(abp.appPath + 'WorkflowDefinitions/Designer?workflowDefinitionId=' + data.record.definitionId);
                                },
                                visible: abp.auth.isGranted("WorkflowManagement.WorkflowDefinitions.Design")
                            },
                            {
                                text: l('Delete'),
                                action: function (data) {
                                    w2.workflowDefinitions.workflowDefinition
                                        .delete(data.record.definitionId)
                                        .then(function () {
                                            dataTable.ajax.reload();
                                            abp.notify.success(l('SuccessfullyDeleted'));
                                        });
                                },
                                confirmMessage: function (data) {
                                    return l(
                                        'WorkflowDefinition:DeleteConfirmationMessage',
                                        data.record.displayName
                                    );
                                },
                                visible: abp.auth.isGranted("WorkflowManagement.WorkflowDefinitions.Design")
                            }
                        ]
                    }
                }
            ]
        })
    );

    defineWorkflowInputModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $("#CreateWorkflowDefinition").click(function (e) {
        e.preventDefault();
        createWorkflowDefinitionModal.open();
    });

    createWorkflowDefinitionModal.onResult(function () {
        if (arguments?.length > 1) {
            arguments[1].xhr.then(res => {
                window.open(abp.appPath + 'WorkflowDefinitions/Designer?workflowDefinitionId=' + res);
            });
        }

        dataTable.ajax.reload();
    });

    newWorkflowInstanceModal.onResult(function () {
        if (arguments?.length > 1) {
            arguments[1].xhr.then(res => {
                window.open(abp.appPath + 'WorkflowInstances/Designer?id=' + res);
            });
        }
    });
});