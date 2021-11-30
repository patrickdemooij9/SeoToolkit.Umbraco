(function () {
    "use strict";

    function SeoSettingsController($scope, $rootScope, $http, editorState) {

        var vm = this;
        vm.loading = true;
        vm.edit = false;

        vm.metaValues = {};

        vm.startEdit = startEdit;
        vm.finishEdit = finishEdit;
        vm.isUrl = isUrl;

        function init() {
            $http.get("backoffice/uSeoToolkit/SeoSettings/Get?nodeId=" + editorState.current.id + "&contentTypeId=" + editorState.current.contentTypeId).then(
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

            $http.post("backoffice/uSeoToolkit/SeoSettings/Save",
                {
                    nodeId: editorState.current.id,
                    contentTypeId: editorState.current.contentTypeId,
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
            });

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ContentApps.SeoSettingsController", SeoSettingsController);
})();