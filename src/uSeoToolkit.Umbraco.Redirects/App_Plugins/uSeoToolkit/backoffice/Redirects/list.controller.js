(function () {
    "use strict";

    function redirectListController($timeout, $http, listViewHelper, notificationsService, editorService) {
        var vm = this;

        vm.items = [];
        vm.selection = [];

        vm.options = {
            filter: '',
            orderBy: "from",
            orderDirection: "asc",
            bulkActionsAllowed: true,
            includeProperties: [
                { alias: "to", header: "To" },
                { alias: "domain", header: "Domain" },
                { alias: "statusCode", header: "Status Code"}
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
            edit(item.id);
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
            var redirectDialogOptions = {
                title: "Create redirect",
                view: "/App_Plugins/uSeoToolkit/Redirects/Dialogs/createRedirect.html",
                size: "small",
                submit: function (model) {
                    $http.post("backoffice/uSeoToolkit/Redirects/Create",
                        {
                            domain: model.domain,
                            customDomain: model.customDomain,
                            isRegex: model.linkType === 2,
                            oldUrl: model.oldUrl,
                            newUrl: model.newUrl,
                            newNodeId: model.newNodeId,
                            newCultureId: model.newCultureId,
                            redirectCode: parseInt(model.redirectCode)
                        }).then(function (response) {
                            notificationsService.success("Created new redirect!");

                            loadItems();
                            editorService.close();
                        }, function (response) {
                            notificationsService.error(response.data.ExceptionMessage);
                        });
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.open(redirectDialogOptions);
        }

        function edit(id) {
            create(); //TODO: Fix
        }

        function deleteSelection() {
            vm.loading = true;
            $http.post("backoffice/uSeoToolkit/Redirects/Delete",
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
                    "id": i.Id,
                    "icon": "icon-trafic",
                    "domain": i.Domain,
                    "name": i.OldUrl,
                    "to": i.NewUrl,
                    "statusCode": i.StatusCode,
                    "published": true
                }
            });
        }

        function init() {
            loadItems();
        }

        function loadItems() {
            vm.loading = true;
            $http.get("backoffice/uSeoToolkit/Redirects/GetAll").then(function (response) {
                setItems(response.data);
                vm.loading = false;

                //This is a very hacky way to rename the name column in the table
                $timeout(function () {
                        $(".redirects-table .umb-table__name localize")[0].innerText = "From";
                    },
                    0);
            });
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.Redirects.ListController", redirectListController);
})();