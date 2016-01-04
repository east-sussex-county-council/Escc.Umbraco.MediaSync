angular.module("umbraco").controller("Escc.MediaUsageController", function ($scope, editorState, mediaUsageResource, notificationsService) {

    // Check the pre-values for showing the path
    $scope.showPath = $scope.model.config.showPath && $scope.model.config.showPath !== '0' ? true : false;

    // Indicate that we are loading
    $scope.loading = true;

    // Get data about usage from the API
    mediaUsageResource.getMediaUsage(editorState.current.id)
        .then(function (response) {
            $scope.media = response.data;
            $scope.loading = false;
        });
});
