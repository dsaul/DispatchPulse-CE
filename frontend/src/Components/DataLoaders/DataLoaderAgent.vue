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
import { Agent } from '@/Data/CRM/Agent/Agent';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {

	},

})

export default class DataLoaderAgent extends Vue {


	@Prop({ default: null }) public readonly agentId!: string;


	protected loadingData = false;


	public mounted(): void {
		this.LoadData();
	}

	public LoadData(): void {

		//console.log('loaddata')
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				if (null == Agent.ForId(this.agentId)) {

					const promises: Array<Promise<any>> = [];

					if (Agent.PermAgentsCanRequest()) {
						const rtr = Agent.FetchForId(this.agentId);
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

	@Watch('agentId')
	protected agentIdChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars
		this.LoadData();
	}

}

</script>
<style scoped></style>