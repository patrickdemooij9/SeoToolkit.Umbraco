﻿(function () {
    "use strict";

    function createRedirectController($scope, $http, editorService, contentResource, mediaResource, languageResource) {

        var vm = this;

        vm.submit = submit;
        vm.close = close;
        vm.openLinkPicker = openLinkPicker;

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
        }

        function openLinkPicker() {
            var linkPickerDialogOptions = {
                title: "Set link",
                view: "/App_Plugins/uSeoToolkit/Redirects/Dialogs/linkPicker.html",
                size: "small",
                submit: function (model) {
                    vm.newUrlData = model;
                    if (model.linkType === '1') {
                        vm.newUrlProperty.value = model.value;
                    } else if (model.linkType === '2') {
                        contentResource.getById(model.value).then(function (content) {
                            languageResource.getById(model.culture).then(function (language) {
                                const url = content.urls.find(function(url) {
                                    return url.culture === language.culture;
                                });
                                if (url) {
                                    vm.newUrlProperty.value = url.text;
                                } else {
                                    vm.newUrlProperty.value = "No URL found!";
                                }
                            });
                        });
                    } else if (model.linkType === '3') {
                        mediaResource.getById(model.value).then(function (media) {
                            if (media.mediaLink !== '') {
                                vm.newUrlProperty.value = media.mediaLink;
                            } else {
                                vm.newUrlProperty.value = "No URL found!";
                            }
                        });
                    }
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

        init();
    }

    angular.module("umbraco").controller("uSeoToolkit.Redirects.CreateController", createRedirectController);
})();