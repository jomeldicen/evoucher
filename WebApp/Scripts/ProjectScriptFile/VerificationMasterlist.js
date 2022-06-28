app.controller("VerificationMasterlist", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.VerificationMasterlist = [];
    $scope.loading = false;

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.clearData = function () {
        $scope.loading = false;
        $scope.selectedAll = false;
        document.body.style.cursor = 'default';
        $scope.data = {};
        $scope.data.nvFabIcon = "fa-bars";
    };

    $scope.setting1 = {
        scrollableHeight: '200px',
        scrollable: true
    };

    //-----------------------------------------------------------
    // Set parameter for search, filter and sorting
    $scope.pagingInfo = {
        page: 1,
        itemsPerPage: "10",
        sortBy: 'Id',
        reverse: true,
        search: '',
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        value1: '',
        DateFrom: new Date(),
        DateTo: new Date()
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetVerificationMasterlistData();
    };

    // Sorting of content
    $scope.sort = function (sortBy) {
        if (sortBy === $scope.pagingInfo.sortBy) {
            $scope.pagingInfo.reverse = !$scope.pagingInfo.reverse;
        } else {
            $scope.pagingInfo.sortBy = sortBy;
            $scope.pagingInfo.reverse = false;
        }
        $scope.pagingInfo.page = 1;
        $scope.GetVerificationMasterlistData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetVerificationMasterlistData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetVerificationMasterlistData();
    }
    //-----------------------------------------------------------

    $scope.GetVerificationMasterlistData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/VerificationMasterlist/GetVerificationMasterlist';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.VerificationMasterlist = response.data.RECIPIENTLIST;
                $scope.Template = response.data.TemplateLIST;
                $scope.Ctrl = response.data.CONTROLS;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                $scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;

            }
            $timeout(function () {
                $scope.clearData();
            }, 1000);
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    
    // Start Delete control
    $scope.ResendEmail = function (data) {
       swal({
            title: "Re-sending email",
            text: "Do you want to continue?",
            icon: "info",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.ResendEmailConfirmed(data);
            }
        });
    }

    $scope.ResendEmailConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        document.body.style.cursor = 'wait';

        urlData = '../api/VerificationMasterlist/ReSendEmail';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetVerificationMasterlistData();
                swal(
                    'System Message Confirmation',
                    'Email successfully sent',
                    'success'
                    );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            document.body.style.cursor = 'default';
            $scope.loading = false;
        });
    };

    // Report Export
    $scope.ExportReport = function (control) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        var reportParameter = [];

        reportParameter.push({
            param2: $scope.pagingInfo.value1, // type of column 
            param4: $scope.pagingInfo.search,  // string data if not date
        });

        var json = JSON.stringify(reportParameter);

        var path = "../Report/ExportReport";
        if (control === 'Download')
            path = "../Report/ExportReport";
        else if (control === 'Print')
            path = "../Reports/ReportViewer.aspx";

        window.open(path + '?rep=v5nf1F10jklxM30923fg12&contype=001x&json=' + json, '_blank');
    };
}]);