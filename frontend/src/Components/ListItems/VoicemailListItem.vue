<template>
	<div class="e2e-voicemails-list-item">
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>voicemail</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title  style="white-space: normal;">
					{{VoicemailDisplayNameForId(value.id)}}
				</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
					<v-tooltip
						v-if="VoicemailDisplayTypeForId(value.id)"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-icon left small>mdi-shape</v-icon>
								{{VoicemailDisplayTypeForId(value.id)}}
							</v-chip>
						</template>
						<span>Type of Message</span>
					</v-tooltip>
					<v-tooltip
						v-if="VoicemailDisplayCallerIdNameForId(value.id)"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-icon left small>mdi-account</v-icon>
								{{VoicemailDisplayCallerIdNameForId(value.id)}}
							</v-chip>
						</template>
						<span>Caller ID Name</span>
					</v-tooltip>
					<v-tooltip
						v-if="VoicemailDisplayCallerIdNumberForId(value.id)"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-icon left small>mdi-phone-incoming</v-icon>
								{{VoicemailDisplayCallerIdNumberForId(value.id)}}
							</v-chip>
						</template>
						<span>Caller ID Number</span>
					</v-tooltip>
					<v-tooltip
						v-if="VoicemailDisplayCallbackNumberForId(value.id)"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip 
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<v-icon left small>mdi-phone-outgoing</v-icon>
								{{VoicemailDisplayCallbackNumberForId(value.id)}}
							</v-chip>
						</template>
						<span>Callback Number</span>
					</v-tooltip>
					
					
					
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
							:disabled="disabled || !PermVoicemailsCanDelete()"
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
import { IVoicemail, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import SignalRConnection from '@/RPC/SignalRConnection';

@Component({
	
})
export default class VoicemailListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IVoicemail | null;
	
	protected PermVoicemailsCanDelete = Voicemail.PermVoicemailsCanDelete;
	
	protected VoicemailDisplayNameForId = Voicemail.DisplayNameForId;
	protected VoicemailDisplayTypeForId = Voicemail.DisplayTypeForId;
	protected VoicemailDisplayCallerIdNameForId = Voicemail.DisplayCallerIdNameForId;
	protected VoicemailDisplayCallerIdNumberForId = Voicemail.DisplayCallerIdNumberForId;
	protected VoicemailDisplayCallbackNumberForId = Voicemail.DisplayCallbackNumberForId;
	
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