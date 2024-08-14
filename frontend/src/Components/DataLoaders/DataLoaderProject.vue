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
import { Project } from '@/Data/CRM/Project/Project';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {

	},

})

export default class DataLoaderProject extends Vue {


	@Prop({ default: null }) public readonly projectId!: string;


	protected loadingData = false;



	public mounted(): void {
		this.LoadData();
	}

	public LoadData(): void {
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				if (null == Project.ForId(this.projectId)) {

					const promises: Array<Promise<any>> = [];

					if (Project.PermProjectsCanRequest()) {
						const rtr = Project.FetchForId(this.projectId);
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

	@Watch('projectId')
	protected projectIdChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars
		this.LoadData();
	}

}

</script>
<style scoped></style>