app.controller("global", ['$scope', '$http', '$rootScope', function ($scope, $http, $rootScope) {

    //catch access token 
    var tokenKey = 'accessToken';
    var ulhId = 'ULHID';
    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    document.body.style.cursor = 'default';
    var url = window.location.pathname;

    $scope.data = {};

    $scope.clearData = function () {
        $scope.data = {};
    };

    $scope.GetMenuBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        urlData = '../api/Menu/GetSideMenu';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                var Menu = response.data.MENU;
                if (Menu.length !== 0) {
                    var menuhtml = "";
                    for (var i = 0; i < Menu.length; i++) {
                        menuhtml += GetMenuHtml(Menu[i], false)[0];
                    }
                    $scope.MenuHtml = menuhtml;
                }
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            //console.log(response);
        });
    };

    var GetMenuHtml = function (menu, isActive) {
        var html = "";
        var htmlm = "";

        if (menu.nvPageUrl === url)
            isActive = true;

        for (var j = 0; j < menu.Child.length; j++) {
            var value = GetMenuHtml(menu.Child[j], isActive);
            htmlm += value[0];
            isActive = value[1];
        }
        if (menu.Child.length > 0) {
            var cls1 = (isActive) ? " menu-open" : "";
            var cls2 = (isActive) ? " active" : "";
            html = '<li class="nav-item has-treeview' + cls1 + '">'
                + '<a href="' + menu.nvPageUrl + '" class="nav-link' + cls2 + '">'
                + '<i class="nav-icon fas ' + menu.nvFabIcon + '"></i>'
                + '<p>'
                + menu.nvMenuName
                + '<i class="fas fa-angle-left right"></i>'
                + '</p>'
                + '</a>'
                + '<ul class="nav nav-treeview">'
                + htmlm
                + '</ul>'
                + '</li>';
        }
        else {
            var cls = (menu.nvPageUrl === url) ? " active" : "";
            html = '<li class="nav-item">'
                + '<a href="' + menu.nvPageUrl + '" class="nav-link' + cls + '">'
                + '<i class="nav-icon fas ' + menu.nvFabIcon + '"></i>'
                + '<p>'
                + menu.nvMenuName
                + '</p>'
                + '</a>'
                + '</li>';
        }

        return [html, isActive];
    };

    $scope.GetBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        urlData =  '../api/CurrentUser';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.GetName = "Hi, " + response.data.GETDATA[0].Name;
                $scope.GetUser = response.data.GETDATA[0].Name;
                $scope.GetRole = response.data.GETDATA[0].RoleName;
                $scope.Photo = response.data.GETDATA[0].Photo;
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            //console.log(response);
        });
    } ;

    //Logout
    $scope.logout = function () {
        var token = sessionStorage.getItem(tokenKey);
        var ulhid = sessionStorage.getItem(ulhId);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        urlLogoutData =  '../api/Logout/Logout';
        $http({
            method: 'POST',
            url: urlLogoutData,
            params: {'ID' : ulhid},
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                sessionStorage.removeItem(tokenKey);
                sessionStorage.removeItem(ulhId);
                sessionStorage.clear();           
                window.location.href = response.data;
            }
        }, function (response) {
            //console.log(response);
        });
    };

}]);
