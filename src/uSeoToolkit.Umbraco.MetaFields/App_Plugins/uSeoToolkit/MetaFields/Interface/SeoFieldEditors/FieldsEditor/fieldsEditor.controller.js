(function () {
    "use strict";

    function fieldsEditorController($scope, $http, notificationsService) {

        var vm = this;

        vm.field = $scope.field;
        vm.hasInheritance = $scope.hasInheritance;
        vm.customSelectedFields = [];
        vm.customBaseFields = [];
        vm.loading = true;

        function init() {

            $http.get("backoffice/uSeoToolkit/MetaFieldsSettings/GetAdditionalFields").then(function (response) {

                if (response.status !== 200) {
                    vm.loading = false;
                    notificationsService.error(
                        "Error",
                        "Something went wrong! Please try again later!");
                    return;
                }

                var selectedFields = [];
                var fields = getAllContentFields(vm.field.editor.config.dataTypes);
                response.data.forEach(function (field) {
                    if (!vm.hasInheritance && field.onlyShowIfInherited)
                        return;

                    fields.push({
                        name: field.name,
                        value: field.value,
                        onlyShowIfInherited: field.onlyShowIfInherited,
                        source: 2
                    });
                });
                fields.forEach(function (d) {
                    if (vm.field.value) {
                        const currentField = vm.field.value.find(function(v) {
                            return d.value === v.value;
                        });
                        if (currentField) {
                            selectedFields.push(d);
                            return;
                        }
                    }
                    vm.customBaseFields.push(d);
                });

                if (vm.field.value) {
                    //Make sure ordering is correct
                    vm.field.value.forEach(function (v) {
                        var value = selectedFields.find(function (s) {
                            return s.value === v.value;
                        });
                        if (value) {
                            vm.customSelectedFields.push(value);
                        }
                    });
                }

                $scope.$on('uSeoToolkit.SaveField', beforeSaveEventHandler);

                vm.loading = false;
            });
        }

        function getAllContentFields(fields) {
            return $scope.model.groups.flatMap(function (g) {
                return g.properties;
            }).filter(function (g) {
                return fields.includes(g.editor);
            }).map(function (g) {
                return { name: g.label, value: g.alias, source: 1 };
            });
        }

        function beforeSaveEventHandler($event, $args) {
            vm.field.value = vm.customSelectedFields.map(function (v) {
                return {
                    name: v.name,
                    value: v.value,
                    source: v.source
                };
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.SeoFieldEditors.FieldsEditorController", fieldsEditorController);
})();