(function () {
    "use strict";

    function propertyEditorController($scope, umbPropEditorHelper) {

        var vm = this;

        vm.field = $scope.field;

        vm.model = {
            view: vm.field.editor.config.view,
            value: vm.field.value
        }

        function init()
        {
            $scope.$on('uSeoToolkit.SaveField', beforeSaveEventHandler);
        }

        function beforeSaveEventHandler($event, $args) {
            vm.field.value = vm.model.value;
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.SeoFieldEditors.PropertyEditorController", propertyEditorController);
})();