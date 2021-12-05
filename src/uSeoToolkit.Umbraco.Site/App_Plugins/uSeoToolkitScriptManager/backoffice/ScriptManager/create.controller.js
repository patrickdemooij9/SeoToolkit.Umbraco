(function() {
    "use strict";

    function siteManagerCreateController($scope, $http) {

        var vm = this;

        vm.page = {};
        vm.content = {};
        vm.definitions = [];

        function init() {
            loadDefinitions();
        }

        function loadDefinitions() {
            vm.page.isLoading = true;
            $http.get("backoffice/uSeoToolkit/ScriptManager/GetAllDefinitions").then(function (response) {
                vm.definitions = response.data;

                vm.page.isLoading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ScriptManager.CreateController", siteManagerCreateController);
})();