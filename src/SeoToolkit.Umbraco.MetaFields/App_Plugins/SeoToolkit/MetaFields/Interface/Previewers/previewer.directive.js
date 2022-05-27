angular.module("umbraco.directives")
    .directive('seoFieldPreviewer', function () {
        return {
            restrict: 'E',
            template: '<ng-include src="getTemplateUrl()"/>',
            scope: {
                previewer: '<',
                fields: '<'
            },
            controller: function ($scope) {
                //function used on the ng-include to resolve the template
                $scope.getTemplateUrl = function () {
                    return $scope.previewer.view;
                }
            }
        }
    });