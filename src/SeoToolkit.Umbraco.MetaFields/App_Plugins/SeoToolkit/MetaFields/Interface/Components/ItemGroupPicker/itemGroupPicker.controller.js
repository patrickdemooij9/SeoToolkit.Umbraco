(function() {

    "use strict";

    function ItemGroupPickerController($scope) {

        var vm = this;
        
        vm.availableItems = $scope.model.availableItems;
        vm.model = {
            selection: []
        };

        if ($scope.model.selection) {
            vm.model.selection = $scope.model.selection;
        }

        vm.close = close;
        vm.submit = submit;
        vm.selectItem = selectItem;

        function selectItem(item) {
            if (vm.model.selection.includes(item)) {
                vm.model.selection.splice(vm.model.selection.indexOf(item), 1);
            } else {
                vm.model.selection.push(item);
            }
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        function submit() {
            if ($scope.model.submit) {
                $scope.model.submit(vm.model);
            }
        }
    }

    angular.module("umbraco").controller("SeoToolkit.MetaFields.ItemGroupPickerController", ItemGroupPickerController);

})();