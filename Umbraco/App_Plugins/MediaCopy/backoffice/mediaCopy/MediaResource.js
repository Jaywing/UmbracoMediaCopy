angular
    .module("umbraco.resources")
    .factory("mediaExtendedResource", function($q, $http) {
        return {
            copy: function(params) {
                var dfrd = $.Deferred();

                $http({
                    url: "backoffice/MediaCopy/MediaCopy/PostCopy",
                    method: "POST",
                    params: params
                })
                    .success(function(result) { dfrd.resolve(result); })
                    .error(function(result) { dfrd.resolve(result); });

                return dfrd.promise();
            }
        };
    });
