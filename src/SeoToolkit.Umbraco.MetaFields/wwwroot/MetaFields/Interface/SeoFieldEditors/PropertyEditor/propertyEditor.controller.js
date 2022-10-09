(function () {
    "use strict";

    function propertyEditorController($scope, umbPropEditorHelper) {

        var vm = this;

        vm.field = $scope.field;

        vm.model = {
            view: vm.field.editor.config.view,
            config: vm.field.editor.config,
            value: vm.field.value
        }

        if (vm.field.editor.config.isPreValue) {
            vm.isPreValue = vm.field.editor.config.isPreValue;
        } else {
            vm.isPreValue = false;
        }

        function init()
        {
            $scope.$on('SeoToolkit.SaveField', beforeSaveEventHandler);
        }

        function beforeSaveEventHandler($event, $args) {
            vm.field.value = vm.model.value;
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.SeoFieldEditors.PropertyEditorController", propertyEditorController);
})();