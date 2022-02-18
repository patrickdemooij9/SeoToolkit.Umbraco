(function () {
    "use strict";

    function robotsTxtDetailController($scope, $http, notificationsService) {

        var vm = this;

        vm.loading = false;
        vm.editProperty = {
            label: "Robots.txt",
            description: "Robots.txt is used to let bots know what they are able to access",
            view: "textarea"
        }

        vm.save = save;

        function save() {
            $http.post("backoffice/uSeoToolkit/RobotsTxt/Save", {
                content: vm.editProperty.value
            }).then(function (response) {
                if (response.status !== 200) {
                    notificationsService.error("Something went wrong while saving Robots.txt");
                } else {
                    notificationsService.success("Robots.txt saved!");
                }
            });
        }

        function init() {
            vm.loading = true;
            $http.get("backoffice/uSeoToolkit/RobotsTxt/Get").then(function (response) {
                vm.editProperty.value = response.data;
                vm.loading = false;
            })
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.RobosTxt.DetailController", robotsTxtDetailController);
})();