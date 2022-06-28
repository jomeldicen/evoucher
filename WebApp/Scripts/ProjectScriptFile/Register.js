app.controller("cRegistration", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token   
    var tokenKey = 'accessToken';
    var token = sessionStorage.getItem(tokenKey);

    $scope.data = {};
    $scope.clearData = function (token) {
        $scope.data = {};
        $scope.detailsData = {};
    };

    $scope.registrationData = function (data) {
        $scope.$broadcast('show-errors-event');

        if ($scope.registrationForm.$invalid) {
            swal(
                'Incomplete Form',
                'Please input required data.',
                'error'
              );
            return;
        }

        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';
        url =  '../api/Registration/Register';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            $scope.res = response;
            var obj = response.data;
            if (response.status == 200) {
                document.body.style.cursor = 'default';
                $scope.clearData();
                if (obj) {
                    swal(
                        'Successfully Registered',
                        '',
                        'success'
                      );
                }
                else {
                    swal(
                    'Successfully Registered',
                    'A confirmation link has been sent to your email address',
                    'success'
                  );
                }
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response.data.Message;
            if (obj == "duplicate_email") {
                swal(
                    'Error',
                    'Email ID is already Taken',
                    'error'
                  );
            }
            else if (obj == "User Role Not Defined") {
                swal(
                    'Error',
                    'User Role Not Defined',
                    'error'
                  );
            }
            else {
                console.log(response.data);
                swal(
                    'Registration Failed!',
                    'Please Insert Your Information Correctly',
                    'error'
                  );
            }
        });
    };

}]);
