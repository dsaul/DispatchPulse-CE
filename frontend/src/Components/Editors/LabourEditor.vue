<template>
	<div>

		<v-app-bar v-if="showAppBar" color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="isLoadingData" :indeterminate="true" absolute top color="white">
			</v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">Agent<span v-if="Name">: {{ Name }}</span></v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn dark icon v-on="on">
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
				<v-tabs v-model="tab" background-color="transparent" align-with-title show-arrows>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab :disabled="!value"
						@click="$router.replace({ query: { ...$route.query, tab: 'General' } }).catch(((e) => { }));">
						General
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
						<div class="title">Labour Entry Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The labour entry no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the labour entry while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
			<v-tabs v-if="!showAppBar" v-model="tab" background-color="transparent" grow show-arrows
				style="visibility: none; height:0px;">
				<v-tab>
					General
				</v-tab>
			</v-tabs>

			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>

						<v-form ref="generalForm">
							<v-container>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Who, What, Where</div>
									</v-col>
								</v-row>


								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<AgentSelectField :isDialogue="isDialogue" v-model="AgentId"
											hint="Select the agent this entry applies to."
											:rules="[ValidateRequiredField]"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermCRMLabourManualEntries()" />
									</v-col>
								</v-row>


								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select :items="AllLabourTypes()" label="Labour Type"
											hint="Select the type of labour entry you're making."
											:rules="[ValidateRequiredField]" item-text="json.name" item-value="id"
											persistent-hint v-model="TypeId" class="e2e-labour-editor-labour-type"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermCRMLabourManualEntries()">
											<template v-slot:item="data">
												<v-list flat>
													<v-list-item style="width: 100%;">
														<v-list-item-avatar style="margin-top:0px; margin-bottom: 0px;">
															<v-icon>{{ data.item.json.icon }}</v-icon>
														</v-list-item-avatar>

														<v-list-item-content style="padding: 0px;">
															<v-list-item-title>
																{{ data.item.json.name }}
															</v-list-item-title>
															<v-list-item-subtitle>
																{{ data.item.json.description }}

															</v-list-item-subtitle>
														</v-list-item-content>
													</v-list-item>
												</v-list>
											</template>


										</v-select>
									</v-col>
								</v-row>


								<v-row v-if="TypeIsBillable">

									<v-col cols="12" sm="8" offset-sm="2">
										<ProjectSelectField :disabled="connectionStatus != 'Connected'"
											:isDialogue="isDialogue" v-model="ProjectId"
											hint="Select the project this entry applies to."
											:rules="[ValidateRequiredField]" :readonly="!PermCRMLabourManualEntries()"
											:showDetails="false" />
									</v-col>
								</v-row>
								<v-row v-if="TypeIsNonBillable">

									<v-col cols="12" sm="8" offset-sm="2">
										<ProjectSelectField :disabled="connectionStatus != 'Connected'"
											:isDialogue="isDialogue" v-model="ProjectId"
											hint="Select the project this entry applies to." :rules="[]"
											:readonly="!PermCRMLabourManualEntries()" :showDetails="false" />
									</v-col>
								</v-row>

								<v-row v-if="(TypeIsBillable || TypeIsNonBillable) && ProjectId">

									<v-col cols="12" sm="8" offset-sm="2">
										<AssignmentSelectField :disabled="connectionStatus != 'Connected'"
											:isDialogue="isDialogue" v-model="AssignmentId"
											hint="Select the assignment this entry applies to."
											:showOnlyProjectId="ProjectId" :readonly="!PermCRMLabourManualEntries()" />
									</v-col>
								</v-row>

								<v-row v-if="TypeIsException">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select :disabled="connectionStatus != 'Connected'"
											:items="AllLabourSubtypeExceptionTypes()" label="Exception Type"
											hint="Select the type of exception you're entering."
											:rules="[ValidateRequiredField]" item-text="json.name" item-value="id"
											persistent-hint v-model="ExceptionTypeId"
											:readonly="!PermCRMLabourManualEntries()">
											<template v-slot:item="data">
												<v-list flat>
													<v-list-item style="width: 100%;">
														<v-list-item-avatar style="margin-top:0px; margin-bottom: 0px;">
															<v-icon>{{ data.item.json.icon }}</v-icon>
														</v-list-item-avatar>

														<v-list-item-content style="padding: 0px;">
															<v-list-item-title>
																{{ data.item.json.name }}
															</v-list-item-title>
															<v-list-item-subtitle>
																{{ data.item.json.description }}

															</v-list-item-subtitle>
														</v-list-item-content>
													</v-list-item>
												</v-list>
											</template>


										</v-select>
									</v-col>
								</v-row>

								<v-row v-if="TypeIsHoliday">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select :disabled="connectionStatus != 'Connected'"
											:items="AllLabourSubtypeHolidaysTypes()" label="Holiday Type"
											hint="Select the type of holiday you're entering."
											:rules="[ValidateRequiredField]" item-text="json.name" item-value="id"
											persistent-hint v-model="HolidayTypeId"
											:readonly="!PermCRMLabourManualEntries()">
											<template v-slot:item="data">
												<v-list flat>
													<v-list-item style="width: 100%;">
														<v-list-item-avatar style="margin-top:0px; margin-bottom: 0px;">
															<v-icon>{{ data.item.json.icon }}</v-icon>
														</v-list-item-avatar>

														<v-list-item-content style="padding: 0px;">
															<v-list-item-title>
																{{ data.item.json.name }}
															</v-list-item-title>
															<v-list-item-subtitle>
																{{ data.item.json.description }}

															</v-list-item-subtitle>
														</v-list-item-content>
													</v-list-item>
												</v-list>
											</template>


										</v-select>
									</v-col>
								</v-row>

								<v-row v-if="TypeIsNonBillable">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select :disabled="connectionStatus != 'Connected'"
											:items="AllLabourSubtypeNonBillableTypes()" label="Non Billable Type"
											hint="Select the type of non billable entry you're making."
											:rules="[ValidateRequiredField]" item-text="json.name" item-value="id"
											persistent-hint v-model="NonBillableTypeId"
											:readonly="!PermCRMLabourManualEntries()">
											<template v-slot:item="data">
												<v-list flat>
													<v-list-item style="width: 100%;">
														<v-list-item-avatar style="margin-top:0px; margin-bottom: 0px;">
															<v-icon>{{ data.item.json.icon }}</v-icon>
														</v-list-item-avatar>

														<v-list-item-content style="padding: 0px;">
															<v-list-item-title>
																{{ data.item.json.name }}
															</v-list-item-title>
															<v-list-item-subtitle>
																{{ data.item.json.description }}

															</v-list-item-subtitle>
														</v-list-item-content>
													</v-list-item>
												</v-list>
											</template>


										</v-select>
									</v-col>
								</v-row>














								<v-row v-if="TypeIsBillable || TypeIsNonBillable">

									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Location Type</div>
									</v-col>

								</v-row>

								<v-row v-if="TypeIsBillable || TypeIsNonBillable">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-radio-group v-model="LocationType" style="margin-top: 0px;"
											:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
											<v-radio label="None" value="none" color="primary"
												class="e2e-location-type-none">
											</v-radio>
											<v-radio label="Travel" value="travel" color="primary"
												class="e2e-location-type-travel">
											</v-radio>
											<v-radio label="On Site" value="on-site" color="primary"
												class="e2e-location-type-on-site">
											</v-radio>
											<v-radio label="Remote" value="remote" color="primary"
												class="e2e-location-type-remote">
											</v-radio>
										</v-radio-group>
									</v-col>
								</v-row>

								<div v-if="TypeIsBillable || TypeIsHoliday || TypeIsNonBillable || TypeIsException">
									<v-row>
										<v-col cols="12" sm="8" offset-sm="2">
											<div class="title">When</div>
										</v-col>
									</v-row>

									<v-row>
										<v-col cols="12" sm="8" offset-sm="2"
											style="padding-top: 0px; padding-bottom: 0px;">
											<v-radio-group v-model="TimeMode" style="margin-top: 0px;"
												:rules="[ValidateRequiredField]"
												:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
												<v-radio label="No Time" value="none" color="primary"
													class="e2e-time-mode-no-time">
												</v-radio>
												<v-radio label="Date &amp; Hours" value="date-and-hours" color="primary"
													class="e2e-time-mode-date-hours">
												</v-radio>
												<v-radio label="Start and Stop Time" value="start-stop-timestamp"
													color="primary" class="e2e-time-mode-start-stop-timestamp">
												</v-radio>
											</v-radio-group>
										</v-col>
									</v-row>

									<v-row v-if="TimeMode == 'date-and-hours'">

										<v-col cols="12" sm="8" offset-sm="2">
											<p>
												<v-input v-model="StartDateLocal" :rules="[ValidateRequiredField]"
													:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
													<v-date-picker v-model="StartDateLocal" value="ISO 8601"
														:elevation="1" class="e2e-date-and-hours-date-picker"
														:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
													</v-date-picker>
												</v-input>



											</p>

											<p style="margin-top: 40px;">
												<v-slider v-model="Hours" :max="24" :min="0" step="0.5" label="Hours"
													thumb-label ticks :rules="[ValidateRequiredField]"
													:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
													<template v-slot:append>
														<v-text-field v-model="Hours"
															class="mt-0 pt-0 e2e-hours-text-field" hide-details
															single-line type="number" step="0.5" min="0" max="24"
															style="width: 60px"
															:disabled="connectionStatus != 'Connected'">
														</v-text-field>
													</template>
												</v-slider>
											</p>
										</v-col>
									</v-row>

									<v-row v-if="TimeMode == 'start-stop-timestamp'">
										<v-col cols="12" sm="8" offset-sm="2">
											<div class="subtitle-1" style="font-weight: bold;">Start</div>
										</v-col>
									</v-row>
									<v-row v-if="TimeMode == 'start-stop-timestamp'">
										<v-col cols="12" sm="8" offset-sm="2">
											<v-date-picker :full-width="false" v-model="StartDateLocal"
												style="margin: 5px;" value="ISO 8601" :elevation="1"
												:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
											</v-date-picker>

											<v-time-picker :full-width="false" :ampm-in-title="true"
												:allowed-minutes="TimePicker5MinuteStep" style="margin: 5px;"
												v-model="StartTimeLocal" :elevation="1"
												:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
											</v-time-picker>
										</v-col>
									</v-row>
									<v-row v-if="TimeMode == 'start-stop-timestamp'">
										<v-col cols="12" sm="8" offset-sm="2">
											<div class="subtitle-1" style="font-weight: bold;">End</div>
										</v-col>
									</v-row>
									<v-row v-if="TimeMode == 'start-stop-timestamp'">
										<v-col cols="12" sm="8" offset-sm="2">
											<v-date-picker :full-width="false" v-model="EndDateLocal"
												style="margin: 5px;" value="ISO 8601" :elevation="1"
												:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
											</v-date-picker>

											<v-time-picker :full-width="false" :ampm-in-title="true"
												:allowed-minutes="TimePicker5MinuteStep" style="margin: 5px;"
												v-model="EndTimeLocal" :elevation="1"
												:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
											</v-time-picker>
										</v-col>
									</v-row>

								</div> <!-- /TypeIsBillable || TypeIsHoliday || TypeIsNonBillable || TypeIsException -->

								<v-row v-if="TypeIsPayOutBanked">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="BankedPayOutAmount" label="Pay Out Amount" prefix="$"
											type="number" step=".01"
											hint="Enter the amount of banked money you wish to withdraw."
											:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row
									v-if="TypeIsBillable || TypeIsHoliday || TypeIsNonBillable || TypeIsException || TypeIsPayOutBanked">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title" style="font-weight: bold;">Other</div>
									</v-col>
								</v-row>
								<v-row v-if="TypeIsBillable">
									<v-col cols="12" sm="8" offset-sm="2"
										style="padding-top: 0px; padding-bottom: 0px;">
										<v-switch v-model="IsExtra" label="Extra (Not Included in Contract)"
											style="margin-top:0px;"
											:disabled="LabourForcedExtra || connectionStatus != 'Connected' || !PermCRMLabourManualEntries()"
											:hint="LabourForcedExtra ? 'Extra is locked on due to project settings.' : null"
											:persistent-hint="LabourForcedExtra" class="e2e-is-extra">
										</v-switch>
										<v-switch v-model="IsBilled" label="Billed (To Client)" style="margin-top:0px;"
											:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
										</v-switch>
									</v-col>
								</v-row>
								<v-row
									v-if="TypeIsBillable || TypeIsHoliday || TypeIsNonBillable || TypeIsException || TypeIsPayOutBanked">
									<v-col cols="12" sm="8" offset-sm="2"
										style="padding-top: 0px; padding-bottom: 0px;">
										<v-switch v-model="IsPaidOut" label="Paid Out (To Agent)"
											style="margin-top:0px;"
											:disabled="connectionStatus != 'Connected' || !PermCRMLabourManualEntries()">
										</v-switch>
									</v-col>
								</v-row>
								<v-row
									v-if="TypeIsBillable || TypeIsHoliday || TypeIsNonBillable || TypeIsException || TypeIsPayOutBanked">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-textarea v-model="Notes" label="Notes" hint="Other notes about this entry."
											class="e2e-notes" :disabled="connectionStatus != 'Connected'">
										</v-textarea>
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
											hint="The id of this labour entry.">
										</v-text-field>
									</v-col>
								</v-row>


							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
			</v-tabs-items>
		</div>

		<v-footer v-if="showFooter" color="#747389" class="white--text" app inset>
			<v-btn :disabled="!value || connectionStatus != 'Connected'" color="white" text rounded @click="DialoguesOpen({
			name: 'DeleteLabourDialogue', state: {
				redirectToIndex: true,
				id: value.id,
			}
		})">
				<v-icon left>delete</v-icon>
				Delete
			</v-btn>

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
import AssignmentSelectField from '@/Components/Fields/AssignmentSelectField.vue';
import Clamp from '@/Utility/Clamp';
import { DateTime } from 'luxon';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import TimePicker5MinuteStep from '@/Utility/TimePicker5MinuteStep';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
import { LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
import { LabourSubtypeHoliday } from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { LabourType } from '@/Data/CRM/LabourType/LabourType';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { Project } from '@/Data/CRM/Project/Project';
import Dialogues from '@/Utility/Dialogues';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		LabourList,
		AssignmentsList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		AddMenuButton,
		AssignmentSelectField,
		ReloadButton,
		NotificationBellButton,
	},

})
export default class LabourEditor extends EditorBase {

	//this.$emit('input', null);
	@Prop({ default: null }) declare public readonly value: ILabour | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	@Prop({ default: true }) public readonly hideTabs!: boolean;

	public $refs!: {
		generalForm: Vue,
	};

	protected ValidateRequiredField = ValidateRequiredField;
	protected TimePicker5MinuteStep = TimePicker5MinuteStep;
	protected AllLabourTypes = LabourType.All;
	protected AllLabourSubtypeExceptionTypes = LabourSubtypeException.All;
	protected AllLabourSubtypeNonBillableTypes = LabourSubtypeNonBillable.All;
	protected AllLabourSubtypeHolidaysTypes = LabourSubtypeHoliday.All;
	protected DialoguesOpen = Dialogues.Open;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;
	protected debounceId: ReturnType<typeof setTimeout> | null = null;


	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
		];
	}

	public ValidateFields(): void {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		// Make sure the assignment is the same project when changing the project.

		if (this.value.json.assignmentId) {
			const assignment = Assignment.ForId(this.value.json.assignmentId);
			if (assignment
				&& assignment.json.projectId !== this.value.json.projectId
			) {
				// If it doesn't match, remove it.
				this.value.json.assignmentId = null;
			}
		}

		// If the project that is selected is extra labour only, then change the labour to be extra.

		const project = Project.ForId(this.value.json.projectId);
		if (project) {
			if (project.json.forceLabourAsExtra) {
				this.IsExtra = true;
			}
		}
	}


	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			Agenda: 0,
			agenda: 0,
			Labour: 1,
			labour: 1,
			Admin: 2,
			admin: 2,
		};
	}

	protected get Id(): guid | null {
		if (!this.value ||
			!this.value.id
		) {
			return null;
		}

		return this.value.id;
	}


	protected get TypeId(): guid | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.typeId
		) {
			return null;
		}

		return this.value.json.typeId;
	}

	protected set TypeId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.typeId = IsNullOrEmpty(val) ? null : val;

		this.ValidateFields();

		this.SignalChanged();
	}

	protected get ExceptionTypeId(): guid | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.exceptionTypeId
		) {
			return null;
		}

		return this.value.json.exceptionTypeId;
	}

	protected set ExceptionTypeId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.exceptionTypeId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get HolidayTypeId(): guid | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.holidayTypeId
		) {
			return null;
		}

		return this.value.json.holidayTypeId;
	}

	protected set HolidayTypeId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.holidayTypeId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get NonBillableTypeId(): guid | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.nonBillableTypeId
		) {
			return null;
		}

		return this.value.json.nonBillableTypeId;
	}

	protected set NonBillableTypeId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.nonBillableTypeId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}


	protected get Hours(): number {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.hours
		) {
			return 0;
		}

		return this.value.json.hours;
	}

	protected set Hours(val: number) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		if (!val) {
			val = 0;
		}

		val = Clamp(val, 0, 24);

		//console.log('clamp val', val);

		this.value.json.hours = val;

		this.SignalChanged();
	}



	protected get TypeIsBillable(): boolean {

		const typeId = this.TypeId;
		if (!typeId) {
			return false;
		}

		const type = LabourType.ForId(typeId);
		if (!type) {
			return false;
		}

		return type.json.isBillable;
	}

	protected get TypeIsHoliday(): boolean {

		const typeId = this.TypeId;
		if (!typeId) {
			return false;
		}

		const type = LabourType.ForId(typeId);
		if (!type) {
			return false;
		}

		return type.json.isHoliday;
	}

	protected get TypeIsNonBillable(): boolean {

		const typeId = this.TypeId;
		if (!typeId) {
			return false;
		}

		const type = LabourType.ForId(typeId);
		if (!type) {
			return false;
		}

		return type.json.isNonBillable;
	}

	protected get TypeIsException(): boolean {

		const typeId = this.TypeId;
		if (!typeId) {
			return false;
		}

		const type = LabourType.ForId(typeId);
		if (!type) {
			return false;
		}

		return type.json.isException;
	}

	protected get TypeIsPayOutBanked(): boolean {

		const typeId = this.TypeId;
		if (!typeId) {
			return false;
		}

		const type = LabourType.ForId(typeId);
		if (!type) {
			return false;
		}

		return type.json.isPayOutBanked;
	}

	protected get TimeMode(): 'none' | 'date-and-hours' | 'start-stop-timestamp' | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.timeMode
		) {
			return null;
		}

		return this.value.json.timeMode;
	}

	protected set TimeMode(val: 'none' | 'date-and-hours' | 'start-stop-timestamp' | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.timeMode = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}


	protected get LocationType(): 'none' | 'travel' | 'on-site' | 'remote' | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.locationType
		) {
			return null;
		}

		return this.value.json.locationType;
	}

	protected set LocationType(val: 'none' | 'travel' | 'on-site' | 'remote' | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.locationType = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}



	protected get StartDateLocal(): string | null {

		if (!this.value) {
			return null;
		}

		const json = this.value.json;
		if (!json) {
			return null;
		}

		const iso8601 = json.startISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}

		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}

		const local = utc.toLocal();
		if (!local) {
			return null;
		}

		return local.toFormat('yyyy-MM-dd');
	}



	protected set StartDateLocal(val: string | null) {



		if (!this.value) {
			return;
		}

		let validatedVar = null;

		do {
			if (!val ||
				!this.value) {
				validatedVar = null;
				break;
			}


			const dbISO8601 = this.value.json.startISO8601;

			let dbUtc = null;

			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}

			const dbLocal = dbUtc.toLocal();

			const valDateLocal = DateTime.fromFormat(val, 'yyyy-MM-dd');
			const dbLocalMod = dbLocal.set({
				year: valDateLocal.year,
				month: valDateLocal.month,
				day: valDateLocal.day,
			});

			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();

		} while (false);

		//console.log('set StartDateLocal', val, validatedVar);


		this.value.json.startISO8601 = validatedVar;
		this.SignalChanged();



	}


	protected get StartTimeLocal(): string | null {

		if (!this.value) {
			return null;
		}

		const json = this.value.json;
		if (!json) {
			return null;
		}

		const iso8601 = json.startISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}

		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}

		const local = utc.toLocal();
		if (!local) {
			return null;
		}

		return local.toFormat('HH:mm');
	}

	protected set StartTimeLocal(val: string | null) {

		console.log('set StartTimeLocal', val);

		if (!this.value) {
			return;
		}

		let validatedVar = null;

		do {
			if (!val ||
				!this.value
			) {
				validatedVar = null;
				break;
			}


			const dbISO8601 = this.value.json.startISO8601;

			let dbUtc = null;

			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}

			const dbLocal = dbUtc.toLocal();

			const valDateLocal = DateTime.fromFormat(val, 'HH:mm');
			const dbLocalMod = dbLocal.set({
				hour: valDateLocal.hour,
				minute: valDateLocal.minute,
			});

			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();

		} while (false);




		this.value.json.startISO8601 = validatedVar;
		this.SignalChanged();


	}

	protected get EndDateLocal(): string | null {

		if (!this.value) {
			return null;
		}

		const json = this.value.json;
		if (!json) {
			return null;
		}

		const iso8601 = json.endISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}

		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}

		const local = utc.toLocal();
		if (!local) {
			return null;
		}

		return local.toFormat('yyyy-MM-dd');
	}



	protected set EndDateLocal(val: string | null) {



		if (!this.value) {
			return;
		}

		let validatedVar = null;

		do {
			if (!val ||
				!this.value) {
				validatedVar = null;
				break;
			}


			const dbISO8601 = this.value.json.endISO8601;

			let dbUtc = null;

			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}

			const dbLocal = dbUtc.toLocal();

			const valDateLocal = DateTime.fromFormat(val, 'yyyy-MM-dd');
			const dbLocalMod = dbLocal.set({
				year: valDateLocal.year,
				month: valDateLocal.month,
				day: valDateLocal.day,
			});

			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();

		} while (false);

		//console.log('set EndDateLocal', val, validatedVar);


		this.value.json.endISO8601 = validatedVar;
		this.SignalChanged();



	}


	protected get EndTimeLocal(): string | null {

		if (!this.value) {
			return null;
		}

		const json = this.value.json;
		if (!json) {
			return null;
		}

		const iso8601 = json.endISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}

		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}

		const local = utc.toLocal();
		if (!local) {
			return null;
		}

		return local.toFormat('HH:mm');
	}

	protected set EndTimeLocal(val: string | null) {

		console.log('set EndTimeLocal', val);

		if (!this.value) {
			return;
		}

		let validatedVar = null;

		do {
			if (!val ||
				!this.value
			) {
				validatedVar = null;
				break;
			}


			const dbISO8601 = this.value.json.endISO8601;

			let dbUtc = null;

			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}

			const dbLocal = dbUtc.toLocal();

			const valDateLocal = DateTime.fromFormat(val, 'HH:mm');
			const dbLocalMod = dbLocal.set({
				hour: valDateLocal.hour,
				minute: valDateLocal.minute,
			});

			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();

		} while (false);




		this.value.json.endISO8601 = validatedVar;
		this.SignalChanged();


	}




















	protected get BankedPayOutAmount(): number | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.bankedPayOutAmount
		) {
			return null;
		}

		return this.value.json.bankedPayOutAmount;
	}

	protected set BankedPayOutAmount(val: number | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.bankedPayOutAmount = val;
		this.SignalChanged();
	}


	protected get AgentId(): guid | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.agentId
		) {
			return null;
		}

		return this.value.json.agentId;
	}

	protected set AgentId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.agentId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get IsExtra(): boolean {

		// We need to check this here because of new labour entries.
		if (this.LabourForcedExtra) {
			return true;
		}

		return Labour.IsExtraForId(this.value?.id || null);
	}

	protected set IsExtra(val: boolean) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.isExtra = val;
		this.SignalChanged();
	}

	protected get IsBilled(): boolean {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isBilled
		) {
			return false;
		}

		return this.value.json.isBilled;
	}

	protected set IsBilled(val: boolean) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.isBilled = val;
		this.SignalChanged();
	}

	protected get IsPaidOut(): boolean {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isPaidOut
		) {
			return false;
		}

		return this.value.json.isPaidOut;
	}

	protected set IsPaidOut(val: boolean) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.isPaidOut = val;
		this.SignalChanged();
	}

	protected get Notes(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.notes
		) {
			return null;
		}

		return this.value.json.notes;
	}

	protected set Notes(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.notes = val;
		this.SignalChanged();
	}

	protected get ProjectId(): guid | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.projectId
		) {
			return null;
		}

		return this.value.json.projectId;
	}

	protected set ProjectId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.projectId = IsNullOrEmpty(val) ? null : val;

		if (!val) {
			this.SignalChanged();
			return;
		}



		this.ValidateFields();



		this.SignalChanged();
	}

	protected get LabourForcedExtra(): boolean {

		const projectId = this.ProjectId;
		if (!projectId) {
			return false;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return false;
		}

		return project.json.forceLabourAsExtra || false;
	}



	protected get AssignmentId(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.assignmentId
		) {
			return null;
		}

		return this.value.json.assignmentId;
	}

	protected set AssignmentId(val: string | null) {
		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.assignmentId = IsNullOrEmpty(val) ? null : val;
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
<style>
div.v-picker--date div.v-picker__title {

	padding-bottom: 30px;
	margin-bottom: 2px;
}
</style>