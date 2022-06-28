app.requires.push('datatables');
app.controller("cPageVisited", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};

    var token = sessionStorage.getItem(tokenKey);
    if (token == null) {
        window.location.href =  "../Auth/Login";
    }

    $scope.GetPageVisitedBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        urlData = '../api/Log/GetPageVisited';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers
        }).then(function (response) {
            if (response.status == 200) {
                $scope.VisitedList = response.data.VISITEDLIST;
                $scope.TotalVisit = response.data.TOTALVISIT;
                $scope.HighestVisitedBy = response.data.HIGHESTVISITEDBY;
                $scope.HighestVisitedPage = response.data.HIGHESTVISITEDPAGE;
                $scope.TodayTotalVisit = response.data.TODAYTOTALVISIT;
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            console.log(response);
        });
    };

}]);