angular.module("umbraco").component("siteAuditCreate", {
    templateUrl: "/App_Plugins/uSeoToolkit/Interface/SiteAudit/create.html",
    controller: ["$http", "$routeParams", "eventsService", "editorService", function ($http, $routeParams, eventsService, editorService) {
        this.$onInit = function () {
            state.isLoading = true;
            $http.get("backoffice/uSeoToolkit/SiteAudit/GetAllChecks").then(function (response) {
                state.checks = response.data;
                state.selectedAudit = {
                    isEdit: true,
                    name: "",
                    checks: state.checks.map(it => it.Id)
                };
                state.isLoading = false;
            });
        }

        var state = this.state = {
            isLoading: false,
            checks: [],
            selectedAudit: null
        }

        var fn = this.fn = {
            toggleCheck: function (check) {
                if (state.selectedAudit) {
                    if (state.selectedAudit.checks.includes(check.Id)) {
                        var index = state.selectedAudit.checks.indexOf(check.Id);
                        state.selectedAudit.checks.splice(index);
                    } else {
                        state.selectedAudit.checks.push(check.Id);
                    }
                }
            },
            openNodeDialog: function () {
                var contentNodePickerOptions = {
                    submit: function (model) {
                        state.selectedAudit.selectedNode = model.selection[0];
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                }
                editorService.contentPicker(contentNodePickerOptions);
            },
            createAudit: function (startAudit) {
                $http.post("backoffice/uSeoToolkit/SiteAudit/CreateAudit",
                    {
                        Name: state.selectedAudit.name,
                        SelectedNodeId: state.selectedAudit.selectedNode.id,
                        Checks: state.selectedAudit.checks,
                        StartAudit: startAudit,
                        MaxPagesToCrawl: state.selectedAudit.maxPagesToCrawl,
                        DelayBetweenRequests: state.selectedAudit.delayBetweenRequests
                    }
                ).then(function (response) {
                    console.log(response);
                    $routeParams.id = response.data.Id;
                    eventsService.emit("uSeoToolkit.ViewUpdate", "SiteAuditDetail");
                });
            },
            cancel: function () {
                eventsService.emit("uSeoToolkit.ViewUpdate", "SiteAuditOverview");
            }
        }
    }]
});