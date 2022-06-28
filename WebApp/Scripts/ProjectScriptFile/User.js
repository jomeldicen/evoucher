app.controller("User", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = {};
    $scope.User = [];
    $scope.loading = false;
    $scope.disableButton = true;
    $scope.selectedAll = false;

    var token = sessionStorage.getItem(tokenKey);
    if (token === null) {
        window.location.href = "../Auth/Login";
    }

    $scope.init = function (PageUrl) {
        $scope.pagingInfo.PageUrl = PageUrl;
    }

    $scope.clearData = function () {
        $scope.data = {};
        $scope.data.nvFabIcon = "fa-bars";
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
        PageUrl: ''
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetUserBasicData();
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
        $scope.GetUserBasicData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetUserBasicData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetUserBasicData();
    }
    //-----------------------------------------------------------

    $scope.GetUserBasicData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        $scope.loading = true;

        urlData =  '../api/User';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.User = response.data.USER;
                $scope.Role = response.data.ROLE;
                $scope.Ranks = response.data.RANK;
                $scope.Departments = response.data.DEPARTMENT;
                $scope.Ctrl = response.data.CONTROLS;
                $scope.pagingInfo.totalItems = response.data.COUNT;
                $scope.disableButton = ($scope.pagingInfo.totalItems) ? false : true;
            }
            $timeout(function () {
                $scope.loading = false;
            }, 1000);
        }, function (response) {
            var obj = response.data.Message;
            swal('Loading Failed!', obj, 'error');
            document.body.style.cursor = 'default';
            $scope.disableButton = true;
            $scope.loading = false;
        });
    };

    $scope.GetSingleUser = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        urlData = '../api/User/GetSingleUser';
        $http({
            method: 'GET',
            url: urlData,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.User = response.data.USER[0];
                $scope.Role = response.data.ROLES;
                $scope.data = response.data.USER[0];
                convertImgToDataURLviaCanvas($scope.data.Photo, function (base64_data) {
                    $scope.data.Photo = base64_data;
                });
                $scope.LoginList = response.data.LOGINLIST;
                $scope.VisitedList = response.data.VISITEDLIST;
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Loading Failed!', obj, 'error');
            document.body.style.cursor = 'default';
        });
    };

    $scope.UpdateProfile = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.userUpdateForm.$invalid) {
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
        url = '../api/User/UpdateProfile';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            var obj = response.data.Message;
            if (response.status === 200) {
                document.body.style.cursor = 'default';
                $scope.GetSingleUser();
                swal(
                    'Successfully Updated',
                    'Your Information Updated Successfully',
                    'success'
                    );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response.data.Message;
            if (obj === "duplicate_email") {
                swal(
                    'Error',
                    'Email ID is already Taken',
                    'error'
                  );
            }
            else if (obj === "Change Profile Not Allowed") {
                swal(
                    'Error',
                    'Change Profile Not Allowed.',
                    'error'
                  );
            }
            else {
                swal(
                    'Update Failed!',
                    'Please Insert Your Information Correctly',
                    'error'
                  );
            }
        });
    };

    $scope.SaveUser = function (data) {
        $scope.$broadcast('show-errors-event');

        if ($scope.userForm.$invalid) {
            swal(
                'Incomplete Form',
                'Please input required data.',
                'error'
              );
            return;
        }

        data.EmailVerificationDisabled = true;// (data.EmailVerificationDisabled) ? true : false;
        data.BlockedAccount = "False";
        data.ResetPassword = "False";
        data.EmailConfirmed = true;
        data.MiddleName = "";
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        document.body.style.cursor = 'wait';
        url =  '../api/Registration/CreateUser';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            var obj = response.data;
            if (response.status === 200) {
                document.body.style.cursor = 'default';
                $scope.clearData();
                $scope.GetUserBasicData();
                $('#createUserModal').modal('hide');
                if (obj)
                {
                    swal(
                        'Successfully Registered',
                        '',
                        'success'
                      );
                }
                else {
                    swal(
                        'Successfully Registered',
                        'A confirmation link has been sent to user email address',
                        'success'
                      );
                }
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response.data;
            if (obj.Message === "duplicate_email") {
                swal(
                    'Error',
                    'Email ID is already Taken',
                    'error'
                  );
            } else if (obj.Message === "photo") {
                swal(
                    'Error',
                    'Please upload photo',
                    'error'
                  );
            }
            else {
                swal(
                    'Registration Failed!',
                    'Please Insert Your Information Correctly',
                    'error'
                  );
            }
        });
    };

    $scope.DeletePicture = function (data) {
        angular.element("input[type='file']").val(null);
        data.Photo = null;
    };

    var convertImgToDataURLviaCanvas = function (url, callback) {
        var img = new Image();
        img.crossOrigin = 'Anonymous';
        img.onload = function () {
            var canvas = document.createElement('CANVAS');
            var ctx = canvas.getContext('2d');
            var dataURL;
            canvas.height = this.height;
            canvas.width = this.width;
            ctx.drawImage(this, 0, 0);
            dataURL = canvas.toDataURL();
            callback(dataURL);
            canvas = null;
        };
        img.src = url;
    };

    $scope.EditUserData = function (dt) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        url = "/api/User/GetUserById";
        $http({
            method: 'GET',
            url: url,
            params: { 'ID': dt.Id },
            headers: headers
        }).then(function (response) {
            $scope.res = response;
            var obj = response.data;
            if (response.status === 200) {
                $scope.data = response.data.USER[0];
                convertImgToDataURLviaCanvas($scope.data.Photo, function (base64_data) {
                    $scope.data.Photo = base64_data;
                });
            }
        }, function (response) {
            console.log(response);
            if (response.data.Message === "Super Admin can't be edit") {
                $('#updateUserModal').modal('hide');
                s = "'";
                swal(
                    'Error',
                    'Super Admin can' + s + 't be edit.',
                    'error'
                    );
            }
        });
    };

    $scope.UpdateUser = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.userUpdateForm.$invalid) {
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
        url = '../api/User/UpdateUser';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            var obj = response.data;
            if (response.status === 200) {
                document.body.style.cursor = 'default';
                $scope.clearData();
                $scope.GetUserBasicData();
                $('#updateUserModal').modal('hide');
                swal(
                    'Successfully Updated',
                    'Your Information Updated Successfully',
                    'success'
                    );
            }
        }, function (response) {
            document.body.style.cursor = 'default';
            var obj = response.data.Message;
            if (obj === "duplicate_email") {
                swal(
                    'Error',
                    'Email ID is already Taken',
                    'error'
                  );
            }
            else if (obj === "Self") {
                swal(
                    'Error',
                    'You can not change your own Id',
                    'error'
                  );
            }
            else {
                swal(
                    'Update Failed!',
                    'Please Insert Your Information Correctly',
                    'error'
                  );
            }
        });
    };

    $scope.DeleteUser = function (data) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.DeleteUserConfirmed(data);
            }
        });
    };

    $scope.DeleteUserConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }
        url = "/api/User/RemoveData";
        $http({
            method: 'POST',
            url: url,
            params: {'ID' : data.Id},
            headers: headers,
        }).then(function (response) {
            $scope.res = response;
            if (response.status === 200) {
                swal(
                  'Successfully Deleted.',
                  'Information Delete Successfully',
                  'success'
                );
                $scope.clearData();
                $scope.GetUserBasicData();
            }
        }, function (response) {
            console.log(response);
            var obj = response.data.Message;
            if (obj === "Super Admin can't be delete") {
                s = "'";
                swal(
                    'Error',
                    'Super Admin can' + s + 't be delete.',
                    'error'
                  );
            }
            else if (obj === "Self") {
                swal(
                    'Error',
                    'You can not delete your own Id.',
                    'error'
                  );
            }
            else {
                swal(
                    'Error',
                    'You can not delete this data because user related data exist.',
                    'error'
                  );
            }
        });
    };

    $scope.EditPassData = function (data) {
        $scope.clearData();
        $scope.data = data;
    }

    $scope.ChangePassword = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.newForm.$invalid) {
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
        url = "../api/ChangePassword/ChangePassword"
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            $scope.res = response;
            var obj = response.data;
            if (response.status === 200) {
                swal(
                    'Successfully Saved',
                    'Password Changed Successfully',
                    'success'
                );
                $scope.clearData();
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

    $scope.ResetPassword = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.newForm.$invalid) {
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
        url = "../api/ChangePassword/ResetPassword"
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            $scope.res = response;
            var obj = response.data;
            if (response.status === 200) {
                $('#updateUserPass').modal('hide');
                swal(
                    'Successfully Saved',
                    'Password Changed Successfully',
                    'success'
                );
                $scope.clearData();
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };

}]);

//image upload and convert into byte
app.directive("fileread", [function () {
    return {
        scope: {
            fileread: "="
        },
        link: function (scope, element, attributes) {
            element.bind("change", function (changeEvent) {
                var reader = new FileReader();
                reader.onload = function (loadEvent) {
                    scope.$apply(function () {
                        scope.fileread = loadEvent.target.result;
                    });
                }
                reader.readAsDataURL(changeEvent.target.files[0]);
            });
        }
    }
}]);
//End