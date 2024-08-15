angular.module('umbraco.resources').factory('redirectsApiResource',
    function (umbRequestHelper, Upload) {
        // the factory object returned
        var baseUrl = "backoffice/SeoToolkit/Redirects/";

        return {
            validateRedirects: function (fileExtension, file, domainId) {
                return umbRequestHelper.resourcePromise(
                    Upload.upload({
                        url: baseUrl + 'Validate?fileExtension=' + fileExtension + '&domain=' + domainId,
                        file: file
                    }).then(function (response) {
                        return response;
                    }).catch(function (error) {
                        var errorMsg = error.data ? error.data : "Failed to validate redirects";
                        return Promise.reject(errorMsg);
                    }),
                );
            },
            importRedirects: function () {
                return umbRequestHelper.resourcePromise(
                    Upload.upload({
                        url: baseUrl + 'Import'
                    }).then(function (response) {
                        return response;
                    }).catch(function (error) {
                        var errorMsg = error.data ? error.data : "Failed to import redirects";
                        return Promise.reject(errorMsg);
                    }),
                );
            },
        };
    });
