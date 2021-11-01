angular.module("umbraco").component("siteAuditDetail", {
    templateUrl: "/App_Plugins/uSeoToolkit/Interface/SiteAudit/detail.html",
    controller: ["$routeParams", "$http", "eventsService", "siteAuditHub", function ($routeParams, $http, eventsService, siteAuditHub) {
        this.$onInit = function () {
            var auditId = $routeParams.id;
            if (!auditId) {
                eventsService.emit("uSeoToolkit.ViewUpdate", "SiteAuditOverview");
                return;
            }

            state.isLoading = true;

            siteAuditHub.initHub(function (hub) {
                state.hub = hub;

                var clientId = fn.getClientId();
                if (!clientId) {
                    state.hub.on("update",
                        function (update) {
                            console.log(update);
                            state.audit = update;
                            fn.loadAudit(false);
                        });

                    state.hub.start(function () {
                        fn.loadCurrentAudit(auditId);
                    });
                    return;
                }

                fn.loadCurrentAudit(auditId);
            });
        }

        var state = this.state = {
            isLoading: false,
            audit: null,
            errors: 0,
            warnings: 0,
            onlyShowIssues: false,
            hub: null
        }

        var fn = this.fn = {
            loadCurrentAudit: function (id) {
                $http.get("backoffice/uSeoToolkit/SiteAudit/Connect?auditId=" + id + "&clientId=" + fn.getClientId()).then(function (response) {
                    state.audit = response.data;
                    state.isLoading = false;

                    fn.loadAudit(true);
                });
            },
            loadAudit: function (hideShow) {
                state.errors = 0;
                state.warnings = 0;
                state.audit.PagesCrawled.forEach(function (p) {
                    p.Errors = 0;
                    p.Warnings = 0;
                    p.Show = hideShow ? false : p.Show;
                    p.Results.forEach(function (r) {
                        if (r.IsError) {
                            state.errors++;
                            p.Errors++;
                        }
                        if (r.IsWarning) {
                            state.warnings++;
                            p.Warnings++;
                        }
                    });
                });
            },
            openPage: function (page) {
                page.Show = true;
            },
            closePage: function (page) {
                page.Show = false;
            },
            toggleShowIssues: function () {
                state.onlyShowIssues = !state.onlyShowIssues;
            },
            pageFilter: function (page) {
                if (!state.onlyShowIssues) {
                    return true;
                }
                return page.Errors > 0 || page.Warnings > 0;
            },
            getClientId: function () {
                if ($.connection !== undefined && $.connection.hub !== undefined) {
                    return $.connection.hub.id;
                }
                return "";
            }
        }
    }]
});