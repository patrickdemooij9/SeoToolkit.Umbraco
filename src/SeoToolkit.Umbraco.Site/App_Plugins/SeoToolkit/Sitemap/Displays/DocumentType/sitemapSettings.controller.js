(function () {

    function SitemapSettingsController($scope, $http, editorState, notificationsService) {
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
            description: "The priority for this document type"
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
                    vm.priorityProperty.value = settings.priority;

                    vm.loading = false;
                });
        }

        function save() {
            $http.post("backoffice/SeoToolkit/SitemapSettings/SetPageTypeSettings",
                {
                    contentTypeId: editorState.getCurrent().id,
                    hideFromSitemap: vm.hideFromSitemapProperty.value === "1" ? 1 : 0,
                    changeFrequency: vm.changeFrequencyProperty.value,
                    priority: vm.priorityProperty.value
                }).then(function (response) {
                    if (response.status !== 200) {
                        notificationsService.error("Something went wrong while saving sitemap settings");
                    }
                });
        }

        $scope.$on("seoSettingsSubmitting",
            function () {
                save();
            });

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.Displays.SitemapSettingsController", SitemapSettingsController);
})();