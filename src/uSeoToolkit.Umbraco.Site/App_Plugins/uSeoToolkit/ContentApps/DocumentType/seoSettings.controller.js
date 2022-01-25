(function() {
    "use strict";

    function SeoSettingsController($scope, $http) {

        var vm = this;

        vm.loading = true;
        vm.displays = [];

        vm.model = {
            enableSeoSettings: false
        }

        vm.toggleSeoSettings = function () {
            vm.model.enableSeoSettings = !vm.model.enableSeoSettings;
        }

        function init() {
            $http.get("backoffice/uSeoToolkit/SeoSettings/Get?contentTypeId=" + $scope.model.id).then(
                function(response) {
                    if (response.status === 200) {
                        vm.model.enableSeoSettings = response.data.isEnabled;
                        vm.displays = response.data.displays;
                        vm.displays[0].active = true;

                        vm.loading = true;
                    }
                });
        }

        function save() {

        }

        $scope.$on("formSubmitting",
            function () {
                save();
            });

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ContentApps.SeoSettingsController", SeoSettingsController);
})();