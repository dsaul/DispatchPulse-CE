import Utility from './Utility.js';

export class PageOneTimePayment {

	public static _PaymentForm;

	public static Start(): void {
		//console.debug('Hello world! PageOneTimePayment');

		// If this page has been posted, the form elements won't be here.
		if (document.querySelector('#sq-card-number')) {
			// Create and initialize a payment form object
			PageOneTimePayment._PaymentForm = new (<any>window).SqPaymentForm({
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

										let e: HTMLDivElement;

										switch (error.field) {
											case "cardNumber":
												e = document.querySelector<HTMLDivElement>('.errorMessage.cc-n');
												e.style.display = "block";
												e.innerText = error.message;
												break;
											case "cvv":
												e = document.querySelector<HTMLDivElement>('.errorMessage.cc-cvv');
												e.style.display = "block";
												e.innerText = error.message;
												break;
											case "expirationDate":
												e = document.querySelector<HTMLDivElement>('.errorMessage.cc-exp');
												e.style.display = "block";
												e.innerText = error.message;
												break;
											case "postalCode":
												e = document.querySelector<HTMLDivElement>('.errorMessage.cc-postal');
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
						const nonceInput = document.querySelector<HTMLInputElement>('input[name="Nonce"]');
						nonceInput.value = nonce;

						const nonceForm = document.querySelector<HTMLFormElement>('#NonceForm');
						nonceForm.submit();

					}
				}
			});

			PageOneTimePayment._PaymentForm.build();


			const buttons = document.querySelectorAll('.button-credit-card');
			buttons.forEach
				((currentValue, currentIndex, listObj) => {
					currentValue.addEventListener('click', (event) => {
						PageOneTimePayment.ClickPay(event);
					});
				},
					this
			);

			const depositAmountInput = document.querySelector<HTMLInputElement>('input[name="FormDepositAmount"]');
			console.debug('depositAmountInput', depositAmountInput);
			depositAmountInput.addEventListener('keyup', (event) => {
				PageOneTimePayment.ReValidateDepositAmount(event);
			});
		}
		


	}

	public static ReValidateDepositAmount(event): void {

		console.debug('ReValidateDepositAmount');

		document.querySelector<HTMLDivElement>('.errorMessage.cc-deposit-amount').style.display = "none";

		const input = document.querySelector<HTMLInputElement>('input[name="FormDepositAmount"]');
		const errorDiv = document.querySelector<HTMLDivElement>('.errorMessage.cc-deposit-amount');

		const cellSubtotal = document.querySelector<HTMLTableCellElement>('td#cellSubtotal');
		const cellFees = document.querySelector<HTMLTableCellElement>('td#cellFees');
		const cellTotal = document.querySelector<HTMLTableCellElement>('td#cellTotal');
		const summaryTable = document.querySelector<HTMLTableCellElement>('table#summaryTable');

		const payButton = document.querySelector<HTMLButtonElement>('#sq-creditcard');
			
		
		cellSubtotal.innerText = '';
		cellFees.innerText = '';
		cellTotal.innerText = '';
		summaryTable.style.display = "none"
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


		cellSubtotal.innerText = `\$${Utility.FormatMoney(parsed)}`;

		const fixedFee = 0.30;
		const percentageFee = 2.9;



		let totalWithFees = (parsed + fixedFee) / (1 - (percentageFee / 100));
		totalWithFees = parseFloat(totalWithFees.toFixed(2)); // Restrict it to two decimal places


		cellFees.innerText = `\$${Utility.FormatMoney(totalWithFees - parsed)}`;
		cellTotal.innerText = `\$${Utility.FormatMoney(totalWithFees)}`;
		summaryTable.style.display = "table"

		payButton.innerText = `Pay \$${Utility.FormatMoney(totalWithFees)}`;
	}

	public static ClickPay(event): void {
		event.preventDefault();

		document.querySelector<HTMLDivElement>('.errorMessage.cc-deposit-amount').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-n').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-cvv').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-exp').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-postal').style.display = "none";

		// Validate deposit amount.

		var input = document.querySelector<HTMLInputElement>('input[name="FormDepositAmount"]');
		var errorDiv = document.querySelector<HTMLDivElement>('.errorMessage.cc-deposit-amount');

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

(window as any).DEBUG_PageOneTimePayment = PageOneTimePayment;

export default {};
