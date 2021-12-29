angular.module("umbraco").component("siteAuditOverview", {
    templateUrl: "/App_Plugins/uSeoToolkit/Interface/SiteAudit/overview.html",
    controller: ["$http", "$routeParams", "eventsService", function ($http, $routeParams, eventsService) {
        this.$onInit = function () {
            state.isLoading = true;
            $http.get("backoffice/uSeoToolkit/SiteAudit/GetAll").then(function (response) {
                state.items = response.data.map(function(i) {
                    return {
                        "Id": i.Id,
                        "icon": "icon-document",
                        "name": i.Name,
                        "published": true
                    }
                });
                state.isLoading = false;
            });
        }

        var state = this.state = {
            isLoading: false
        }

        var fn = this.fn = {
            startCreateAudit: function () {
                eventsService.emit("uSeoToolkit.ViewUpdate", "SiteAuditCreate");
            },
            clickItem: function (item) {
                $routeParams.id = item.Id;
                eventsService.emit("uSeoToolkit.ViewUpdate", "SiteAuditDetail");
            },
            selectItem: function(selectedItem, $index, $event) {
                alert("select node");
            }
        }
    }]
});