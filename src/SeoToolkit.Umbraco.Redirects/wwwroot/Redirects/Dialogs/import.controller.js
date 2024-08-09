(function () {
    "use strict";

    function importController($http, $scope, formHelper, localizationService, redirectsApiResource, notificationsService) {

        var vm = this;

        vm.submit = submit;
        vm.close = close;
        vm.handleFiles = handleFiles;
        vm.submit = submit;
        vm.import = importFile;
        vm.validated = false;
        vm.validationSuccess = "";
        vm.validationFailed = "";
        vm.importSuccess = "";
        vm.importFailed = "";
        vm.fileTypePropertyLabel = "Select the file type";
        vm.fileTypePropertyDescription = "Select the type of file you want to import the redirects from";
        vm.filePropertyLabel = "Select a file to import";
        vm.domainPropertyLabel = "Select the domain to import for";
        vm.domainPropertyDescription = "if nothing is selected, redirects will be imported for all sites";

        var labelKeys = [
            "redirect_fileTypePropertyLabel", "redirect_fileTypePropertyDescription",
            "redirect_filePropertyLabel",
            "redirect_validationSuccess", "redirect_validationFailed",
            "redirect_importSuccess", "redirect_importFailed",
            "redirect_domainPropertyLabel","redirect_domainPropertyDescription",
        ];
        localizationService.localizeMany(labelKeys).then(function (data) {
            vm.fileTypeSelection.label = data[0];
            vm.fileTypeSelection.description = data[1];
            vm.fileSelection.label = data[2];
            vm.validationSuccess = data[3];
            vm.validationFailed = data[4];
            vm.importSuccess = data[5];
            vm.importFailed = data[6];
            vm.domainPropertyLabel = data[7];
            vm.domainPropertyDescription = data[8];
        });

        vm.fileTypes = [
            {
                label:"CSV",
                extension:".csv"
            },
            {
                label:"Excel",
                extension:".xls,.xlsx"
            },
        ];

        vm.fileTypeSelection = {
            alias: "fileType",
            label: vm.fileTypePropertyLabel,
            description: vm.fileTypePropertyDescription,
            value: 0,
            validation: {
                mandatory: true
            }
        }

        vm.domainSelection = {
            alias: "domain",
            label: vm.domainPropertyLabel,
            description: vm.domainPropertyDescription,
            value: "0",
            validation: {
                mandatory: true
            }
        }

        vm.fileSelection = {
            alias: "file",
            label: vm.filePropertyLabel,
            value: 0,
            validation: {
                mandatory: true
            }
        }

        function handleFiles(files) {
            if (files && files.length > 0) {
                vm.fileSelection.value = files[0];
            }
        }

        function submit() {
            if (formHelper.submitForm({ scope: $scope, formCtrl: $scope.createRedirectForm })) {
                redirectsApiResource.validateRedirects(vm.fileTypeSelection.value.label, vm.fileSelection.value, vm.domainSelection.value).then(function (response) {
                    console.log(response);
                    if (response.StatusCode == 200) {
                        vm.notification = vm.validationSuccess;
                        vm.validated = true;
                    } else {
                        vm.notification = `${vm.validationFailed} ${response.Error}`;
                        vm.validated = false;
                    }
                });
            }
        }
        function importFile() {
            if (formHelper.submitForm({ scope: $scope, formCtrl: $scope.createRedirectForm })) {
                redirectsApiResource.importRedirects().then(function (response) {
                    if (response.StatusCode == 200) {
                        notificationsService.success(`${vm.fileSelection.value.name} ${vm.importSuccess}`);
                        vm.validated = true;
                        $scope.model.close();
                    } else {
                        vm.notification = `${vm.importFailed} ${response.Error}`;
                        vm.validated = false;
                    }
                });
            }
        }
        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        init();

        function init(){
            $http.get("backoffice/SeoToolkit/Redirects/GetDomains").then(function (response) {
                vm.domains = response.data.map(function (item) {
                    return { id: item.Id, name: item.Name }
                });
                vm.domains.splice(0, 0, { id: 0, name: "All Sites" });
            });
        }
    }

    angular.module("umbraco").controller("SeoToolkit.Redirects.ImportController", importController);
})();