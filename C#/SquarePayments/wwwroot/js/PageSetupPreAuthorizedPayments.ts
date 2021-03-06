export class PageSetupPreAuthorizedPayments {
	public static _PaymentForm;

	public static Start(): void {
		//console.debug('Hello world! PageOneTimePayment');

		// If this page has been posted, the form elements won't be here.
		if (document.querySelector('#sq-card-number')) {
			// Create and initialize a payment form object
			PageSetupPreAuthorizedPayments._PaymentForm = new (<any>window).SqPaymentForm({
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

			PageSetupPreAuthorizedPayments._PaymentForm.build();


			const buttons = document.querySelectorAll('.button-credit-card');
			buttons.forEach
				((currentValue, currentIndex, listObj) => {
					currentValue.addEventListener('click', (event) => {
						PageSetupPreAuthorizedPayments.ClickPay(event);
					});
				},
				this
			);

			
		}



	}

	public static ClickPay(event): void {
		event.preventDefault();

		document.querySelector<HTMLDivElement>('.errorMessage.cc-authorization-form').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-n').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-cvv').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-exp').style.display = "none";
		document.querySelector<HTMLDivElement>('.errorMessage.cc-postal').style.display = "none";

		const input = document.querySelector<HTMLInputElement>('input[name="authorizationFormFile"]');
		if (input && 0 == input.files.length) {
			const e = document.querySelector<HTMLDivElement>('.errorMessage.cc-authorization-form');
			e.style.display = "block";
			e.innerText = "You must provide a credit card authorization form.";
			return;
		} else if (input && 0 < input.files.length) {
			const file = input.files[0];

			switch (file.type) {
				// Things that chrome detects mime types.
				case "application/pdf":
				case "image/png":
				case "image/jpeg":
					break;
				default:
					// for doc/docx/odf check file extension

					if (file.name.match(/\.(docx|doc|odf)$/i)) {
						break;
					}

					const e = document.querySelector<HTMLDivElement>('.errorMessage.cc-authorization-form');
					e.style.display = "block";
					e.innerText = "The authorization form must be one of these types: PDF, PNG, JPEG, DOC, DOCX, ODF";
					return;
			}
		}


		

		// Request a nonce from the SqPaymentForm object
		PageSetupPreAuthorizedPayments._PaymentForm.requestCardNonce();
	}
}

(window as any).DEBUG_PageOneTimePayment = PageSetupPreAuthorizedPayments;

export default {};