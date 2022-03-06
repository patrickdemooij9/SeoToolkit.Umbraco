(function() {
    "use strict";

    function createRedirectController($scope) {

        var vm = this;

        vm.submit = submit;
        vm.close = close;

        vm.domainProperty = {
            alias: "domain",
            label: "Domain",
            description: "Choose the domain set within Umbraco or use a custom domain",
            view: "dropdownFlexible",
            value: "",
            config: {
                items: ["Test1", "Test2", "Test3"]
            },
            validation: {
                mandatory: true
            }
        }

        function submit() {

        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }
    }

    angular.module("umbraco").controller("uSeoToolkit.Redirects.CreateController", createRedirectController);
})();