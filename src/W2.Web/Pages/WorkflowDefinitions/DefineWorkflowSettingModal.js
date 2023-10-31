abp.modals.DefineWorkflowSettingModal = function () {

    function initModal(modalManager, args) {
        var $modal = modalManager.getModal();

        $modal.find('#AddSetting').click(function (e) {
            console.log("hello")
            e.preventDefault();
            addPropertyElements();
        });

        $modal.find('#ClearSetting').click(function (e) {
            e.preventDefault();
            clearProperties($modal);
        });

        $modal.find('.remove-row').click(function (e) {
            removeRowButtonClick(e);
        });
    };

    function clearProperties(modal) {
        modal.find('#SettingContainer').empty();
    }

    function addPropertyElements() {
        const count = $('.property-wrapper').length;
        const newElementHtml = `        
            <div class="row property-wrapper">
                <div class="col-4">
                    <div class="mb-3">
	                    <label class="form-label" for="WorkflowSettingDefinition_PropertyDefinitionViewModels_${count}__key">Setting Key</label><span> * </span>
	                    <input type="text" data-val="true" 
                            data-val-required="The Property Key field is required." 
                            id="WorkflowSettingDefinition_PropertyDefinitionViewModels_${count}__Key"
                            name="WorkflowSettingDefinition.PropertyDefinitionViewModels[${count}].Key"
                            value="" class="form-control property-name"
                            aria-describedby="WorkflowSettingDefinition_PropertyDefinitionViewModels_${count}__Key-error"
                            aria-invalid="false" />
	                    <span class="text-danger field-validation-valid"
                            data-valmsg-for="WorkflowSettingDefinition.PropertyDefinitionViewModels[${count}].Key" data-valmsg-replace="true"></span>
                    </div>
                </div>
                <div class="col-4">
                    <div class="mb-3">
                        <label class="form-label" for="WorkflowSettingDefinition_PropertyDefinitionViewModels_${count}__Value">Setting Value</label>
                        <input data-val="true"
                            data-val-required="The Property Value field is required."
                            id="WorkflowSettingDefinition_PropertyDefinitionViewModels_${count}__Value"
                            name="WorkflowSettingDefinition.PropertyDefinitionViewModels[${count}].Value"
                            value="" class="form-control property-type"
                            aria-describedby="WorkflowSettingDefinition_PropertyDefinitionViewModels_${count}__Value-error"
                            aria-invalid="false"
                            
                        />
                        <span class="text-danger field-validation-valid"
                            data-valmsg-for="WorkflowSettingDefinition.PropertyDefinitionViewModels[${count}].Value" data-valmsg-replace="true"></span>
                    </div>
                </div>
                <div class="col">
                    <div class="mb-3">
                        <label class="form-label" style="width:100%;">&nbsp;</label>
                        <button class="remove-row btn btn-danger" type="button">
                            <i class="fa fa-minus"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        $(newElementHtml).appendTo('#SettingContainer');
        refreshEvents();
    }

    function refreshEvents() {
        $('.remove-row').click(function (e) {
            removeRowButtonClick(e);
        });
    }

    function removeRowButtonClick(e) {
        e.preventDefault();
        clearProperty(e);
    }

    function clearProperty(e) {
        const currentRow = $(e.target).parents('.property-wrapper');
        currentRow.remove();
        const rows = $('.property-wrapper');
        const count = rows.length;
        for (var i = 0; i < count; i++) {
            const row = $(rows[i]);
            const nameInput = row.find('.property-name');
            if (nameInput.length) {
                const nameInputId = `WorkflowSettingDefinition_PropertyDefinitionViewModels_${i}__Key`;
                const nameInputName = `WorkflowSettingDefinition.PropertyDefinitionViewModels[${i}].Key`;
                nameInput.attr('id', nameInputId);
                nameInput.attr('name', nameInputName);
                nameInput.attr('aria-describedby', `${nameInputId}-error`);
                nameInput.siblings('label').attr('for', nameInputId);
                nameInput.siblings('span').attr('data-valmsg-for', nameInputName);
            }
            const typeSelect = row.find('.property-type');
            if (typeSelect.length) {
                const typeSelectId = `WorkflowSettingDefinition_PropertyDefinitionViewModels_${i}__Value`;
                const typeSelectName = `WorkflowSettingDefinition.PropertyDefinitionViewModels[${i}].Value`;
                typeSelect.attr('id', typeSelectId);
                typeSelect.attr('name', typeSelectName);
                typeSelect.attr('aria-describedby', `${typeSelectId}-error`);
                nameInput.siblings('label').attr('for', typeSelectId);
                nameInput.siblings('span').attr('data-valmsg-for', typeSelectName);
            }
        }
    }

    return {
        initModal: initModal
    };
}