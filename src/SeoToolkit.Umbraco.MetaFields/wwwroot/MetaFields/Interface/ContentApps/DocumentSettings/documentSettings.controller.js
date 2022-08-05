﻿(function () {
    "use strict";

    function ContentAppDocumentSettings($scope, $http, notificationsService, editorService, editorState) {

        var unsubscribe = [];

        var vm = this;

        vm.loading = true;

        vm.customBaseFields = {};
        vm.customSelectedFields = {};

        vm.formatFieldValue = formatFieldValue;

        vm.openContentTypeDialog = function () {
            var editor = {
                multiPicker: false,
                filterCssClass: "not-allowed not-published",
                filter: function (item) {
                    return item.nodeType === "container" ||
                        (vm.model.inheritance != null && vm.model.inheritance.id === item.id) ||
                        editorState.getCurrent().id === item.id ||
                        !$scope.model.compositeContentTypes.includes(item.alias);
                },
                submit: function (model) {
                    if (model.selection.length > 0) {
                        const item = model.selection[0];
                        vm.model.inheritance = {
                            id: item.id,
                            name: item.name
                        };
                        vm.model.fields.forEach(function (propItem) {
                            propItem.useInheritedValue = true;
                        });
                    } else {
                        vm.removeInheritance();
                    }

                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            }

            editorService.contentTypePicker(editor);
        }

        vm.toggleUseInheritedValue = function (item, value) {
            item.useInheritedValue = value;
        }

        vm.removeInheritance = function () {
            vm.model.fields.forEach(function (item) {
                item.useInheritedValue = false;
            });
            vm.model.inheritance = null;
        }

        unsubscribe.push($scope.$on("seoSettingsSubmitting",
            function () {
                $scope.$broadcast("SeoToolkit.SaveField");

                save();
            }));

        function init() {
            $http.get("backoffice/SeoToolkit/MetaFieldsSettings/Get?nodeId=" + editorState.getCurrent().id).then(function (response) {
                vm.model = response.data.contentModel;
                vm.model.nodeId = editorState.getCurrent().id;

                vm.loading = false;
            });
        }

        function save() {
            var postModel = {
                nodeId: vm.model.nodeId,
                fields: Object.assign({},
                    ...vm.model.fields.map(function (v) {
                        return ({
                            [v.alias]: {
                                useInheritedValue: v.useInheritedValue,
                                value: v.value
                            }
                        });
                    })),
                inheritanceId: vm.model.inheritance != null ? vm.model.inheritance.id : null
            };

            $http.post("backoffice/SeoToolkit/MetaFieldsSettings/Save", postModel).then(function (response) {
                if (response.status !== 200) {
                    notificationsService.error("Something went wrong while saving the document type settings");
                }
            });
        }

        function formatFieldValue(field) {
            if (Array.isArray(field.value)) {
                var values = [];
                field.value.forEach(function (v) {
                    if (v.name) {
                        values.push(v.name);
                    } else {
                        values.push(v);
                    }
                });
                return values.join(', ');
            }
            return field.value;
        }

        vm.$onDestroy = function () {
            unsubscribe.forEach(x => x());
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.ContentApps.DocumentSettingController", ContentAppDocumentSettings);
})();