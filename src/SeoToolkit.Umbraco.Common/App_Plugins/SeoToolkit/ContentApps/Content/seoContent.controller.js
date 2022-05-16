(function() {

    function SeoContentController($http, editorState, eventsService)
    {
        var unsubscribe = [];

        var vm = this;
        vm.displays = [];

        vm.setActive = setActive;

        function setActive(display) {
            var currentDisplay = vm.displays.find(display => {
                return display.active;
            });
            if (currentDisplay) {
                currentDisplay.active = false;
            }

            display.active = true;
        }

        function init() {
            $http.get("backoffice/SeoToolkit/SeoContent/Get?contentId=" + editorState.getCurrent().id).then(
                function (response) {
                    if (response.status === 200) {
                        vm.displays = response.data.displays;
                        vm.displays[0].active = true;

                        vm.loading = false;
                    }
                });
        }

        unsubscribe.push(eventsService.on("app.tabChange",
            (e, data) => {
                if (data.alias !== "seoContent") {
                    return;
                }

                init();
            }));

        vm.$onDestroy = function () {
            unsubscribe.forEach(x => x());
        }
    }

    angular.module("umbraco").controller("SeoToolkit.ContentApps.SeoContentController", SeoContentController);
})();