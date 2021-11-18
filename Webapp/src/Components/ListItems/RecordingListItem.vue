<template>
	<div>
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>domain</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title  style="white-space: normal;">{{value.json.name}}</v-list-item-title>
				<!--<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
					Recording
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
							@click="$emit('DeleteEntry', value.id)"
							:disabled="disabled || !PermRecordingsCanDelete()"
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
import { IRecording, Recording } from '@/Data/CRM/Recording/Recording';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import SignalRConnection from '@/RPC/SignalRConnection';

@Component({
	
})
export default class RecordingListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IRecording | null;
	
	protected PermRecordingsCanDelete = Recording.PermRecordingsCanDelete;
	
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