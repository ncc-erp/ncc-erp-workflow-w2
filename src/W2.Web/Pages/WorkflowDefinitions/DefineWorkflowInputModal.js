abp.modals.DefineWorkflowInputModal = function () {

    function initModal(modalManager, args) {
        var $modal = modalManager.getModal();

        $modal.find('#AddProperty').click(function (e) {
            e.preventDefault();
            addPropertyElements();
        });

        $modal.find('#ClearProperties').click(function (e) {
            e.preventDefault();
            clearProperties($modal);
        });

        $modal.find('.remove-row').click(function (e) {
            removeRowButtonClick(e);
        });
    };

    function clearProperties(modal) {
        modal.find('#PropertiesContainer').empty();
    }

    function addPropertyElements() {
        const count = $('.property-wrapper').length;
        const newElementHtml = `        
            <div class="row property-wrapper">
                <div class="col-5">
                    <div class="mb-3">
	                    <label class="form-label" for="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Name">Property Name</label><span> * </span>
	                    <input type="text" data-val="true" 
                            data-val-required="The Property Name field is required." 
                            id="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Name"
                            name="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Name"
                            value="" class="form-control property-name"
                            aria-describedby="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Name-error"
                            aria-invalid="false" />
	                    <span class="text-danger field-validation-valid"
                            data-valmsg-for="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Name" data-valmsg-replace="true"></span>
                    </div>
                </div>
                <div class="col-5">
                    <div class="mb-3">
                        <label class="form-label" for="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Type">Property Type</label>
                        <select class="form-select form-control property-type" data-val="true"
                            data-val-required="The Property Type field is required."
                            id="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Type"
                            name="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Type"
                            aria-describedby="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Type-error"
                            aria-invalid="false">
                            <option selected="selected" value="Text">Text</option>
                            <option value="Numeric">Numeric</option>
                            <option value="DateTime">Date Time</option>
                            <option value="RichText">Rich Text</option>
                            <option value="UserList">User List</option>
                            <option value="MyProject">My Project</option>
                            <option value="MyPMProject">My PM Project</option>
                            <option value="OfficeList">Office List</option>
                        </select>
                        <span class="text-danger field-validation-valid"
                            data-valmsg-for="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Type" data-valmsg-replace="true"></span>
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
        $(newElementHtml).appendTo('#PropertiesContainer');
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
                const nameInputId = `WorkflowInputDefinition_PropertyDefinitionViewModels_${i}__Name`;
                const nameInputName = `WorkflowInputDefinition.PropertyDefinitionViewModels[${i}].Name`;
                nameInput.attr('id', nameInputId);
                nameInput.attr('name', nameInputName);
                nameInput.attr('aria-describedby', `${nameInputId}-error`);
                nameInput.siblings('label').attr('for', nameInputId);
                nameInput.siblings('span').attr('data-valmsg-for', nameInputName);
            }
            const typeSelect = row.find('.property-type');
            if (typeSelect.length) {
                const typeSelectId = `WorkflowInputDefinition_PropertyDefinitionViewModels_${i}__Type`;
                const typeSelectName = `WorkflowInputDefinition.PropertyDefinitionViewModels[${i}].Type`;
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