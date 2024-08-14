<template>

	<div>

		<v-app-bar color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="IsLoadingData" :indeterminate="true" absolute top
				color="white"></v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">Phone Numbers</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="ReLoadData()" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn dark icon v-on="on">
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
				<v-tabs v-model="tab" background-color="transparent" align-with-title show-arrows>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab
						@click="$router.replace({ query: { ...$route.query, tab: 'Phone Numbers' } }).catch(((e) => { }));"
						class="e2e-dids-index-tab-Phone Numbers">
						Phone Numbers
					</v-tab>
				</v-tabs>
			</template>

		</v-app-bar>

		<v-breadcrumbs :items="breadcrumbs" style="background: white; padding-bottom: 5px; padding-top: 15px;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>

		<v-alert v-if="connectionStatus != 'Connected'" type="error" elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;">
			Disconnected from server.
		</v-alert>

		<v-tabs-items v-model="tab" style="background: transparent;">
			<v-tab-item style="flex: 1;">
				<DIDsList ref="didsList" :disabled="connectionStatus != 'Connected'" />
			</v-tab-item>
		</v-tabs-items>

		<div style="height: 50px;"></div>

		<AddDIDDialogue v-model="addDIDModel" :isOpen="addDIDOpen" @Save="SaveAddDID" @Cancel="CancelAddDID"
			ref="addDIDDialogue" />

		<v-footer color="#747389" class="white--text" app inset>
			<v-row no-gutters>
				<v-spacer />

				<AddMenuButton :disabled="connectionStatus != 'Connected'">
					<v-list-item @click="OpenAddDID()" class="e2e-add-menu-add-did"
						:disabled="connectionStatus != 'Connected' || !PermDIDsCanPush()">
						<v-list-item-icon>
							<v-icon>phone</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Phone Number</v-list-item-title>
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
import DIDsList from '@/Components/Lists/DIDsList.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import AddDIDDialogue from '@/Components/Dialogues2/DIDs/AddDIDDialogue.vue';

@Component({
	components: {
		AddMenuButton,
		DIDsList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		AddDIDDialogue,
	},
})
export default class DIDsIndex extends ViewBase {

	public $refs!: {
		didsList: DIDsList,
		addDIDDialogue: AddDIDDialogue,
	};



	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		'Phone Numbers': 0,
		'phone numbers': 0,
	};

	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'Phone Numbers',
			disabled: true,
			to: '/section/dids/index',
		},
	];


	protected PermDIDsCanPush = DID.PermDIDsCanPush;

	protected addDIDModel: IDID | null = null;
	protected addDIDOpen = false;

	protected loadingData = false;

	constructor() {
		super();

	}



	public get IsLoadingData(): boolean {


		if (this.$refs.didsList && this.$refs.didsList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}

	public ReLoadData(): void {

		this.LoadData();

		if (this.$refs.didsList) {
			this.$refs.didsList.LoadData();
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

	protected OpenAddDID(): void {

		//console.debug('OpenAddCalendars');

		this.addDIDModel = DID.GetEmpty();
		this.addDIDOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.addDIDDialogue) {
				this.$refs.addDIDDialogue.SwitchToTabFromRoute();
			}
		});



	}

	protected CancelAddDID(): void {

		//console.debug('CancelAddDID');

		this.addDIDOpen = false;

	}

	protected SaveAddDID(): void {

		//console.debug('SaveAddDID');



		if (!this.addDIDModel || !this.addDIDModel.id || IsNullOrEmpty(this.addDIDModel.id)) {
			console.error('!this.addDIDModel || !this.addDIDModel.id || IsNullOrEmpty(this.addDIDModel.id)');
			return;
		}

		const payload: Record<string, IDID> = {};
		payload[this.addDIDModel.id] = this.addDIDModel;
		DID.UpdateIds(payload);

		this.addDIDOpen = false;
		this.$router.push(`/section/dids/${this.addDIDModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line



	}

}
</script>
