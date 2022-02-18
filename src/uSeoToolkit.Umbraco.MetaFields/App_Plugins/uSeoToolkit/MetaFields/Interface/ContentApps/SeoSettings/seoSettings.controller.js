(function () {
    "use strict";

    function SeoSettingsController($scope, $routeParams, $rootScope, $http, editorState) {

        var vm = this;
        vm.loading = true;
        vm.edit = false;

        vm.metaValues = {};

        vm.startEdit = startEdit;
        vm.finishEdit = finishEdit;
        vm.isUrl = isUrl;
        vm.culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;

        vm.isContentDirty = function () {
            var currentEditorItem = editorState.getCurrent();
            var isDirty = false;
            currentEditorItem.variants.forEach(function(variant) {
                if (variant.isDirty) {
                    isDirty = true;
                }
            });
            return isDirty;
        }

        function init() {
            var url = "backoffice/uSeoToolkit/MetaFields/Get?nodeId=" + editorState.current.id;
            if (vm.culture) {
                url += "&culture=" + vm.culture;
            }
            $http.get(url).then(
                function (response) {
                    vm.fields = response.data.fields;
                    vm.loading = false;
                });
        }

        function startEdit() {
            vm.fields.forEach(function (field) {
                field.editModel = {
                    view: field.editView,
                    value: field.userValue,
                    config: field.editConfig
                }
            });
            vm.edit = true;
        }

        function finishEdit() {
            $scope.$broadcast("formSubmitting");

            const userValues = Object.assign({},
                ...vm.fields.map(function (field) {
                    return ({ [field.alias]: field.editModel.value });
                }));

            $http.post("backoffice/uSeoToolkit/MetaFields/Save",
                {
                    nodeId: editorState.current.id,
                    contentTypeId: editorState.current.contentTypeId,
                    culture: vm.culture,
                    userValues: userValues
                }).then(function (response) {
                    vm.edit = false;
                    vm.fields = response.data.fields;
                });
        }

        function isUrl(value) {
            if (value && value.startsWith('http')) {
                return true;
            }
            return false;
        }

        $rootScope.$on("app.tabChange",
            (e, data) => {
                if (data.alias !== "seoSettings") {
                    return;
                }

                init();
            });
    }

    angular.module("umbraco").controller("uSeoToolkit.ContentApps.MetaFieldsController", SeoSettingsController);
})();