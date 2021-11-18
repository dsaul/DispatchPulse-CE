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
import { Product } from '@/Data/CRM/Product/Product';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		
	},
	
})

export default class DataLoaderProduct extends Vue {
	
	
	@Prop({	default: null }) public readonly productId!: string;
	
	
	protected loadingData = false;
	
	
	public mounted(): void {
		this.LoadData();
	}
	
	public LoadData(): void {
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				if (null == Product.ForId(this.productId)) {
					
					const promises: Array<Promise<any>> = [];
					
					if (Product.PermProductsCanRequest()) {
						const rtr = Product.FetchForId(this.productId);
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
	
	@Watch('productId')
	protected productIdChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars
		this.LoadData();
	}
	
}

</script>
<style scoped>

</style>