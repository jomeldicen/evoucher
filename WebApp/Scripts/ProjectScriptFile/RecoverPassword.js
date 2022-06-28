app.controller("cPasswordRecovery", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    $scope.data = {};
    $scope.ClearData = function () {
        $scope.data = {};
    };

    //Recover Password data start
    $scope.RecoverPassword = function (data) {

        $scope.$broadcast('show-errors-event');
        if ($scope.loginForm.$invalid) {
            swal(
                'Incomplete Form',
                'Please input required data.',
                'error'
              );
            return;
        }
        document.body.style.cursor = 'wait';
        
        url = '../api/Logout/ForgotPasswordSendMail';
        $http({
            method: 'POST',
            url: url,
            data: data,
        }).then(function (response) {
            $scope.res = response;
            var obj = response.data;
            if (response.status === 200) {
                document.body.style.cursor = 'default';
                swal(
                       'Recovery Email Sent Successfully',
                       'Please check your email.',
                       'success'
                     );
                $scope.clearData();
            }
        }, function (response) {
            var obj = response.data;
            if (obj === "invalid email") {
                document.body.style.cursor = 'default';
                swal(
                    'Error',
                    'Could not find your email id',
                    'error'
                  );
            }
            else if (obj === "Recover Password Not Allowed") {
                document.body.style.cursor = 'default';
                swal(
                    'Error',
                    'Recover Password Not Allowed',
                    'error'
                  );
            }
        });
    };
    //Recover Password data End

    //forgot password start
    $scope.ChangePassword = function (data) {

        $scope.$broadcast('show-errors-event-insert');

        if ($scope.loginForm.$invalid) {
            swal(
                'Incomplete Form',
                'Please input required data.',
                'error'
              );
            return;
        }
        document.body.style.cursor = 'wait';

        //catch userid & code for change password from url
        var userId = null, tmp = [], code = null;
        var items = window.location.search.substr(1).split("&");
        for (var index = 0; index < items.length; index++) {
            tmp = items[index];
            if (index === 0) {
                userId = decodeURIComponent(tmp);
                userId = userId.substring(7, userId.length);
            }
            if (index === 1) {
                code = decodeURIComponent(tmp);
                code = code.substring(5, code.length);
            }
        }
        //set user id and code data
        data.userId = userId;
        data.code = code;
        url = '../api/Logout/SetPassword'
        $http({
            method: 'POST',
            url: url,
            data: data,
        }).then(function (response) {
            $scope.res = response;
            var obj = response.data;
            if (response.status === 200) {
                document.body.style.cursor = 'default';

                swal({
                    title: "Password Updated!",
                    text: "Your password has been changed successfully",
                    icon: "success",
                    dangerMode: false,
                }).then((willUpdate) => {
                    if (willUpdate) {
                        window.location.href = window.location.origin;
                    }
                });    
            }
        }, function (response) {
            document.body.style.cursor = 'default';
        });
    }
    //forgot password End
}]);

jQuery(document).ready(function () {
    var getUrl = window.location;
    var baseUrl = getUrl.protocol + "//" + getUrl.host;// + "/" + getUrl.pathname.split('/')[1];

    /*Fullscreen background*/
    $.backstretch([
        baseUrl + "/Content/images/1.jpg",
        baseUrl + "/Content/images/2.jpg",
        baseUrl + "/Content/images/3.jpg"
    ], { duration: 3000, fade: 750 });
});