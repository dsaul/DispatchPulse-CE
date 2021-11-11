<template>
	<v-card
		:class="{
			'note-card': true,
			'entry-resolved': Resolved,
			'entry-no-longer-relevant': NoLongerRelevant,
		}"
		>
		<div style="flex: 1">
			<ContentStyledText 
				v-if="value.json.contentType == 'styled-text'"
				v-model="value.json.content"
				:strikethrough="NoLongerRelevant"
				/>
			<ContentCheckbox
				v-else-if="value.json.contentType == 'checkbox'"
				v-model="value.json.content"
				:note="value"
				/>
			<ContentImage
				v-else-if="value.json.contentType == 'image'"
				v-model="value.json.content"
				/>
			<ContentVideo
				v-else-if="value.json.contentType == 'video'"
				v-model="value.json.content"
				/>
			
			
			<v-card-text
				v-if="Tags"
				class="internal-notice"
				>
				
				<v-chip 
					label
					outlined
					small
					style="margin-right: 5px;"
					v-for="(tag, index) in Tags"
					:key="index"
					top
					>
					{{tag}}
				</v-chip>
				
				<v-chip 
					v-if="HasAssignment"
					label
					outlined
					small
					style="margin-right: 5px;"
					top
					color="primary"
					:to="`/section/assignments/${value.json.assignmentId}?tab=General`"
					>
					Assignment
				</v-chip>
				
				<v-tooltip
					v-if="rootProject && rootProject.id !== value.json.projectId"
					top
					>
					<template v-slot:activator="{ on }" v-on="on">
						<v-chip 
							label
							outlined
							small
							style="margin-right: 5px;"
							v-on="on"
							color="primary"
							@click.stop.prevent.once=""
							:to="`/section/projects/${value.json.projectId}?tab=General`"
							>
							From a Child Project
						</v-chip>
					</template>
					<span>{{ProjectNameForId(value.id) || 'No Name'}}</span>
				</v-tooltip>
				
			</v-card-text>
			<v-row>
				<v-col cols="12" sm="6">
					<v-list-item class="grow">
						<v-avatar size="40" color="secondary" style="margin-right: 10px;">
							<span class="white--text">{{OriginalInitials}}</span>
						</v-avatar>

						<v-list-item-content style="padding-top: 0px;padding-bottom: 0px;">
							<v-list-item-title>{{OriginalName}}</v-list-item-title>
							<div class="caption">Posted {{OriginalTime}}</div>
						</v-list-item-content>
					</v-list-item>
				</v-col>
				<v-col
					v-if="LastModifiedName != OriginalName || LastModifiedTime != OriginalTime"
					cols="12"
					sm="6"
					>
					<v-list-item class="grow">
						<v-avatar size="40" color="secondary" style="margin-right: 10px;">
							<span class="white--text">{{LastModifiedInitials}}</span>
						</v-avatar>

						<v-list-item-content style="padding-top: 0px;padding-bottom: 0px;">
							<v-list-item-title>{{LastModifiedName}}</v-list-item-title>
							<div class="caption">Edited {{LastModifiedTime}}</div>
						</v-list-item-content>
					</v-list-item>
				</v-col>
			</v-row>
		</div>
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
					v-if="!Resolved"
					:disabled="Resolved || disabled || !PermProjectNotesCanPush()"
					@click="$emit('Resolved', value.id)"
					>
					<v-list-item-icon>
						<v-icon>done</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Mark Resolved</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					v-if="Resolved"
					:disabled="!Resolved || disabled || !PermProjectNotesCanPush()"
					@click="$emit('NotResolved', value.id)"
					>
					<v-list-item-icon>
						<v-icon>done</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Mark Not Resolved</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				
				
				<v-list-item
					v-if="!NoLongerRelevant"
					:disabled="NoLongerRelevant || disabled || !PermProjectNotesCanPush()"
					@click="$emit('NoLongerRelevant', value.id)"
					>
					<v-list-item-icon>
						<v-icon>cancel</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Mark No Longer Relevant</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					v-if="NoLongerRelevant"
					:disabled="!NoLongerRelevant || disabled || !PermProjectNotesCanPush()"
					@click="$emit('Relevant', value.id)"
					>
					<v-list-item-icon>
						<v-icon>cancel</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Mark Relevant</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				
				<v-list-item
					v-if="!InternalOnly"
					:disabled="InternalOnly || disabled || !PermProjectNotesCanPush()"
					@click="$emit('MarkInternalOnly', value.id)"
					>
					<v-list-item-icon>
						<v-icon>business</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Mark Internal Only</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item
					v-if="InternalOnly"
					:disabled="!InternalOnly || disabled || !PermProjectNotesCanPush()"
					@click="$emit('MarkNotInternalOnly', value.id)"
					>
					<v-list-item-icon>
						<v-icon>business</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Mark Not Internal Only</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				
				
				
				
				<v-divider />
				<v-list-item
					@click="$emit('DeleteEntry', value.id)"
					:disabled="disabled || !PermProjectNotesCanDelete()"
					>
					<v-list-item-icon>
						<v-icon>delete</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Delete Note</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				
			</v-list>
		</v-menu>
	</v-card>
</template>
<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import CardBase from '../CardBase';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ContentCheckbox from '@/Components/Cards/ProjectNote/ContentCheckbox.vue';
import ContentImage from '@/Components/Cards/ProjectNote/ContentImage.vue';
import ContentStyledText from '@/Components/Cards/ProjectNote/ContentStyledText.vue';
import ContentVideo from '@/Components/Cards/ProjectNote/ContentVideo.vue';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';

@Component({
	components: {
		ContentCheckbox,
		ContentImage,
		ContentStyledText,
		ContentVideo,
	},
})
export default class ProjectNoteCard extends CardBase {
	
	@Prop({ default: null }) public readonly value!: IProjectNote;
	@Prop({ default: null }) public readonly rootProject!: IProject;
	
	protected ProjectNameForId = Project.NameForId;
	protected PermProjectNotesCanPush = ProjectNote.PermProjectNotesCanPush;
	protected PermProjectNotesCanDelete = ProjectNote.PermProjectNotesCanDelete;
	
	constructor() {
		super();
		
		//console.log('ProjectNoteCard', this.value.json.contentType);
	}
	
	get HasAssignment(): boolean {
		
		if (!this.value ||
			!this.value.json
			) {
			return false;
		}
		
		return !!this.value.json.assignmentId;
	}
	
	
	get LastModifiedTime(): string | null {
		
		if (!this.value ||
			!this.value.lastModifiedISO8601 || 
			IsNullOrEmpty(this.value.lastModifiedISO8601)
			) {
			return null;
		}
		
		const d =  DateTime.fromISO(this.value.lastModifiedISO8601);
		return d.toLocaleString(DateTime.DATETIME_FULL);
	}
	
	get LastModifiedName(): string | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.lastModifiedBillingId ||
			IsNullOrEmpty(this.value.json.lastModifiedBillingId)
			) {
			return null;
		}
		
		const contact = BillingContacts.ForId(this.value.json.lastModifiedBillingId);
		if (!contact) {
			return null;
		}
		return contact.fullName;
	}
	
	get LastModifiedInitials(): string | null {
		
		const name: string | null = this.LastModifiedName;
		if (name == null) {
			return null;
		}
		
		const matches = name.match(/\b(\w)/g);
		if (matches == null) {
			return null;
		}
		
		let result = '';
		
		for (const part of matches) {
			if (result.length >= 2) {
				break;
			}
			if (IsNullOrEmpty(part)) {
				continue;
			}
			
			result += part.toUpperCase();
		}
		
		return IsNullOrEmpty(result) ? null : result;
	}
	
	
	
	
	
	get OriginalTime(): string | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.originalISO8601 ||
			IsNullOrEmpty(this.value.json.originalISO8601)
			) {
			return null;
		}
		
		const d =  DateTime.fromISO(this.value.json.originalISO8601);
		return d.toLocaleString(DateTime.DATETIME_FULL);
	}
	
	get OriginalName(): string | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.originalBillingId ||
			IsNullOrEmpty(this.value.json.originalBillingId)
			) {
			return null;
		}
		
		const contact = BillingContacts.ForId(this.value.json.originalBillingId);
		if (!contact) {
			return null;
		}
		return contact.fullName;
	}
	
	get OriginalInitials(): string | null {
		
		const name: string | null = this.OriginalName;
		if (name == null) {
			return null;
		}
		
		const matches = name.match(/\b(\w)/g);
		if (matches == null) {
			return null;
		}
		
		let result = '';
		
		for (const part of matches) {
			if (result.length >= 2) {
				break;
			}
			if (IsNullOrEmpty(part)) {
				continue;
			}
			
			result += part.toUpperCase();
		}
		
		return IsNullOrEmpty(result) ? null : result;
	}
	
	get InternalOnly(): boolean {
		
		if (!this.value ||
			!this.value.json
			) {
			return false;
		}
		
		return this.value.json.internalOnly;
	}
	
	
	get Resolved(): boolean {
		
		if (!this.value ||
			!this.value.json
			) {
			return false;
		}
		
		return this.value.json.resolved;
	}
	
	
	get NoLongerRelevant(): boolean {
		
		if (!this.value ||
			!this.value.json
			) {
			return false;
		}
		
		return this.value.json.noLongerRelevant;
	}
	
	get Tags(): string[] {
		
		const ret = [];
		
		if (this.InternalOnly) {
			ret.push('Internal Only');
		}
		if (this.Resolved) {
			ret.push('Resolved');
		}
		if (this.NoLongerRelevant) {
			ret.push('No Longer Relevant');
		}
		
		return ret;
		
	}
	
	get TagsCommaSeparated(): string | null {
		
		let ret = '';
		const tags = this.Tags;
		
		for (let i = 0; i < tags.length; i++) {
			
			if (i !== 0) {
				ret += ', ';
			}
			
			if (i !== 0 && i === tags.length - 1) {
				ret += 'and ';
			}
			
			ret += tags[i];
			
			
		}
		
		if (IsNullOrEmpty(ret)) {
			return null;
		}
		
		return ret;
	}
	
	
	
	
	
}
</script>
<style scoped>

.note-card {
	margin: 30px;
	padding: 10px;
	display: flex;
}

.relation-notice {
	margin: 0px;
	margin-top: 5px;
	margin-bottom: 5px;
	padding-top: 0px;
	padding-bottom: 0px;
	font-size: 12px;
}

.internal-notice {
	
	margin: 0px;
	margin-top: 5px;
	margin-bottom: 5px;
	padding-top: 0px;
	padding-bottom: 0px;
	font-size: 12px;
}

.entry-resolved {
	
	opacity: 0.5;
	/* background-color: rgba(89,143,36,0.2); */
}

.entry-no-longer-relevant {
	
	opacity: 0.5;
	/* background-color: rgba(142,42,35,0.2); */
}

</style>