<template>
	<div class="outerdiv" style="">
		<v-dialog
			v-model="selectDialogOpen"
			persistent
			scrollable
			:fullscreen="MobileDeviceWidth()"
			>
			<!--<template v-slot:activator="{ on }">
				<v-btn color="primary" dark v-on="on">Open Dialog</v-btn>
			</template>-->
			<v-card>
				<v-card-title>Select a Project</v-card-title>
				<v-divider></v-divider>
				<v-card-text >
					
					<ProjectList
						:openEntryOnClick="false"
						:showMenuButton="false"
						@ClickEntry="OnClickEntry"
						:excludeIds="excludeIds"
						class="e2e-project-select-list"
						ref="list"
						/>
					
					<v-dialog
						v-model="newDialogueVisible"
						persistent
						scrollable
						:fullscreen="MobileDeviceWidth()"
						>
						<v-card>
							<v-card-title>Create and Select New Project</v-card-title>
							<v-divider></v-divider>
							<v-card-text>
								<ProjectEditor 
									ref="editor"
									v-model="newDialogueObject"
									:showAppBar="false"
									:showFooter="false "
									preselectTabName="General"
									:isMakingNew="true"
									/>
							</v-card-text>
							<v-divider></v-divider>
							<v-card-actions>
								<v-spacer/>
								<v-btn color="red darken-1" text @click="NewDialogueCancel()">Close</v-btn>
								<v-btn color="green darken-1" text @click="NewDialogueSaveAndAdd()">Save and Select</v-btn>
							</v-card-actions>
						</v-card>
					</v-dialog>
					
				</v-card-text>
				<v-divider></v-divider>
				<v-card-actions>
					<v-btn color="green darken-1" text @click="OpenNewDialogue()">New</v-btn>
					<v-btn color="red darken-1" text @click="ClearValue()">Clear</v-btn>
					<v-spacer/>
					<v-btn color="green darken-1" text @click="selectDialogOpen = false;">Close</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>
		<v-input
			:messages="[]"
			ref="input"
			persistent-hint
			:hint="hint"
			:required="required"
			v-model="value"
			:rules="rules"
			:disabled="disabled"
			:readonly="readonly"
			>
			<template v-slot:label>
				<span style="font-size: 12px;">{{label}}</span>
			</template>
			<template v-slot:append>
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn
						icon
						v-on="on"
						:disabled="disabled"
						>
							<v-icon>more_vert</v-icon>
						</v-btn>
					</template>

					<v-list dense>
						<v-list-item
							@click="OpenProject()"
							:disabled="isDialogue || IsValueEmpty || disabled"
							>
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-divider />
						<v-list-item
							@click="SelectNewProject()"
							:disabled="disabled || readonly"
							>
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Select Different…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						
						<v-list-item
							@click="ClearValue()"
							:disabled="IsValueEmpty || disabled || readonly"
							>
							<v-list-item-icon>
								<v-icon>clear</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Clear</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						
						<slot name="custom-menu-options"></slot>
						
					</v-list>
				</v-menu>
			</template>
			
			
			
			<div style="margin-top: 5px; margin-bottom: 5px; width: 100%; cursor: text; display: flex; align-items: stretch;">
				
				<div
					v-if="!Project && value"
					>
					{{value}} (can't find)
				</div>
				<div 
					v-else-if="Project"
					@click="SelectNewProject()"
					:style="{
						flex: '1',
						display: 'flex',
						'align-items': 'flex-end',
						color: disabled ? 'rgba(0, 0, 0, 0.38)' : 'inherit',
					}"
					>
					{{ProjectCombinedDescriptionForId(Project.id) || Project.id}}
				</div>
				<div
					v-else
					@click="SelectNewProject()"
					style="flex: 1; text-align: center;"
					class="e2e-project-select-field-select-or-add-project"
					>
					<v-btn
						color="primary"
						text
						:disabled="disabled || readonly"
						>
						Select or Add Project
					</v-btn>
				</div>
				<div
					v-if="Project && !isDialogue"
					class="d-none d-sm-flex"
					>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								:to="`/section/projects/${value}?tab=General`"
								color="primary"
								label
								outlined
								style="margin: 4px;"
								v-on="on">
								Open
								<v-icon right small>open_in_new</v-icon>
							</v-chip>
						</template>
						<span>Open this project.</span>
					</v-tooltip>
				</div>
			</div>
			
			
			
			
			
			
			<!-- <div style="margin-top: 5px; margin-bottom: 5px; width: 100%; cursor: text;" :class="{
					'text-align-center': !Project
				}">
				<span v-if="Project" style="color: black;">{{ProjectCombinedDescriptionForId(Project.id) || Project.id}}</span>
				<span v-else>
					<v-btn color="primary" text>Select or Add Project</v-btn>
				</span>
			</div> -->
			
			
			
			
		</v-input>
		
		<div v-if="ShowDetails && Project" style="margin-bottom: 20px;">
			
			<div class="subtitle-1" style="font-weight: bold;">General Information</div>
			<table>
				<tr v-if="ProjectNameForId(Project.id)">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">Name:</td>
					<td style="font-size: 14px;">
						{{ProjectNameForId(Project.id)}}
					</td>
				</tr>
				<tr v-if="Project.json.parentId">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">Parent:</td>
					<td style="font-size: 14px;">
						<router-link :to="`/section/projects/${Project.json.parentId}?tab=General`"  style="font-size: 14px;">
							{{ParentProjectName}}
						</router-link>
						
					</td>
				</tr>
				<tr v-if="ProjectStatus">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">Status:</td>
					<td style="font-size: 14px;">{{ProjectStatus.json.name}}</td>
				</tr>
				<tr v-if="Project.lastModifiedISO8601">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">Modified:</td>
					<td style="font-size: 14px;">
						{{ProjectLastModifiedLocalDisplay}}
						<span v-if="ProjectLastModifiedWho"> by {{ProjectLastModifiedWho.fullName}}</span>
					</td>
				</tr>
				
				
				
				<!--<tr v-for="(obj) in Project.json.addresses" :key="obj.id">
					
				</tr>-->
			</table>
		</div>
		
		
		<div v-if="ShowDetails && Project && Project.json.addresses && Project.json.addresses.length > 0" style="margin-bottom: 20px;">
			<div class="subtitle-1" style="font-weight: bold;">Addresses</div>
			<table>
				<tr v-for="(obj) in Project.json.addresses" :key="obj.id">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
						<span v-if="obj.label">{{obj.label}}:</span>
					</td>
					<td style="font-size: 14px;"><a :href="`https://www.google.com/maps/dir/current+location/${obj.value.replace(/[\r\n\x0B\x0C\u0085\u2028\u2029\/]+/g, ' ')}`" target="_blank">{{obj.value}}</a></td>
				</tr>
			</table>
		</div>
		
		
		
		<div v-if="ShowDetails && Project && Project.json.companies && Project.json.companies.length > 0" style="margin-bottom: 20px;">
			<div class="subtitle-1" style="font-weight: bold;">Companies</div>
			<table>
				<tr v-for="(obj) in Project.json.companies" :key="obj.id">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
						<span v-if="obj.label">{{obj.label}}:</span>
					</td>
					<td style="font-size: 14px;">
						<router-link
							:to="`/section/companies/${obj.value}`"
							>
							{{CompanyNameForId(obj.value)}}
						</router-link>
					</td>
				</tr>
			</table>
		</div>
		
		
		<div v-if="ShowDetails && Project && Project.json.contacts && Project.json.contacts.length > 0" style="margin-bottom: 20px;">
			<div class="subtitle-1" style="font-weight: bold;">Contacts</div>
			<table>
				<tr v-for="(obj) in Project.json.contacts" :key="obj.id">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
						<span v-if="obj.label">{{obj.label}}:</span>
					</td>
					<td style="font-size: 14px;">
						<router-link
							:to="`/section/contacts/${obj.value}`"
							>
							{{ContactNameForId(obj.value)}}
						</router-link>
					</td>
				</tr>
			</table>
		</div>
		
		
		<div v-if="ShowDetails && Project && 
			(Project.json.hasStartISO8601 || Project.json.hasEndISO8601) && (
				Project.json.startTimeMode !== 'none' || Project.json.endTimeMode !== 'none'
			)">
			<div class="subtitle-1" style="font-weight: bold;">Schedule</div>
			
			<tr v-if="Project.json.startISO8601">
				<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">Start Date &amp; Time:</td>
				<!-- Change between datetime and date in luxon depending on start time mode -->
				<td style="font-size: 14px;">
					{{ProjectStartScheduleDescriptionForId(Project.id)}}
				</td>
			</tr>
			
			<tr v-if="Project.json.endISO8601">
				<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">End Date &amp; Time:</td>
				<!-- Change between datetime and date in luxon depending on start time mode -->
				<td style="font-size: 14px;">
					{{ProjectEndScheduleDescriptionForId(Project.id)}}
				</td>
			</tr>
			
		</div>
		
		
	</div>
			
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import FieldBase from './FieldBase';
import { DateTime } from 'luxon';
import ProjectEditor from '@/Components/Editors/ProjectEditor.vue';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Company } from '@/Data/CRM/Company/Company';
import { IProjectStatus, ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		ProjectList,
		ProjectEditor,
	},
	
})

export default class ProjectSelectField extends FieldBase {
	
	@Prop({ default: 'Project' }) declare public readonly label: string | null;
	@Prop({ default: () => [] }) public readonly excludeIds!: Array<string | null>;
	
	public $refs!: {
		input: Vue,
		editor: ProjectEditor,
		list: ProjectList,
	};
	
	protected ProjectCombinedDescriptionForId = Project.CombinedDescriptionForId;
	protected ProjectStartScheduleDescriptionForId = Project.StartScheduleDescriptionForId;
	protected ProjectEndScheduleDescriptionForId = Project.EndScheduleDescriptionForId;
	protected ProjectNameForId = Project.NameForId;
	protected ContactNameForId = Contact.NameForId;
	protected CompanyNameForId = Company.NameForId;
	protected BillingContactForId = BillingContacts.ForId;
	protected ProjectStatusForId = ProjectStatus.ForId;
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected newDialogueVisible = false;
	protected newDialogueObject: IProject | null = null;
	
	
	public mounted(): void {
		//console.log('mounted', this.$refs);
		//console.debug('company select field ', this, this.$store.state.Database.companies);
		
		if (null != this.value && !IsNullOrEmpty(this.value)) {

			const projects = Project.ForId(this.value);
			if (null == projects) {
				const id = this.value;
				SignalRConnection.Ready(() => {
					BillingPermissionsBool.Ready(() => {
						Project.FetchForId(id);
					});
				});
				
			}
		}


		// Set the flex direction for the input element.
		do {
			
			const defaultInputSlots = this.$refs.input.$slots.default;
			
			if (!defaultInputSlots) {
				break;
			}
			
			for (const node of defaultInputSlots) {
				if (!node) {
					continue;
				}
				
				const elm: HTMLElement = node.elm as HTMLElement;
				if (!elm) {
					continue;
				}
				
				
				
				const elmParent = elm.parentElement;
				if (!elmParent) {
					continue;
				}
				
				elmParent.style.flexDirection = 'column';
				elmParent.style.alignItems = 'start';
				elmParent.style.borderBottom = 'thin solid grey';
				
				//console.log(elm);
			}
			
			
			
			
		} while (false);
		
	}
	
	
	
	
	protected get Project(): IProject | null {
		
		if (this.value === null) {
			return null;
		}
		
		const project = Project.ForId(this.value);
		if (!project) {
			return null;
		}
		
		return project;
	}
	
	protected get ParentProject(): IProject | null {
		
		if (this.value === null) {
			return null;
		}
		if (!this.Project ||
			!this.Project.json ||
			!this.Project.json.parentId) {
			return null;
		}
		
		const project = Project.ForId(this.Project.json.parentId);
		if (!project) {
			return null;
		}
		
		return project;
	}
	
	protected get ParentProjectName(): string | null {
		
		if (!this.Project ||
			!this.Project.json) {
			return null;
		}
		
		return Project.CombinedDescriptionForId(this.Project.json.parentId) || null;
	}
	
	protected get ProjectStatus(): IProjectStatus | null {
		
		if (!this.Project ||
			!this.Project.json) {
			return null;
		}
		
		const statusId = this.Project.json.statusId;
		if (!statusId) {
			return null;
		}
		
		return ProjectStatus.ForId(statusId);
	}
	
	protected get ProjectLastModifiedLocalDisplay(): string | null {
		if (!this.Project ||
			!this.Project.json) {
			return null;
		}
		
		const lastModifiedISO8601 = this.Project.lastModifiedISO8601;
		if (!lastModifiedISO8601 || IsNullOrEmpty(lastModifiedISO8601)) {
			return null;
		}
		
		const dateUTC = DateTime.fromISO(lastModifiedISO8601);
		if (!dateUTC) {
			return null;
		}
		
		const dateLocal = dateUTC.toLocal();
		if (!dateLocal) {
			return null;
		}
		
		return dateLocal.toLocaleString(DateTime.DATETIME_SHORT) || null;
	}
	
	protected get ProjectLastModifiedWho(): IBillingContacts | null {
		if (!this.Project ||
			!this.Project.json) {
			return null;
		}
		
		const lastModifiedBillingId = this.Project.json.lastModifiedBillingId;
		if (!lastModifiedBillingId || IsNullOrEmpty(lastModifiedBillingId)) {
			return null;
		}
		
		return BillingContacts.ForId(lastModifiedBillingId);
	}
	
	
	
	
	protected SelectNewProject(): void {
		
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}
		
		//console.log('SelectNewProject()');
		
		this.selectDialogOpen = true;
		
		requestAnimationFrame(() => {
			this.$refs.list.SelectFilterField();
		});
	}
	
	protected ClearValue(): void {
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}
		
		//console.log('ClearValue()');
		this.$emit('input', null);
		
		this.selectDialogOpen = false;
	}
	
	protected OpenProject(): void {
		
		//console.log('OpenProject', this.value);
		
		if (!IsNullOrEmpty(this.value)) {
			this.$router.push(`/section/projects/${this.value}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		} else {
			console.error('Can\'t go to the project as value is null');
		}
		
		
	}
	
	protected OnClickEntry(id: string): void {
		console.debug('OnClickEntry', id);
		
		this.$emit('input', id);
		this.selectDialogOpen = false;
		
		
	}
	
	protected OpenNewDialogue(): void {
		
		console.debug('OpenNewDialogue()');
		
		this.newDialogueObject = Project.GetEmpty();
		this.newDialogueVisible = true;
		
	}
	
	protected NewDialogueCancel(): void {
		console.debug('NewDialogueCancel()');
		
		this.$refs.editor.ResetValidation();
		this.newDialogueObject = Project.GetEmpty();
		this.newDialogueVisible = false;
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected NewDialogueSaveAndAdd(): void {
		console.debug('NewDialogueSaveAndAdd()', this.newDialogueObject);
		
		if (this.newDialogueObject && this.$refs.editor.IsValidated()) {
			
			// First add the project.
			const state = this.newDialogueObject as IProject;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				
				const payload: Record<string, IProject> = {};
				payload[state.id] = state;
				Project.UpdateIds(payload);
				
				// Then choose this new project.
				this.$emit('input', state.id);
				
				// Close new dialogue.
				this.$refs.editor.ResetValidation();
				this.newDialogueObject = Project.GetEmpty();
				this.newDialogueVisible = false;
				//this.$refs.editor.SelectFirstTab();
				
				// Close select dialogue.
				this.selectDialogOpen = false;
			}
			
			
			
		} else {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Some of the form fields didn\'t pass validation.',
				autoClearInSeconds: 10,
			});
		}
	}
	
}

Vue.component('ProjectSelectField', ProjectSelectField);

</script>
<style scoped>
.outerdiv:after {
	border-color: currentColor;
	border-style: solid;
	border-width: thin 0 thin 0;
}
.text-align-center {
	text-align: center;
}
</style>