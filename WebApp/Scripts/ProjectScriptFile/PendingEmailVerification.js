app.requires.push('datatables');
app.controller("cEmailVerification", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token and user name    
    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};

    var token = sessionStorage.getItem(tokenKey);
    if (token == null) {
        window.location.href =  "../Auth/Login";
    }

    $scope.GetPEVBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        urlData = '../api/PendingEmailVerification';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status == 200) {
                $scope.PEVList = response.data.PEVLIST;
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            console.log(response);
        });
    };

}]);