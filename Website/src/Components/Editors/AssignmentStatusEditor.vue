<template>
	<div>
	
		<v-app-bar
			v-if="showAppBar"
			color="#747389"
			dark
			fixed
			app
			clipped-right
			>
			<v-progress-linear
				v-if="isLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Assignment Status</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

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
					<!--<v-list-item
						@click="DoPrint()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
					<!--<v-list-item
						@click="ExportToCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
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
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'General'}}).catch(((e) => {}));"
						>
						General
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs
			v-if="breadcrumbs"
			:items="breadcrumbs"
			style="padding-bottom: 0px; padding-top: 15px; background: white;"
			>
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
		
		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Assignment Status Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The assignment status no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the assignment status while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
			<v-tabs
				v-if="!showAppBar"
				v-model="tab"
				background-color="transparent"
				grow
				show-arrows
				style="visibility: none; height:0px;"
			>
				<v-tab>
					General
				</v-tab>
			</v-tabs>
			
			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>
						
						<v-form
							autocomplete="newpassword"
							ref="generalForm"
							>
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">General</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											ref="name"
											v-model="Name"
											autocomplete="newpassword"
											label="Name"
											hint="Name of the Assignment Status"
											:rules="[ ValidateRequiredField ]"
											class="e2e-assignment-status-editor-name"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
									
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsOpen"
											label="Is Open"
											class="e2e-assignment-status-editor-is-open"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsReOpened"
											label="Is Re-opened"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsAssigned"
											label="Is Assigned"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsWaitingOnClient"
											label="Is Waiting On Client"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsWaitingOnVendor"
											label="Is Waiting On Vendor"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsBillable"
											label="Is Billable"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsBillableReview"
											label="Is Billable Review"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsInProgress"
											label="Is In Progress"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsNonBillable"
											label="Is Non Billable"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsScheduled"
											label="Is Scheduled"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-checkbox
											v-model="IsDefault"
											label="Is Default"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Advanced</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Id"
											readonly="readonly"
											label="Unique ID"
											hint="The id of this assignment status."
											>
										</v-text-field>
									</v-col>
								</v-row>
							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
			</v-tabs-items>
		</div>
		
		<v-footer
			v-if="showFooter"
			color="#747389"
			class="white--text"
			app
			inset>
			<v-row
				no-gutters
				>
				<v-btn
					:disabled="!value || connectionStatus != 'Connected'"
					color="white"
					text
					rounded
					@click="DialoguesOpen({ name: 'DeleteAssignmentStatusDialogue', state: {
						redirectToIndex: true,
						id: value.id,
					}})"
					>
					<v-icon left>delete</v-icon>
					Delete
				</v-btn>
			</v-row>
		</v-footer>
	</div>
</template>
<script lang="ts">

import EditorBase, { IBreadcrumb, VForm } from './EditorBase';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import ContactList from '@/Components/Lists/ContactList.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ProductSelectField from '@/Components/Fields/ProductSelectField.vue';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import Dialogues from '@/Utility/Dialogues';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { AssignmentStatus, IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';

@Component({
	components: {
		ProjectList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ProductSelectField,
		ReloadButton,
		NotificationBellButton,
	},
	
})
export default class AssignmentStatusEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IAssignmentStatus | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
		quantity: Vue,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected DialoguesOpen = Dialogues.Open;
	protected PermCRMExportAssignmentStatusCSV = AssignmentStatus.PermCRMExportAssignmentStatusCSV;
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	constructor() {
		super();
		
	}
	
	
	
	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
		];
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			General: 0,
			general: 0,
			Projects: 1,
			projects: 1,
		};
	}
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	
	protected get Name(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.name
			) {
			return null;
		}
		
		return this.value.json.name;
	}
	
	protected set Name(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.name = IsNullOrEmpty(val) ? '' : val as string;
		this.SignalChanged();
	}
	
	protected get IsOpen(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isOpen
			) {
			return false;
		}
		
		return this.value.json.isOpen;
	}
	
	protected set IsOpen(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isOpen = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsReOpened(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isReOpened
			) {
			return false;
		}
		
		return this.value.json.isReOpened;
	}
	
	protected set IsReOpened(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isReOpened = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsAssigned(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isAssigned
			) {
			return false;
		}
		
		return this.value.json.isAssigned;
	}
	
	protected set IsAssigned(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isAssigned = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsWaitingOnClient(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isWaitingOnClient
			) {
			return false;
		}
		
		return this.value.json.isWaitingOnClient;
	}
	
	protected set IsWaitingOnClient(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isWaitingOnClient = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsWaitingOnVendor(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isWaitingOnVendor
			) {
			return false;
		}
		
		return this.value.json.isWaitingOnVendor;
	}
	
	protected set IsWaitingOnVendor(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isWaitingOnVendor = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsBillable(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isBillable
			) {
			return false;
		}
		
		return this.value.json.isBillable;
	}
	
	protected set IsBillable(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isBillable = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsBillableReview(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isBillableReview
			) {
			return false;
		}
		
		return this.value.json.isBillableReview;
	}
	
	protected set IsBillableReview(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isBillableReview = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsInProgress(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isInProgress
			) {
			return false;
		}
		
		return this.value.json.isInProgress;
	}
	
	protected set IsInProgress(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isInProgress = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsNonBillable(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isNonBillable
			) {
			return false;
		}
		
		return this.value.json.isNonBillable;
	}
	
	protected set IsNonBillable(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isNonBillable = val ? true : false;
		this.SignalChanged();
	}
	
	protected get IsScheduled(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isScheduled
			) {
			return false;
		}
		
		return this.value.json.isScheduled;
	}
	
	protected set IsScheduled(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isScheduled = val ? true : false;
		this.SignalChanged();
	}
	
	
	
	
	
	
	
	
	
	protected get IsDefault(): boolean | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isDefault
			) {
			return false;
		}
		
		return this.value.json.isDefault;
	}
	
	protected set IsDefault(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isDefault = val ? true : false;
		this.SignalChanged();
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected SignalChanged(): void {
		
		// Debounce
		
		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}
		
		this.debounceId = setTimeout(() => {
			this.$emit('input', this.value);
		}, 250);
	}
}

</script>
<style scoped>

</style>