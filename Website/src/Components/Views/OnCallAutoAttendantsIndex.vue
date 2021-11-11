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
			
			<v-toolbar-title class="white--text">On Call Responders</v-toolbar-title>

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
						class="e2e-on-call-index-tab-auto-attendants"
						>
						Responders
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
				<OnCallAutoAttendantsList 
					ref="onCallAutoAttendantsList"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>
		
		<AddOnCallAutoAttendantDialogue 
			v-model="addOnCallAutoAttendantModel"
			:isOpen="addOnCallAutoAttendantOpen"
			@Save="SaveAddOnCallAutoAttendant"
			@Cancel="CancelAddOnCallAutoAttendant"
			ref="addOnCallAutoAttendantDialogue"
			/>
		
		<v-footer color="#747389" class="white--text" app inset>
			<v-row
				no-gutters
				>
				<v-spacer />
				
				<AddMenuButton
					:disabled="connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="OpenAddOnCallAutoAttendant()"
						class="e2e-add-menu-add-auto-attendant"
						:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()"
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
		</v-footer>
		
	</div>
</template>

<script lang="ts">

import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import OnCallAutoAttendantsList from '@/Components/Lists/OnCallAutoAttendantsList.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { IOnCallAutoAttendant, OnCallAutoAttendant } from '@/Data/CRM/OnCallAutoAttendant/OnCallAutoAttendant';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import AddOnCallAutoAttendantDialogue from '@/Components/Dialogues2/OnCallAutoAttendants/AddOnCallAutoAttendantDialogue.vue';

@Component({
	components: {
		AddMenuButton,
		OnCallAutoAttendantsList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		AddOnCallAutoAttendantDialogue,
	},
})
export default class OnCallAutoAttendantsIndex extends ViewBase {
	
	public $refs!: {
		onCallAutoAttendantsList: OnCallAutoAttendantsList,
		addOnCallAutoAttendantDialogue: AddOnCallAutoAttendantDialogue,
	};
	
	
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Responders: 0,
		responders: 0,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'On Call Responders',
			disabled: true,
			to: '/section/on-call/index',
		},
	];
	
	
	protected PermOnCallAutoAttendantsCanPush = OnCallAutoAttendant.PermOnCallAutoAttendantsCanPush;
	
	protected addOnCallAutoAttendantModel: IOnCallAutoAttendant | null = null;
	protected addOnCallAutoAttendantOpen = false;
	
	protected loadingData = false;
	
	constructor() {
		super();
		
	}
	
	
	
	public get IsLoadingData(): boolean {
		
		
		if (this.$refs.onCallAutoAttendantsList && this.$refs.onCallAutoAttendantsList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.onCallAutoAttendantsList) {
			this.$refs.onCallAutoAttendantsList.LoadData();
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
	
	protected OpenAddOnCallAutoAttendant(): void {
		
		//console.debug('OpenAddCalendars');
		
		this.addOnCallAutoAttendantModel = OnCallAutoAttendant.GetEmpty();
		this.addOnCallAutoAttendantOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.addOnCallAutoAttendantDialogue) {
				this.$refs.addOnCallAutoAttendantDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelAddOnCallAutoAttendant(): void {
		
		//console.debug('CancelAddOnCallAutoAttendant');
		
		this.addOnCallAutoAttendantOpen = false;
		
	}
	
	protected SaveAddOnCallAutoAttendant(): void {
		
		//console.debug('SaveAddOnCallAutoAttendant');
		
		
		
		if (!this.addOnCallAutoAttendantModel ||
			!this.addOnCallAutoAttendantModel.id ||
			IsNullOrEmpty(this.addOnCallAutoAttendantModel.id)) {
			console.error('SaveAddOnCallAutoAttendant error 1');
			return;
		}
		
		const payload: { [id: string]: IOnCallAutoAttendant; } = {};
		payload[this.addOnCallAutoAttendantModel.id] = this.addOnCallAutoAttendantModel;
		OnCallAutoAttendant.UpdateIds(payload);
		
		this.addOnCallAutoAttendantOpen = false;
		this.$router.push(`/section/on-call/${this.addOnCallAutoAttendantModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
		
		
	}
	
}
</script>
