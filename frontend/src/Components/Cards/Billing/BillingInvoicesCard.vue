<template>
	<v-card>
		<v-card-title>Invoices</v-card-title>
		<v-card-text v-if="PermCRMCanRequestInvoices()">
			<v-data-table :headers="invoiceHeaders" :items="AllBillingInvoices()" :items-per-page="5">
				<template v-slot:[`item.amountDue`]="{ item }">
					{{ FormatCurrency(item.amountDue, item.currency) }}
				</template>
				<template v-slot:[`item.amountPaid`]="{ item }">
					{{ FormatCurrency(item.amountPaid, item.currency) }}
				</template>
				<template v-slot:[`item.amountRemaining`]="{ item }">
					{{ FormatCurrency(item.amountRemaining, item.currency) }}
				</template>
			</v-data-table>
		</v-card-text>
		<v-card-text v-else>
			<PermissionsDeniedAlert />
		</v-card-text>
	</v-card>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import CardBase from '@/Components/Cards/CardBase';
import { BillingPackages } from '@/Data/Billing/BillingPackages/BillingPackages';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingInvoices } from '@/Data/Billing/BillingInvoices/BillingInvoices';
import FormatCurrency from '@/Utility/Formatters/FormatCurrency';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class BillingInvoicesCard extends CardBase {

	public $refs!: {

	};

	protected invoiceHeaders = [
		{
			text: 'Invoice #',
			value: 'invoiceNumber',
		},
		{
			text: 'Amount Due',
			value: 'amountDue',
		},
		{
			text: 'Amount Paid',
			value: 'amountPaid',
		},
		{
			text: 'Amount Remaining',
			value: 'amountRemaining',
		},
	];

	protected PermCRMCanRequestInvoices = BillingInvoices.PermCRMCanRequestInvoices;
	protected BillingPackageNameForId = BillingPackages.NameForId;
	protected BillingPackageDisplayNameForId = BillingPackages.DisplayNameForId;
	protected BillingPackageCostForId = BillingPackages.CostForId;
	protected AllBillingInvoices = BillingInvoices.GetAll;
	protected FormatCurrency = FormatCurrency;

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

	}

}
</script>