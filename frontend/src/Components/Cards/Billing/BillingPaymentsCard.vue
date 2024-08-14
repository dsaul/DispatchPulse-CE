<template>
	<v-card>
		<v-card-title>Payments</v-card-title>
		<div v-if="IsInvoiceBilling">
			<v-card-text style="color: black;padding-bottom: 0px;">
				You are currently setup to recieve invoices to your registered e-mail address.
			</v-card-text>
			<div v-if="PermBillingCanMakeOneTimeCompanyCreditCardPayments">
				<v-card-subtitle style="font-weight: bold;">One Time Payment (Interac e-Transfer)</v-card-subtitle>
				<v-card-text style="padding-bottom: 0px;">
					To send a payment with Interac e-Transfer, please send it to
					<input type="text" value="accounts.receivable@saulandpartners.ca"
						style="width: 270px; text-align: center; border: 1px dashed #ccc;" readonly />.
					After sending the e-transfer, please email the same address with the
					date, time, and amount of the payment, we will then apply it to your account.
				</v-card-text>
				<v-card-text style="padding-bottom: 0px;">
					There are no fees to send e-transfers.
				</v-card-text>
				<v-card-subtitle style="font-weight: bold;">One Time Payment (Credit Card)</v-card-subtitle>
				<v-card-text style="padding-bottom: 0px;">
					To make a one time credit card payment, please use the following form,
					please be advised that a 2.9% + 30 cent surcharge is applied to one-time payments
					to cover the credit card processing fees.
				</v-card-text>
				<v-card-text style="padding-bottom: 0px;">
					If you wish to pay with credit cards every month, please setup pre-authorized
					payments, with pre-authorized payments we will cover the processing fees.
				</v-card-text>
				<v-card-text>
					<ExpandingIFrame
						:src="`https://squarepayments.dispatchpulse.com/OneTimePayment?SessionId=${BillingSessionsCurrentSessionId()}`">
					</ExpandingIFrame>
				</v-card-text>
				<v-card-subtitle style="font-weight: bold;">One Time Payment (Cheques)</v-card-subtitle>
				<v-card-text style="padding-bottom: 0px;">
					Cheque payments are only accepted for quarterly or yearly payments. Refer to your invoice
					for the address to send cheques. Please ensure enough time for the cheque to
					arrive to ensure there is no interuption of service.
				</v-card-text>
			</div>
			<div v-else>
				<PermissionsDeniedAlert />
			</div>
		</div>
		<v-card-text v-if="IsSquarePreAuthorizedBilling">
			You are currently setup with pre-authorized payments every month.
		</v-card-text>


		<v-card-subtitle style="font-weight: bold;">Pre-Authorized Payments (Credit Card)</v-card-subtitle>
		<v-card-text style="color: black;padding-bottom: 0px;">
			Pre-authorized payments will debit the current balance of your account on or just after the 1st of each
			month.
		</v-card-text>
		<div v-if="PermBillingCanSetupPreAuthorizedCreditCardPayments">
			<v-card-text v-if="!IsSquarePreAuthorizedBilling">
				<ExpandingIFrame
					:src="`https://squarepayments.dispatchpulse.com/SetupPreAuthorizedPayments?SessionId=${BillingSessionsCurrentSessionId()}`">
				</ExpandingIFrame>
				<!-- <ExpandingIFrame
					:src="`https://localhost:5001/SetupPreAuthorizedPayments?SessionId=${BillingSessionsCurrentSessionId()}`"
					>
				</ExpandingIFrame> -->
			</v-card-text>
			<v-card-text v-if="IsSquarePreAuthorizedBilling">
				<ExpandingIFrame
					:src="`https://squarepayments.dispatchpulse.com/DisablePreAuthorizedPayments?SessionId=${BillingSessionsCurrentSessionId()}`">
				</ExpandingIFrame>
				<!-- <ExpandingIFrame
					:src="`https://localhost:5001/DisablePreAuthorizedPayments?SessionId=${BillingSessionsCurrentSessionId()}`"
					>
				</ExpandingIFrame> -->
			</v-card-text>
		</div>
		<div v-else>
			<PermissionsDeniedAlert />
		</div>
	</v-card>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import CardBase from '@/Components/Cards/CardBase';
import { BillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import FormatCurrency from '@/Utility/Formatters/FormatCurrency';
import ExpandingIFrame from '@/Components/Frames/ExpandingIFrame.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { BillingJournalEntries } from '@/Data/Billing/BillingJournalEntries/BillingJournalEntries';

@Component({
	components: {
		PermissionsDeniedAlert,
		ExpandingIFrame,
	},
})
export default class BillingPaymentsCard extends CardBase {

	public $refs!: {

	};


	protected PermBillingCanMakeOneTimeCompanyCreditCardPayments =
		BillingJournalEntries.PermBillingCanMakeOneTimeCompanyCreditCardPayments;
	protected PermBillingCanSetupPreAuthorizedCreditCardPayments =
		BillingJournalEntries.PermBillingCanSetupPreAuthorizedCreditCardPayments;

	protected FormatCurrency = FormatCurrency;
	protected BillingSessionsCurrentSessionId = BillingSessions.CurrentSessionId;

	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;

	public get IsLoadingData(): boolean {

		// if (this.$refs.assignmentsList && this.$refs.assignmentsList.IsLoadingData) {
		// 	return true;
		// }

		return this.loadingData;
	}

	public LoadData(): void {

		//console.log('@@@');

		this.loadingData = true;

		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}

		this._LoadDataTimeout = setTimeout(() => {

			this.loadingData = false;

		}, 250);

		// if (this.$refs.assignmentsList) {
		// 	this.$refs.assignmentsList.LoadData();
		// }

		//console.debug('IsInvoiceBilling', this.IsInvoiceBilling);

	}

	// public MountedAfter() {

	// }


	protected get IsInvoiceBilling(): boolean {
		const company = BillingCompanies.CompanyForCurrentBillingContact();

		if (!company) {
			return false;
		}

		return company.paymentMethod === 'Invoice';
	}

	protected get IsSquarePreAuthorizedBilling(): boolean {
		const company = BillingCompanies.CompanyForCurrentBillingContact();

		if (!company) {
			return false;
		}

		return company.paymentMethod === 'Square Pre-Authorized';
	}





}
</script>