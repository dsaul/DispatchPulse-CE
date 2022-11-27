<template>
	<div
		:class="{
			outerdiv: true,
		}"
		>
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
				<v-card-title>Select a Recording</v-card-title>
				<v-divider></v-divider>
				<v-card-text >
					
					<RecordingList
						:showMenuButton="false"
						:openEntryOnClick="false"
						@ClickEntry="OnClickEntry"
						ref="list"
						/>
					
					<v-dialog
						v-model="newDialogueVisible"
						persistent
						scrollable
						:fullscreen="MobileDeviceWidth()"
						>
						<v-card>
							<v-card-title>Create and Select New Recording</v-card-title>
							<v-divider></v-divider>
							<v-card-text>
								<RecordingEditor 
									ref="editor"
									v-model="newDialogueObject"
									:showAppBar="false"
									:showFooter="false "
									preselectTabName="Admin"
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
						<!-- <v-list-item
							@click="OpenRecording()"
							:disabled="isDialogue || IsValueEmpty || disabled"
							>
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-divider /> -->
						<v-list-item
							@click="SelectNewRecording()"
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
					v-if="!Recording && value"
					>
					{{value}} (can't find)
				</div>
				<div 
					v-else-if="RecordingForId(value)"
					@click="SelectNewRecording()"
					:style="{
						flex: '1',
						display: 'flex',
						'align-items': 'flex-end',
						color: disabled ? 'rgba(0, 0, 0, 0.38)' : 'inherit',
					}"
					>
					{{RecordingNameForId(value) || 'Empty recording name.'}}
				</div>
				<div
					v-else
					@click="SelectNewRecording()"
					style="flex: 1; text-align: center;"
					>
					<v-btn
						color="primary"
						text
						:disabled="disabled || readonly"
						>
						Select or Add Recording
					</v-btn>
				</div>
				<!-- <div
					v-if="RecordingForId(value) && !isDialogue"
					class="d-none d-sm-flex"
					>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								:to="`/section/recordings/${value}?tab=General`"
								color="primary"
								label
								outlined
								style="margin: 4px;"
								v-on="on">
								Open
								<v-icon right small>open_in_new</v-icon>
							</v-chip>
						</template>
						<span>Open this recording.</span>
					</v-tooltip>
				</div> -->
			</div>
			
		</v-input>
	</div>
			
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import FieldBase from './FieldBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import RecordingEditor from '@/Components/Editors/RecordingEditor.vue';
import { DateTime } from 'luxon';
import { IRecording, Recording } from '@/Data/CRM/Recording/Recording';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import EditorBase from '../Editors/EditorBase';
import RecordingList from '@/Components/Lists/RecordingList.vue';

@Component({
	components: {
		RecordingList,
		RecordingEditor,
	},
	
})

export default class RecordingSelectField extends FieldBase {
	
	@Prop({ default: 'Recording' }) declare public readonly label: string | null;
	
	
	public $refs!: {
		input: Vue,
		editor: RecordingEditor,
		list: RecordingList,
	};
	
	protected RecordingForId = Recording.ForId;
	protected RecordingNameForId = Recording.NameForId;
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected newDialogueVisible = false;
	protected newDialogueObject: IRecording | null = null;
	
	public mounted(): void {
		//console.log('mounted', this.$refs);
		//console.debug('recording select field ', this, this.$store.state.Database.recordings);
		
		//console.log(this.value);
		if (null != this.value && !IsNullOrEmpty(this.value)) {

			const recording = Recording.ForId(this.value);
			if (null == recording) {
				const id = this.value;
				SignalRConnection.Ready(() => {
					BillingPermissionsBool.Ready(() => {
						Recording.FetchForId(id);
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
	
	protected get Recording(): IRecording | null {
		const recording = Recording.ForId(this.value);
		if (!recording) {
			return null;
		}
		return recording;
	}
	
	protected SelectNewRecording(): void {
		
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}
		
		
		console.log('SelectNewRecording()');
		
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
		
		console.log('ClearValue()');
		this.$emit('input', null);
		
		this.selectDialogOpen = false;
	}
	
	protected OpenRecording(): void {
		
		
		
		if (!IsNullOrEmpty(this.value)) {
			this.$router.push(`/section/recordings/${this.value}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		} else {
			console.error('Can\'t go to the recording as value is null');
		}
	}
	
	protected OnClickEntry(id: string): void {
		//console.debug('OnSelectedId', id);
		
		this.$emit('input', id);
		this.selectDialogOpen = false;
	}
	
	protected OpenNewDialogue(): void {
		
		console.debug('OpenNewDialogue()');
		
		this.newDialogueObject = Recording.GetEmpty();
		this.newDialogueVisible = true;
		
	}
	
	protected NewDialogueCancel(): void {
		console.debug('NewDialogueCancel()');
		
		this.$refs.editor.ResetValidation();
		this.newDialogueObject = Recording.GetEmpty();
		this.newDialogueVisible = false;
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected NewDialogueSaveAndAdd(): void {
		console.debug('NewDialogueSaveAndAdd()', this.newDialogueObject);
		
		if (this.newDialogueObject && (this.$refs.editor as EditorBase).IsValidated()) {
			
			// First add the recording.
			const state = this.newDialogueObject as IRecording;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				
				const payload: { [id: string]: IRecording; } = {};
				payload[state.id] = state;
				Recording.UpdateIds(payload);
				
				
				// Then choose this new recording.
				this.$emit('input', state.id);
				
				// Close new dialogue.
				this.$refs.editor.ResetValidation();
				this.newDialogueObject = Recording.GetEmpty();
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

Vue.component('RecordingSelectField', RecordingSelectField);

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