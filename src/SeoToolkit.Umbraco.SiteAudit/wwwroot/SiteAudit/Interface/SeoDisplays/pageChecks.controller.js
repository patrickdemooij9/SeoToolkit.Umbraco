(function () {
    function SiteAuditPageChecks($scope, $http, editorState, notificationsService) {

        var vm = this;

        vm.pageChecks = [];
        vm.hasRan = false;

        $http.get("backoffice/SeoToolkit/SiteAuditPageCheck/GetPageChecks").then(function (response) {
            if (response.status === 200) {
                vm.pageChecks = response.data.map(function (check) {
                    return {
                        check,
                        hasError: false,
                        errorMessage: ''
                    }
                });
            }
        });

        vm.startChecks = function () {
            vm.saveButtonState = "busy";

            $http.post("backoffice/SeoToolkit/SiteAuditPageCheck/RunPageChecks", {
                contentId: editorState.current.id
            }).then(function (response) {
                const crawledPage = response.data.pagesCrawled[0];
                if (crawledPage.statusCode !== 200) {
                    notificationsService.error("Could not load page properly. Make sure the page can be loaded.");
                    return;
                }

                vm.pageChecks.forEach((item) => {
                    const errorCheck = crawledPage.results.find((pageCheckResult) => {
                        return item.check.id === pageCheckResult.checkId;
                    });
                    if (errorCheck) {
                        item.hasError = true;
                        item.errorMessage = errorCheck.message;
                    } else {
                        item.hasError = false;
                        item.errorMessage = "";
                    }
                })

                vm.saveButtonState = "success";
                vm.hasRan = true;
                console.log(response);
            });
        }
    }

    angular.module("umbraco").controller("SeoToolkit.SiteAuditPageChecksController", SiteAuditPageChecks);
})();