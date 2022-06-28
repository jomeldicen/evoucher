app.controller("cLogin", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token 
    var tokenKey = 'accessToken';
    var ulhId = 'ULHID';
    var home = 'index';
    var token = sessionStorage.getItem(tokenKey);
    if (token !== null) {
        window.location.href = ".." + sessionStorage.getItem(home);
    }

    document.body.style.cursor = 'default';
    var url = window.location.pathname;

    //login data 
    $scope.login = function (data) {

        $scope.$broadcast('show-errors-event');
        $scope.ro = true;

        if ($scope.loginForm.$invalid) {
            swal(
                'Incomplete Form',
                'Please input required data.',
                'error'
            );
            $scope.ro = false;
            return;
        }

        $scope.showErrorMessage = "";
        document.body.style.cursor = 'wait';
        var urllogin = "/token";

        $http({
            url: urllogin,
            method: "POST",
            data: "username=" + data.Email + "&password=" + data.Password +
                "&grant_type=password",
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
        }).then(function (response) {
            document.body.style.cursor = 'default';

            swal("Gotcha!", "Welcome to E-Voucher", "success", {
                buttons: {
                    catch: {
                        text: "Go",
                        value: "catch",
                    }
                },
                ClassName: "asdsad",
            })
                .then((value) => {
                    switch (value) {
                        case "catch":
                            // Cache the access token in session storage.
                            sessionStorage.setItem(tokenKey, response.data.access_token);
                            sessionStorage.setItem(ulhId, response.data.ULHID);
                            sessionStorage.setItem(home, response.data.index);
                            var obj = response.data.index;
                            window.location.href = window.location.origin + obj;
                            $scope.ro = false;
                            break;

                        default:
                            swal("Oops!", "Please hit the button", "warning");
                    }
                });

        }, function (response) {
            document.body.style.cursor = 'default';
            if (response.status === 500) {
                swal(
                    'Error!',
                    'Internal Server Error!',
                    'error'
                );
            } else {
                var obj = response.data;
                if (obj.error_description === "Account pending approval.") {
                    swal(
                        'Account pending approval',
                        'Your account pending approval.Please Verify Your Email Address',
                        'error'
                    );
                }
                else {
                    swal(
                        'Sorry!',
                        'The Email or password is incorrect.',
                        'error'
                    );
                }
            }
            $scope.ro = false;
        });
    };

}]);

jQuery(document).ready(function () {
    var getUrl = window.location;
    var baseUrl = getUrl.protocol + "//" + getUrl.host;// + "/" + getUrl.pathname.split('/')[1];

    /*Fullscreen background*/
    $.backstretch([
        baseUrl + "/Content/images/2.jpg",
        baseUrl + "/Content/images/3.jpg"
    ], { duration: 3000, fade: 750 });
});