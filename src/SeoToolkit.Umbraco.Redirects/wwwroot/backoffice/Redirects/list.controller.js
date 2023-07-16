(function () {
    "use strict";

    function redirectListController($timeout, $http, listViewHelper, notificationsService, editorService) {
        var vm = this;

        vm.items = [];
        vm.selection = [];
        vm.searchFilter = "";

        vm.options = {
            filter: '',
            orderBy: "Name",
            orderDirection: "asc",
            bulkActionsAllowed: true,
            includeProperties: [
                { alias: "to", header: "To", allowSorting: true },
                { alias: "domain", header: "Domain" },
                { alias: "statusCode", header: "Status Code", allowSorting: true },
                { alias: "lastUpdated", header: "Last Updated", allowSorting: true}
            ]
        };

        vm.selectItem = selectItem;
        vm.clickItem = clickItem;
        vm.selectAll = selectAll;
        vm.isSelectedAll = isSelectedAll;
        vm.isSortDirection = isSortDirection;
        vm.sort = sort;
        vm.clearSelection = clearSelection;

        vm.nextPage = nextPage;
        vm.prevPage = prevPage;
        vm.changePage = changePage;
        vm.goToPage = goToPage;

        vm.search = search;

        vm.create = openRedirectDialog;
        vm.deleteSelection = deleteSelection;

        vm.pageNumber = 1;
        vm.pageSize = 20;
        vm.totalPages = 1;

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

                loadItems();
            }
        }

        function clearSelection() {
            listViewHelper.clearSelection(vm.items, null, vm.selection);
        }

        function nextPage(pageNumber) {
            goToPage(pageNumber);
        }

        function prevPage(pageNumber) {
            goToPage(pageNumber);
        }

        function changePage(pageNumber) {
            goToPage(pageNumber);
        }

        function goToPage(pageNumber) {
            vm.selection = [];
            vm.pageNumber = pageNumber;
            loadItems();
        }

        function search() {
            goToPage(1);
        }

        function openRedirectDialog(model) {
            var redirectDialogOptions = {
                title: "Create redirect",
                view: "/App_Plugins/SeoToolkit/Redirects/Dialogs/createRedirect.html",
                size: "small",
                submit: function (model) {
                    $http.post("backoffice/SeoToolkit/Redirects/Save",
                        {
                            id: model.id,
                            domain: model.domain,
                            customDomain: model.customDomain,
                            isRegex: model.urlType === 2,
                            oldUrl: model.oldUrl,
                            newUrl: model.newUrl,
                            newNodeId: model.newNodeId,
                            newCultureId: model.newCultureId,
                            redirectCode: parseInt(model.redirectCode)
                        }).then(function (response) {
                            if (model.id === 0) {
                                notificationsService.success("Your redirect has been created!");
                            } else {
                                notificationsService.success("Your redirect has been updated!");
                            }

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
            if (model) {
                redirectDialogOptions.redirect = model;
            }

            editorService.open(redirectDialogOptions);
        }

        function edit(id) {
            $http.get("backoffice/SeoToolkit/Redirects/Get?id=" + id).then(function (response) {
                const redirect = response.data;

                openRedirectDialog({
                    id: redirect.Id,
                    domain: redirect.Domain,
                    customDomain: redirect.CustomDomain ?? "",
                    oldUrl: redirect.OldUrl,
                    isRegex: redirect.IsRegex,
                    newUrl: redirect.NewUrl,
                    newNodeId: redirect.NewNodeId,
                    newCultureId: redirect.NewCultureId,
                    statusCode: redirect.RedirectCode
                });
            });
        }

        function deleteSelection() {
            vm.loading = true;
            $http.post("backoffice/SeoToolkit/Redirects/Delete",
                {
                    ids: vm.selection.map(function (item) {
                        return item.id;
                    })
                }).then(function (response) {
                    setItems(response.data.items);
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
                    "statusCode": i.StatusCode === 301 ? 'Permanent (301)'
                                : i.StatusCode === 302 ? 'Temporary (302)'
                            : undefined,
                    "lastUpdated": i.LastUpdated,
                    "published": true
                }
            });
        }

        function init() {
            loadItems();
        }

        function loadItems() {
            vm.loading = true;
            $http.get("backoffice/SeoToolkit/Redirects/GetAll?pageNumber=" + vm.pageNumber + "&pageSize=" + vm.pageSize + "&orderBy=" + vm.options.orderBy + "&orderDirection=" + vm.options.orderDirection + "&search=" + vm.searchFilter).then(function (response) {

                vm.totalPages = response.data.totalPages;
                setItems(response.data.items);
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

    angular.module("umbraco").controller("SeoToolkit.Redirects.ListController", redirectListController);
})();