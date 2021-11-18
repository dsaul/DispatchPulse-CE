<template>
	<div>
		<content-placeholders v-if="loadingData">
			<content-placeholders-text :lines="1" />
		</content-placeholders>
		<div v-else>
			<slot></slot>
		</div>
	</div>
</template>
<script lang="ts">

import { Component, Vue, Prop, Watch } from 'vue-property-decorator';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		
	},
	
})

export default class DataLoaderAssignment extends Vue {
	
	
	@Prop({	default: null }) public readonly assignmentId!: string;
	
	
	protected loadingData = false;
	
	
	public mounted(): void {
		this.LoadData();
	}
	
	public LoadData(): void {
		
		//console.log('loaddata')
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				if (null == Assignment.ForId(this.assignmentId)) {
				
					const promises: Array<Promise<any>> = [];
					
					if (Assignment.PermAssignmentCanRequest()) {
						const rtr = Assignment.FetchForId(this.assignmentId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
					}
					
					
					if (promises.length > 0) {
						
						this.loadingData = true;
						
						Promise.all(promises).finally(() => {
							this.loadingData = false;
						});
					}
					
					
				}
				
			});
		});
	}
	
	@Watch('assignmentId')
	protected assignmentIdChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars
		this.LoadData();
	}
	
}

</script>
<style scoped>

</style>