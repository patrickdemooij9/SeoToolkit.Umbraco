(function() {

    function OpenGraphPreviewerController($scope, entityResource) {

        var vm = this;

        vm.currentTab = "facebook";
        vm.fields = $scope.fields;

        vm.title = '';
        vm.description = '';
        vm.image = '';

        vm.setCurrentTab = setCurrentTab;
        vm.getValue = getValue;
        vm.getDomain = getDomain;

        function getValue(fieldAlias) {
            var field = vm.fields.find(it => {
                return it.alias === fieldAlias;
            });
            if (!field) {
                return "";
            }
            return field.getValue();
        }

        function setCurrentTab(tab) {
            vm.currentTab = tab;
        }

        function getDomain() {
            return window.location.hostname;
        }

        function updateTitle(value) {
            vm.title = value;
        }

        function updateDescription(value) {
            vm.description = value;
        }

        function updateImage(value) {
            if (value && value.startsWith('umb://')) {
                entityResource.getById(value, "Media").then(function(media) {
                    vm.image = media.metaData.MediaPath;
                });
            } else {
                vm.image = value;
            }
        }

        function init() {
            $scope.$watch(function() {
                return getValue('openGraphTitle');
            }, updateTitle);
            $scope.$watch(function () {
                return getValue('openGraphDescription');
            }, updateDescription);
            $scope.$watch(function () {
                return getValue('openGraphImage');
            }, updateImage);


            updateTitle(getValue('openGraphTitle'));
            updateDescription(getValue('openGraphDescription'));
            updateImage(getValue('openGraphImage'));
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.Previewers.OpenGraph", OpenGraphPreviewerController);
})();