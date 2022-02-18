(function () {
    "use strict";

    function siteManagerEditController($scope, $http, $routeParams, $location, formHelper, notificationsService) {

        var vm = this;

        vm.save = save;
        vm.back = back;

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
            if ($routeParams.id && $routeParams.id > -1) {
                loadScript($routeParams.id);
            } else {
                loadDefinitions();
                setupWatch();
            }
        }

        function save() {
            if (formHelper.submitForm({ scope: $scope })) {
                vm.page.saveButtonState = "busy";

                const postModel = {
                    name: vm.content.name,
                    definitionAlias: vm.content.selectedDefinition,
                    fields: []
                };
                if (vm.content.id && vm.content.id > -1) {
                    postModel.id = vm.content.id;
                }
                for (var i = 0; i < vm.preValues.length; i++) {
                    postModel.fields.push({
                        key: vm.preValues[i].alias,
                        value: vm.preValues[i].value
                    });
                }
                $http.post("backoffice/uSeoToolkit/ScriptManager/Save", postModel).then(function (response) {
                    formHelper.resetForm({ scope: $scope });

                    if (response.status === 200) {
                        vm.content.id = response.data.id;

                        vm.page.saveButtonState = "success";
                        notificationsService.success("Script Saved", "Your script has successfully been saved!");
                    } else {
                        vm.page.saveButtonState = "error";
                        notificationsService.error("Error", "Something went wrong while saving your script!");
                    }
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

        function loadScript(id) {
            //TODO: Clean up
            vm.page.isLoading = true;
            $http.get("backoffice/uSeoToolkit/ScriptManager/GetAllDefinitions").then(function (definitionResponse) {
                vm.definitions = definitionResponse.data;

                $http.get("backoffice/uSeoToolkit/ScriptManager/Get?id=" + id).then(function (response) {
                    vm.content = {
                        id: response.data.id,
                        name: response.data.name,
                        selectedDefinition: response.data.definitionAlias
                    }

                    const definition = vm.definitions.find(d => {
                        return d.alias === response.data.definitionAlias;
                    });
                    if (definition) {
                        createPreValueProps(definition.fields);
                    } else {
                        vm.preValues = [];
                    }

                    vm.preValues.forEach(function(preValue) {
                        preValue.value = response.data.config[preValue.alias];
                    });

                    setupWatch();

                    vm.page.isLoading = false;
                });
            });
        }

        function back() {
            $location.path("uSeoToolkit/ScriptManager/list").search("id", null);;
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

        function setupWatch() {
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

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ScriptManager.EditController", siteManagerEditController);
})();