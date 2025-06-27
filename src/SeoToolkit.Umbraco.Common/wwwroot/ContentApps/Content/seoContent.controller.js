(function () {

    function SeoContentController($scope, $http, editorState, eventsService, notificationsService, contentAppHelper) {
        var unsubscribe = [];

        var vm = this;
        vm.displays = [];
        vm.defaultButtonScope = null;
        vm.nodeId = "";
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

        function save() {
            if(vm.nodeId === editorState.current.id){
                $scope.$broadcast("seoContentSubmitting");
                notificationsService.success("SEO content saved!");
            }
        }

        function init() {
            if (!contentAppHelper.CONTENT_BASED_APPS.includes("seoContent")) {
                contentAppHelper.CONTENT_BASED_APPS.push("seoContent");
            }
            $http.get("backoffice/SeoToolkit/SeoContent/Get?contentId=" + editorState.getCurrent().id).then(
                function (response) {
                    if (response.status === 200) {
                        vm.displays = response.data.displays;
                        vm.displays[0].active = true;
                    }
                });

            vm.nodeId = editorState.current?.id ?? 0;
        }

        function subscribeToContentEvents() {
            unsubscribe.push(eventsService.on("content.saved", () => {
                if (vm.nodeId === 0) {
                    vm.nodeId = editorState.current.id;
                }

                save();
            }));
            unsubscribe.push(eventsService.on("content.unpublished", () => {
                save();
            }));
        }

        if (editorState.current.apps.find((item) => item.alias === 'seoContent' && item.active))
        {
            subscribeToContentEvents();
        }
        else
        {
            unsubscribe.push(eventsService.on("app.tabChange",
                (e, data) => {
                    if (data.alias !== "seoContent" || vm.nodeId !== editorState.current.id) {
                        return;
                    }

                    subscribeToContentEvents();
                }));
        }

        vm.$onDestroy = function () {
            unsubscribe.forEach(x => x());
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.ContentApps.SeoContentController", SeoContentController);
})();