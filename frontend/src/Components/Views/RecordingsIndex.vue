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
			
			<v-toolbar-title class="white--text">All Recordings</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="LoadData()" />

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
					<v-list-item
						:disabled="true"
						>
						<v-list-item-content>
							<v-list-item-title>No Items.</v-list-item-title>
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

					<v-tab @click="$router.replace({query: { ...$route.query, tab: 'Recordings'}}).catch(((e) => {}));">
						Recordings
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
				
				
				<RecordingsDataTable
					ref="dataTable"
					:disabled="connectionStatus != 'Connected'"
					@edit-entry="OpenEditRecording"
					@delete-entry="OpenDeleteRecording"
					/>
				
				
				
				
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>

		<AddRecordingDialogue 
			v-model="addRecordingModel"
			:isOpen="addRecordingOpen"
			@Save="SaveAddRecording"
			@Cancel="CancelAddRecording"
			ref="addRecordingDialogue"
			/>

		<DeleteRecordingDialogue 
			v-model="deleteRecordingModel"
			:isOpen="deleteRecordingOpen"
			@Delete="SaveDeleteRecording"
			@Cancel="CancelDeleteRecording"
			ref="deleteRecordingDialogue"
			/>
		
		<EditRecordingDialogue 
			v-model="editRecordingModel"
			:isOpen="editRecordingOpen"
			@Save="SaveEditRecording"
			@Cancel="CancelEditRecording"
			ref="editRecordingDialogue"
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
						@click="OpenAddRecording()"
						:disabled="connectionStatus != 'Connected' || !PermRecordingsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>mdi-file-upload</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Upload&hellip;</v-list-item-title>
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
import RecordingsDataTable from '@/Components/DataTables/RecordingsDataTable.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { IRecording, Recording } from '@/Data/CRM/Recording/Recording';
import AddRecordingDialogue from '@/Components/Dialogues2/Recordings/AddRecordingDialogue.vue';
import DeleteRecordingDialogue from '@/Components/Dialogues2/Recordings/DeleteRecordingDialogue.vue';
import EditRecordingDialogue from '@/Components/Dialogues2/Recordings/EditRecordingDialogue.vue';

@Component({
	components: {
		AddMenuButton,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		RecordingsDataTable,
		ReloadButton,
		NotificationBellButton,
		AddRecordingDialogue,
		DeleteRecordingDialogue,
		EditRecordingDialogue,
	},
})
export default class RecordingsIndex extends ViewBase {
	
	public $refs!: {
		dataTable: RecordingsDataTable,
		addRecordingDialogue: AddRecordingDialogue,
		deleteRecordingDialogue: DeleteRecordingDialogue,
		editRecordingDialogue: EditRecordingDialogue,
	};
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Contacts: 0,
		contacts: 0,
		Companies: 1,
		companies: 1,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Recordings',
			disabled: true,
			to: '/section/recordings',
		},
	];
	
	protected PermRecordingsCanPush = Recording.PermRecordingsCanPush;

	protected addRecordingModel: IRecording | null = null;
	protected addRecordingOpen = false;

	protected editRecordingModel: IRecording | null = null;
	protected editRecordingOpen = false;

	protected deleteRecordingModel: IRecording | null = null;
	protected deleteRecordingOpen = false;

	protected loadingData = false;
	
	
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.dataTable && this.$refs.dataTable.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.dataTable) {
			this.$refs.dataTable.LoadData();
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


	protected OpenAddRecording(): void {
		
		//console.debug('OpenAddCalendars');
		
		this.addRecordingModel = Recording.GetEmpty();
		this.addRecordingOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.addRecordingDialogue) {
				this.$refs.addRecordingDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelAddRecording(): void {
		
		//console.debug('CancelAddRecording');
		this.addRecordingModel = Recording.GetEmpty();
		this.addRecordingOpen = false;
		
	}
	
	protected SaveAddRecording(): void {
		
		//console.debug('SaveAddRecording');
		
		
		
		if (!this.addRecordingModel ||
			!this.addRecordingModel.id ||
			IsNullOrEmpty(this.addRecordingModel.id)) {
			console.error('SaveAddRecording error 1');
			return;
		}
		
		const payload: { [id: string]: IRecording; } = {};
		payload[this.addRecordingModel.id] = this.addRecordingModel;
		Recording.UpdateIds(payload);
		
		this.addRecordingOpen = false;
		this.addRecordingModel = Recording.GetEmpty();
		// this.$router.push(`/section/on-call/${this.addRecordingModel.id}?tab=General`)
		//.catch(((e: Error) => { }));// eslint-disable-line
		
		
		
	}


	protected OpenEditRecording(val: IRecording): void {
		
		//console.debug('OpenAddCalendars');
		
		this.editRecordingModel = val;
		this.editRecordingOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.editRecordingDialogue) {
				this.$refs.editRecordingDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelEditRecording(): void {
		
		//console.debug('CancelEditRecording');
		this.editRecordingModel = Recording.GetEmpty();
		this.editRecordingOpen = false;
		
	}
	
	protected SaveEditRecording(): void {
		
		//console.debug('SaveEditRecording');
		
		
		
		if (!this.editRecordingModel ||
			!this.editRecordingModel.id ||
			IsNullOrEmpty(this.editRecordingModel.id)) {
			console.error('SaveEditRecording error 1');
			return;
		}
		
		const payload: { [id: string]: IRecording; } = {};
		payload[this.editRecordingModel.id] = this.editRecordingModel;
		Recording.UpdateIds(payload);
		
		this.editRecordingOpen = false;
		this.editRecordingModel = Recording.GetEmpty();

	}




	protected OpenDeleteRecording(val: IRecording): void {
		
		//console.debug('OpenAddCalendars');
		
		this.deleteRecordingModel = val;
		this.deleteRecordingOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.deleteRecordingDialogue) {
				this.$refs.deleteRecordingDialogue.SwitchToTabFromRoute();
			}
		});
		
		
	}
	
	protected CancelDeleteRecording(): void {
		
		//console.debug('CancelDeleteRecording');
		this.editRecordingModel = Recording.GetEmpty();
		this.deleteRecordingOpen = false;
		
	}
	
	protected SaveDeleteRecording(): void {
		
		console.debug('SaveDeleteRecording', this.deleteRecordingModel);
		
		
		
		if (!this.deleteRecordingModel ||
			!this.deleteRecordingModel.id ||
			IsNullOrEmpty(this.deleteRecordingModel.id)) {
			console.error('SaveDeleteRecording error 1');
			return;
		}
		
		const payload: string[] = [this.deleteRecordingModel.id];
		Recording.DeleteIds(payload);
		
		this.deleteRecordingOpen = false;
		this.deleteRecordingModel = Recording.GetEmpty();
		
		
		
	}





	




























	
	

	
}
</script>
