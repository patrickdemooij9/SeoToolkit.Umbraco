(function() {
    "use strict";

    function welcomeDashboardController($http) {

        var vm = this;

        vm.loading = true;
        vm.modules = [];

        function init() {
            $http.get("backoffice/SeoToolkit/Module/GetModules").then(function(response) {
                vm.modules = response.data;
                vm.loading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.SeoToolkit.WelcomeDashboardController", welcomeDashboardController);
})();