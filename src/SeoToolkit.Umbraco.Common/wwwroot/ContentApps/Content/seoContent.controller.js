(function() {

    function SeoContentController($scope, $http, editorState, eventsService, notificationsService)
    {
        var unsubscribe = [];

        var vm = this;
        vm.displays = [];
        vm.defaultButtonScope = null;

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
            vm.defaultButtonScope.defaultButton.saveButtonState = "busy";
            $scope.$broadcast("seoContentSubmitting");

            notificationsService.success("SEO content saved!");
            vm.defaultButtonScope.defaultButton.saveButtonState = "success";
        }

        function init() {
            $http.get("backoffice/SeoToolkit/SeoContent/Get?contentId=" + editorState.getCurrent().id).then(
                function (response) {
                    if (response.status === 200) {
                        vm.displays = response.data.displays;
                        vm.displays[0].active = true;

                        const maxTries = 20;
                        var tries = 0;
                        var currentScope = $scope.$parent;
                        while (!currentScope.hasOwnProperty("defaultButton") && tries <= maxTries) {
                            currentScope = currentScope.$parent;
                            tries++;
                        }

                        if (maxTries > tries) {
                            vm.defaultButtonScope = currentScope;
                        }

                        if (vm.defaultButtonScope) {
                            vm.defaultButtonScope.defaultButton = {
                                letter: 'S',
                                labelKey: "buttons_save",
                                handler: save,
                                hotKey: "ctrl+s",
                                hotKeyWhenHidden: true,
                                alias: "save",
                                addEllipsis: "true"
                            }
                        }

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