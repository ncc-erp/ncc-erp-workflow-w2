$(function () {
    var l = abp.localization.getResource('W2');

    var newWorkflowInstanceModal = new abp.ModalManager(abp.appPath + 'WorkflowDefinitions/NewWorkflowInstanceModal');

    var defineWorkflowInputModal = new abp.ModalManager({
        viewUrl: abp.appPath + 'WorkflowDefinitions/DefineWorkflowInputModal',
        scriptUrl: '/Pages/WorkflowDefinitions/DefineWorkflowInputModal.js',
        modalClass: 'DefineWorkflowInputModal'
    });

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
                    title: l('WorkflowDefinition:Actions'),
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
                                }
                            },
                            {
                                text: l('WorkflowDefinition:DefineInput'),
                                action: function (data) {
                                    defineWorkflowInputModal.open({ workflowDefinitionId: data.record.definitionId });
                                },
                                visible: function (record) {
                                    return !!!record.inputDefinition;
                                }
                            },
                            {
                                text: l('WorkflowDefinition:OpenDesigner'),
                                action: function (data) {
                                    window.open(abp.appPath + 'Designer?workflowDefinitionId=' + data.record.definitionId);
                                }
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
});