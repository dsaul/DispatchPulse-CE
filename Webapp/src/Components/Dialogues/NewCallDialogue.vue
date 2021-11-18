<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>New Call Assistant</v-card-title>
			<v-divider></v-divider>
			<v-card-text>
				<v-form
					autocomplete="newpassword"
					ref="form"
					>
				
					<v-stepper v-model="currentStep" vertical style="-webkit-box-shadow: inherit; box-shadow: inherit;">
						<v-stepper-step
							:complete="currentStep > 1"
							editable
							step="1"
							
							>
							Who is involved in this project?
							<small>Add homeowner, designer, architect, decision maker, etc&hellip;</small>
						</v-stepper-step>

						<v-stepper-content step="1">
							
							
							<LabeledContactEditRowArrayAdapter
								v-model="Contacts"
								:reducePadding="true"
								class="e2e-contacts-array"
								/>
							
							<!-- Used solely to display the validation errors for the above array. -->
							<v-input 
								v-model="Contacts"
								:rules="[
									this.EnsureContactArrayHasAtLeastOneContact
								]"
								/>
							
						</v-stepper-content>
						
						<v-stepper-step
							:complete="currentStep > 2"
							editable
							step="2"
							class="e2e-step-project"
							>
							Project
							<small>Select which project this call is about.</small>
						</v-stepper-step>
						
						<v-stepper-content step="2">
							
							<ProjectSelectField
								v-model="ProjectId"
								:required="true"
								:rules="[
									this.EnsureProjectFieldIsFilled
								]"
								class="e2e-new-call-dialogue-project-select-field"
								/>
							
						</v-stepper-content>
						
						<v-stepper-step
							:complete="currentStep > 3"
							editable
							step="3"
							class="e2e-step-assignments"
							>
							Assignments
							<small>How many people should be assigned to this?</small>
						</v-stepper-step>
						
						<v-stepper-content step="3">
							<p style="margin-top: 40px;">
								<v-slider
									v-model="NumberOfAssignments"
									:max="100"
									:min="1"
									step="1"
									label="Assignment Count"
									thumb-label
									ticks
									:rules="[ ValidateRequiredField ]"
									>
									<template v-slot:append>
										<v-text-field
											v-model="NumberOfAssignments"
											class="mt-0 pt-0"
											hide-details
											single-line
											type="number"
											step="1"
											min="1"
											max="100"
											style="width: 60px"
											>
										</v-text-field>
									</template>
								</v-slider>
							</p>
						</v-stepper-content>

						<v-stepper-step
							step="4"
							editable
							class="e2e-step-additional-information"
							>
							Additional Information
						</v-stepper-step>
						<v-stepper-content step="4">
							<v-textarea
								v-model="WorkRequested"
								label="Work Requested"
								class="e2e-new-call-dialogue-work-requested"
								auto-grow
								>
							</v-textarea>
							<v-textarea
								v-model="InternalComments"
								label="Internal Comments (Not to be Shared With Clients)"
								class="e2e-new-call-dialogue-internal-comments"
								auto-grow
								>
							</v-textarea>
						</v-stepper-content>
					</v-stepper>
				
				</v-form>
			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer/>
				<v-btn
					color="blue darken-1"
					text
					@click="ResetAndClose()"
					>Close</v-btn>
				<v-btn
					color="blue darken-1"
					text
					@click="Next()"
					class="e2e-new-call-dialogue-next-button"
					>Next</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import AssistantDialogueBase from '@/Components/Dialogues/AssistantDialogueBase';
import LabeledContactEditRowArrayAdapter from '@/Components/Rows/LabeledContactEditRowArrayAdapter.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import EnsureProjectFieldIsFilled from '@/Utility/Validators/EnsureProjectFieldIsFilled';
import EnsureContactArrayHasAtLeastOneContact from '@/Utility/Validators/EnsureContactArrayHasAtLeastOneContact';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { ProjectNoteStyledText } from '@/Data/CRM/ProjectNoteStyledText/ProjectNoteStyledText';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';

@Component({
	components: {
		LabeledContactEditRowArrayAdapter,
	},
})
export default class NewCallDialogue extends AssistantDialogueBase {
	
	public static GenerateEmpty(): 
	{
			contacts: ILabeledContactId[];
			projectId: string;
			numberOfAssignments: number;
			workRequested: string;
			internalComments: string;
		} {
		return {
			contacts: [LabeledContactEditRowArrayAdapter.GenerateEmptyRow()],
			projectId: '',
			numberOfAssignments: 1,
			workRequested: '',
			internalComments: '',
		};
	}
	
	public $refs!: {
		form: HTMLFormElement,
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	protected ValidateRequiredField = ValidateRequiredField;
	protected EnsureProjectFieldIsFilled = EnsureProjectFieldIsFilled;
	protected EnsureContactArrayHasAtLeastOneContact = EnsureContactArrayHasAtLeastOneContact;
	
	constructor() {
		super();
		this.ModelState = NewCallDialogue.GenerateEmpty();
	}
	
	get MaxSteps(): number {
		return 4;
	}
	
	get DialogueName(): string {
		return 'NewCallDialogue';
	}
	
	protected Next(): void {
		
		//const validated = this.$refs.form.validate();
		
		// currentStep #'s
		// 1 = contacts
		// 2 = project
		// 3 = assignments
		// 4 = additional info
		
		const contactsValidation = this.EnsureContactArrayHasAtLeastOneContact(this.Contacts);
		if (typeof contactsValidation === 'string') {
			Notifications.AddNotification({
				severity: 'error',
				message: contactsValidation,
				autoClearInSeconds: 10,
			});
			this.currentStep = 1;
			return;
		}
		
		const projectValidation = this.EnsureProjectFieldIsFilled(this.ProjectId);
		if (typeof projectValidation === 'string') {
			Notifications.AddNotification({
				severity: 'error',
				message: projectValidation,
				autoClearInSeconds: 10,
			});
			this.currentStep = 2;
			return;
		}
		
		
		//console.log('this.currentStep', this.currentStep, this.MaxSteps);
		
		
		if (+this.currentStep !== +this.MaxSteps) {
			this.currentStep++;
			return;
		}
		
		this.Finalize();
		
	}
	
	protected Finalize(): void {
		
		console.log('finalize');
		
		// Contact added by selection process. Must be assigned in project.
		const project = Project.ForId(this.ProjectId);
		
		if (!project || !project.id) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'There was an error retrieving the project.',
				autoClearInSeconds: 10,
			});
			return;
		}
		
		let projectContacts = project.json.contacts;
		if (!projectContacts) {
			console.log('!projectContacts so we\'re setting it to []');
			projectContacts = [];
		}
		
		// We need to go throught the submitted contacts, and add 
		// the non duplicates to the project contacts.
		
		for (const newContact of this.Contacts) {
			
			const nLabel = (newContact.label || '').trim();
			const nValue = (newContact.value || '').trim();
			
			let skip = false;
			for (const projectContact of projectContacts) {
				
				const pLabel = (projectContact.label || '').trim();
				const pValue = (projectContact.value || '').trim();
				
				if (nLabel === pLabel && nValue === pValue) {
					skip = true;
					break;
				}
			}
			
			if (skip === true) {
				continue;
			}
			
			projectContacts.push(newContact);
		}
		
		project.json.contacts = projectContacts;
		
		const payloadP: Record<string, IProject> = {};
		payloadP[project.id] = project;
		Project.UpdateIds(payloadP);
		
		// Project added by selection process.
		// Assignments must be created and tied to project. Additional 
		// information must be added to the assignments.
		
		const payloadA: Record<string, IAssignment> = {};
		const payloadNotes: Record<string, IProjectNote> = {};
		
		for (let i = 0; i < this.ModelState.numberOfAssignments; i++) {
			const assignment = Assignment.GetEmpty();
			if (assignment.id) {
				assignment.json.projectId = this.ProjectId;
				assignment.json.workRequested = this.WorkRequested;
				payloadA[assignment.id] = assignment;
			}
			
			
			const newNote = ProjectNote.GetEmpty();
			if (newNote.id) {
				newNote.lastModifiedISO8601 = DateTime.utc().toISO();
				newNote.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				newNote.json.originalBillingId = BillingContacts.CurrentBillingContactId();
				newNote.json.originalISO8601 = newNote.lastModifiedISO8601;
				newNote.json.assignmentId = assignment.id || null;
				newNote.json.projectId = project.id || null;
				
				newNote.json.contentType = 'styled-text';
				newNote.json.content = ProjectNoteStyledText.GetEmpty();
				newNote.json.content.html = escape(this.InternalComments);
				newNote.json.internalOnly = true;
				payloadNotes[newNote.id] = newNote;
			}
			
			
		}
		
		Assignment.UpdateIds(payloadA);
		ProjectNote.UpdateIds(payloadNotes);
		
		
		// Open the project that was just created.
		this.$router.push(`/section/projects/${this.ProjectId}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		
		this.ResetAndClose();
		
	}
	
	protected ResetAndClose(): void {
		this.ModelState = NewCallDialogue.GenerateEmpty();
		this.$refs.form.resetValidation();
		Dialogues.Close(this.DialogueName);
		this.currentStep = 1;
		
	}
	
	protected Confirm(): void {
		
		//console.log('confirm', this.ModelState);
		
		
		
		//Project.DeleteIds([this.ModelState.id]);
		Dialogues.Close(this.DialogueName);
		this.ModelState = NewCallDialogue.GenerateEmpty();
	}
	
	
	get Contacts(): ILabeledContactId[] {
		
		if (!this.ModelState ||
			!this.ModelState.contacts
			) {
			return [];
		}
		
		return this.ModelState.contacts;
	}
	
	set Contacts(val: ILabeledContactId[]) {
		
		if (!this.ModelState) {
			return;
		}
		
		const tmp = this.ModelState;
		tmp.contacts = val == null ? [] : val;
		this.ModelState = tmp;
	}
	
	get ProjectId(): string {
		
		if (!this.ModelState ||
			!this.ModelState.projectId) {
			return '';
		}
		
		return this.ModelState.projectId;
		
	}
	
	set ProjectId(val: string) {
		if (!this.ModelState) {
			return;
		}
		
		const tmp = this.ModelState;
		tmp.projectId = IsNullOrEmpty(val) ? '' : val;
		this.ModelState = tmp;
	}
	
	get NumberOfAssignments(): number {
		
		if (!this.ModelState ||
			!this.ModelState.numberOfAssignments) {
			return 0;
		}
		
		return this.ModelState.numberOfAssignments;
		
	}
	
	set NumberOfAssignments(val: number) {
		if (!this.ModelState) {
			return;
		}
		
		const tmp = this.ModelState;
		tmp.numberOfAssignments = val ? +val : 0;
		this.ModelState = tmp;
	}
	
	get WorkRequested(): string {
		
		if (!this.ModelState ||
			!this.ModelState.workRequested) {
			return '';
		}
		
		return this.ModelState.workRequested;
		
	}
	
	set WorkRequested(val: string) {
		if (!this.ModelState) {
			return;
		}
		
		const tmp = this.ModelState;
		tmp.workRequested = IsNullOrEmpty(val) ? '' : val;
		this.ModelState = tmp;
	}
	
	get InternalComments(): string {
		
		if (!this.ModelState ||
			!this.ModelState.internalComments) {
			return '';
		}
		
		return this.ModelState.internalComments;
		
	}
	
	set InternalComments(val: string) {
		if (!this.ModelState) {
			return;
		}
		
		const tmp = this.ModelState;
		tmp.internalComments = IsNullOrEmpty(val) ? '' : val;
		this.ModelState = tmp;
	}
	
	
	
	
	
	
	
	
	
	
}
</script>