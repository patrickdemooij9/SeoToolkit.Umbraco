(function () {
    "use strict";

    function SeoSettingsController($scope, $http, notificationsService) {

        var vm = this;

        vm.loading = true;
        vm.displays = [];

        vm.setActive = setActive;

        vm.model = {
            enableSeoSettings: false
        }

        vm.setSeoSettings = function (value) {
            vm.model.enableSeoSettings = value;
        }

        function setActive(display) {
            var currentDisplay = vm.displays.find(display => {
                return display.active;
            });
            if (currentDisplay) {
                currentDisplay.active = false;
            }

            display.active = true;
        }

        function init() {
            $http.get("backoffice/uSeoToolkit/SeoSettings/Get?contentTypeId=" + $scope.model.id).then(
                function (response) {
                    if (response.status === 200) {
                        vm.model.enableSeoSettings = response.data.isEnabled;
                        vm.displays = response.data.displays;
                        vm.displays[0].active = true;

                        vm.loading = true;
                    }
                });
        }

        function save() {
            $http.post("backoffice/uSeoToolkit/SeoSettings/Set",
                {
                    contentTypeId: $scope.model.id,
                    enabled: vm.model.enableSeoSettings
                }).then(function (response) {
                    if (response.status !== 200) {
                        notificationsService.error("Something went wrong while saving SEO settings");
                    } else {
                        notificationsService.success("SEO settings saved!");
                    }
                });
        }

        $scope.$on("formSubmitting",
            function () {
                save();

                if (vm.model.enableSeoSettings) {
                    $scope.$broadcast("seoSettingsSubmitting");
                }
            });

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ContentApps.SeoSettingsController", SeoSettingsController);
})();