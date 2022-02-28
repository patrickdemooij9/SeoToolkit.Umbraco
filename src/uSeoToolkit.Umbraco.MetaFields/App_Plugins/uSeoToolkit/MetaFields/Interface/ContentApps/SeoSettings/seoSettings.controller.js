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
                    if (vm.edit) {
                        continueEdit(response.data.fields);
                    } else {
                        vm.fields = response.data.fields;
                    }
                    vm.loading = false;
                });
        }

        function startEdit() {
            vm.fields.forEach(function (field) {
                var value = vm.edit ? field.editModel.userValue : field.userValue;
                field.editModel = {
                    view: field.editView,
                    value: value,
                    config: field.editConfig
                }
            });
            vm.edit = true;
        }

        function continueEdit(newFields) {
            var oldFields = vm.fields;
            vm.fields = newFields;
            vm.fields.forEach(function(field) {
                const oldField = oldFields.find(function(f) {
                    return f.alias === field.alias;
                });
                if (!oldField) {
                    return;
                }

                field.editModel = oldField.editModel;
            });
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
                if (data.alias !== "metaFieldsSeoSettings") {
                    return;
                }

                init();
            });
    }

    angular.module("umbraco").controller("uSeoToolkit.ContentApps.MetaFieldsController", SeoSettingsController);
})();