﻿(function () {
    "use strict";

    function SeoSettingsController($scope, $routeParams, $rootScope, $http, editorState, notificationsService, eventsService) {

        var vm = this;
        vm.loading = true;
        vm.edit = false;

        vm.groups = [];
        vm.metaValues = {};
        vm.defaultButtonScope = null;

        vm.startEdit = startEdit;
        vm.finishEdit = finishEdit;
        vm.isUrl = isUrl;
        vm.getFieldsByGroup = getFieldsByGroup;
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
            var url = "backoffice/SeoToolkit/MetaFields/Get?nodeId=" + editorState.current.id;
            if (vm.culture) {
                url += "&culture=" + vm.culture;
            }
            $http.get(url).then(
                function (response) {
                    vm.groups = response.data.groups;
                    setFields(response.data.fields);
                    vm.loading = false;
                });

            var maxTries = 20;
            var tries = 0;
            var currentScope = $scope.$parent;
            while (!currentScope.hasOwnProperty("defaultButton") && tries <= maxTries) {
                currentScope = currentScope.$parent;
                tries++;
            }

            if (maxTries > tries) {
                vm.defaultButtonScope = currentScope;
            }
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

            if (vm.defaultButtonScope) {
                vm.defaultButtonScope.defaultButton = {
                    letter: 'S',
                    labelKey: "metaFields_finish",
                    handler: finishEdit,
                    hotKey: "ctrl+s",
                    hotKeyWhenHidden: true,
                    alias: "save",
                    addEllipsis: "true"
                }
            }

            vm.edit = true;
        }

        function setFields(newFields) {
            const isInit = vm.fields == null;
            if (isInit) {
                vm.fields = [];
            }
            newFields.forEach(function(field) {
                const existingField = vm.fields.find(function (f) {
                    return f.alias === field.alias;
                });

                if (existingField) {
                    //Update existing field with newest values
                    existingField.title = field.title;
                    existingField.description = field.description;
                    existingField.editConfig = field.editConfig;
                    existingField.editView = field.editView;

                    //TODO: If values are not changed by user, then use default values
                } else {
                    field.editModel = {
                        view: field.editView,
                        value: field.userValue,
                        systemValue: field.value,
                        config: field.editConfig
                    };
                    vm.fields.push(field);
                }
            });
        }

        function finishEdit() {
            $scope.$broadcast("formSubmitting");

            const userValues = Object.assign({},
                ...vm.fields.map(function (field) {
                    return ({ [field.alias]: field.editModel.value });
                }));

            $http.post("backoffice/SeoToolkit/MetaFields/Save",
                {
                    nodeId: editorState.current.id,
                    contentTypeId: editorState.current.contentTypeId,
                    culture: vm.culture,
                    userValues: userValues
                }).then(function (response) {
                    vm.edit = false;
                    vm.fields = response.data.fields;

                    if (vm.defaultButtonScope) {
                        vm.defaultButtonScope.defaultButton = null;
                    }

                    notificationsService.success("SEO content saved!");
                });
        }

        function getFieldsByGroup(group) {
            return vm.fields.filter(function(field) {
                return field.groupAlias === group.alias;
            });
        }

        function isUrl(value) {
            if (value && value.startsWith('http')) {
                return true;
            }
            return false;
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.ContentApps.MetaFieldsController", SeoSettingsController);
})();