(function () {
    "use strict";

    function fieldsEditorController($scope) {

        var vm = this;

        vm.field = $scope.field;
        vm.customSelectedFields = [];
        vm.customBaseFields = [];

        function init() {
            var selectedFields = [];
            getAllContentFields(vm.field.editor.config.dataTypes).forEach(function (d) {
                if (vm.field.value && vm.field.value.includes(d.value)) {
                    selectedFields.push(d);
                } else {
                    vm.customBaseFields.push(d);
                }
            });

            if (vm.field.value) {
                //Make sure ordering is correct
                vm.field.value.forEach(function (v) {
                    var value = selectedFields.find(function(s) {
                        return s.value === v;
                    });
                    if (value) {
                        vm.customSelectedFields.push(value);
                    }
                });
            }

            $scope.$on('uSeoToolkit.SaveField', beforeSaveEventHandler);
        }

        function getAllContentFields(fields) {
            return $scope.model.groups.flatMap(function (g) {
                return g.properties;
            }).filter(function (g) {
                return fields.includes(g.editor);
            }).map(function (g) {
                return { name: g.label, value: g.alias };
            });
        }

        function beforeSaveEventHandler($event, $args) {
            vm.field.value = vm.customSelectedFields.map(function (v) {
                return v.value;
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.SeoFieldEditors.FieldsEditorController", fieldsEditorController);
})();