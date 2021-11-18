<template>
	
	<div>
	
		<v-app-bar color="#747389" dark fixed app clipped-right >
			<v-progress-linear
				v-if="IsLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Voicemails</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="ReLoadData()" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn
					dark
					icon
					v-on="on"
					>
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
					<v-list-item disabled>
						<v-list-item-content>
							<v-list-item-title>No Items</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>
			
			
			<template v-slot:extension>
				<v-tabs
				v-model="tab"
				
				background-color="transparent"
				align-with-title
				show-arrows
				>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab
						@click="$router.replace({query: { ...$route.query, tab: 'Responders'}}).catch(((e) => {}));"
						class="e2e-voicemails-index-tab-messages"
						>
						Messages
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs :items="breadcrumbs" style="background: white; padding-bottom: 5px; padding-top: 15px;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>
		
		<v-alert
			v-if="connectionStatus != 'Connected'"
			type="error"
			elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;"
			>
			Disconnected from server.
		</v-alert>
		
		<v-tabs-items v-model="tab" style="background: transparent;">
			<v-tab-item style="flex: 1;">
				<VoicemailList 
					ref="VoicemailList"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>
		
		<!-- <AddVoicemailDialogue 
			v-model="addVoicemailModel"
			:isOpen="addVoicemailOpen"
			@Save="SaveAddVoicemail"
			@Cancel="CancelAddVoicemail"
			ref="addVoicemailDialogue"
			/> -->
		
		<!-- <v-footer color="#747389" class="white--text" app inset>
			<v-row
				no-gutters
				>
				<v-spacer />
				
				<AddMenuButton
					:disabled="connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="OpenAddVoicemail()"
						class="e2e-add-menu-add-auto-attendant"
						:disabled="connectionStatus != 'Connected' || !PermVoicemailsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>mdi-menu-open</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Auto Attendant</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
				
			</v-row>
		</v-footer> -->
		
	</div>
</template>

<script lang="ts">

import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import VoicemailList from '@/Components/Lists/VoicemailList.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { IVoicemail, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

/// <reference path="../../Data/CRM/Voicemail/Voicemail.ts" />

@Component({
	components: {
		AddMenuButton,
		VoicemailList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		// AddVoicemailDialogue,
	},
})
export default class VoicemailsIndex extends ViewBase {
	
	public $refs!: {
		VoicemailList: VoicemailList,
		// addVoicemailDialogue: AddVoicemailDialogue,
	};
	
	
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Messages: 0,
		messages: 0,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'Voicemails',
			disabled: true,
			to: '/section/voicemails/index',
		},
	];
	
	
	protected PermVoicemailsCanPush = Voicemail.PermVoicemailsCanPush;
	
	protected addVoicemailModel: IVoicemail | null = null;
	protected addVoicemailOpen = false;
	
	protected loadingData = false;
	
	
	public get IsLoadingData(): boolean {
		
		
		if (this.$refs.VoicemailList && this.$refs.VoicemailList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.VoicemailList) {
			this.$refs.VoicemailList.LoadData();
		}
		
	}

	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				this.loadingData = true;
				
				setTimeout(() => {
					this.loadingData = false;
				}, 250);
				
			});
		});
	}
	
	protected MountedAfter(): void {
		this.SwitchToTabFromRoute();
		this.LoadData();
	}
	
	protected SwitchToTabFromRoute(): void {
		// Select the tab in the query string.
		if (IsNullOrEmpty(this.$route.query.tab as string | null)) {
			this.tab = 0;
		} else {
			const index = this.tabNameToIndex[this.$route.query.tab as string];
			this.tab = index;
		}
	}
	
	// protected OpenAddVoicemail() {
		
	// 	//console.debug('OpenAddCalendars');
		
	// 	this.addVoicemailModel = Voicemail.GetEmpty();
	// 	this.addVoicemailOpen = true;
		
	// 	requestAnimationFrame(() => {
	// 		if (this.$refs.addVoicemailDialogue) {
	// 			this.$refs.addVoicemailDialogue.SwitchToTabFromRoute();
	// 		}
	// 	});
		
		
		
	// }
	
	protected CancelAddVoicemail(): void {
		
		//console.debug('CancelAddVoicemail');
		
		this.addVoicemailOpen = false;
		
	}
	
	protected SaveAddVoicemail(): void {
		
		//console.debug('SaveAddVoicemail');
		
		
		
		if (!this.addVoicemailModel ||
			!this.addVoicemailModel.id ||
			IsNullOrEmpty(this.addVoicemailModel.id)) {
			console.error('SaveAddVoicemail error 1');
			return;
		}
		
		const payload: Record<string, IVoicemail> = {};
		payload[this.addVoicemailModel.id] = this.addVoicemailModel;
		Voicemail.UpdateIds(payload);
		
		this.addVoicemailOpen = false;
		this.$router.push(`/section/on-call/${this.addVoicemailModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
		
		
	}
	
}
</script>
