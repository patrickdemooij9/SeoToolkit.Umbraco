(function () {

    function siteAuditDetailController($timeout, $routeParams, $http, siteAuditHub) {

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
            { "name": "Delete", "function": deleteAudit, "icon": "icon-delete" }
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

                $timeout(function() {
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
            vm.audit.PagesCrawled.forEach(function (p) {
                p.Errors = 0;
                p.Warnings = 0;
                p.Show = hideShow ? false : p.Show;
                p.Results.forEach(function (r) {
                    var addToResults = false;
                    if (r.IsError) {
                        vm.errors++;
                        p.Errors++;
                        addToResults = true;
                    }
                    if (r.IsWarning) {
                        vm.warnings++;
                        p.Warnings++;
                        addToResults = true;
                    }

                    if (addToResults) {
                        if (!vm.checkResults[r.CheckId]) {
                            vm.checkResults[r.CheckId] = { count: 0 };
                            if (r.IsError) {
                                vm.checkResults[r.CheckId].isError = true;
                            } else {
                                vm.checkResults[r.CheckId].isWarning = true;
                            }
                        }
                        vm.checkResults[r.CheckId].count++;
                    }
                });
            });
        }

        function deleteAudit() {
            console.log("Ouch");
        }

        function openPage(page) {
            page.Show = true;
        };

        function closePage(page) {
            page.Show = false;
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

            return result.CheckId === vm.filterCheck.Id;
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
            var pages = vm.audit.PagesCrawled
                .filter(function(item) {
                    if (!vm.onlyShowIssues && !vm.filterCheck) {
                        return true;
                    }

                    var result = true;
                    if (vm.onlyShowIssues) {
                        result = item.Errors > 0 || item.Warnings > 0;
                    }
                    if (result && vm.filterCheck) {
                        result = item.Results.some(function(item) {
                            return item.CheckId === vm.filterCheck.Id;
                        });
                    }
                    return result;
                });
            console.log(pages);
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
            return vm.audit.Checks.find(function (item) {
                return item.Id === id;
            });
        }

        function getClientId() {
            if ($.connection !== undefined && $.connection.hub !== undefined) {
                return $.connection.hub.id;
            }
            return "";
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.SiteAudit.DetailController", siteAuditDetailController);
})();