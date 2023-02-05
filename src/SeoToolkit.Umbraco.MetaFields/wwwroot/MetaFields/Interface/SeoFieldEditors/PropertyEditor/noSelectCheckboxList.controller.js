(function () {
    function noSelectCheckboxListController($scope) {
        var vm = this;

        const noneValue = 'none';

        vm.items = [];

        vm.change = change;

        function change(model, value) {
            var index = $scope.model.value.indexOf(value);
            if (model === true) {

                if (value === noneValue) {
                    $scope.model.value = [];
                    vm.items.forEach(item => {
                        if (item.value !== noneValue) {
                            item.checked = false;
                        }
                    });
                } else {
                    if ($scope.model.value.includes(noneValue)) {
                        var noneIndex = $scope.model.value.indexOf(noneValue);
                        if (noneIndex !== -1) {
                            $scope.model.value.splice(noneIndex, 1);
                        }

                        var item = vm.items.find(item => item.value === noneValue);
                        if (item) {
                            item.checked = false;
                        }
                    }
                }

                //if it doesn't exist in the model, then add it
                if (index < 0) {
                    $scope.model.value.push(value);
                }
            } else {
                //if it exists in the model, then remove it
                if (index >= 0) {
                    $scope.model.value.splice(index, 1);
                }
            }
        }

        function init() {
            if (!Array.isArray($scope.model.value)) {
                $scope.model.value = [];
            }

            $scope.model.config.prevalues.forEach(function (prevalue) {
                var newItem = { ...prevalue };
                if ($scope.model.value.find(item => item === newItem.value)) {
                    newItem.checked = true;
                }

                vm.items.push(newItem);
            });

            var noneItem = {
                value: noneValue,
                label: 'None',
                checked: $scope.model.value.find(item => item === noneValue) != null
            };
            vm.items.push(noneItem);
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.SeoFieldEditors.NoSelectCheckboxList", noSelectCheckboxListController);
})();