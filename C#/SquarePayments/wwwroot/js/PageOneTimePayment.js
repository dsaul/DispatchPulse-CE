define(["require", "exports", "./Utility.js"], function (require, exports, Utility_js_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.PageOneTimePayment = void 0;
    class PageOneTimePayment {
        static Start() {
            //console.debug('Hello world! PageOneTimePayment');
            // If this page has been posted, the form elements won't be here.
            if (document.querySelector('#sq-card-number')) {
                // Create and initialize a payment form object
                PageOneTimePayment._PaymentForm = new window.SqPaymentForm({
                    //applicationId: "sandbox-sq0idb-G1jDR_HkAUpzgY2pXRZmnw",
                    applicationId: "sq0idp-50GC250n3qMxOCA3LaOfJg",
                    inputClass: 'sq-input',
                    autoBuild: false,
                    // Customize the CSS for SqPaymentForm iframe elements
                    inputStyles: [
                        {
                            fontSize: '16px',
                            lineHeight: '24px',
                            padding: '16px',
                            placeholderColor: '#a0a0a0',
                            backgroundColor: 'transparent',
                        }
                    ],
                    // Initialize the credit card placeholders
                    cardNumber: {
                        elementId: 'sq-card-number',
                        placeholder: 'Card Number'
                    },
                    cvv: {
                        elementId: 'sq-cvv',
                        placeholder: 'CVV'
                    },
                    expirationDate: {
                        elementId: 'sq-expiration-date',
                        placeholder: 'MM/YY'
                    },
                    postalCode: {
                        elementId: 'sq-postal-code',
                        placeholder: 'Postal'
                    },
                    // SqPaymentForm callback functions
                    callbacks: {
                        /*
                        * callback function: cardNonceResponseReceived
                        * Triggered when: SqPaymentForm completes a card nonce request
                        */
                        cardNonceResponseReceived: (errors, nonce, cardData) => {
                            if (errors) {
                                // Log errors from nonce generation to the browser developer console.
                                errors.forEach((error) => {
                                    //console.error(error);
                                    switch (error.type) {
                                        case "VALIDATION_ERROR":
                                            let e;
                                            switch (error.field) {
                                                case "cardNumber":
                                                    e = document.querySelector('.errorMessage.cc-n');
                                                    e.style.display = "block";
                                                    e.innerText = error.message;
                                                    break;
                                                case "cvv":
                                                    e = document.querySelector('.errorMessage.cc-cvv');
                                                    e.style.display = "block";
                                                    e.innerText = error.message;
                                                    break;
                                                case "expirationDate":
                                                    e = document.querySelector('.errorMessage.cc-exp');
                                                    e.style.display = "block";
                                                    e.innerText = error.message;
                                                    break;
                                                case "postalCode":
                                                    e = document.querySelector('.errorMessage.cc-postal');
                                                    e.style.display = "block";
                                                    e.innerText = error.message;
                                                    break;
                                                default:
                                                    console.error('Unhandled error:', error);
                                                    break;
                                            }
                                            break;
                                        default:
                                            console.error('Unhandled error:', error);
                                            break;
                                    }
                                });
                                return;
                            }
                            // Post the website form to actually 
                            const nonceInput = document.querySelector('input[name="Nonce"]');
                            nonceInput.value = nonce;
                            const nonceForm = document.querySelector('#NonceForm');
                            nonceForm.submit();
                        }
                    }
                });
                PageOneTimePayment._PaymentForm.build();
                const buttons = document.querySelectorAll('.button-credit-card');
                buttons.forEach((currentValue, currentIndex, listObj) => {
                    currentValue.addEventListener('click', (event) => {
                        PageOneTimePayment.ClickPay(event);
                    });
                }, this);
                const depositAmountInput = document.querySelector('input[name="FormDepositAmount"]');
                console.debug('depositAmountInput', depositAmountInput);
                depositAmountInput.addEventListener('keyup', (event) => {
                    PageOneTimePayment.ReValidateDepositAmount(event);
                });
            }
        }
        static ReValidateDepositAmount(event) {
            console.debug('ReValidateDepositAmount');
            document.querySelector('.errorMessage.cc-deposit-amount').style.display = "none";
            const input = document.querySelector('input[name="FormDepositAmount"]');
            const errorDiv = document.querySelector('.errorMessage.cc-deposit-amount');
            const cellSubtotal = document.querySelector('td#cellSubtotal');
            const cellFees = document.querySelector('td#cellFees');
            const cellTotal = document.querySelector('td#cellTotal');
            const summaryTable = document.querySelector('table#summaryTable');
            const payButton = document.querySelector('#sq-creditcard');
            cellSubtotal.innerText = '';
            cellFees.innerText = '';
            cellTotal.innerText = '';
            summaryTable.style.display = "none";
            payButton.innerText = "Pay";
            // Remove non numeric.
            input.value = input.value.replace(/[^\d.]/g, '');
            // make sure we don't have two decimals.
            if ((input.value.match(/\./g) || []).length > 1) {
                errorDiv.style.display = "block";
                errorDiv.innerText = "There are two decimal points in your deposit amount, please remove one.";
                return;
            }
            // if we have more digits then 2 after the decimal place, fail validation.
            if (-1 != input.value.indexOf('.')) {
                const words = input.value.split('.');
                if (words[1].length > 2) {
                    errorDiv.style.display = "block";
                    errorDiv.innerText = "There can't be more then two numbers past the decimal point.";
                    return;
                }
            }
            // Make sure the field is filled in.
            if (input.value == null || input.value.trim() == '') {
                errorDiv.style.display = "block";
                errorDiv.innerText = "You must enter a deposit amount.";
                return;
            }
            // Amount must be above $10
            const parsed = parseFloat(input.value);
            if (parsed < 10) {
                errorDiv.style.display = "block";
                errorDiv.innerText = "The minimum credit card payment is $10.";
                return;
            }
            cellSubtotal.innerText = `\$${Utility_js_1.default.FormatMoney(parsed)}`;
            const fixedFee = 0.30;
            const percentageFee = 2.9;
            let totalWithFees = (parsed + fixedFee) / (1 - (percentageFee / 100));
            totalWithFees = parseFloat(totalWithFees.toFixed(2)); // Restrict it to two decimal places
            cellFees.innerText = `\$${Utility_js_1.default.FormatMoney(totalWithFees - parsed)}`;
            cellTotal.innerText = `\$${Utility_js_1.default.FormatMoney(totalWithFees)}`;
            summaryTable.style.display = "table";
            payButton.innerText = `Pay \$${Utility_js_1.default.FormatMoney(totalWithFees)}`;
        }
        static ClickPay(event) {
            event.preventDefault();
            document.querySelector('.errorMessage.cc-deposit-amount').style.display = "none";
            document.querySelector('.errorMessage.cc-n').style.display = "none";
            document.querySelector('.errorMessage.cc-cvv').style.display = "none";
            document.querySelector('.errorMessage.cc-exp').style.display = "none";
            document.querySelector('.errorMessage.cc-postal').style.display = "none";
            // Validate deposit amount.
            var input = document.querySelector('input[name="FormDepositAmount"]');
            var errorDiv = document.querySelector('.errorMessage.cc-deposit-amount');
            // Remove non numeric.
            input.value = input.value.replace(/[^\d.]/g, '');
            // make sure we don't have two decimals.
            if ((input.value.match(/\./g) || []).length > 1) {
                errorDiv.style.display = "block";
                errorDiv.innerText = "There are two decimal points in your deposit amount, please remove one.";
                return;
            }
            // if we have more digits then 2 after the decimal place, fail validation.
            if (-1 != input.value.indexOf('.')) {
                const words = input.value.split('.');
                if (words[1].length > 2) {
                    errorDiv.style.display = "block";
                    errorDiv.innerText = "There can't be more then two numbers past the decimal point.";
                    return;
                }
            }
            // Make sure the field is filled in.
            if (input.value == null || input.value.trim() == '') {
                errorDiv.style.display = "block";
                errorDiv.innerText = "You must enter a deposit amount.";
                return;
            }
            // Amount must be above $10
            const parsed = parseFloat(input.value);
            if (parsed < 10) {
                errorDiv.style.display = "block";
                errorDiv.innerText = "The minimum credit card payment is $10.";
                return;
            }
            // Request a nonce from the SqPaymentForm object
            PageOneTimePayment._PaymentForm.requestCardNonce();
        }
    }
    exports.PageOneTimePayment = PageOneTimePayment;
    window.DEBUG_PageOneTimePayment = PageOneTimePayment;
    exports.default = {};
});
//# sourceMappingURL=PageOneTimePayment.js.map