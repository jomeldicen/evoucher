app.controller("cRedeemed", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token   
    var tokenKey = 'accessToken';
    var token = sessionStorage.getItem(tokenKey);

    $scope.data = {};
    $scope.clearData = function (token) {
        $scope.data = {};
        $scope.detailsData = {};
    };

    $scope.messageicon = '/Content/images/success.png';
    $scope.messagecode = 200;
    $scope.messagedesc = '';
    $scope.isDone = false;

    $scope.GetMerchant = function (UniqueID) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        urlData = '../../api/Redeemed/GetMerchant';
        $http({
            method: 'GET',
            url: urlData,
            params: { 'ID': UniqueID },
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.data = response.data.VOUCHERINFO;
                $scope.Merchants = response.data.MerchantList;

                // if no merchant LOV maintain in the system
                if ($scope.Merchants.length === 0)
                    $scope.GetVoucherDataConfirmed(UniqueID, 0);
            }
        }, function (response) {
            $scope.messageicon = '/Content/images/error.png';
            $scope.messagecode = 404;
            $scope.messagedesc = response.data.Message;
            swal('Notification!', 'Your code could not be redeemed', 'warning');
            $scope.clearData();
        });
    };

    $scope.GetVoucherData = function (UniqueID, Merchant) {
        swal({
            title: "Voucher Confirmation",
            text: "You are set to claim this voucher under Merchant " + Merchant.Name + ". Do you want to proceed?",
            icon: "info",
            buttons: true,
            dangerMode: false,
        })
        .then((willUpdate) => {
            if (willUpdate) {
                $scope.GetVoucherDataConfirmed(UniqueID, Merchant.Id);
            }
        });
    }

    $scope.GetVoucherDataConfirmed = function (UniqueID, MerchantID) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.messageicon = '/Content/images/success.png';
        $scope.messagecode = 200;

        urlData = '../../api/Redeemed/GetVoucherData';
        $http({
            method: 'GET',
            url: urlData,
            params: { 'ID': UniqueID, 'MerchantID': MerchantID },
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.messageicon = '/Content/images/success.png';
                $scope.messagecode = 200;
                $scope.data = response.data.VoucherInfo;
                $scope.merchant = response.data.MerchantInfo;
                $scope.isDone = true;
                swal('Congratulations!', 'Thank you for validating your voucher', 'success');
            }
        }, function (response) {
            $scope.messageicon = '/Content/images/error.png';
            $scope.messagecode = 404;
            $scope.messagedesc = response.data.Message;
            swal('Notification!', 'Your code could not be redeemed', 'warning');
            $scope.isDone = false;
            $scope.clearData();
        });
    };

}]);


//jQuery(document).ready(function () {
//    var getUrl = window.location;
//    var baseUrl = getUrl.protocol + "//" + getUrl.host;// + "/" + getUrl.pathname.split('/')[1];

//    /*Fullscreen background*/
//    $.backstretch([
//        baseUrl + "/Content/images/bg.jpg"
//    ], { duration: 3000, fade: 750 });
//});