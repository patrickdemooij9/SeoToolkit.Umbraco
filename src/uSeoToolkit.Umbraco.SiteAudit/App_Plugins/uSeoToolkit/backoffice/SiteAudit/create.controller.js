(function () {
    "use strict";

    function siteAuditCreateController($http, $routeParams, $location, editorService) {

        var vm = this;

        vm.isLoading = false;
        vm.checks = [];
        vm.selectedAudit = null;

        vm.toggleCheck = toggleCheck;
        vm.openNodeDialog = openNodeDialog;
        vm.createAudit = createAudit;
        vm.cancel = cancel;

        function toggleCheck(check) {
            if (vm.selectedAudit) {
                if (vm.selectedAudit.checks.includes(check.Id)) {
                    const index = vm.selectedAudit.checks.indexOf(check.Id);
                    vm.selectedAudit.checks.splice(index, 1);
                } else {
                    vm.selectedAudit.checks.push(check.Id);
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
            $http.post("backoffice/uSeoToolkit/SiteAudit/CreateAudit",
                {
                    Name: vm.selectedAudit.name,
                    SelectedNodeId: vm.selectedAudit.selectedNode.id,
                    Checks: vm.selectedAudit.checks,
                    StartAudit: startAudit,
                    MaxPagesToCrawl: vm.selectedAudit.maxPagesToCrawl,
                    DelayBetweenRequests: vm.selectedAudit.delayBetweenRequests
                }
            ).then(function (response) {
                $location.path("uSeoToolkit/SiteAudit/detail").search("id", response.data);
            });
        }

        function cancel() {
            $location.path("uSeoToolkit/SiteAudit/list");
        }

        function init() {
            vm.isLoading = true;
            $http.get("backoffice/uSeoToolkit/SiteAudit/GetAllChecks").then(function (response) {
                vm.checks = response.data;
                vm.selectedAudit = {
                    isEdit: true,
                    name: "",
                    checks: vm.checks.map(it => it.Id)
                };
                vm.isLoading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.SiteAudit.CreateController", siteAuditCreateController);
})();