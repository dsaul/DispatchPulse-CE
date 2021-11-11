<template>
	<v-card>
		<v-card-title>Journal Entries</v-card-title>
		<v-card-text
			v-if="PermCRMCanRequestJournalEntries()"
			>
			<v-data-table
				:headers="journalEntriesHeaders"
				:items="AllBillingJournalEntries()"
				:items-per-page="5"
				>
				<template v-slot:[`item.timestamp`]="{ item }">
					{{ISO8601ToLocalDateOnly(item.timestamp)}}
				</template>
				<template v-slot:[`item.type`]="{ item }">
					{{item.type}}
				</template>
				<template v-slot:[`item.description`]="{ item }">
					{{item.description}}
				</template>
				<template v-slot:[`item.quantity`]="{ item }">
					{{item.quantity}}
				</template>
				<template v-slot:[`item.unitPrice`]="{ item }">
					{{FormatCurrency(item.unitPrice, item.currency)}}
				</template>
				<template v-slot:[`item.taxPercentage`]="{ item }">
					{{item.taxPercentage}} %
				</template>
				<template v-slot:[`item.taxActual`]="{ item }">
					{{FormatCurrency(item.taxActual, item.currency)}}
				</template>
				<template v-slot:[`item.total`]="{ item }">
					{{FormatCurrency(item.total, item.currency)}}
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
import { BillingJournalEntries } from '@/Data/Billing/BillingJournalEntries/BillingJournalEntries';
import ISO8601ToLocalDateOnly from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDateOnly';
import FormatCurrency from '@/Utility/Formatters/FormatCurrency';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class BillingJournalEntriesCard extends CardBase {
	
	public $refs!: {
		
	};
	
	protected journalEntriesHeaders = [
		{
			text: 'Timestamp',
			value: 'timestamp',
		},
		{
			text: 'Type',
			value: 'type',
		},
		{
			text: 'Description',
			value: 'description',
		},
		{
			text: 'Quantity',
			value: 'quantity',
		},
		{
			text: 'Unit $',
			value: 'unitPrice',
		},
		{
			text: 'Tax %',
			value: 'taxPercentage',
		},
		{
			text: 'Tax $',
			value: 'taxActual',
		},
		{
			text: 'Total $',
			value: 'total',
		},
	];
	
	protected PermCRMCanRequestJournalEntries = BillingJournalEntries.PermCRMCanRequestJournalEntries;
	
	protected BillingPackageNameForId = BillingPackages.NameForId;
	protected BillingPackageDisplayNameForId = BillingPackages.DisplayNameForId;
	protected BillingPackageCostForId = BillingPackages.CostForId;
	protected AllBillingJournalEntries = BillingJournalEntries.GetAll;
	protected ISO8601ToLocalDateOnly = ISO8601ToLocalDateOnly;
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