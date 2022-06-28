app.requires.push('datatables');
app.controller("cLoginHistory", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};

    var token = sessionStorage.getItem(tokenKey);
    if (token == null) {
        window.location.href =  "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        $scope.pagingInfo.PageUrl = PageUrl;
    }

    //-----------------------------------------------------------
    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'vULHID',
        reverse: false,
        search: '',
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        PageUrl: ''
    };

    $scope.GetLoginHistoryBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        urlData = '../api/Log/GetLoginHistory';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers
        }).then(function (response) {
            if (response.status == 200) {
                $scope.LoginList = response.data.LOGINLIST;
                $scope.TotalLogin = response.data.TOTALLOGIN;
                $scope.HighestLoginBy = response.data.HIGHESTLOGINBY;
                $scope.HighestLogin = response.data.HIGHESTLOGIN;
                $scope.TodayTotalLogin = response.data.TODAYTOTALLOGIN;
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            console.log(response);
        });
    };

}]);