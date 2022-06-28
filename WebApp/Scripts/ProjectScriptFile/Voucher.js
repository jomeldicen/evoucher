app.controller("Voucher", ['$scope', '$http', '$window', '$timeout', '$rootScope', function ($scope, $http, $window, $timeout, $rootScope) {

    var tokenKey = 'accessToken';
    document.body.style.cursor = 'default';
    $scope.data = { TemplateID: '', EventName: '', VoucherCode: '', Amount: 0, Recipient: '' };
    $scope.Voucher = [];
    $scope.countChecked = 0;
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
        $scope.loading = false;
        $scope.selectedAll = false;
        document.body.style.cursor = 'default';
        $scope.data = { TemplateID: '', EventName: '', VoucherCode: '', Amount: 0, Recipient: '' };
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
        sortBy: 'VoucherCode',
        reverse: false,
        search: '',
        multiplesearch: { Published: 'Yes' },
        totalItems: 0,
        PageUrl: '',
        DateFrom: new Date(),
        DateTo: new Date()
    };

    // Page search
    $scope.search = function () {
        $scope.pagingInfo.page = 1;
        $scope.GetVoucherData();
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
        $scope.GetVoucherData();
    };

    $scope.selectPage = function (page) {
        $scope.pagingInfo.page = page;
        $window.scrollTo(0, angular.element('.control-section').offsetTop);
        $scope.GetVoucherData();
    };

    $scope.setItemsPerPage = function (num) {
        $scope.pagingInfo.itemsPerPage = num;
        $scope.pagingInfo.page = 1; //reset to first page
        $scope.GetVoucherData();
    }
    //-----------------------------------------------------------

    $scope.GetVoucherData = function () {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/Voucher/GetVoucher';
        $http({
            method: 'GET',
            url: urlData,
            params: $scope.pagingInfo,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.Voucher = response.data.VOUCHERLIST;
                $scope.Template = response.data.TEMPLATELIST;
                $scope.Option = response.data.OPTIONS;
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

    // Start Checkbox control
    $scope.selectAll = function () {
        angular.forEach($scope.Voucher, function (item) {
            if (item.VoucherStatus !== 'Registered') {
                item.isChecked = !$scope.selectedAll;
                if (item.isChecked)
                    $scope.countChecked++;
                else
                    $scope.countChecked--;
            }
        });

    };

    $scope.checkSelected = function (item) {
        if (item)
            $scope.countChecked++;
        else
            $scope.countChecked--;
    }
    // End Checkbox control
    
    // Start Upload control
    $scope.SelectFile = function (file) {
        $scope.SelectedFile = file;
    };


    $scope.Import = function () {
        $scope.$broadcast('show-errors-event');
        if ($scope.voucherForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        }

        var regex = /^([a-zA-Z0-9\s_\\.\-:])+(.xls|.xlsx)$/;

        if (regex.test($scope.SelectedFile.name.toLowerCase())) {
            if (typeof (FileReader) != "undefined") {
                var reader = new FileReader();
                //For Browsers other than IE.
                if (reader.readAsBinaryString) {
                    reader.onload = function (e) {
                        $scope.ProcessExcel(e.target.result);
                    };
                    reader.readAsBinaryString($scope.SelectedFile);
                } else {
                    //For IE Browser.
                    reader.onload = function (e) {
                        var data = "";
                        var bytes = new Uint8Array(e.target.result);
                        for (var i = 0; i < bytes.byteLength; i++) {
                            data += String.fromCharCode(bytes[i]);
                        }
                        $scope.ProcessExcel(data);
                    };
                    reader.readAsArrayBuffer($scope.SelectedFile);
                }
            } else {
                $window.alert("This browser does not support HTML5.");
            }
        } else {
            $window.alert("Please upload a valid Excel file.");
        }
    };

    $scope.ProcessExcel = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        //Read the Excel File data.
        var workbook = XLSX.read(data, {
            type: 'binary'
        });

        //Fetch the name of First Sheet.
        var firstSheet = workbook.SheetNames[0];

        //Read all rows from First Sheet into an JSON array.
        var rows = XLSX.utils.sheet_to_row_object_array(workbook.Sheets[firstSheet]);
        rows = rows.map(function (x) {
            x.EventName = $scope.data.EventName;
            x.TemplateID = $scope.data.TemplateID;
            return x;
        });
        
        if (rows.length > 0) {
            $scope.ro = true;
            var $modal = $('.js-loading-bar'),
                $bar = $modal.find('.progress-bar');

            $bar.css({ width: "70%" });

            $scope.loading = true;
            document.body.style.cursor = 'wait';

            url = '../api/Voucher/UploadVoucher';
            $http({
                method: 'POST',
                url: url,
                data: rows,
                headers: headers,
            }).then(function (response) {
                $bar.css({ width: "95%" });

                if (response.status === 200) {
                    $bar.css({ width: "100%" });

                    setTimeout(function () {
                        document.body.style.cursor = 'default';
                        $('#uploadModal').modal('hide');
                        $scope.clearData();
                        $scope.GetVoucherData();
                        swal('Successfully updated', response.data, 'success');
                        $bar.css({ width: "0%" });
                        $scope.ro = false;
                    }, 1500);

                }
            }, function (response) {
                var obj = response.data.Message;
                swal('Error Message', obj, 'error');
                $scope.clearData();
                $('#uploadModal').modal('hide');
                $bar.css({ width: "0%" });
                $scope.ro = false;
            });
        }
    };
    // End Upload control

    // Start Create control
    $scope.AddData = function () {
        $scope.clearData();
        $scope.data = {
            Id: 0,
            Published: "True",
            'OptionIDs': []
        };
    };
    // End Create control

    // Start Activate and Deactivate control
    $scope.StatusData = function (data, a) {
        if ($scope.countChecked === 0) {
            swal(
                'Update Failed!',
                'Please first make a selection from the list.',
                'warning'
            );
        } else {
            swal({
                title: "Update Confirmation",
                text: "You are going to tag this record with discrepancy! Do you want to proceed?",
                icon: "warning",
                buttons: true,
                dangerMode: true,
            })
            .then((willUpdate) => {
                if (willUpdate) {
                    $scope.StatusDataConfirmed(data, a);
                }
            });
        }
    }

    $scope.StatusDataConfirmed = function (item, WithDiscrepancy) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        var data = {};
        data.dsList = [];
        data.WithDiscrepancy = WithDiscrepancy;
        data.Remarks = $scope.Remarks;
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked)
                data.dsList.push(item[i]);
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = '../api/Voucher/UpdateStatus';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetVoucherData();
                $('#discprenacyModal').modal('hide');
                swal(
                    'Successfully updated',
                    'Record was updated successfully',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End Activate and Deactivate control

    // Start individual Edit control
    $scope.EditVoucherData = function (data) {
        $scope.clearData();
        data.TemplateID = data.TemplateID.toString();
        $scope.data = data;
    };

    $scope.SaveVoucher = function (data) {
        $scope.$broadcast('show-errors-event');
        if ($scope.VoucherForm.$invalid) {
            swal(
                'Error Message',
                'Please fill-out or tag mandatory fields to be able to proceed.',
                'warning'
            );
            return;
        } else {
            $scope.SaveVoucherConfirmed(data);
        }
    }

    $scope.SaveVoucherConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        urlData = '../api/Voucher/SaveVoucher';
        $http({
            method: 'POST',
            url: urlData,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetVoucherData();
                $('#createVoucherModal').modal('hide');

                swal(
                    'System Message Confirmation',
                    'Record successfully saved',
                    'success'
                    );
            }
        }, function (response) {
            var obj = response.data.Message;
            if (obj === "Exists")
                swal('Error Message', 'Record already exist', 'error');
            else
                swal('Error Message', obj, 'error');

            document.body.style.cursor = 'default';
            $scope.loading = false;             
        });
    };
    // End individual Edit control

    // Start Delete control
    $scope.DeleteData = function (data) {
        if ($scope.countChecked === 0) {
            swal(
                'Delete Failed!',
                'Please first make a selection from the list.',
                'warning'
            );
        } else {
            swal({
                title: "Are you sure?",
                text: "Once deleted, you will not be able to recover this!",
                icon: "warning",
                buttons: true,
                dangerMode: true,
            })
            .then((willDelete) => {
                if (willDelete) {
                    $scope.DeleteDataConfirmed(data);
                }
            });
        }
    }

    $scope.DeleteDataConfirmed = function (item) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        var data = {};
        data.dsList = [];
        for (var i = 0; i < item.length; i++) {
            if (item[i].isChecked)
                data.dsList.push(item[i]);
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = '../api/Voucher/RemoveRecords';
        $http({
            method: 'POST',
            url: url,
            data: data,
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetVoucherData();
                swal(
                    'System Message Confirmation',
                    'Record successfully deleted',
                    'success'
                );
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End Delete control

    // Start individual Delete control
    $scope.DeleteVoucher = function (data) {
        swal({
            title: "Are you sure?",
            text: "Once deleted, you will not be able to recover this!",
            icon: "warning",
            buttons: true,
            dangerMode: true,
        })
        .then((willDelete) => {
            if (willDelete) {
                $scope.DeleteVoucherConfirmed(data);
            }
        });
    }

    $scope.DeleteVoucherConfirmed = function (data) {
        var token = sessionStorage.getItem(tokenKey);
        var headers = {};
        if (token) {
            headers.Authorization = 'Bearer ' + token;
        }

        $scope.loading = true;
        document.body.style.cursor = 'wait';

        url = "/api/Voucher/RemoveData";
        $http({
            method: 'POST',
            url: url,
            params: {'ID' : data.Id},
            headers: headers,
        }).then(function (response) {
            if (response.status === 200) {
                $scope.clearData();
                $scope.GetVoucherData();

                swal(
                    'System Message Confirmation',
                    'Record successfully deleted',
                    'success'
                )
            }
        }, function (response) {
            var obj = response.data.Message;
            swal('Error Message', obj, 'error');
            $scope.clearData();
        });
    };
    // End individual Delete control

    $scope.CopyToClipboard = function () {
        /* Get the text field */
        var copyText = document.getElementById("myInput");

        /* Select the text field */
        copyText.select();
        copyText.setSelectionRange(0, 99999); /* For mobile devices */

        /* Copy the text inside the text field */
        document.execCommand("copy");

        /* Alert the copied text */
        alert("Copied the text: " + copyText.value);
    }

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

        window.open(path + '?rep=t2xf1F10jklxM30923llkj&contype=001x&json=' + json, '_blank');
    };
}]);

app.directive('convertToNumber', function () {
    return {
        require: 'ngModel',
        link: function (scope, element, attrs, ngModel) {
            ngModel.$parsers.push(function (val) {
                return val != null ? parseInt(val, 10) : null;
            });
            ngModel.$formatters.push(function (val) {
                return val != null ? '' + val : null;
            });
        }
    };
});