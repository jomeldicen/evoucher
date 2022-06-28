app.controller("Settings", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token 
    var tokenKey = 'accessToken';
    $scope.settings = {};
    $scope.loading = false;

    $scope.ClearData = function () {
        $scope.loading = false;
        document.body.style.cursor = 'default';
        $scope.settings = {};
    };

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href =  "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        //$scope.pagingInfo.PageUrl = PageUrl;
    }

    $scope.GetSettingsData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/Settings';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.ClearData();
                $scope.settings = response.data.SETTINGS[0];
                $scope.Role = response.data.ROLE;
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Loading Failed!', obj, 'error');
            $scope.ClearData();
        });
    };

    $scope.UpdateSettingsData = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.settingForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed',
                'error'
              );
            return;
        }

        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        url = "../api/Settings/UpdateSettings"
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                swal({
                    title: 'System Message Confirmation',
                    text: 'System settings successfully updated',
                    icon: 'success',
                    dangerMode: false,
                }).then((willUpdate) => {
                    if (willUpdate) {
                        $scope.clearData();
                        $scope.GetSettingsData();
                        window.location.reload();
                    }
                });               
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

}]);