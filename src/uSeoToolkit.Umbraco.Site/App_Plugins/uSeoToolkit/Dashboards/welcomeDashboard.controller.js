(function() {
    "use strict";

    function welcomeDashboardController($http) {

        var vm = this;

        vm.loading = true;
        vm.modules = [];

        function init() {
            $http.get("backoffice/uSeoToolkit/Module/GetModules").then(function(response) {
                vm.modules = response.data;
                vm.loading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.uSeoToolkit.WelcomeDashboardController", welcomeDashboardController);
})();