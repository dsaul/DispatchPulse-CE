<template>
	<div class="e2e-agent-list-item">
		<v-list-item :class="{ paddingLeftRight5px: reducePadding, }">
			<v-list-item-avatar :class="{ marginRight5px: reducePadding, }">
				<v-icon>person</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title style="white-space: normal;">
					<span v-if="!isPlaceholder && value && value.json">{{value.json.name}}</span>
					<span v-if="isPlaceholder">{{placeholderValues.name}}</span>
				</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
					<v-chip
						v-if="showEmploymentStatus && EmploymentStatusName"
						label
						outlined
						small
						style="margin-right: 5px;"
						>
						{{EmploymentStatusName}}
					</v-chip>
					<div v-else-if="!EmploymentStatusName && loadingData">
						<content-placeholders>
							<content-placeholders-text :lines="1" />
						</content-placeholders>
					</div>
					
					<v-chip
						v-if="showTitle && value && value.json && value.json.title"
						label
						outlined
						small
						style="margin-right: 5px;"
						>
						{{value.json.title}}
					</v-chip>
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
							:disabled="isDialogue || disabled || !PermAgentsCanRequest()"
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
							:disabled="disabled || !PermAgentsCanDelete()"
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
import { EmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	
})
export default class AgentsListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IAgent | null;
	
	//placeholderValues.name
	
	@Prop({ default: false }) public readonly reducePadding!: boolean;
	@Prop({ default: true }) public readonly showEmploymentStatus!: boolean;
	@Prop({ default: true }) public readonly showTitle!: boolean;
	
	protected PermAgentDisplayOwn = Agent.PermAgentDisplayOwn;
	protected PermAgentsCanRequest = Agent.PermAgentsCanRequest;
	protected PermAgentsCanPush = Agent.PermAgentsCanPush;
	protected PermAgentsCanDelete = Agent.PermAgentsCanDelete;
	protected loadingData = false;
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				if (this.value == null) {
					return;
				}
				
				const promises: Array<Promise<any>> = [];
		
				if (null != this.value.json.employmentStatusId && !IsNullOrEmpty(this.value.json.employmentStatusId)) {
					
					const es = EmploymentStatus.ForId(this.value.json.employmentStatusId);
					if (null == es && EmploymentStatus.PermEmploymentStatusCanRequest()) {
						
						const rtr = EmploymentStatus.FetchForId(this.value.json.employmentStatusId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
				}
				
				if (promises.length > 0) {
					
					this.loadingData = true;
					
					Promise.all(promises).finally(() => {
						this.loadingData = false;
					});
				}
				
			});
		});
		
		
		
		
		
		
	}
	
	
	
	
	
	protected get EmploymentStatusName(): string | null {
		
		//console.debug('get EmploymentStatusId', this.value);
		
		if (!this.value || 
			!this.value.json || 
			!this.value.json.employmentStatusId || 
			IsNullOrEmpty(this.value.json.employmentStatusId)
		) {
			return null;
		} else {
			
			const aes = EmploymentStatus.ForId(this.value.json.employmentStatusId);
			if (!aes) {
				return null;
			}
			//console.log('aes', aes);
			return aes.json.name;
		}
	}
	
	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}
	
}

</script>
<style scoped>
.marginRight5px {
	margin-right: 5px !important;
}
.paddingLeftRight5px {
	padding-left: 5px !important;
	padding-right: 5px !important;
}
</style>