angular.module("umbraco.directives")
    .directive('seoFieldEditor', function () {
        return {
            restrict: 'E',
            template: '<ng-include src="getTemplateUrl()"/>',
            scope: {
                field: '=',
                hasInheritance: '<'
            },
            controller: function ($scope) {
                //function used on the ng-include to resolve the template
                $scope.getTemplateUrl = function () {
                    return $scope.field.editor.view;
                }
            }
        }
    });