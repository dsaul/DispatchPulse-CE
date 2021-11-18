<template>
	<div>
		<v-list
			v-if="PermProjectNotesCanRequest"
			:dense="dense">
			<v-text-field
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4"
				v-model="filter"
				hide-details
				label="Filter"
				prepend-inner-icon="search"
				solo
				style="margin-bottom: 10px;"
				ref="filterField"
				>
			</v-text-field>
			
			<v-card outlined style="margin: 15px;" v-if="showAddQuickNote">
				<v-card-title style="padding-bottom: 0px;">Add Quick Note</v-card-title>
				<v-card-text>
					
					<v-textarea
						label=""
						hint="Work performed, things not to forget, etc."
						v-model="addNoteText"
						rows="1"
						auto-grow
						class="e2e-add-quick-note-text-area"
						:disabled="disabled"
						:readonly="!PermProjectNotesCanPush()"
						>
						<template v-slot:append-outer>
							<v-btn
								@click="OnClickAddNote"
								color="primary"
								text
								class="e2e-add-quick-note-text-area-save"
								:disabled="disabled || !PermProjectNotesCanPush()"
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
						:disabled="disabled || !PermProjectNotesCanPush()"
						>
					</v-switch>
				</v-card-text>
			</v-card>
			
			<div v-if="PageRows.length != 0">
				<template>
					<div class="text-center" v-if="showTopPagination === true">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							
							>
						</v-pagination>
					</div>
				</template>
				
				<v-list-item-group
					v-if="dense"
					>
					<ProjectNoteCardSmall 
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						v-model="PageRows[index]"
						:isDialogue="isDialogue"
						@DeleteEntry="DeleteEntry(row.id)"
						@Resolved="EntryResolved(row.id)"
						@NotResolved="EntryNotResolved(row.id)"
						@NoLongerRelevant="EntryNoLongerRelevant(row.id)"
						@Relevant="EntryRelevant(row.id)"
						@MarkInternalOnly="EntryInternalOnly(row.id)"
						@MarkNotInternalOnly="EntryNotInternalOnly(row.id)"
						:rootProject="rootProject"
						:disabled="disabled"
						/>
					
				</v-list-item-group>
				<v-list-item-group
					v-else
					color="primary">
					<ProjectNoteCard 
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						v-model="PageRows[index]"
						:isDialogue="isDialogue"
						@DeleteEntry="DeleteEntry(row.id)"
						@Resolved="EntryResolved(row.id)"
						@NotResolved="EntryNotResolved(row.id)"
						@NoLongerRelevant="EntryNoLongerRelevant(row.id)"
						@Relevant="EntryRelevant(row.id)"
						@MarkInternalOnly="EntryInternalOnly(row.id)"
						@MarkNotInternalOnly="EntryNotInternalOnly(row.id)"
						:rootProject="rootProject"
						:disabled="disabled"
						/>
				</v-list-item-group>
				
				<template>
					<div class="text-center">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							>
						</v-pagination>
					</div>
				</template>
			</div>
			<div v-else>
				<v-alert
					outlined
					type="info"
					elevation="0"
					style="margin-left: 15px; margin-right: 15px; margin-bottom: 0px;"
					>
					{{emptyMessage}}
				</v-alert>
			</div>
			
		</v-list>
		<PermissionsDeniedAlert v-else />
	</div>
</template>

<script lang="ts">

import { Component, Vue, Prop } from 'vue-property-decorator';
import ProjectListItem from '@/Components/ListItems/ProjectListItem.vue';
import _ from 'lodash';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ListBase from './ListBase';
import ProjectNoteCard from '@/Components/Cards/ProjectNote/ProjectNoteCard.vue';
import ProjectNoteCardSmall from '@/Components/Cards/ProjectNote/ProjectNoteCardSmall.vue';
import { DateTime } from 'luxon';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { IProjectNoteStyledText, ProjectNoteStyledText } from '@/Data/CRM/ProjectNoteStyledText/ProjectNoteStyledText';
import { IProjectNoteCheckbox } from '@/Data/CRM/ProjectNoteCheckbox/ProjectNoteCheckbox';
import { IProjectNoteImage } from '@/Data/CRM/ProjectNoteImage/ProjectNoteImage';
import { IProjectNoteVideo } from '@/Data/CRM/ProjectNoteVideo/ProjectNoteVideo';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		ProjectListItem,
		ProjectNoteCard,
		ProjectNoteCardSmall,
		PermissionsDeniedAlert,
	},
})
export default class NoteList extends ListBase {
	
	@Prop({ default: null }) public readonly showOnlyProjectId!: string;
	@Prop({ default: null }) public readonly showOnlyAssignmentId!: string;
	@Prop({ default: 'There are no notes to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: true }) public readonly isReverseSort!: boolean;
	@Prop({ default: true }) public readonly dense!: boolean;
	@Prop({ default: false }) public readonly showChildrenOfProjectIdAsWell!: boolean;
	@Prop({ default: null }) public readonly rootProject!: IProject;
	@Prop({ default: false }) public readonly showAddQuickNote!: IProject;
	
	public $refs!: {
		filterField: Vue,
	};
	
	protected PermProjectNotesCanRequest = ProjectNote.PermProjectNotesCanRequest;
	protected PermProjectNotesCanPush = ProjectNote.PermProjectNotesCanPush;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	protected addNoteText: string | null = null;
	protected addNoteTextInternal = false;
	protected errorAddNoteTextEmpty = false;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.debug('NoteList LoadData()');
		
		
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					if (ProjectNote.PermProjectNotesCanRequest()) {
						const rtr = ProjectNote.RequestProjectNotes.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToProjectId: this.showOnlyProjectId,
							showChildrenOfProjectIdAsWell: this.showChildrenOfProjectIdAsWell,
						});
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
					}
					
					
					if (promises.length > 0) {
						
						this.loadingData = true;
						
						Promise.all(promises).finally(() => {
							this.loadingData = false;
						});
					}
					
				});
			});
		
		}, 250);
		
	}
	
	public SelectFilterField(): void {
		//console.log('SelectFilterField()', this.$refs.filterField);
		if (this.$refs.filterField) {
			const input = this.$refs.filterField.$el.querySelector('input');
			if (input) {
				input.focus();
			}
		}
	}
	
	/*protected GetEntryRouteForId(id: string): string {
		return `/section/projects/${id}?tab=General`;
	}*/
	
	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteProjectNoteDialogue';
	}
	
	protected GetDeleteDialogueModelState(id: string): {
		redirectToIndex: boolean;
		id: guid;
	} {
		
		return {
			redirectToIndex: false,
			id,
		};
	}
	
	
	protected EntryResolved(noteId: string): void {
		
		const note = ProjectNote.ForId(noteId);
		if (!note || !note.id || IsNullOrEmpty(note.id)) {
			console.error('!note || !note.id || IsNullOrEmpty(note.id)');
			return;
		}
		
		note.json.resolved = true;
		
		const payload: Record<string, IProjectNote> = {};
		payload[note.id] = note;
		ProjectNote.UpdateIds(payload);
		
		
	}
	
	protected EntryNotResolved(noteId: string): void {
		
		const note = ProjectNote.ForId(noteId);
		if (!note || !note.id || IsNullOrEmpty(note.id)) {
			console.error('!note || !note.id || IsNullOrEmpty(note.id)');
			return;
		}
		
		note.json.resolved = false;
		
		const payload: Record<string, IProjectNote> = {};
		payload[note.id] = note;
		ProjectNote.UpdateIds(payload);
		
		
	}
	
	protected EntryNoLongerRelevant(noteId: string): void {
		
		const note = ProjectNote.ForId(noteId);
		if (!note || !note.id || IsNullOrEmpty(note.id)) {
			console.error('!note || !note.id || IsNullOrEmpty(note.id)');
			return;
		}
		
		note.json.noLongerRelevant = true;
		
		const payload: Record<string, IProjectNote> = {};
		payload[note.id] = note;
		ProjectNote.UpdateIds(payload);
	}
	
	protected EntryRelevant(noteId: string): void {
		
		const note = ProjectNote.ForId(noteId);
		if (!note || !note.id || IsNullOrEmpty(note.id)) {
			console.error('!note || !note.id || IsNullOrEmpty(note.id)');
			return;
		}
		
		note.json.noLongerRelevant = false;
		
		const payload: Record<string, IProjectNote> = {};
		payload[note.id] = note;
		ProjectNote.UpdateIds(payload);
	}
	
	protected EntryInternalOnly(noteId: string): void {
		
		const note = ProjectNote.ForId(noteId);
		if (!note || !note.id || IsNullOrEmpty(note.id)) {
			console.error('!note || !note.id || IsNullOrEmpty(note.id)');
			return;
		}
		
		note.json.internalOnly = true;
		
		const payload: Record<string, IProjectNote> = {};
		payload[note.id] = note;
		ProjectNote.UpdateIds(payload);
	}
	
	protected EntryNotInternalOnly(noteId: string): void {
		
		const note = ProjectNote.ForId(noteId);
		if (!note || !note.id || IsNullOrEmpty(note.id)) {
			console.error('!note || !note.id || IsNullOrEmpty(note.id)');
			return;
		}
		
		note.json.internalOnly = false;
		
		const payload: Record<string, IProjectNote> = {};
		payload[note.id] = note;
		ProjectNote.UpdateIds(payload);
	}
	
	
	
	
	
	protected GetRawRows(): Record<string, IProjectNote> {
		return this.$store.state.Database.projectNotes;
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IProjectNote, key: string): boolean {
		
		let result = true;
		
		do {
			if (!o || !o.id || !o.json) {
				result = false;
				break;
			}
			
			if (this.showOnlyProjectId) {
				
				let projects = [];
				
				const noteEntryProject = Project.ForId(o.json.projectId);
				const showOnlyProject = Project.ForId(this.showOnlyProjectId);
				
				
				if (this.showChildrenOfProjectIdAsWell) {
					
					projects = Project.RecursiveChildProjectsOfId(this.showOnlyProjectId);
				} else {
					
					
					if (showOnlyProject) {
						projects.push(showOnlyProject);
					}
					
				}
				
				const found = !!_.find(projects, (value) => {
					return noteEntryProject?.id === value.id;
				});
				
				if (!found) {
					result = false;
					break;
				}
				
			}
			
			if (this.showOnlyAssignmentId) {
				if (o.json.assignmentId !== this.showOnlyAssignmentId) {
					result = false;
					break;
				}
			}
			
			
			if (this.excludeIds && this.excludeIds.length > 0) {
				
				let isInExcludeList = false;
				
				for (const id of this.excludeIds) {
					if (null === id) {
						continue;
					}
					
					if (o.id === id) {
						isInExcludeList = true;
						break;
					}
				}
				
				if (isInExcludeList) {
					result = false;
					break;
				}
				
			}
			
			if (this.showFilter) {
				let haystack = '';
				
				
				
				// Billing Full Name 
				if (null !== o.json.lastModifiedBillingId) {
					
					const billingContact = BillingContacts.ForId(o.json.lastModifiedBillingId);
					
					if (billingContact) {
						haystack += billingContact.fullName;
					}
					
				}
				
				// Last Modified
				if (null !== o.lastModifiedISO8601) {
					haystack += DateTime.fromISO(o.lastModifiedISO8601).toLocaleString(DateTime.DATETIME_FULL);
				}
				
				switch (o.json.contentType) {
					case 'styled-text':
						haystack += (o.json.content as IProjectNoteStyledText).html;
						break;
					case 'checkbox':
						haystack += (o.json.content as IProjectNoteCheckbox).text;
						break;
					case 'image':
						haystack += (o.json.content as IProjectNoteImage).uri;
						break;
					case 'video':
						haystack += (o.json.content as IProjectNoteVideo).uri;
						break;
				}
				
				haystack = haystack.replace(/\W/g, '');
				haystack = haystack.toLowerCase();
				
				
				let needle = this.filter.toLowerCase();
				needle = needle.replace(/\W/g, '');
				
				//console.log('haystack:',haystack,'needle:',needle);
				
				if (haystack.indexOf(needle) === -1) {
					result = false;
					break;
				}
			}
			
			
		} while (false);
		
		return result;
		
	}
	
	
	
	protected RowSortBy(o: IProjectNote): string {
		
		return o.lastModifiedISO8601 || '1';
	}
	
	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
	
	protected OnClickAddNote(): void {
		
		console.log('OnClickAddNote');
		
		if (!this.addNoteText || IsNullOrEmpty(this.addNoteText)) {
			this.errorAddNoteTextEmpty = true;
			return;
		}
		
		
		
		const newNote = ProjectNote.GetEmpty();
		if (!newNote.id || IsNullOrEmpty(newNote.id)) {
			console.error('!newNote.id || IsNullOrEmpty(newNote.id)');
			return;
		}
		
		newNote.lastModifiedISO8601 = DateTime.utc().toISO();
		newNote.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		newNote.json.originalBillingId = BillingContacts.CurrentBillingContactId();
		newNote.json.originalISO8601 = newNote.lastModifiedISO8601;
		newNote.json.assignmentId = null;
		newNote.json.projectId = this.showOnlyProjectId || null;
		
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

</script>