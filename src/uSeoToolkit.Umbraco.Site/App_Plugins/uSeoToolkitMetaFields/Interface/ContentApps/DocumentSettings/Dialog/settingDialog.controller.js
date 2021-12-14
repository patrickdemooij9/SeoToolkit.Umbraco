(function () {
    "use strict";

    function settingDialogController($scope) {
        var vm = this;

        $scope.field = $scope.model.field;

        vm.close = close;
        vm.submit = submit;

        function close() {
            if ($scope.model && $scope.model.close) {
                $scope.model.close();
            }
        }

        function submit() {
            $scope.$broadcast("uSeoToolkit.SaveField");
            if ($scope.model && $scope.model.submit) {
                $scope.model.submit($scope.model);
            }
        }
    }

    angular.module("umbraco").controller("uSeoToolkit.DocumentSettings.SettingDialogController", settingDialogController);
})();