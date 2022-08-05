(function () {

    function SitemapSettingsController($scope, $http, editorState, notificationsService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = false;

        vm.hideFromSitemapProperty = {
            label: "Hide from sitemap",
            description: "Determines if pages of this type should be hidden from the sitemap",
            view: "boolean"
        }

        vm.changeFrequencyProperty = {
            label: "Change frequency",
            description: "The change frequency for this document type"
        }

        vm.priorityProperty = {
            label: "Priority",
            description: "The priority for this document type",
            view: "slider",
            enabled: false,
            config: {
                initVal1: 0,
                minVal: 0,
                maxVal: 1,
                step: 0.1
            }
        }

        vm.changeFrequencies = [
            { name: "Always", value: "always" },
            { name: "Hourly", value: "hourly" },
            { name: "Daily", value: "daily" },
            { name: "Weekly", value: "weekly" },
            { name: "Monthly", value: "monthly" },
            { name: "Yearly", value: "yearly" },
            { name: "Never", value: "never" }
        ];

        function init() {
            $http.get("backoffice/SeoToolkit/SitemapSettings/GetPageTypeSettings?contentTypeId=" + editorState.getCurrent().id)
                .then(function (response) {
                    var settings = response.data;

                    vm.hideFromSitemapProperty.value = settings.hideFromSitemap;
                    vm.changeFrequencyProperty.value = settings.changeFrequency;
                    
                    if (Utilities.isString(settings.priority) || Utilities.isNumber(settings.priority)) {
                        vm.priorityProperty.value = settings.priority.toString();
                        vm.priorityProperty.enabled = true;
                    } else {
                        vm.priorityProperty.enabled = false;
                    }

                    vm.loading = false;
                });
        }

        function save() {
            $http.post("backoffice/SeoToolkit/SitemapSettings/SetPageTypeSettings",
                {
                    contentTypeId: editorState.getCurrent().id,
                    hideFromSitemap: vm.hideFromSitemapProperty.value === "1" ? 1 : 0,
                    changeFrequency: vm.changeFrequencyProperty.value,
                    priority: vm.priorityProperty.enabled ? vm.priorityProperty.value : undefined
                }).then(function (response) {
                if (response.status !== 200) {
                    notificationsService.error("Something went wrong while saving sitemap settings");
                }
            });
        }

        unsubscribe.push($scope.$on("seoSettingsSubmitting",
            function () {
                save();
            }));

        vm.$onDestroy = function () {
            unsubscribe.forEach(x => x());
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.Displays.SitemapSettingsController", SitemapSettingsController);
})();