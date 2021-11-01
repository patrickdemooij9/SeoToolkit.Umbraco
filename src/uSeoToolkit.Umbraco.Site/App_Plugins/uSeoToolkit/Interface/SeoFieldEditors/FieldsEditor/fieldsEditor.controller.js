(function () {
    "use strict";

    function fieldsEditorController($scope) {

        var vm = this;

        vm.field = $scope.field;
        vm.customSelectedFields = [];
        vm.customBaseFields = [];

        function init() {
            getAllContentFields(vm.field.editor.config.dataTypes).forEach(function (d) {
                if (vm.field.value && vm.field.value.includes(d.value)) {
                    vm.customSelectedFields.push(d);
                } else {
                    vm.customBaseFields.push(d);
                }
            });

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