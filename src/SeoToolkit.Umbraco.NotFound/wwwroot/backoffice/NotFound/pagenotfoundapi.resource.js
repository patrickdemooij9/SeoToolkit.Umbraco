angular.module('umbraco.resources').factory('pageNotFoundApiResource',
    function ($http, umbRequestHelper) {

        var baseUrl = "backoffice/SeoToolkit/PageNotFound/";

        return {
            setData: function (data) {
                return umbRequestHelper.resourcePromise(
                    $http.post(baseUrl + "SetKeyValue", data), "Failed to set value in database");
            },
            getData: function () {
                return umbRequestHelper.resourcePromise(
                    $http.get(baseUrl + "GetValue"), "Failed to get Id.");
            }
        };
    }
);