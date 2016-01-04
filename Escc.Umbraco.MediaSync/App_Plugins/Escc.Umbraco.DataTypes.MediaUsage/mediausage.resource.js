//adds the resource to umbraco.resources module:
angular.module('umbraco.resources').factory('mediaUsageResource',
    function ($q, $http) {
        return {
            getMediaUsage: function (id) {
                return $http({
                    url: "backoffice/Escc/MediaUsageApi/GetMediaUsage",
                    method: "GET",
                    params: { id: id }
                });
            }
        };
    }
);