<template>
	<div class="e2e-products-list-item">
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>local_grocery_store</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title style="white-space: normal;">{{value.json.name}}</v-list-item-title>
				<!--<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
				</v-list-item-subtitle>-->
			</v-list-item-content>

			<v-list-item-action v-if="showMenuButton">
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn
							icon
							v-on="on"
							:disabled="disabled"
							>
							<v-icon>more_vert</v-icon>
						</v-btn>
					</template>

					<v-list dense>
						<v-list-item
							:disabled="isDialogue || disabled || !PermProductsCanRequest()"
							@click="$emit('OpenEntry', value.id)"
							>
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Edit…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item
							@click="$emit('DeleteEntry', value.id)"
							:disabled="disabled || !PermProductsCanDelete()"
							>
							<v-list-item-icon>
								<v-icon>delete</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Delete…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
					</v-list>
				</v-menu>
			</v-list-item-action>
		</v-list-item>
	</div>

</template>

<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import ListItemBase from './ListItemBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import { IProduct, Product } from '@/Data/CRM/Product/Product';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	
})
export default class ProductsListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IProduct;
	
	protected PermProductsCanRequest = Product.PermProductsCanRequest;
	protected PermProductsCanPush = Product.PermProductsCanPush;
	protected PermProductsCanDelete = Product.PermProductsCanDelete;
	
	protected loadingData = false;
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				if (this.value == null) {
					return;
				}
				
				const promises: Array<Promise<any>> = [];
				
				
				if (promises.length > 0) {
					
					this.loadingData = true;
					
					Promise.all(promises).finally(() => {
						this.loadingData = false;
					});
				}
				
			});
		});
		
	}
	
	
	/*get DateUsed(): string | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.dateUsedISO8601
			) {
				return null;
			}
		
		const d =  DateTime. fromISO(this.value.json.dateUsedISO8601 );
		return d.toLocaleString(DateTime.DATE_FULL);
	}*/
	
	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}
}

</script>