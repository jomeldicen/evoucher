var app = angular.module('app', ['ngMaterial', 'ngMessages', 'material.svgAssetsCache', 'ngSanitize', 'ui.bootstrap', 'angularjs-dropdown-multiselect', 'summernote', 'ngFileUpload']);

app.directive('showErrors', function ($timeout) {

    return {
        restrict: 'A',
        require: '^form',
        link: function (scope, el, attrs, formCtrl) {

            // find the text box element, which has the 'name' attribute
            var inputEl = el[0].querySelector("[name]");

            // convert the native text box element to an angular element
            var inputNgEl = angular.element(inputEl);

            // get the name on the text box so we know the property to check
            // on the form controller
            var inputName = inputNgEl.attr('name');

            //var helpText = angular.element(el[0].querySelector(".help-block"));

            // only apply the has-error class after the user leaves the text box
            inputNgEl.bind('blur', function () {
                if (formCtrl[inputName]) el.toggleClass('has-error', formCtrl[inputName].$invalid);
                //helpText.toggleClass('hide', formCtrl[inputName].$valid);
            });

            inputNgEl.bind('blur', function () {
                if (formCtrl[inputName]) el.toggleClass('has-danger', formCtrl[inputName].$invalid);
                //helpText.toggleClass('hide', formCtrl[inputName].$valid);
            });

            inputNgEl.bind('blur', function () {
                if (formCtrl[inputName]) el.toggleClass('has-success', formCtrl[inputName].$valid);
                //helpText.toggleClass('hide', formCtrl[inputName].$valid);
            });

            scope.$on('show-errors-event', function () {
                if (formCtrl[inputName]) {
                    el.toggleClass('has-error', formCtrl[inputName].$invalid);
                    //el.children('input').toggleClass('is-invalid');
                }                
            });

            scope.$on('show-errors-event', function () {
                if (formCtrl[inputName]) el.toggleClass('has-danger', formCtrl[inputName].$invalid);
            });

            scope.$on('show-errors-event', function () {
                if (formCtrl[inputName]) el.toggleClass('has-success', formCtrl[inputName].$valid);
            });

            scope.$on('show-errors-reset', function () {
                $timeout(function () {
                    el.removeClass('has-error');
                    el.removeClass('has-danger');
                    //el.children('input').removeClass('is-invalid');
                }, 0, false);
            });
        }
    }
});

app.filter('roundup', function () {
    return function (value) {
        return Math.ceil(value);
    };
});


app.directive('date', function (dateFilter) {
    return {
        require: 'ngModel',
        link: function (scope, elm, attrs, ctrl) {

            var dateFormat = attrs['date'] || 'yyyy-MM-dd';

            ctrl.$formatters.unshift(function (modelValue) {
                return dateFilter(modelValue, dateFormat);
            });
        }
    };
})
