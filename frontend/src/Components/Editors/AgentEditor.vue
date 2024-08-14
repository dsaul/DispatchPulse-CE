<template>
	<div>

		<v-app-bar v-if="showAppBar" color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="isLoadingData" :indeterminate="true" absolute top
				color="white"></v-progress-linear>

			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">Agent<span v-if="Name">: {{ Name }}</span></v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item @click="OnlineHelpFiles()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Agent Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn dark icon v-on="on">
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
					<v-list-item @click="DoPrint()"
						:disabled="connectionStatus != 'Connected' || !PermCRMReportLabourPDF()">
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report Labour&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item @click="CSVDownloadAgent(value)" :disabled="!PermCRMExportAgentsCSV()">
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>


			<template v-slot:extension>
				<v-tabs v-model="tab" background-color="transparent" align-with-title show-arrows>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab :disabled="!value"
						@click="$router.replace({ query: { ...$route.query, tab: 'Agenda' } }).catch(((e) => { }));">
						Agenda
					</v-tab>
					<v-tab :disabled="!value"
						@click="$router.replace({ query: { ...$route.query, tab: 'Projects' } }).catch(((e) => { }));">
						Projects
					</v-tab>
					<v-tab :disabled="!value"
						@click="$router.replace({ query: { ...$route.query, tab: 'Labour' } }).catch(((e) => { }));">
						Labour
					</v-tab>
					<v-tab :disabled="!value"
						@click="$router.replace({ query: { ...$route.query, tab: 'Admin' } }).catch(((e) => { }));">
						Admin
					</v-tab>
				</v-tabs>
			</template>

		</v-app-bar>

		<v-breadcrumbs v-if="breadcrumbs" :items="breadcrumbs"
			style="background: white; padding-bottom: 5px; padding-top: 15px;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>

		<v-alert v-if="connectionStatus != 'Connected'" type="error" elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;">
			Disconnected from server.
		</v-alert>

		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Agent Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The agent no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the agent while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
			<v-tabs v-if="!showAppBar" v-model="tab" background-color="transparent" grow show-arrows>
				<v-tab :disabled="isMakingNew">
					Agenda
				</v-tab>
				<v-tab :disabled="isMakingNew">
					Projects
				</v-tab>
				<v-tab :disabled="isMakingNew">
					Labour
				</v-tab>
				<v-tab>
					Admin
				</v-tab>
			</v-tabs>

			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<AssignmentsList :showOnlyAgentId="$route.params.id" :focusIsProject="true" :isDialogue="isDialogue"
						:disabled="connectionStatus != 'Connected'" />
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<ProjectList :showOnlyAgentId="$route.params.id" :isDialogue="isDialogue"
						:disabled="connectionStatus != 'Connected'" />
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<LabourList :showOnlyAgentId="$route.params.id" :focusIsProject="true" :openEntryOnClick="false"
						:isReverseSort="true" :isDialogue="isDialogue" ref="labourList"
						:disabled="connectionStatus != 'Connected'" />
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<v-card flat>

						<v-form ref="adminForm">
							<v-container>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">General</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="Name" autocomplete="newpassword" label="Name" required
											:rules="[
			ValidateNameRequired,
			ValidateGreaterThan2Characters,
		]" hint="The name of this agent" class="e2e-agent-editor-name" :disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="Title" autocomplete="newpassword" label="Title"
											hint="The title of this agent, for example: Electrician" required :rules="[
			OptionalButMustBeMoreThan4Characters
		]" class="e2e-agent-editor-title" :disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Notifications</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="NotificationSMSNumber" autocomplete="newpassword"
											label="SMS/Text Phone Number" hint="The number to send SMSes to." type="tel"
											required :rules="[
			OptionalPhoneNumber
		]" class="e2e-agent-editor-sms-number" :disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>


								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Payroll</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select label="Employment Status" auto-select-first
											hint="Whether this agent is an employee or not."
											:items="EmploymentStatusAll()" item-text="json.name" item-value="id"
											v-model="EmploymentStatusId" class="e2e-agent-editor-employment-status"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-select>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field autocomplete="newpassword" type="number" min="0" max="9999.99"
											step="0.01" label="Hourly Wage" prefix="$" suffix="per hour"
											hint="Enter the wage this agent is paid per hour." v-model="HourlyWage"
											class="e2e-agent-editor-hourly-wage"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Phone Access</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field autocomplete="newpassword" type="text" label="Phone Id"
											hint="Enter an ID that people dial into the phone, you can put in 0-9, no spaces, don't use * or #."
											v-model="PhoneId" :disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field autocomplete="newpassword" type="text" label="Phone Passcode"
											hint="Enter a passcode for the agent to use to log in, you can put in 0-9, no spaces, don't use * or #."
											v-model="PhonePasscode" :disabled="connectionStatus != 'Connected'"
											:readonly="!PermAgentsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>










								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Advanced</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="Id" readonly="readonly" label="Unique ID"
											hint="The id of this agent.">
										</v-text-field>
									</v-col>
								</v-row>



							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
			</v-tabs-items>
		</div>
		<AddAssignmentDialogue2 v-model="addAssignmentModel" :isOpen="addAssignmentOpen" @Save="SaveAddAssignment"
			@Cancel="CancelAddAssignment" ref="addAssignmentDialogue" />
		<v-footer v-if="showFooter" color="#747389" class="white--text" app inset>
			<v-btn :disabled="!value || connectionStatus != 'Connected' || !PermAgentsCanDelete()" color="white" text
				rounded @click="DialoguesOpen({
			name: 'DeleteAgentDialogue', state: {
				redirectToIndex: true,
				id: value.id,
			}
		})">
				<v-icon left>delete</v-icon>
				Delete
			</v-btn>

			<v-spacer />

			<AddMenuButton :disabled="!value || connectionStatus != 'Connected'">
				<v-list-item @click="OpenAddAssignment()"
					:disabled="connectionStatus != 'Connected' || !PermAssignmentCanPush()">
					<v-list-item-icon>
						<v-icon>local_shipping</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Assignment…</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item @click="DialoguesOpen({
			name: 'ModifyLabourDialogue',
			state: {
				json: {
					agentId: $route.params.id
				}
			}
		})" :disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf() || !PermCRMLabourManualEntries()">
					<v-list-item-icon>
						<v-icon>timelapse</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Add Labour…</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</AddMenuButton>
		</v-footer>
	</div>

</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import EditorBase, { IBreadcrumb, VForm } from './EditorBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import AssignmentsList from '@/Components/Lists/AssignmentsList.vue';
import LabourList from '@/Components/Lists/LabourList.vue';
import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import CSVDownloadAgent from '@/Data/CRM/Agent/CSVDownloadAgent';
import { EmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';
import TelephoneFieldMask from '@/Utility/TelephoneFieldMask';
import ValidateGreaterThan2Characters from '@/Utility/Validators/ValidateGreaterThan2Characters';
import ValidateNameRequired from '@/Utility/Validators/ValidateNameRequired';
import OptionalPhoneNumber from '@/Utility/Validators/OptionalPhoneNumber';
import OptionalButMustBeMoreThan4Characters from '@/Utility/Validators/OptionalButMustBeMoreThan4Characters';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import Dialogues from '@/Utility/Dialogues';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import AddAssignmentDialogue2 from '@/Components/Dialogues2/Assignments/AddAssignmentDialogue2.vue';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { DateTime } from 'luxon';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		ReloadButton,
		LabourList,
		AssignmentsList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		AddMenuButton,
		ProjectList,
		AddAssignmentDialogue2,
		NotificationBellButton,
	},

})
export default class AgentEditor extends EditorBase {

	//this.$emit('input', null);
	@Prop({ default: null }) declare public readonly value: IAgent | null;

	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;




	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;

	public $refs!: {
		adminForm: Vue,
		labourList: LabourList,
		addAssignmentDialogue: AddAssignmentDialogue2,
	};

	protected CSVDownloadAgent = CSVDownloadAgent;
	protected EmploymentStatusAll = EmploymentStatus.All;
	protected TelephoneFieldMask = TelephoneFieldMask;
	protected ValidateGreaterThan2Characters = ValidateGreaterThan2Characters;
	protected ValidateNameRequired = ValidateNameRequired;
	protected OptionalPhoneNumber = OptionalPhoneNumber;
	protected OptionalButMustBeMoreThan4Characters = OptionalButMustBeMoreThan4Characters;
	protected DialoguesOpen = Dialogues.Open;
	protected PermLabourCanPushSelf = Labour.PermLabourCanPushSelf;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermAgentsCanDelete = Agent.PermAgentsCanDelete;
	protected PermAgentsCanPush = Agent.PermAgentsCanPush;
	protected PermCRMExportAgentsCSV = Agent.PermCRMExportAgentsCSV;
	protected PermCRMReportLabourPDF = Labour.PermCRMReportLabourPDF;
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	protected loadingData = false;

	protected addAssignmentModel: IAssignment | null = null;
	protected addAssignmentOpen = false;

	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.adminForm as VForm,
		];
	}




	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			Agenda: 0,
			agenda: 0,
			Projects: 1,
			projects: 1,
			Labour: 2,
			labour: 2,
			Admin: 3,
			admin: 3,
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

		this.value.json.name = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get Title(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.title
		) {
			return null;
		}

		return this.value.json.title;
	}

	protected set Title(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.title = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get NotificationSMSNumber(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.notificationSMSNumber
		) {
			return null;
		}

		return this.value.json.notificationSMSNumber;
	}

	protected set NotificationSMSNumber(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.notificationSMSNumber = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}




	protected get HourlyWage(): number | string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.hourlyWage
		) {
			return null;
		}

		return this.value.json.hourlyWage;
	}

	protected set HourlyWage(val: number | string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.hourlyWage = val == null ? null : Number(val);
		this.SignalChanged();
	}


	protected get PhoneId(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.phoneId
		) {
			return null;
		}

		return this.value.json.phoneId;
	}

	protected set PhoneId(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		let mod = val;
		if (mod != null) {
			mod = mod.replace(/\D/g, '');
		}

		this.value.json.phoneId = mod;
		this.SignalChanged();
	}

	protected get PhonePasscode(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.phonePasscode
		) {
			return null;
		}

		return this.value.json.phonePasscode;
	}

	protected set PhonePasscode(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		let mod = val;
		if (mod != null) {
			mod = mod.replace(/\D/g, '');
		}

		this.value.json.phonePasscode = mod;
		this.SignalChanged();
	}



















	protected get EmploymentStatusId(): guid | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.employmentStatusId
		) {
			return null;
		}

		return this.value.json.employmentStatusId;
	}

	protected set EmploymentStatusId(val: guid | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.employmentStatusId = IsNullOrEmpty(val) ? null : val;
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


	protected DoPrint(): void {

		const defaultFilterDateStart: string = DateTime.local().minus({ years: 50 }).toFormat('yyyy-LL-dd');
		const defaultFilterDateEnd: string = DateTime.local().plus({ years: 50 }).toFormat('yyyy-LL-dd');



		Dialogues.Open({
			name: 'LabourReportDialogue',
			state: {
				allLoadedLabour: true,
				specificLabour: [null],
				filterByAgentId: this.value?.id,
				filterDateStart: this.$refs.labourList ? this.$refs.labourList.FilterDateStart : defaultFilterDateStart,
				filterDateEnd: this.$refs.labourList ? this.$refs.labourList.FilterDateEnd : defaultFilterDateEnd,
			},
		});



	}

	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');

		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}


	protected OpenAddAssignment(): void {

		//console.debug('OpenAddAssignment');

		this.addAssignmentModel = Assignment.GetEmpty();
		this.addAssignmentModel.json.agentIds = [this.$route.params.id];
		this.addAssignmentOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.addAssignmentDialogue) {
				this.$refs.addAssignmentDialogue.SwitchToTabFromRoute();
			}
		});

	}

	protected CancelAddAssignment(): void {

		//console.debug('CancelAddAssignment');

		this.addAssignmentOpen = false;

	}

	protected SaveAddAssignment(): void {

		//console.debug('SaveAddAssignment');

		this.addAssignmentOpen = false;

		if (!this.addAssignmentModel ||
			!this.addAssignmentModel.id) {
			return;
		}

		const payload: Record<string, IAssignment> = {};
		payload[this.addAssignmentModel.id] = this.addAssignmentModel;
		Assignment.UpdateIds(payload);

		this.$router.push(`/section/assignments/${this.addAssignmentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line

	}





}

</script>
<style scoped></style>