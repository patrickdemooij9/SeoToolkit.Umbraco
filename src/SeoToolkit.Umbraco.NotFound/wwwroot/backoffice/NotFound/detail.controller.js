angular.module("umbraco")
    .controller("SeoToolkit.NotFound.DetailController",
        function ($scope, editorService, $routeParams, pageNotFoundApiResource, entityResource, notificationsService) {

            var vm = this;
            var selectedItem = "";
            $scope.renderModel = [];

            pageNotFoundApiResource.getData().then(function (response) {
                if (response != "") {
                    setRenderModel(response);
                }
            }, function () {
                selectedItem = -1;
            });

            vm.remove = function removeSelectedContent() {
                selectedItem = "-1";
                $scope.renderModel = [];
            }

            vm.submit = function submit() {              
                vm.saveButtonState = "busy";
                saveSelectedContent(selectedItem);
            }

            vm.openContentPicker = function openContentPicker() {
                var contentPickerOptions = {
                    multiPicker: false,
                    startNodeId: $routeParams.id,
                    submit: function (model) {           
                        selectedItem = model.selection[0].id;
                        setRenderModel(selectedItem);
                        editorService.close();
                    },
                    close: function () {
                        editorService.close();
                    }
                };
                editorService.contentPicker(contentPickerOptions);
            }

            function setRenderModel(nodeId) {
                $scope.renderModel = [];
                if (nodeId < 1 || !nodeId) {
                    $scope.renderModel = [];
                } else {
                    entityResource.getById(nodeId, "Document").then(function (item) {
                        $scope.renderModel.push({
                            "name": item.name,
                            "id": item.id,
                            "udi": item.udi,
                            "icon": item.icon,
                            "path": item.path,
                            "url": item.url,
                            "key": item.key,
                            "trashed": item.trashed,
                            "published": (item.metaData && item.metaData.IsPublished === false && entityType === "Document") ? false : true
                        });
                        selectedItem = item.id;
                    }, function (error) {
                        $scope.renderModel = [];
                    });
                }
            }
            function saveSelectedContent(selectedId) {    
                pageNotFoundApiResource.setData(selectedId.toString()).then(function (response) {
                    setRenderModel(selectedId.toString())
                    notificationsService.success("Saved");
                    vm.saveButtonState = "success";
                }, function (error) {
                    notificationsService.failed("Error saving value", error);
                    vm.saveButtonState = "error";
                });
            }

        });