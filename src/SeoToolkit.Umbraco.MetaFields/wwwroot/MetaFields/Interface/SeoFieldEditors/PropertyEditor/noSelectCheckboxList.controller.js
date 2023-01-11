(function () {
    function noSelectCheckboxListController($scope) {
        var vm = this;

        vm.prevalues = $scope.model.config.prevalues;
    }

    angular.module("umbraco").controller("SeoToolkit.SeoFieldEditors.NoSelectCheckboxList", noSelectCheckboxListController);
})();