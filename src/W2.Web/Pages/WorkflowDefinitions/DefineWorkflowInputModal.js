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
    };

    function clearProperties(modal) {
        modal.find('#PropertiesContainer').empty();
    }

    function addPropertyElements() {
        let count = $('.property-wrapper').length;
        let newElementHtml = `        
            <div class="row property-wrapper">
                <div class="col">
                    <div class="mb-3">
	                    <label class="form-label" for="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Name">Property Name</label><span> * </span>
	                    <input type="text" data-val="true" 
                            data-val-required="The Property Name field is required." 
                            id="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Name"
                            name="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Name"
                            value="" class="form-control valid"
                            aria-describedby="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Name-error"
                            aria-invalid="false" />
	                    <span class="text-danger field-validation-valid"
                            data-valmsg-for="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Name" data-valmsg-replace="true"></span>
                    </div>
                </div>
                <div class="col">
                    <div class="mb-3">
                        <label class="form-label" for="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Type">Property Type</label>
                        <select class="form-select form-control valid" data-val="true"
                            data-val-required="The Property Type field is required."
                            id="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Type"
                            name="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Type"
                            aria-describedby="WorkflowInputDefinition_PropertyDefinitionViewModels_${count}__Type-error"
                            aria-invalid="false">
                            <option selected="selected" value="Text">Text</option>
                            <option value="Numeric">Numeric</option>
                            <option value="DateTime">DateTime</option>
                            <option value="DropdownList">DropdownList</option>
                            <option value="TextMultiLine">TextMultiLine</option>
                        </select>
                        <span class="text-danger field-validation-valid"
                            data-valmsg-for="WorkflowInputDefinition.PropertyDefinitionViewModels[${count}].Type" data-valmsg-replace="true"></span>
                    </div>
                </div>
            </div>
        `;
        $(newElementHtml).appendTo('#PropertiesContainer');
    }

    return {
        initModal: initModal
    };
}