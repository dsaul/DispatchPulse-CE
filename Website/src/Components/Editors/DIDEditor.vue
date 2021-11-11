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
				>
			</v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Phone Number: {{DIDNumber}}</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item
					@click="OnlineHelpFiles()"
					>
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>DID Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</HelpMenuButton>
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
						<div class="title">DID Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The calendar no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the calendar while you were opening it.</li>
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
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<!-- https://duckduckgo.com/ac/?callback=autocompleteCallback&q=a&kl=wt-wt&_=1577800653510 -->
										<v-text-field
											v-model="DIDNumber"
											autocomplete="newpassword"
											label="Number"
											id="Number"
											hint="The phone number, this cannot be changed after it is added."
											persistent-hint
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-calendar-editor-name"
											:disabled="connectionStatus != 'Connected' || !PermDIDsCanPush()"
											:readonly="false == isMakingNew || !PermDIDsCanPush()"
											>
										</v-text-field>
										<v-alert
											v-if="isMakingNew"
											border="left"
											colored-border
											type="info"
											elevation="2"
											style="margin-top: 10px;"
											
											>
											<div>This is the number that as it appears to the system when a call comes in, or shows up on caller id.</div>
											<div>If you're unsure what to put here, enter a number formatted like 12045554444 and you should be good. Don't put letters, spaces, or any punctuation.</div>
										</v-alert>
									</v-col>
								</v-row>
								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Registration Status</div>
									</v-col>
								</v-row>
								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
										<p>This number is currently <DIDRegisteredChip ref="regStatusChip" :DIDNumber="value" /> on the phone system.</p>
										<p>
											<RegisterDIDButton
												:disabled="connectionStatus != 'Connected' || !PermDIDsCanPush()"
												@reload-status="OnReloadStatus"
												:DIDNumber="value"
												:didRegisterPasscode="didRegisterPasscode"
												/>
											<DeRegisterDIDButton
												:disabled="connectionStatus != 'Connected' || !PermDIDsCanPush()"
												@reload-status="OnReloadStatus"
												:DIDNumber="value"
												/>
										</p>
										<p>
											<v-text-field
												label="Phone Number Register Passcode"
												v-model="didRegisterPasscode"
												type="password"
												hint="To get this register passcode, call +1 (204) 817-3921 and press 1, the system will read out a passcode for you. This confirms that you control this number. Once registered, it cannot be used by anyone else until this number is deregistered."
												persistent-hint
												>
											</v-text-field>
										</p>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Inbound Assignment</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<p>What should happen when someone calls this number and it reaches our system?</p>
										<v-radio-group
											v-model="AssignToType"
											:disabled="connectionStatus != 'Connected' || !PermDIDsCanPush()"
											>
											<v-radio
												key="Hangup"
												label="Hangup"
												value="Hangup"
												>
											</v-radio>
											<v-radio
												key="OnCallAutoAttendant"
												label="On-Call Responder"
												value="OnCallAutoAttendant"
												>
											</v-radio>
										</v-radio-group>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">On-Call Responder</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<p>If a number with this caller id calls one of our on-call responder access numbers, which responder should handle the call?</p>
										<OnCallAutoAttendantSelectField 
											v-model="AssignToID"
											:readonly="connectionStatus != 'Connected' || !PermDIDsCanPush()"
											/>
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
											hint="The id of this did."
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
		
		<DeleteDIDDialogue 
			v-model="deleteDIDModel"
			:isOpen="deleteDIDOpen"
			@Delete="SaveDeleteDID"
			@Cancel="CancelDeleteDID"
			ref="deleteDIDDialogue"
			/>
		
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
					:disabled="!value || connectionStatus != 'Connected' || !PermDIDsCanDelete()"
					color="white"
					text
					rounded
					@click="OpenDeleteDID()"
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
import _ from 'lodash';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import DIDRegisteredChip from '@/Components/Chips/DIDRegisteredChip.vue';
import DeRegisterDIDButton from '@/Components/Buttons/DeRegisterDIDButton.vue';
import RegisterDIDButton from '@/Components/Buttons/RegisterDIDButton.vue';
import OnCallAutoAttendantSelectField from '@/Components/Fields/OnCallAutoAttendantSelectField.vue';
import DeleteDIDDialogue from '@/Components/Dialogues2/DIDs/DeleteDIDDialogue.vue';

@Component({
	components: {
		ProjectList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		DIDRegisteredChip,
		DeRegisterDIDButton,
		RegisterDIDButton,
		OnCallAutoAttendantSelectField,
		DeleteDIDDialogue,
	},
	
})
export default class DIDEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IDID | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		regStatusChip: DIDRegisteredChip,
		generalForm: Vue,
		deleteDIDDialogue: DeleteDIDDialogue,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected PermDIDsCanDelete = DID.PermDIDsCanDelete;
	protected PermDIDsCanPush = DID.PermDIDsCanPush;
	
	protected deleteDIDModel: IDID | null = null;
	protected deleteDIDOpen = false;
	
	protected didRegisterPasscode = '';
	
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	constructor() {
		super();
		
	}
	
	
	
	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
		];
	}
	
	protected OnReloadStatus(): void {
		if (this.$refs.regStatusChip) {
			this.$refs.regStatusChip.LoadData();
		}
		
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			General: 0,
			general: 0,
			Projects: 1,
			projects: 1,
		};
	}
	
	protected get AssignToType(): 'Hangup' | 'OnCallAutoAttendant' {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.assignToType) {
			return 'Hangup';
		}
		
		return this.value.json.assignToType;
	}
	
	protected set AssignToType(val: 'Hangup' | 'OnCallAutoAttendant') {
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IDID;
		
		clone.json.assignToType = val;
		
		this.$emit('input', clone);
	}
	
	protected get AssignToID(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.assignToID ||
			IsNullOrEmpty(this.value.json.assignToID)
			) {
			return null;
		}
		
		return this.value.json.assignToID;
	}
	
	protected set AssignToID(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.assignToID = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get DIDNumber(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.DIDNumber
			) {
			return null;
		}
		
		return this.value.json.DIDNumber;
	}
	
	protected set DIDNumber(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.DIDNumber = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	protected OpenDeleteDID(): void {
		
		this.deleteDIDModel = this.value;
		this.deleteDIDOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.deleteDIDDialogue) {
				this.$refs.deleteDIDDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelDeleteDID(): void {
		
		this.deleteDIDOpen = false;
		
	}
	
	protected SaveDeleteDID(): void {
		
		
		if (!this.deleteDIDModel || !this.deleteDIDModel.id || IsNullOrEmpty(this.deleteDIDModel.id)) {
			console.error('!this.deleteDIDModel || !this.deleteDIDModel.id || IsNullOrEmpty(this.deleteDIDModel.id)');
			return;
		}
		
		const payload: string[] = [this.deleteDIDModel.id];
		DID.DeleteIds(payload);
		
		this.deleteDIDOpen = false;
		this.$router.push(`/section/dids/index`).catch(((e: Error) => { }));// eslint-disable-line
		
		
		
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
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}
	
}

</script>
<style scoped>

</style>