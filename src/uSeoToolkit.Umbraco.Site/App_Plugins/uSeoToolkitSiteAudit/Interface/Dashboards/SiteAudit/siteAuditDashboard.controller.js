(function () {
    "use strict";

    function SiteAuditDashboard($scope, eventsService) {

        var vm = this;

        vm.showOverview = true;
        vm.showCreate = false;
        vm.showEdit = false;

        var unsubscribe = eventsService.on("uSeoToolkit.ViewUpdate", function (event, args) {
            vm.showOverview = (args == 'SiteAuditOverview');
            vm.showCreate = (args == 'SiteAuditCreate');
            vm.showDetail = (args == 'SiteAuditDetail');
        });

        $scope.$on("$destroy", function () {
            unsubscribe();
        });
    }

    angular.module("umbraco").controller("USeoToolkit.SiteAuditDashboard.controller", SiteAuditDashboard);
})();