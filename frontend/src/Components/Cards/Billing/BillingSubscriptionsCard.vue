<template>
	<v-card>
		<v-card-title>Subscriptions</v-card-title>
		<v-card-text
			v-if="PermCRMCanRequestSubscriptions()"
			>
			<v-data-table
				:headers="subscriptionsHeaders"
				:items="AllBillingSubscriptions()"
				:items-per-page="5"
				>
				<template v-slot:[`item.packageId`]="{ item }">
					{{BillingPackageNameForId(item.packageId)}}
				</template>
				<template v-slot:[`item.name`]="{ item }">
					{{BillingPackageDisplayNameForId(item.packageId)}}
				</template>
				<template v-slot:[`item.cost`]="{ item }">
					{{BillingPackageCostForId(item.packageId)}}
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
import { BillingSubscriptions } from '@/Data/Billing/BillingSubscriptions/BillingSubscriptions';
import { BillingPackages } from '@/Data/Billing/BillingPackages/BillingPackages';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class BillingSubscriptionsCard extends CardBase {
	
	public $refs!: {
		
	};
	
	protected subscriptionsHeaders = [
		{
			text: 'Package',
			value: 'packageId',
		},
		{
			text: 'Name',
			value: 'name',
		},
		{
			text: 'Cost/month',
			value: 'cost',
		},
	];
	
	protected PermCRMCanRequestSubscriptions = BillingSubscriptions.PermCRMCanRequestSubscriptions;
	protected BillingPackageNameForId = BillingPackages.NameForId;
	protected BillingPackageDisplayNameForId = BillingPackages.DisplayNameForId;
	protected BillingPackageCostForId = BillingPackages.CostForId;
	protected AllBillingSubscriptions = BillingSubscriptions.GetAll;
	
	private loadingData = false;
	private _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
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