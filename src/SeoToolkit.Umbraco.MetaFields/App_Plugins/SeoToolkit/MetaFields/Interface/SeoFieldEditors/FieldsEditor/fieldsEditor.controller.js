(function () {
    "use strict";

    function fieldsEditorController($scope, $http, notificationsService, editorState, editorService, eventsService) {

        var vm = this;

        //TODO: Check if inheritance works as expected
        vm.field = $scope.field;
        vm.hasInheritance = $scope.hasInheritance;
        vm.customSelectedFields = [];
        vm.customBaseFields = [];
        vm.loading = true;
        vm.sortableOptions = {
            axis: "y",
            containment: "parent",
            distance: 10,
            opacity: 0.7,
            tolerance: "pointer",
            scroll: true,
            zIndex: 6000
        };

        vm.removeField = removeField;
        vm.openFieldPicker = openFieldPicker;

        function init() {

            $http.get("backoffice/SeoToolkit/MetaFieldsSettings/GetAdditionalFields").then(function (response) {

                if (response.status !== 200) {
                    vm.loading = false;
                    notificationsService.error(
                        "Error",
                        "Something went wrong! Please try again later!");
                    return;
                }

                var selectedFields = [];
                var fields = getAllContentFields(vm.field.editor.config.dataTypes);
                response.data.forEach(function (field) {
                    if (!vm.hasInheritance && field.onlyShowIfInherited)
                        return;

                    fields.push({
                        name: field.name,
                        value: field.value,
                        onlyShowIfInherited: field.onlyShowIfInherited,
                        source: 2
                    });
                });
                fields.forEach(function (d) {
                    if (vm.field.value) {
                        const currentField = vm.field.value.find(function (v) {
                            return d.value === v.value;
                        });
                        if (currentField) {
                            selectedFields.push(d);
                        }
                    }
                    vm.customBaseFields.push(d);
                });

                if (vm.field.value) {
                    //Make sure ordering is correct
                    vm.field.value.forEach(function (v) {
                        var value = selectedFields.find(function (s) {
                            return s.value === v.value;
                        });
                        if (value) {
                            vm.customSelectedFields.push(value);
                        }
                    });
                }

                $scope.$on('SeoToolkit.SaveField', beforeSaveEventHandler);

                vm.loading = false;
            });
        }

        function openFieldPicker() {
            const editor = {
                title: "Field",
                view: "/App_Plugins/SeoToolkit/MetaFields/Interface/Components/ItemGroupPicker/itemGroupPicker.html",
                size: "small",
                availableItems: vm.customBaseFields.map(function (item) { return item; }),
                selection: vm.customSelectedFields.map(function (item) { return item; }),
                submit: model => {
                    vm.customSelectedFields = model.selection;
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            }

            editorService.open(editor);
        }

        function removeField(field) {
            const index = vm.customSelectedFields.indexOf(field);
            if (index >= 0) {
                vm.customSelectedFields.splice(index, 1);
            }
        }

        function getAllContentFields(fields) {
            return editorState.getCurrent().groups.flatMap(function (g) {
                return g.properties;
            }).filter(function (g) {
                return fields.includes(g.editor);
            }).map(function (g) {
                return { name: g.label, value: g.alias, source: 1 };
            });
        }

        function beforeSaveEventHandler($event, $args) {
            vm.field.value = vm.customSelectedFields.map(function (v) {
                return {
                    name: v.name,
                    value: v.value,
                    source: v.source
                };
            });
        }

        init();
    }

    angular.module("umbraco").controller("SeoToolkit.SeoFieldEditors.FieldsEditorController", fieldsEditorController);
})();