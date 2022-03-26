(function () {
    "use strict";

    function createRedirectController($scope, $http, editorService, contentResource, mediaResource, languageResource) {

        var vm = this;

        vm.submit = submit;
        vm.close = close;
        vm.openLinkPicker = openLinkPicker;

        vm.id = 0;
        vm.domains = [];
        vm.urlTypes = [{ name: "Url", value: 1 }, { name: "Regex", value: 2 }];
        vm.oldUrlType = 1;
        vm.newUrlData = null;

        vm.domainProperty = {
            alias: "domain",
            label: "Domain",
            description: "Choose the domain set within Umbraco or use a custom domain",
            value: 0,
            validation: {
                mandatory: true
            }
        }

        vm.customDomainProperty = {
            alias: "customDomain",
            label: "Custom Domain",
            description: "Choose a custom domain if it isn't present in Umbraco",
            value: "",
            view: "textbox",
            validation: {
                mandatory: vm.domainProperty.value === -1
            }
        }

        vm.oldUrlProperty = {
            alias: "oldUrl",
            label: "From Url",
            description: "Relative url of where the redirect starts",
            value: "",
            view: "textbox",
            validation: {
                mandatory: true
            }
        }

        vm.newUrlProperty = {
            alias: "newUrl",
            label: "New Url",
            description: "The url where the redirect is going to",
            value: "",
            validation: {
                mandatory: true
            }
        };

        vm.statusCodeProperty = {
            alias: "statusCode",
            label: "Status Code",
            description: "Status code of the redirect",
            value: "302",
            view: "radiobuttons",
            validation: {
                mandatory: true
            },
            config: {
                items: [{ name: "Permanent (301)", value: 301 },{name: "Temporary (302)", value: 302}]
            }
        };

        function init() {
            $http.get("backoffice/uSeoToolkit/Redirects/GetDomains").then(function (response) {
                vm.domains = response.data.map(function (item) {
                    return { id: item.Id, name: item.Name }
                });
                vm.domains.splice(0, 0, { id: 0, name: "All Sites" });
                vm.domains.push({ id: -1, name: "Custom Domain" });
            });

            if ($scope.model.redirect) {
                vm.id = $scope.model.redirect.id;
                if ($scope.model.redirect.domain) {
                    vm.domainProperty.value = $scope.model.redirect.domain;
                } else {
                    vm.domainProperty.value = $scope.model.redirect.customDomain ? -1 : 0;
                }
                vm.customDomainProperty.value = $scope.model.redirect.customDomain;
                vm.oldUrlProperty.value = $scope.model.redirect.oldUrl;
                vm.oldUrlType = $scope.model.redirect.isRegex ? 2 : 1;
                vm.statusCodeProperty.value = $scope.model.redirect.statusCode.toString();

                var urlData = {
                    linkType: '1',
                    value: $scope.model.redirect.newUrl,
                    culture: null
                };
                if ($scope.model.redirect.newNodeId) {
                    urlData.value = $scope.model.redirect.newNodeId;
                    if ($scope.model.redirect.newCultureId) {
                        urlData.linkType = '2';
                        urlData.culture = $scope.model.redirect.newCultureId;
                    } else {
                        urlData.linkType = '3';
                    }
                }
                handleUrlData(urlData);
            }
        }

        function openLinkPicker() {
            var linkPickerDialogOptions = {
                title: "Set link",
                view: "/App_Plugins/uSeoToolkit/Redirects/Dialogs/linkPicker.html",
                size: "small",
                submit: function (model) {
                    handleUrlData(model);
                    editorService.close();
                },
                close: function () {
                    editorService.close();
                }
            };
            editorService.open(linkPickerDialogOptions);
        }

        function submit() {
            if ($scope.model.submit) {
                $scope.model.submit({
                    id: vm.id,
                    domain: vm.domainProperty.value > 0 ? vm.domainProperty.value : null,
                    customDomain: vm.domainProperty.value === -1 ? vm.customDomainProperty.value : null,
                    urlType: vm.oldUrlType,
                    oldUrl: vm.oldUrlProperty.value,
                    newUrl: vm.newUrlData.linkType === '1' ? vm.newUrlData.value : null,
                    newNodeId: vm.newUrlData.linkType !== '1' ? vm.newUrlData.value : null,
                    newCultureId: vm.newUrlData.linkType === '2' ? vm.newUrlData.culture : null,
                    redirectCode: vm.statusCodeProperty.value
                });
            }
        }

        function close() {
            if ($scope.model.close) {
                $scope.model.close();
            }
        }

        function handleUrlData(urlData) {
            vm.newUrlData = urlData;
            if (urlData.linkType === '1') {
                vm.newUrlProperty.value = urlData.value;
            } else if (urlData.linkType === '2') {
                contentResource.getById(urlData.value).then(function (content) {
                    languageResource.getById(urlData.culture).then(function (language) {
                        const url = content.urls.find(function (url) {
                            return url.culture === language.culture;
                        });
                        if (url) {
                            vm.newUrlProperty.value = url.text;
                        } else {
                            vm.newUrlProperty.value = "No URL found!";
                        }
                    });
                });
            } else if (urlData.linkType === '3') {
                mediaResource.getById(urlData.value).then(function (media) {
                    if (media.mediaLink !== '') {
                        vm.newUrlProperty.value = media.mediaLink;
                    } else {
                        vm.newUrlProperty.value = "No URL found!";
                    }
                });
            }
        }

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.Redirects.CreateController", createRedirectController);
})();