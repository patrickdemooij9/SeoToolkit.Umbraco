(function () {
    "use strict";

    function MetaFieldsPreviewerController($scope) {

        var vm = this;

        vm.fields = $scope.fields;

        vm.getValue = getValue;

        function getValue(fieldAlias) {
            var field = vm.fields.find(it => {
                return it.alias === fieldAlias;
            });
            if (!field) {
                return "";
            }
            return field.getValue();
        }
    }

    angular.module("umbraco").controller("SeoToolkit.Previewers.MetaFields", MetaFieldsPreviewerController);
})();