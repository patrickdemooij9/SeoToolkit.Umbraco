(function () {
    "use strict";

    function robotsTxtDetailController($scope, $http, notificationsService, formHelper, overlayService) {

        var vm = this;
        vm.validationErrors = [];

        vm.model = "";
        vm.loading = false;

        vm.save = save;

        function save(skipValidation) {
            vm.validationErrors = [];
            $http.post("backoffice/SeoToolkit/RobotsTxt/Save", {
                skipValidation: !!skipValidation,
                content: vm.model
            }).then(function (response) {
                notificationsService.success("Robots.txt saved!");
                vm.model = response.data;
                formHelper.resetForm({ scope: $scope });
            }, function (response) {
                vm.validationErrors = response.data;
                notificationsService.error("Something went wrong while saving Robots.txt");

                // Display an overlay to allow the user to force the save action and skip validation
                if (!skipValidation && vm.validationErrors) {
                    overlayService.open({
                        title: 'Robots.txt validation didn\'t pass',
                        subtitle: 'The input submitted didn\'t pass the validation.',
                        view: 'default',
                        content: 'Are you sure you want to save the robots.txt? - ignoring the validation errors could result in potentially using an invalid robots.txt, which has a negative impact on SEO.',
                        submitButtonLabel: 'Ignore and save',
                        submitButtonStyle: 'danger',
                        closeButtonLabel: 'Close and fix errors',
                        submit: function() {
                            save(true);
                            overlayService.close();
                        },
                        close: function() {
                            overlayService.close();
                        }
                    });
                }
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