angular.module("umbraco").component("siteAuditOverview", {
    templateUrl: "/App_Plugins/SeoToolkit/Interface/SiteAudit/overview.html",
    controller: ["$http", "$routeParams", "eventsService", function ($http, $routeParams, eventsService) {
        this.$onInit = function () {
            state.isLoading = true;
            $http.get("backoffice/SeoToolkit/SiteAudit/GetAll").then(function (response) {
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
                eventsService.emit("SeoToolkit.ViewUpdate", "SiteAuditCreate");
            },
            clickItem: function (item) {
                $routeParams.id = item.Id;
                eventsService.emit("SeoToolkit.ViewUpdate", "SiteAuditDetail");
            },
            selectItem: function(selectedItem, $index, $event) {
                alert("select node");
            }
        }
    }]
});