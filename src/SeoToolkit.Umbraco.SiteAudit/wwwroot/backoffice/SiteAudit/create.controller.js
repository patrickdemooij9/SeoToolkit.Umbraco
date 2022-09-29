(function () {
    "use strict";

    function siteAuditCreateController($http, $routeParams, $location, editorService) {

        var vm = this;

        vm.isLoading = false;
        vm.checks = [];
        vm.selectedAudit = null;
        vm.allowSettingMiminumDelay = false;

        vm.toggleCheck = toggleCheck;
        vm.openNodeDialog = openNodeDialog;
        vm.createAudit = createAudit;
        vm.cancel = cancel;

        function toggleCheck(check) {
            if (vm.selectedAudit) {
                if (vm.selectedAudit.checks.includes(check.id)) {
                    const index = vm.selectedAudit.checks.indexOf(check.id);
                    vm.selectedAudit.checks.splice(index, 1);
                } else {
                    vm.selectedAudit.checks.push(check.id);
                }
            }
        }

        function openNodeDialog() {
            const contentNodePickerOptions = {
                submit: function (model) {
                    vm.selectedAudit.selectedNode = model.selection[0];
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            }
            editorService.contentPicker(contentNodePickerOptions);
        }

        function createAudit(startAudit) {
            $http.post("backoffice/SeoToolkit/SiteAudit/CreateAudit",
                {
                    Name: vm.selectedAudit.name,
                    SelectedNodeId: vm.selectedAudit.selectedNode.id,
                    Checks: vm.selectedAudit.checks,
                    StartAudit: startAudit,
                    MaxPagesToCrawl: vm.selectedAudit.maxPagesToCrawl,
                    DelayBetweenRequests: vm.selectedAudit.delayBetweenRequests
                }
            ).then(function (response) {
                $location.path("SeoToolkit/SiteAudit/detail").search("id", response.data);
            });
        }

        function cancel() {
            $location.path("SeoToolkit/SiteAudit/list");
        }

        function init() {
            vm.isLoading = true;
            $http.get("backoffice/SeoToolkit/SiteAudit/GetConfiguration").then(function (response) {
                vm.checks = response.data.checks;
                vm.allowSettingMiminumDelay = response.data.allowMinimumDelayBetweenRequestSetting;
                vm.selectedAudit = {
                    isEdit: true,
                    name: "",
                    checks: vm.checks.map(it => it.id),
                    delayBetweenRequests: response.data.minimumDelayBetweenRequest
                };
                vm.isLoading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.SiteAudit.CreateController", siteAuditCreateController);
})();