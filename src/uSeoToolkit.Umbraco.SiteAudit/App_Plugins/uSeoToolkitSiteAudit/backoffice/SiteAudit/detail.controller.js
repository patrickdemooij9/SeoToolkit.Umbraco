(function () {

    function siteAuditDetailController($timeout, $routeParams, $http, $location, siteAuditHub, localizationService, overlayService) {

        var vm = this;

        vm.isLoading = true;
        vm.errors = 0;
        vm.warnings = 0;
        vm.checkResults = {};
        vm.pages = [];
        vm.overviewPage = "allPages";
        vm.filterCheck = null;

        vm.pagination = {
            pageNumber: 1,
            totalPages: 1,
            itemsPerPage: 10
        };

        vm.openPage = openPage;
        vm.closePage = closePage;
        vm.toggleShowIssues = toggleShowIssues;
        vm.resultFilter = resultFilter;
        vm.getCheckById = getCheckById;
        vm.setOverviewPage = setOverviewPage;
        vm.filterResultsOnCheck = filterResultsOnCheck;
        vm.deleteAudit = deleteAudit;
        vm.stopAudit = stopAudit
        //vm.initChart = initChart;

        //Pagination
        vm.nextPage = nextPage;
        vm.prevPage = prevPage;
        vm.changePage = changePage;
        vm.goToPage = goToPage;

        //Dropdown
        vm.toggle = toggle;
        vm.close = close;
        vm.select = select;

        vm.dropdownOpen = false;
        vm.items = [
            { "name": "Delete", "function": deleteAudit, "icon": "icon-delete" },
            {
                "name": "Stop audit", "function": stopAudit, "icon": "icon-delete", "visibleFunc": function () {
                    return vm.audit.status === "Running";
                }
            }
        ];

        function init() {
            var auditId = $routeParams.id;
            vm.isLoading = true;

            siteAuditHub.initHub(function (hub) {
                vm.hub = hub;

                const clientId = getClientId();
                vm.hub.on("update",
                    function (update) {
                        console.log(update);
                        vm.audit = update;
                        loadPage(vm.pagination.pageNumber);
                        loadAudit(false);
                    });

                vm.hub.start();

                $timeout(function () {
                    loadCurrentAudit(auditId);
                }, 1000);
            });
        }

        /*function initChart() {
            var ctx = document.getElementById('myChart');
            var myChart = new Chart(ctx, {
                type: 'doughnut',
                responsive: true,
                data: {
                    datasets: [{
                        data: [35, 50],
                        backgroundColor: ["#3F9F3F", "#222"]
                    }]
                }
            });
        }*/

        function loadCurrentAudit(id) {
            $http.get("backoffice/uSeoToolkit/SiteAudit/Connect?auditId=" + id + "&clientId=" + getClientId()).then(function (response) {
                console.log(response);
                vm.audit = response.data;

                loadPage(vm.pagination.pageNumber);

                loadAudit(true);
                vm.isLoading = false;
            });
        }

        function loadAudit(hideShow) {
            vm.errors = 0;
            vm.warnings = 0;
            vm.checkResults = {};
            vm.audit.pagesCrawled.forEach(function (p) {
                p.errors = 0;
                p.warnings = 0;
                p.show = hideShow ? false : p.show;
                p.results.forEach(function (r) {
                    var addToResults = false;
                    if (r.isError) {
                        vm.errors++;
                        p.errors++;
                        addToResults = true;
                    }
                    if (r.isWarning) {
                        vm.warnings++;
                        p.warnings++;
                        addToResults = true;
                    }

                    if (addToResults) {
                        if (!vm.checkResults[r.checkId]) {
                            vm.checkResults[r.checkId] = { count: 0 };
                            if (r.isError) {
                                vm.checkResults[r.checkId].isError = true;
                            } else {
                                vm.checkResults[r.checkId].isWarning = true;
                            }
                        }
                        vm.checkResults[r.checkId].count++;
                    }
                });
            });
        }

        function deleteAudit() {
            const dialog = {
                view: "views/propertyeditors/listview/overlays/delete.html",
                submitButtonLabelKey: "contentTypeEditor_yesDelete",
                submitButtonStyle: "danger",
                submit: function (model) {
                    $http.post("backoffice/uSeoToolkit/SiteAudit/Delete",
                        {
                            ids: [vm.audit.id]
                        }).then(function (response) {
                            overlayService.close();
                            $location.path("uSeoToolkit/SiteAudit/list");
                        });
                },
                close: function () {
                    overlayService.close();
                }
            };

            localizationService.localize("general_delete").then(value => {
                dialog.title = value;
                overlayService.open(dialog);
            });
        }

        function stopAudit() {
            console.log("Trying to stop!");
        }

        function openPage(page) {
            page.show = true;
        };

        function closePage(page) {
            page.show = false;
        };

        function setOverviewPage(overviewPage) {
            vm.filterCheck = null;
            loadPage(1);
            vm.overviewPage = overviewPage;
        }

        function toggleShowIssues() {
            vm.onlyShowIssues = !vm.onlyShowIssues;
            loadPage(1);
        };

        function filterResultsOnCheck(checkId) {
            var check = getCheckById(checkId);
            vm.filterCheck = check;
            loadPage(1);
            vm.overviewPage = "filteredCheck";
        }

        function resultFilter(result) {
            if (!vm.filterCheck) {
                return true;
            }

            return result.checkId === vm.filterCheck.id;
        }

        function nextPage(pageNumber) {
            loadPage(pageNumber);
        }

        function prevPage(pageNumber) {
            loadPage(pageNumber);
        }

        function changePage(pageNumber) {
            loadPage(pageNumber);
        }

        function goToPage(pageNumber) {
            loadPage(pageNumber);
        }

        function loadPage(pageNumber) {
            var pageIndex = pageNumber - 1;
            var pages = vm.audit.pagesCrawled
                .filter(function (item) {
                    if (!vm.onlyShowIssues && !vm.filterCheck) {
                        return true;
                    }

                    var result = true;
                    if (vm.onlyShowIssues) {
                        result = item.errors > 0 || item.warnings > 0;
                    }
                    if (result && vm.filterCheck) {
                        result = item.results.some(function (item) {
                            return item.checkId === vm.filterCheck.id;
                        });
                    }
                    return result;
                });
            console.log(vm.audit);
            vm.pagination.pageNumber = pageNumber;
            vm.pagination.totalPages = pages.length / vm.pagination.itemsPerPage;
            vm.pages = pages.slice(pageIndex * vm.pagination.itemsPerPage, pageIndex * vm.pagination.itemsPerPage + vm.pagination.itemsPerPage);
        }

        function toggle() {
            vm.dropdownOpen = !vm.dropdownOpen;
        }

        function close() {
            vm.dropdownOpen = false;
        }

        function select(item) {
            close();
            item.function();
        }

        function getCheckById(id) {
            return vm.audit.checks.find(function (item) {
                return item.id == id;
            });
        }

        function getClientId() {
            if ($.connection !== undefined) {
                return $.connection.connectionId;
            }
            return "";
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.SiteAudit.DetailController", siteAuditDetailController);
})();