app.controller("Dashboard", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token and user name    
    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.donutData = {};


    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
       //$scope.pagingInfo.PageUrl = PageUrl;
    }

    $scope.GetBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
      
        urlData =  '../api/Dashboard';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.TotalUser = response.data.TOTALUSER;
                $scope.ActiveUser = response.data.ACTIVEUSER;
                $scope.TotalLogin = response.data.TOTALLOGIN;
                $scope.TotalPageVisit = response.data.TOTALPAGEVISIT;
                $scope.RoleWiseUserChart(response.data.RWUDATA);
                $scope.TotalRegisterUserChart(response.data.TRUDATA);
                $scope.Month = response.data.TRUDATA[0];
                $scope.LoginHistoryChart(response.data.LHDATA);
                $scope.TopPageVisitedChart(response.data.TPVDATA)
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            console.log(response);
        });
    };

    $scope.RoleWiseUserChart = function (RWUData) {
        var donutChartCanvas = $('#roleWiseUser').get(0).getContext('2d');
        var donutData = {
            labels: RWUData[0],
            datasets: [
              {
                  data: RWUData[1],
                  backgroundColor: RWUData[2],
              }
            ]
        };
        var donutOptions = {
            maintainAspectRatio: false,
            responsive: true,
        };
        var donutChart = new Chart(donutChartCanvas, {
            type: 'doughnut',
            data: donutData,
            options: donutOptions
        });
    };

    $scope.TotalRegisterUserChart = function (TRUData) {
        var $barChart = $('#totalRegisterUser')
        var barData = {
            labels: TRUData[0],
            datasets: [
              {
                  backgroundColor: '#007bff',
                  data: TRUData[1],
                  label: "Register"
              }
            ]
        }
        var barOptions = {
            maintainAspectRatio: false,
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
        var barChart = new Chart($barChart, {
            type: 'bar',
            data: barData,
            options: barOptions
        })
    };

    $scope.LoginHistoryChart = function (LHData) {
        var $visitorsChart = $('#visitors-chart')
        var visitorsChart = new Chart($visitorsChart, {
            data: {
                labels: ['1st', '2nd', '3rd', '4th', '5th', '6th', '7th', '8th', '9th', '10th', '11th', '12th', '13th', '14th', '15th', '16th', '17th', '18th', '19th', '20th', '21st', '22nd', '23rd', '24th', '25th', '26th', '27th', '28th', '29th', '30th', '31st'],
                datasets: [
                    {
                        type: 'line',
                        data: LHData[0],
                        label: "Login",
                        borderColor: '#007bff',
                        pointBackgroundColor: '#007bff',
                        fill: false
                    },
                    {
                        type: 'line',
                        data: LHData[1],
                        label: "Login",
                        borderColor: '#1aff1a',
                        pointBackgroundColor: '#1aff1a',
                        fill: false
                    },
                    {
                        type: 'line',
                        data: LHData[2],
                        label: "Login",
                        borderColor: '#00ccff',
                        pointBackgroundColor: '#00ccff',
                        fill: false
                    },
                    {
                        type: 'line',
                        data: LHData[3],
                        label: "Login",
                        borderColor: '#da4c3e',
                        pointBackgroundColor: '#da4c3e',
                        fill: false
                    },
                    {
                        type: 'line',
                        data: LHData[4],
                        label: "Login",
                        borderColor: '#996633',
                        pointBackgroundColor: '#996633',
                        fill: false
                    },
                    {
                        type: 'line',
                        data: LHData[5],
                        label: "Login",
                        borderColor: '#ced4da',
                        pointBackgroundColor: '#ced4da',
                        fill: false
                    }
                ]
            },
            options: {
                maintainAspectRatio: false,
                responsive: true,
                legend: {
                    display: false
                },
                scales: {
                    yAxes: [{
                        gridLines: {
                            display: true,
                            color: '#efefef',
                            drawBorder: true
                        },
                        ticks: {
                            beginAtZero: true,
                        }
                    }],
                    xAxes: [{
                        display: true,
                        gridLines: {
                            display: false,
                        }
                    }]
                }
            }
        })
    };

    $scope.TopPageVisitedChart = function (TPVData) {
        var $barChart = $('#topPageVisit')
        var barData = {
            labels: TPVData[0],
            datasets: [
              {
                  backgroundColor: '#007bff',
                  data: TPVData[1],
                  label: "Visit"
              }
            ]
        }
        var barOptions = {
            maintainAspectRatio: false,
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        }
        var barChart = new Chart($barChart, {
            type: 'bar',
            data: barData,
            options: barOptions
        })
    };

}]);