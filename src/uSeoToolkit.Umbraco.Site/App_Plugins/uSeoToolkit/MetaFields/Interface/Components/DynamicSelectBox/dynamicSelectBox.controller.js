angular.module("umbraco").component("dynamicSelectBox",
    {
        templateUrl: "/App_Plugins/uSeoToolkit/MetaFields/Interface/Components/DynamicSelectBox/dynamicSelectBox.html",
        bindings: {
            baseList: "<",
            selectedList: "=",
            field: "="
        },
        controllerAs: 'vm',
        controller: ['$scope', function ($scope) {

            var vm = this;

            vm.addField = addField;
            vm.removeField = removeField;

            function addField() {
                var exists = vm.baseList.find(function (i) {
                    return i.value === vm.selectedField;
                });

                if (exists) {
                    vm.selectedList.push(exists);

                    var index = vm.baseList.indexOf(exists);
                    vm.baseList.splice(index, 1);
                }
            }

            function removeField(val) {
                if (vm.field.disabled)
                    return;

                var index = vm.selectedList.indexOf(val);
                vm.selectedList.splice(index, 1);

                vm.baseList.push(val);
            }
        }]
    });