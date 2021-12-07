(function() {
    "use strict";

    function siteManagerCreateController($scope, $http, formHelper) {

        var vm = this;

        vm.save = save;

        vm.page = {};
        vm.content = {
            selectedDefinition: null
        };
        vm.definitions = [];
        vm.selectedDefinitionProperty = {
            alias: "selectedEditor",
            description: "Select a script type",
            label: "Script type"
        };
        vm.preValues = [];

        function init() {
            loadDefinitions();

            $scope.$watch("vm.content.selectedDefinition", function (newVal, oldVal) {
                if (newVal && (newVal !== oldVal)) {
                    const definition = vm.definitions.find(d => {
                        return d.alias === newVal;
                    });
                    if (definition) {
                        createPreValueProps(definition.fields);
                    } else {
                        vm.preValues = [];
                    }
                }
            });
        }

        function save() {
            if (formHelper.submitForm({ scope: $scope })) {
                vm.page.saveButtonState = "busy";

                const postModel = {
                    name: vm.content.name,
                    definitionAlias: vm.content.selectedDefinition,
                    fields: []
                };
                for (var i = 0; i < vm.preValues.length; i++) {
                    postModel.fields.push({
                        key: vm.preValues[i].alias,
                        value: vm.preValues[i].value
                    });
                }
                $http.post("backoffice/uSeoToolkit/ScriptManager/Create", postModel).then(function (response) {
                    formHelper.resetForm({ scope: $scope });
                    vm.page.saveButtonState = "success";
                });
            }
        }

        function loadDefinitions() {
            vm.page.isLoading = true;
            $http.get("backoffice/uSeoToolkit/ScriptManager/GetAllDefinitions").then(function (response) {
                vm.definitions = response.data;

                vm.page.isLoading = false;
            });
        }

        function createPreValueProps(preValues) {
            vm.preValues = [];
            for (var i = 0; i < preValues.length; i++)
                vm.preValues.push({
                    alias: null != preValues[i].key ? preValues[i].key : preValues[i].alias,
                    description: preValues[i].description,
                    label: preValues[i].name,
                    view: preValues[i].view,
                    value: preValues[i].value,
                    config: preValues[i].config
                });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ScriptManager.CreateController", siteManagerCreateController);
})();