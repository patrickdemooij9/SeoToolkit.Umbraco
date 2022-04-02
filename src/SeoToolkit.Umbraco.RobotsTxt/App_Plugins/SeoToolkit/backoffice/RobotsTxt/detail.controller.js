(function () {
    "use strict";

    function robotsTxtDetailController($scope, $http, notificationsService, formHelper) {

        var vm = this;
        vm.validationErrors = [];

        vm.model = "";
        vm.loading = false;

        vm.save = save;

        function save() {
            vm.validationErrors = [];
            $http.post("backoffice/SeoToolkit/RobotsTxt/Save", {
                content: vm.model
            }).then(function (response) {
                notificationsService.success("Robots.txt saved!");
                vm.model = response.data;
                formHelper.resetForm({ scope: $scope });
            }, function (response) {
                vm.validationErrors = response.data;
                notificationsService.error("Something went wrong while saving Robots.txt");
            });
        }

        function init() {
            vm.loading = true;
            $http.get("backoffice/SeoToolkit/RobotsTxt/Get").then(function(response) {
                vm.model = response.data;
                vm.loading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.RobosTxt.DetailController", robotsTxtDetailController);
})();