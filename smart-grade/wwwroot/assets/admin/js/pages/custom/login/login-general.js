/******/
(() => { // webpackBootstrap
    /******/
    "use strict";
    const __webpack_exports__ = {};
    /*!***********************************************************!*\
      !*** ../demo1/src/js/pages/custom/login/login-general.js ***!
      \***********************************************************/

    // Class Definition
    const KTLogin = function () {
        let _login;

        const _handleSignInForm = function () {
            let validation;

            // Init form validation rules. For more info check the FormValidation plugin's official documentation:https://formvalidation.io/
            validation = FormValidation.formValidation(
                KTUtil.getById('kt_login_signin_form'),
                {
                    fields: {
                        username: {
                            validators: {
                                notEmpty: {
                                    message: 'Username is required'
                                }
                            }
                        },
                        password: {
                            validators: {
                                notEmpty: {
                                    message: 'Password is required'
                                }
                            }
                        }
                    },
                    plugins: {
                        trigger: new FormValidation.plugins.Trigger(),
                        submitButton: new FormValidation.plugins.SubmitButton(),
                        //defaultSubmit: new FormValidation.plugins.DefaultSubmit(), // Uncomment this line to enable normal button submit after form validation
                        bootstrap: new FormValidation.plugins.Bootstrap()
                    }
                }
            );

            $('#kt_login_signin_submit').on('click', function (e) {
                e.preventDefault();

                validation.validate().then(function (status) {
                    if (status === 'Valid') {
                        $('#kt_login_signin_form').submit();
                    }
                });
            });
        };

        // Public Functions
        return {
            // public functions
            init: function () {
                _login = $('#kt_login');
                _handleSignInForm();
            }
        };
    }();

// Class Initialization
    jQuery(document).ready(function () {
        KTLogin.init();
    });

    /******/
})()
;
//# sourceMappingURL=login-general.js.map