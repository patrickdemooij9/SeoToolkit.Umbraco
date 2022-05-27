(function () {
    "use strict";

    function SeoSettingsController($scope, $routeParams, $rootScope, $http, editorState, notificationsService, eventsService) {

        var unsubscribe = [];

        var vm = this;
        vm.loading = true;

        vm.groups = [];
        vm.allFields = [];
        vm.metaValues = {};

        vm.isUrl = isUrl;
        vm.culture = $routeParams.cculture ? $routeParams.cculture : $routeParams.mculture;

        vm.isContentDirty = function () {
            var currentEditorItem = editorState.getCurrent();
            var isDirty = false;
            currentEditorItem.variants.forEach(function (variant) {
                if (variant.isDirty) {
                    isDirty = true;
                }
            });
            return isDirty;
        }

        function init() {
            var url = "backoffice/SeoToolkit/MetaFields/Get?nodeId=" + editorState.current.id;
            if (vm.culture) {
                url += "&culture=" + vm.culture;
            }
            $http.get(url).then(
                function (response) {
                    handleResponse(response);

                    vm.loading = false;
                });
        }

        function handleResponse(response) {
            vm.groups = response.data.groups;

            vm.groups.forEach(group => {
                var fields = response.data.fields.filter(field => {
                    return field.groupAlias === group.alias;
                });

                setFields(group, fields);
                group.previewers = response.data.previewers.filter(function (previewer) {
                    return previewer.group === group.alias;
                });
            });
        }

        function setFields(group, newFields) {
            const isInit = group.fields == null;
            if (isInit) {
                group.fields = [];
            }
            newFields.forEach(function (field) {
                const existingField = group.fields.find(function (f) {
                    return f.alias === field.alias;
                });

                if (!existingField) {
                    field.editModel = {
                        view: field.editView,
                        value: field.userValue,
                        config: field.editConfig
                    };
                    field.getValue = function () {
                        return field.editModel.value ? field.editModel.value : field.value;
                    }
                    group.fields.push(field);
                }
            });

            vm.fields = vm.groups.flatMap(function (group) {
                return group.fields;
            });
        }

        function save() {
            $scope.$broadcast("formSubmitting");
            
            const userValues = Object.assign({},
                ...vm.fields.map(function (field) {
                    return ({ [field.alias]: field.editModel.value });
                }));

            vm.loading = true;
            $http.post("backoffice/SeoToolkit/MetaFields/Save",
                {
                    nodeId: editorState.current.id,
                    contentTypeId: editorState.current.contentTypeId,
                    culture: vm.culture,
                    userValues: userValues
                }).then(function (response) {
                    handleResponse(response);
                    vm.loading = false;
                });
        }

        function isUrl(value) {
            if (value && value.startsWith('http')) {
                return true;
            }
            return false;
        }

        unsubscribe.push($scope.$on("seoContentSubmitting",
            function () {
                save();
            }));

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.ContentApps.MetaFieldsController", SeoSettingsController);
})();