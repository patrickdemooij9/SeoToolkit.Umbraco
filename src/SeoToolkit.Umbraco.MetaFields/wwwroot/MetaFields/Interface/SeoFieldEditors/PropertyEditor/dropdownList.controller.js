angular.module("umbraco").controller("SeoToolkit.SeoFieldEditors.DropdownListController",
    function ($scope) {
        
        //ensure when form is saved that we don't store [] or [null] as string values in the database when no items are selected
        $scope.$on("formSubmitting", function () {
            if ($scope.model.value !== null && ($scope.model.value.length === 0 || $scope.model.value[0] === null)) {
                $scope.model.value = null;
            }
        });

        $scope.updateSingleDropdownValue = function() {
            $scope.model.value = $scope.model.singleDropdownValue;
        }
        
        //now we need to check if the value is null/undefined, if it is we need to set it to "" so that any value that is set
        // to "" gets selected by default
        if ($scope.model.value === null || $scope.model.value === undefined) {
            $scope.model.value = "";
        }
        
        // if we run in single mode we'll store the value in a local variable
        // so we can pass an array as the model as our PropertyValueEditor expects that
        $scope.model.singleDropdownValue = "";
        if ($scope.model.value) {
            $scope.model.singleDropdownValue = $scope.model.value;
        }
    });
