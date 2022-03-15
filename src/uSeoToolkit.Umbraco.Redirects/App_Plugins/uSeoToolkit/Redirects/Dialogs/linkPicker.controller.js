(function () {
    "use strict";

    function linkPickerController($scope, languageResource, eventsService, contentResource) {

        var vm = this;

        vm.model = $scope.model;
        vm.dialogTreeApi = {};
        vm.customTreeParams = "";

        vm.submit = submit;
        vm.close = close;
        vm.changeLanguage = changeLanguage;
        vm.changeLinkType = changeLinkType;
        vm.canSubmit = canSubmit;

        vm.allCultures = null;
        vm.nodeCultures = null;

        vm.searchInfo = {
            searchFromId: null,
            searchFromName: null,
            showSearch: false,
            results: [],
            selectedSearchResults: []
        };

        vm.linkTypeProperty = {
            alias: "linkType",
            label: "Link Type",
            description: "The type of your link",
            value: "1",
            view: "radiobuttons",
            validation: {
                mandatory: true
            },
            config: {
                items: [
                    { name: "Url", value: 1 },
                    { name: "Content", value: 2 },
                    { name: "Media", value: 3 }
                ]
            }
        };

        vm.urlProperty = {
            alias: "url",
            label: "Url",
            description: "Relative or absolute URl to redirect to",
            value: "",
            view: "textbox",
            validation: {
                mandatory: true
            }
        };

        vm.languageProperty = {
            alias: "language",
            label: "Language",
            value: ""
        }

        languageResource.getAll().then(function (data) {
            vm.languageProperty.value = data.find(function (item) { return item.isDefault }).id;
            vm.allCultures = data;
        });

        vm.onTreeInit = function () {
            vm.dialogTreeApi.callbacks.treeLoaded(treeLoadedHandler);
            vm.dialogTreeApi.callbacks.treeNodeSelect(nodeSelectHandler);
            vm.dialogTreeApi.callbacks.treeNodeExpanded(nodeExpandedHandler);
        }

        vm.selectListViewNode = function (node) {
            node.selected = node.selected === true ? false : true;
            nodeSelectHandler({
                node: node
            });
        }

        vm.hideSearch = function () {
            vm.searchInfo.showSearch = false;
            vm.searchInfo.searchFromId = null;
            vm.searchInfo.searchFromName = null;
            vm.searchInfo.results = [];
        }

        vm.onSearchResults = function (results) {
            vm.searchInfo.results = results;
            vm.searchInfo.showSearch = true;
        }

        vm.selectResult = function (evt, result) {
            result.selected = result.selected === true ? false : true;
            nodeSelectHandler({
                event: evt,
                node: result
            });
        }

        vm.closeMiniListView = function () {
            vm.miniListView = undefined;
        }

        var oneTimeTreeSync = {
            executed: false,
            treeReady: false,
            sync: function () {
                if (this.executed || !this.treeReady) {
                    return;
                }

                this.executed = true;

                vm.dialogTreeApi.syncTree({
                    path: "-1",
                    tree: "content"
                });
            }
        };

        function treeLoadedHandler() {
            oneTimeTreeSync.treeReady = true;
            oneTimeTreeSync.sync();
        }

        function nodeSelectHandler(args) {
            if (args && args.event) {
                args.event.preventDefault();
                args.event.stopPropagation();
            }

            eventsService.emit("dialogs.linkPicker.select", args);

            if (vm.model.selectedNode) {
                //un-select if there's a current one selected
                vm.model.selectedNode.selected = false;
            }
            vm.model.selectedNode = args.node;
            vm.model.selectedNode.selected = true;
        }

        function nodeExpandedHandler(args) {
            // open mini list view for list views
            if (args.node.metaData.isContainer) {
                openMiniListView(args.node);
            }

            if (Utilities.isArray(args.children) && vm.model.selectedNode != null) {
                args.children.forEach(child => {
                    if (vm.model.selectedNode.id === child.id) {
                        child.selected = true;

                        vm.model.selectedNode = child;
                        return;
                    }
                });
            }
        }

        function openMiniListView(node) {
            vm.miniListView = node;
        }

        function changeLanguage() {
            vm.customTreeParams =
                "culture=" +
                vm.allCultures.find(function (item) { return item.id === vm.languageProperty.value }).culture;

            vm.dialogTreeApi.load({
                section: vm.section,
                customTreeParams: vm.customTreeParams
            });
        }

        $scope.$watch("vm.linkTypeProperty.value",
            function (oldValue, newValue) {
                changeLinkType();
            });

        function changeLinkType() {
            if (vm.linkTypeProperty.value === '2') {
                changeSection("content");
            } else if (vm.linkTypeProperty.value === '3') {
                changeSection("media");
            }
        }

        function changeSection(section) {
            vm.section = section;
            changeLanguage();
        }

        function canSubmit() {
            if (vm.linkTypeProperty.value === '1') {//Url
                return vm.urlProperty.value !== '';
            }
            if (vm.model.selectedNode == null) {
                return false;
            }
            if (vm.linkTypeProperty.value === '2') {
                return vm.model.selectedNode.nodeType === "content";
            }
            return vm.model.selectedNode.nodeType === "media";
        }

        function close() {
            if (vm.model && vm.model.close) {
                vm.model.close();
            }
        }

        function submit() {
            if (vm.model && vm.model.submit) {
                vm.model.submit({
                    linkType: vm.linkTypeProperty.value,
                    culture: vm.languageProperty.value,
                    value: vm.linkTypeProperty.value === '1' ? vm.urlProperty.value : vm.model.selectedNode.id
                });
            }
        }
    }

    angular.module("umbraco").controller("uSeoToolkit.Redirects.LinkPicker", linkPickerController);
})();