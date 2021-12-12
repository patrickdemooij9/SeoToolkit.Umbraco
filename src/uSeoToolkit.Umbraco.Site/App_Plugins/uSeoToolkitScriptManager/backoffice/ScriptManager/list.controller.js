(function () {
    "use strict";

    function scriptManagerListController($scope, $http, $location, listViewHelper, notificationsService) {
        var vm = this;

        vm.items = [];
        vm.selection = [];

        vm.options = {
            filter: '',
            orderBy: "name",
            orderDirection: "asc",
            bulkActionsAllowed: true,
            includeProperties: [
                { alias: "definitionName", header: "Definition" }
            ]
        };

        vm.selectItem = selectItem;
        vm.clickItem = clickItem;
        vm.selectAll = selectAll;
        vm.isSelectedAll = isSelectedAll;
        vm.isSortDirection = isSortDirection;
        vm.sort = sort;
        vm.clearSelection = clearSelection;

        vm.create = create;
        vm.deleteSelection = deleteSelection;

        function selectAll($event) {
            listViewHelper.selectAllItemsToggle(vm.items, vm.selection);
        }

        function isSelectedAll() {
            return listViewHelper.isSelectedAll(vm.items, vm.selection);
        }

        function clickItem(item) {
            listViewHelper.editItem(item, vm);
        }

        function selectItem(selectedItem, $index, $event) {
            listViewHelper.selectHandler(selectedItem, $index, vm.items, vm.selection, $event);
        }

        function isSortDirection(col, direction) {
            return listViewHelper.setSortingDirection(col, direction, vm.options);
        }

        function sort(field, allow, isSystem) {
            if (allow) {
                vm.options.orderBySystemField = isSystem;
                listViewHelper.setSorting(field, allow, vm.options);
            }
        }

        function clearSelection() {
            listViewHelper.clearSelection(vm.items, null, vm.selection);
        }

        function create() {
            $location.path("uSeoToolkit/ScriptManager/edit").search("id", null);;
        }

        function deleteSelection() {
            vm.loading = true;
            $http.post("backoffice/uSeoToolkit/ScriptManager/Delete",
                {
                    ids: vm.selection.map(function (item) {
                        return item.id;
                    })
                }).then(function (response) {
                    setItems(response.data);
                    clearSelection();

                    if (response.status === 200) {
                        notificationsService.success("Selection deleted", "Your selection has successfully been deleted!");
                    } else {
                        notificationsService.error("Error", "Something went wrong while deleting your selection!");
                    }

                    vm.loading = false;
                });
        }

        function setItems(items) {
            vm.items = items.map(function (i) {
                return {
                    "id": i.id,
                    "icon": "icon-document",
                    "name": i.name,
                    "definitionName": i.definitionName,
                    "published": true,
                    "editPath": "uSeoToolkit/ScriptManager/edit?id=" + i.id
                }
            });
        }

        function init() {
            vm.loading = true;
            $http.get("backoffice/uSeoToolkit/ScriptManager/GetAllScripts").then(function (response) {
                setItems(response.data);
                vm.loading = false;
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.ScriptManager.ListController", scriptManagerListController);
})();