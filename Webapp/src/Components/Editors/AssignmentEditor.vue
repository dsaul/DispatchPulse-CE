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
			
			<v-toolbar-title class="white--text">Assignment</v-toolbar-title>

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
						<v-list-item-title>Assignment Tutorial Pages</v-list-item-title>
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
					<v-list-item
						@click="DoPrint()"
						:disabled="connectionStatus != 'Connected' || !PermCRMReportAssignmentPDF()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="CSVDownloadAssignments()"
						:disabled="!PermCRMExportAssignmentCSV()"
						>
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
				<v-tabs
				v-model="tab"
				show-arrows
				background-color="transparent"
				align-with-title
				
				>
					<v-tabs-slider color="white"></v-tabs-slider>

						<v-tab
							:disabled="!value"
							@click="$router.replace({query: { ...$route.query, tab: 'General'}}).catch(((e) => {}));"
							
							>
							General
						</v-tab>
						<v-tab
							:disabled="!value"
							@click="$router.replace({query: { ...$route.query, tab: 'Labour'}}).catch(((e) => {}));"
							>
							Labour
						</v-tab>
						<v-tab
							:disabled="!value"
							@click="$router.replace({query: { ...$route.query, tab: 'Schedule'}}).catch(((e) => {}));"
							>
							Schedule
						</v-tab>
						
						
				</v-tabs>
			</template>
			
			
			
		</v-app-bar>
		
		<v-breadcrumbs
			v-if="breadcrumbs"
			:items="breadcrumbs"
			style="background: white; padding-bottom: 5px; padding-top: 15px;"
			>
			
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>
		
		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Assignment Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The assignment no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the assignment while you were opening it.</li>
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
				center-active
				show-arrows
			>
				<v-tab
					class="e2e-assignment-editor-in-page-tab-general"
					>
					General
				</v-tab>
				<v-tab :disabled="isMakingNew">
					Labour
				</v-tab>
				<v-tab
					class="e2e-assignment-editor-in-page-tab-schedule">
					Schedule
				</v-tab>
			</v-tabs>
			
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
					<v-card flat>
						<v-form ref="generalForm">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<ProjectSelectField
											:isDialogue="isDialogue"
											v-model="ProjectId"
											:required="true"
											:rules="[
												ValidateRequiredField
											]"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermAssignmentCanPush()"
											/>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-textarea
											label="Work Requested"
											hint="Requested work for this assignment."
											v-model="WorkRequested"
											rows="1"
											auto-grow
											class="e2e-assignment-editor-work-requested"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermAssignmentCanPush()"
											>
										</v-textarea>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-select
											v-model="StatusId"
											label="Status"
											auto-select-first
											hint="What status this assignment is in."
											:items="StatusItems"
											item-value="id"
											item-text="json.name"
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-assignment-editor-status-id"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermAssignmentCanPush()"
											>
										</v-select>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Notes</div>
									</v-col>
								</v-row>
								
								
								<v-row no-gutters v-if="value">
									<v-col cols="12" sm="8" offset-sm="2">
										<div 
											v-if="NotesOnMainProjectThatAreNotThisAssignment > 0"
											style="font-size: 14px;margin-top: 20px; margin-bottom: 20px;"
											>
											There are {{NotesOnMainProjectThatAreNotThisAssignment}} 
											other notes on the <router-link :to="`/section/projects/${value.json.projectId}?tab=Notes`">main project</router-link>.
										</div>
										<NoteList
											:showOnlyAssignmentId="value.id"
											:showFilter="false"
											:isDialogue="isDialogue"
											:disabled="connectionStatus != 'Connected'"
											emptyMessage="There are no notes for this assignment."
											:dense="true"
											/>
									</v-col>
								</v-row>
								
								<v-row no-gutters>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-card outlined>
											<v-card-title style="padding-bottom: 0px;">Add Quick Note</v-card-title>
											<v-card-text>
												
												<v-textarea
													label=""
													hint="Work performed, things not to forget, etc."
													v-model="addNoteText"
													rows="1"
													auto-grow
													class="e2e-add-quick-note-text-area"
													:disabled="connectionStatus != 'Connected'"
													:readonly="!PermProjectNotesCanPush()"
													>
													<template v-slot:append-outer>
														<v-btn
															@click="OnClickAddNote"
															color="primary"
															text
															class="e2e-add-quick-note-text-area-save"
															:disabled="connectionStatus != 'Connected' || !PermProjectNotesCanPush()"
															>
															<v-icon left>save</v-icon>
															Save
														</v-btn>
													</template>
													
													
												</v-textarea>
												<v-switch
													v-model="addNoteTextInternal"
													label="Internal Only"
													hide-details
													style="margin-top: 0px;"
													dense
													:disabled="connectionStatus != 'Connected' || !PermProjectNotesCanPush()"
													>
												</v-switch>
											</v-card-text>
										</v-card>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Agents</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<AgentSelectFieldArrayAdapter
											:isDialogue="isDialogue"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermAssignmentCanPush()"
											v-model="AgentIds"
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
											hint="The id of this assignment."
											>
										</v-text-field>
									</v-col>
								</v-row>
								
							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
				
				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-container>
							<v-row>
								<v-col cols="12" sm="10" offset-sm="1">
									<v-btn
										color="primary"
										text
										@click="StartTravel"
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf()"
										>
										<v-icon left>commute</v-icon>
										Start Travel
									</v-btn>
									<v-btn
										color="primary"
										text
										@click="StartOnSite"
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf()"
										>
										<v-icon left>build</v-icon>
										Start On-Site
									</v-btn>
									<v-btn
										color="primary"
										text
										@click="StartRemote"
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf()"
										>
										<v-icon left>business</v-icon>
										Start Remote
									</v-btn>
									<v-btn
										color="primary"
										text
										:disabled="connectionStatus != 'Connected' || !PermLabourCanPushSelf() || !PermCRMLabourManualEntries()"
										@click="DialoguesOpen({
											name: 'ModifyLabourDialogue',
											state: {
												json: {
													projectId: ProjectId,
													assignmentId: $route.params.id,
													agentId: AgentLoggedInAgentId(),
													typeId: LabourTypeDefaultBillableTypeId(),
												}
											}
										})"
										>
										<v-icon left>add</v-icon>
										Manual Entry
									</v-btn>
									
								</v-col>
							</v-row>
						</v-container>
						<LabourList
							:showFilter="false"
							:showOnlyAssignmentId="$route.params.id"
							:focusIsAgent="true"
							:openEntryOnClick="false"
							:isDialogue="isDialogue"
							:disabled="connectionStatus != 'Connected'"
							/>
					</v-card>
				</v-tab-item>
				
				
				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-form ref="scheduleForm">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-alert
											v-if="ForceAssignmentsToUseProjectSchedule"
											type="info"
											>
											The schedule of this assignment is managed by the 
											project, go to <router-link :to="`/section/projects/${value.json.projectId}?tab=Schedule`" style="color: white;">the project</router-link> to change it.
										</v-alert>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Start</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle-1" style="font-weight: bold;">Date</div>
										
										
										
										<v-switch
											v-model="HasStartISO8601"
											:label="`Scheduled Start`"
											:disabled="ForceAssignmentsToUseProjectSchedule || connectionStatus != 'Connected' || !PermAssignmentCanPush()"
											>
										</v-switch>
										
										<div v-if="HasStartISO8601">
											<p>
												<v-date-picker
													:disabled="ForceAssignmentsToUseProjectSchedule || connectionStatus != 'Connected' || !PermAssignmentCanPush()"
													v-model="StartDateLocal"
													:elevation="1"
													class="e2e-date-picker-start"
													>
												</v-date-picker>
											</p>
											
											
											<div class="subtitle-1" style="font-weight: bold;">Time</div>
											
											<v-radio-group
												v-model="StartTimeMode"
												:disabled="connectionStatus != 'Connected' || !PermAssignmentCanPush()"
												>
												<v-radio
													key="none"
													label="No Time"
													value="none"
													:disabled="ForceAssignmentsToUseProjectSchedule"
													>
												</v-radio>
												<v-radio
													key="morning-first-thing"
													label="Morning First Thing"
													value="morning-first-thing"
													:disabled="ForceAssignmentsToUseProjectSchedule"
													>
												</v-radio>
												<v-radio
													key="morning-second-thing"
													label="Morning Second Thing"
													value="morning-second-thing"
													:disabled="ForceAssignmentsToUseProjectSchedule"
													>
												</v-radio>
												<v-radio
													key="afternoon-first-thing"
													label="Afternoon First Thing"
													value="afternoon-first-thing"
													:disabled="ForceAssignmentsToUseProjectSchedule"
													>
												</v-radio>
												<v-radio
													key="afternoon-second-thing"
													label="Afternoon Second Thing"
													value="afternoon-second-thing"
													:disabled="ForceAssignmentsToUseProjectSchedule"
													>
												</v-radio>
												<v-radio
													key="time"
													label="Specific Time"
													value="time"
													:disabled="ForceAssignmentsToUseProjectSchedule"
													>
												</v-radio>
											</v-radio-group>
											
											
											
											<p v-if="StartTimeMode == 'time'">
												<v-time-picker
													:ampm-in-title="true"
													:allowed-minutes="TimePicker5MinuteStep"
													v-model="StartTimeLocal"
													:disabled="ForceAssignmentsToUseProjectSchedule || connectionStatus != 'Connected' || !PermAssignmentCanPush()"
													:elevation="1"
													>
												</v-time-picker>
											</p>
										
										</div>
										
										
										
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title" style="font-weight: bold;">End</div>
										
										<div class="subtitle-1" style="font-weight: bold;">Date</div>
										<v-switch
											v-model="HasEndISO8601"
											:label="`Scheduled End`"
											:disabled="ForceAssignmentsToUseProjectSchedule || connectionStatus != 'Connected' || !PermAssignmentCanPush()"
											>
										</v-switch>
										
										<div v-if="HasEndISO8601">
											<p>
												<v-date-picker
													v-model="EndDateLocal"
													:disabled="ForceAssignmentsToUseProjectSchedule || connectionStatus != 'Connected' || !PermAssignmentCanPush()"
													:elevation="1"
													class="e2e-date-picker-end"
													>
												</v-date-picker>
											</p>
											
											<v-radio-group
												v-model="EndTimeMode"
												:disabled="connectionStatus != 'Connected' || !PermAssignmentCanPush()"
												>
												<v-radio
													key="none"
													label="No Time"
													value="none"
													:disabled="ForceAssignmentsToUseProjectSchedule"
												></v-radio>
												<v-radio
													key="time"
													label="Specific Time"
													value="time"
													:disabled="ForceAssignmentsToUseProjectSchedule"
												></v-radio>
											</v-radio-group>
											
											<p v-if="EndTimeMode == 'time'">
												<v-time-picker
													:ampm-in-title="true"
													:allowed-minutes="TimePicker5MinuteStep"
													v-model="EndTimeLocal"
													:disabled="ForceAssignmentsToUseProjectSchedule || !PermAssignmentCanPush()"
													:elevation="1"
													>
												</v-time-picker>
											</p>
										</div>
									</v-col>
								</v-row>
							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
				
				
			</v-tabs-items>
		</div>
		
		<v-snackbar
			v-model="errorAssignmentNullSnackbarVisible"
			color="error"
			:timeout="6000"
			:top="true"
			>
			Can't find the assignment to edit.
			<template v-slot:action="{ attrs }">
				<v-btn v-bind="attrs" dark text @click="errorAssignmentNullSnackbarVisible = false">
					Close
				</v-btn>
			</template>
		</v-snackbar>
		<v-snackbar
			v-model="errorNoBillableLabourTypeVisible"
			color="error"
			:timeout="6000"
			:top="true"
			>
			There is no default billable labour type.
			<template v-slot:action="{ attrs }">
				<v-btn v-bind="attrs" dark text @click="errorNoBillableLabourTypeVisible = false">
					Close
				</v-btn>
			</template>
		</v-snackbar>
		<v-snackbar
			v-model="errorAddNoteTextEmpty"
			color="error"
			:timeout="6000"
			:top="true"
			>
			You must add some text before adding a note.
			<template v-slot:action="{ attrs }">
				<v-btn v-bind="attrs" dark text @click="errorAddNoteTextEmpty = false">
					Close
				</v-btn>
			</template>
		</v-snackbar>
	
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
					:disabled="!value || connectionStatus != 'Connected' || !PermAssignmentCanDelete()"
					color="white"
					text
					rounded
					@click="DialoguesOpen({ name: 'DeleteAssignmentDialogue', state: {
						redirectToIndex: true,
						id: value.id,
					}})"
					
					>
					<v-icon left>delete</v-icon>
					Delete
				</v-btn>
				
				<v-spacer />
				
				<v-btn
					:disabled="!value || connectionStatus != 'Connected' || !PermAssignmentCanPush()"
					color="white"
					text
					rounded
					@click="DialoguesOpen({
						name: 'CompleteAssignmentDialogue', 
						state: {
							assignment: value
						}
					})"
					
					>
					<v-icon left>check</v-icon>
					Completed
				</v-btn>
				
				<AddMenuButton
					:disabled="!value || connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="DialoguesOpen({
							name: 'ModifyLabourDialogue',
							state: {
								json: {
									projectId: ProjectId,
									assignmentId: $route.params.id,
									agentId: AgentLoggedInAgentId(),
									typeId: LabourTypeDefaultBillableTypeId(),
								}
							}
						})"
						:disabled="connectionStatus != 'Connected' || !PermLabourCanPush() || !PermCRMLabourManualEntries()"
						>
						<v-list-item-icon>
							<v-icon>timelapse</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Add Labour…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="DialoguesOpen({ 
							name: 'AddProjectNoteDialogue', 
							state: { json: { 
								assignmentId: $route.params.id,
								projectId: value.json.projectId,
								}} 
							})"
						:disabled="connectionStatus != 'Connected' || !PermProjectNotesCanPush()"
						>
						<v-list-item-icon>
							<v-icon>note_add</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Add Note…</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
				
				
			</v-row>
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
import _ from 'lodash';
import { DateTime } from 'luxon';
import AgentSelectField from '@/Components/Fields/AgentSelectField.vue';
import { Labour } from '@/Data/CRM/Labour/Labour';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import TimePicker5MinuteStep from '@/Utility/TimePicker5MinuteStep';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { Project } from '@/Data/CRM/Project/Project';
import AgentSelectFieldArrayAdapter from '@/Components/Fields/AgentSelectFieldArrayAdapter.vue';
import CSVDownloadAssignments from '@/Data/CRM/Assignment/CSVDownloadAssignments';
import NoteList from '@/Components/Lists/NoteList.vue';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { ProjectNoteStyledText } from '@/Data/CRM/ProjectNoteStyledText/ProjectNoteStyledText';
import { escape } from 'html-escaper';
import Dialogues from '@/Utility/Dialogues';
import { LabourType } from '@/Data/CRM/LabourType/LabourType';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import { IProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';

@Component({
	components: {
		LabourList,
		AssignmentsList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		AddMenuButton,
		AgentSelectField,
		AgentSelectFieldArrayAdapter,
		NoteList,
		ReloadButton,
		NotificationBellButton,
	},
})
export default class AssignmentEditor extends EditorBase {
	
	@Prop({ default: null }) declare public readonly value: IAssignment | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
		labourForm: Vue,
		scheduleForm: Vue,
	};
	
	protected LabourTypeDefaultBillableTypeId = LabourType.DefaultBillableTypeId;
	protected ValidateRequiredField = ValidateRequiredField;
	protected AgentLoggedInAgentId = Agent.LoggedInAgentId;
	protected TimePicker5MinuteStep = TimePicker5MinuteStep;
	protected CSVDownloadAssignments = CSVDownloadAssignments;
	protected DialoguesOpen = Dialogues.Open;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermAssignmentCanDelete = Assignment.PermAssignmentCanDelete;
	protected PermProjectNotesCanPush = ProjectNote.PermProjectNotesCanPush;
	protected PermLabourCanPush = Labour.PermLabourCanPush;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;
	protected PermLabourCanPushSelf = Labour.PermLabourCanPushSelf;
	protected PermCRMReportAssignmentPDF = Assignment.PermCRMReportAssignmentPDF;
	protected PermCRMExportAssignmentCSV = Assignment.PermCRMExportAssignmentCSV;
	
	protected errorAssignmentNullSnackbarVisible = false;
	protected errorNoBillableLabourTypeVisible = false;
	protected errorAddNoteTextEmpty = false;
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	protected addNoteText: string | null = null;
	protected addNoteTextInternal = false;
	
	constructor() {
		super();
		
	}
	
	
	
	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
			this.$refs.labourForm as VForm,
			this.$refs.scheduleForm as VForm,
		];
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			General: 0,
			general: 0,
			Labour: 1,
			labour: 1,
			Schedule: 2,
			schedule: 2,
		};
	}
	
	protected get EndTimeLocal(): string | null {
		
		if (!this.value ||
			!this.value.id ||
			!this.value.json) {
			return null;
		}
		
		let iso8601 = null;
		if (this.isMakingNew) {
			iso8601 = this.value.json.endISO8601;
		} else {
			iso8601 = Assignment.EndISO8601ForId(this.value.id);
		}
		
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
		
		if (!this.value) {
			return;
		}
		
		let validatedVar = null;
		
		do {
			if (!val) {
				break;
			}
			if (!this.value) {
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
		
		//console.log('set EndDateLocal', val, validatedVar);
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.endISO8601 = validatedVar;
		this.$emit('input', clone);
		
	}
	
	
	protected get EndDateLocal(): string | null {
		
		if (!this.value ||
			!this.value.id ||
			!this.value.json) {
			return null;
		}
		
		let iso8601 = null;
		if (this.isMakingNew) {
			iso8601 = this.value.json.endISO8601;
		} else {
			iso8601 = Assignment.EndISO8601ForId(this.value.id);
		}
		
		if (null == iso8601 || IsNullOrEmpty(iso8601)) {
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
			if (!val) {
				break;
			}
			if (!this.value) {
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
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.endISO8601 = validatedVar;
		this.$emit('input', clone);
		
		
	}
	
	
	
	
	
	protected get StartTimeLocal(): string | null {
		
		if (!this.value || !this.value.id) {
			return null;
		}
		
		
		let iso8601 = null;
		if (this.isMakingNew) {
			iso8601 = this.value.json.startISO8601;
		} else {
			iso8601 = Assignment.StartISO8601ForId(this.value.id);
		}
		
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
		
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.startISO8601 = validatedVar;
		this.$emit('input', clone);
		
	}
	
	
	
	
	
	
	
	protected get StartDateLocal(): string | null {
		
		if (!this.value || !this.value.id) {
			return null;
		}
		
		let iso8601 = null;
		if (this.isMakingNew) {
			iso8601 = this.value.json.startISO8601;
		} else {
			iso8601 = Assignment.StartISO8601ForId(this.value.id);
		}
		
		let startMode = null;
		if (this.isMakingNew) {
			startMode = this.value.json.startTimeMode;
		} else {
			startMode = Assignment.StartTimeModeForId(this.value.id);
		}
		
		
		// If for whatever reason the start time isn't set and time mode isn't none, we should have a time present.
		if ((!iso8601 || IsNullOrEmpty(iso8601)) && startMode !== 'none') {
			
			const clone = _.cloneDeep(this.value) as IAssignment;
			clone.json.startISO8601 = DateTime.utc().toISO();
			this.$emit('input', clone);
		}
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
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.startISO8601 = validatedVar;
		this.$emit('input', clone);
		
		
		
	}
	
	
	protected get EndTimeMode(): 'none' | 'time' {
		
		if (!this.value ||
			!this.value.id || 
			!this.value.json) {
			return 'none';
		}
		
		let timeMode: 'none' | 'time' | null = null;
		if (this.isMakingNew) {
			timeMode = this.value.json.endTimeMode;
		} else {
			timeMode = Assignment.EndTimeModeForId(this.value.id);
		}
		
		if (null == timeMode || IsNullOrEmpty(timeMode)) {
			return 'none';
		}
		
		return timeMode;
	}
	
	protected set EndTimeMode(val: 'none' | 'time') {
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		
		clone.json.endTimeMode = val;
		
		// Update the other variables based on the mode change, notably the time.
		const dbISO8601 = this.value.json.endISO8601;
		if (dbISO8601 != null && !IsNullOrEmpty(dbISO8601)) {
			
			const dbUTC = DateTime.fromISO(dbISO8601);
			const dbLocal = dbUTC.toLocal();
			let mod;
			let modUtc;
			
			switch (val) {
				case 'none':
					
					mod = dbLocal.set({
						hour: 0,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					
					
					clone.json.endISO8601 = modUtc.toISO();
					
					
					
					break;
				case 'time':
					
					break;
			}
			
			
		}
		
		this.$emit('input', clone);
		
	}
	
	
	
	
	
	
	protected get StartTimeMode(): 'none' | 'morning-first-thing' | 'morning-second-thing' | 'afternoon-first-thing' | 'afternoon-second-thing' | 'time' {
		
		if (!this.value ||
			!this.value.id) {
			return 'none';
		}
		
		if (this.isMakingNew) {
			return this.value.json.startTimeMode;
		} else {
			return Assignment.StartTimeModeForId(this.value.id);
		}
		
		
	}
	
	protected set StartTimeMode(val: 'none' | 'morning-first-thing' | 'morning-second-thing' | 'afternoon-first-thing' | 'afternoon-second-thing' | 'time') {
		
		if (!this.value ||
			!this.value.id) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		
		clone.json.startTimeMode = val;
		
		// Update the other variables based on the mode change, notably the time.
		let iso8601 = null;
		if (this.isMakingNew) {
			iso8601 = this.value.json.startISO8601;
		} else {
			iso8601 = Assignment.StartISO8601ForId(this.value.id);
		}
		
		
		
		
		
		if (iso8601 != null && !IsNullOrEmpty(iso8601)) {
			
			const dbUTC = DateTime.fromISO(iso8601);
			const dbLocal = dbUTC.toLocal();
			let mod;
			let modUtc;
			
			switch (val) {
				case 'none':
					
					mod = dbLocal.set({
						hour: 0,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					clone.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'morning-first-thing':
					
					mod = dbLocal.set({
						hour: 8,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					clone.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'morning-second-thing':
					
					mod = dbLocal.set({
						hour: 10,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					clone.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'afternoon-first-thing':
					
					mod = dbLocal.set({
						hour: 13,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					clone.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'afternoon-second-thing':
					
					mod = dbLocal.set({
						hour: 15,
						minute: 0,
						second: 0,
					});
					
					modUtc = mod.toUTC();
					clone.json.startISO8601 = modUtc.toISO();
					
					break;
				case 'time':
					
					break;
			}
			
			
		}
		
		this.$emit('input', clone);
		
		
		
	}
	
	
	
	protected get HasStartISO8601(): boolean {
		
		if (!this.value || !this.value.id) {
			return false;
		}
		
		if (this.isMakingNew) {
			return this.value.json.hasStartISO8601;
		} else {
			return Assignment.HasStartISO8601ForId(this.value.id);
		}
		
		
		
	}
	
	protected set HasStartISO8601(val: boolean) {
		
		//console.debug('HasStartISO8601', val);
		
		
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		
		clone.json.hasStartISO8601 = val;
		if (!val) {
			clone.json.startTimeMode = 'none';
			
			let d = DateTime.local();
			d = d.set({hour: 0, minute: 0, second: 0});
			d = d.toUTC();
			
			clone.json.startISO8601 = d.toISO();
		}
		
		
		this.$emit('input', clone);
	}
	
	protected get HasEndISO8601(): boolean {
		
		if (!this.value || !this.value.id) {
			return false;
		}
		
		if (this.isMakingNew) {
			return this.value.json.hasEndISO8601;
		} else {
			return Assignment.HasEndISO8601ForId(this.value.id);
		}
		
		
	}
	
	protected set HasEndISO8601(val: boolean) {
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		
		clone.json.hasEndISO8601 = val;
		
		this.$emit('input', clone);
	}
	
	
	
	protected get ProjectId(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const projectId = json.projectId;
		if (!projectId || IsNullOrEmpty(projectId)) {
			return null;
		}
		
		return projectId;
	}
	
	protected set ProjectId(val: string | null) {
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.projectId = val;
		this.$emit('input', clone);
	}
	
	protected get AgentIds(): Array<string | null> {
		
		if (!this.value) {
			return [null];
		}
		
		//console.log('get AgentIds ', JSON.stringify(this.value.json.agentIds));
		
		const json = this.value.json;
		if (!json) {
			return [null];
		}
		
		const agentIds = json.agentIds;
		if (!agentIds || json.agentIds.length === 0) {
			return [null];
		}
		
		
		
		
		return agentIds;
	}
	
	protected set AgentIds(val: Array<string | null>) {
		
		//console.debug('set AgentIds', val, this.value);
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.agentIds = val;
		this.$emit('input', clone);
		
		//console.log('set AgentIds #2');
		//console.debug('this.value', this.value);
		
		// this.SignalChanged();
	}
	
	protected get StatusId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.statusId
			) {
			
			
			// Default status
			const allStatus = this.$store.state.Database.assignmentStatus as Record<string, IAssignmentStatus>;
			const toBeScheduledStatus = _.find(allStatus, (o) => o.json.isToBeScheduled);
			
			if (toBeScheduledStatus) {
				this.StatusId = toBeScheduledStatus.id || null;
				return toBeScheduledStatus.id || null;
			}
			
			
			return null;
		}
		
		return this.value.json.statusId;
	}
	
	protected set StatusId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.statusId = val;
		this.$emit('input', clone);
	}
	
	protected get StatusItems(): IAssignmentStatus[] {
		
		const filtered = _.filter(
			this.$store.state.Database.assignmentStatus,
			(o: IAssignmentStatus) => { // eslint-disable-line @typescript-eslint/no-unused-vars
				return true;
			});
		
		const sorted = _.sortBy(filtered, (o: IProjectStatus) => {
			return o.json.name;
		});
		
		
		return sorted;
		
		
	}
	
	
	protected get WorkRequested(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const workRequested = json.workRequested;
		if (!workRequested || IsNullOrEmpty(workRequested)) {
			return null;
		}
		
		return workRequested;
	}
	
	protected set WorkRequested(val: string | null) {
		
		if (!this.value) {
			return;
		}
		
		const clone = _.cloneDeep(this.value) as IAssignment;
		clone.json.workRequested = val;
		this.$emit('input', clone);
	}
	
	protected get NotesOnMainProjectThatAreNotThisAssignment(): number {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.projectId ||
			IsNullOrEmpty(this.value.json.projectId)) {
			return 0;
		}
		
		let ret = 0;
		const notes = ProjectNote.ForProjectIds([this.value.json.projectId]);
		for (const note of notes) {
			if (note.json.assignmentId !== this.value.id) {
				ret++;
			}
		}
		return ret;
		
	}
	
	protected StartTravel(): void {
		if (!this.value ||
			!this.value.id
			) {
			this.errorAssignmentNullSnackbarVisible = true;
			return;
		}
		
		Assignment.StartTravelForId(this.value.id);
	}
	
	
	
	protected StartOnSite(): void {
		
		if (!this.value ||
			!this.value.id
			) {
			this.errorAssignmentNullSnackbarVisible = true;
			return;
		}
		
		Assignment.StartOnSiteForId(this.value.id);
	}
	
	
	protected StartRemote(): void {
		
		//console.debug('StartRemote()');
		
		if (!this.value ||
			!this.value.id
			) {
			this.errorAssignmentNullSnackbarVisible = true;
			return;
		}
		
		Assignment.StartRemoteForId(this.value.id);
	}
	
	
	
	protected OnClickAddNote(): void {
		
		console.log('OnClickAddNote');
		
		if (!this.addNoteText || IsNullOrEmpty(this.addNoteText)) {
			this.errorAddNoteTextEmpty = true;
			return;
		}
		
		
		
		const newNote = ProjectNote.GetEmpty();
		if (newNote.id) {
			newNote.lastModifiedISO8601 = DateTime.utc().toISO();
			newNote.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
			newNote.json.originalBillingId = BillingContacts.CurrentBillingContactId();
			newNote.json.originalISO8601 = newNote.lastModifiedISO8601;
			newNote.json.assignmentId = this.value?.id || null;
			newNote.json.projectId = this.value?.json.projectId || null;
			
			newNote.json.contentType = 'styled-text';
			newNote.json.content = ProjectNoteStyledText.GetEmpty();
			newNote.json.content.html = escape(this.addNoteText);
			newNote.json.internalOnly = this.addNoteTextInternal;
			
			const payload: Record<string, IProjectNote> = {};
			payload[newNote.id] = newNote;
			ProjectNote.UpdateIds(payload);
			
			this.addNoteText = null;
			this.addNoteTextInternal = false;
		}
		
	}
	
	
	
	
	
	
	
	
	
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	protected get ForceAssignmentsToUseProjectSchedule(): boolean {
		
		const projectId = this.ProjectId;
		if (!projectId) {
			return false;
		}
		
		const project = Project.ForId(projectId);
		if (!project) {
			return false;
		}
		
		return project.json.forceAssignmentsToUseProjectSchedule || false;
	}
	
	// protected SignalChanged(): void {
		
	// 	// Debounce
		
	// 	if (this.debounceId) {
	// 		clearTimeout(this.debounceId);
	// 		this.debounceId = null;
	// 	}
		
	// 	this.debounceId = setTimeout(() => {
	// 		this.$emit('input', this.value);
	// 	}, 250);
	// }
	
	protected DoPrint(): void {
		
		Dialogues.Open({ 
			name: 'AssignmentReportDialogue', 
			state: {
				allLoadedAssignments: false,
				specificAssignments: [ this.value?.id ],
			},
		});
		
	}
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}
	
	
}


</script>
<style scoped>

</style>