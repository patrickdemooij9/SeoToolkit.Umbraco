angular.module('umbraco.resources').factory('redirectsApiResource',
    function (umbRequestHelper, Upload) {
        // the factory object returned
        var baseUrl = "backoffice/SeoToolkit/Redirects/";

        return {
            validateRedirects: function (fileExtension, file, domainId) {
                return umbRequestHelper.resourcePromise(
                    Upload.upload({
                        url: baseUrl + 'ValidateRedirects?fileExtension=' + fileExtension + '&domain=' + domainId,
                        file: file
                    }),
                    "Failed to import redirects"
                );
            },
            importRedirects: function (fileExtension) {
                return umbRequestHelper.resourcePromise(
                    Upload.upload({
                        url: baseUrl + 'ImportRedirects'
                    }),
                    "Failed to import redirects"
                );
            },
        };
    });
