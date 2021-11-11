<template>
	<div class="e2e-did-list-item">
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>phone</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title  style="white-space: normal;">{{value.json.DIDNumber}}</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
					<DIDRegisteredChip :DIDNumber="value" />
				</v-list-item-subtitle>
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
							:disabled="isDialogue || disabled"
							@click="$emit('OpenEntry', value.id)"
							>
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item
							@click="$emit('delete-entry', value)"
							:disabled="disabled || !PermDIDsCanDelete()"
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
import { DID, IDID } from '@/Data/CRM/DID/DID';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import SignalRConnection from '@/RPC/SignalRConnection';
import DIDRegisteredChip from '@/Components/Chips/DIDRegisteredChip.vue';

@Component({
	components: {
		DIDRegisteredChip,
	},
})
export default class DIDsListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IDID | null;
	
	protected PermDIDsCanDelete = DID.PermDIDsCanDelete;
	
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
	
	
	
	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}
	
	
	
	
	
}

</script>