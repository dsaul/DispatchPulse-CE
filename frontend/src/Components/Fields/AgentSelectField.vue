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
				<v-card-title>Select an Agent</v-card-title>
				<v-divider></v-divider>
				<v-card-text >
					
					<AgentList
						:openEntryOnClick="false"
						:showMenuButton="false"
						@ClickEntry="OnClickEntry"
						class="e2e-agent-select-list"
						ref="list"
						/>
					
					
					<v-dialog
						v-model="newDialogueVisible"
						persistent
						scrollable
						:fullscreen="MobileDeviceWidth()"
						>
						<v-card>
							<v-card-title>Create and Select New Agent</v-card-title>
							<v-divider></v-divider>
							<v-card-text>
								<AgentEditor 
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
						<v-list-item
							@click="OpenContact()"
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
							@click="SelectNewAgent()"
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
						
						<!-- <div v-if="index"> -->
							<v-divider />
							<v-list-item
								@click="$emit('MoveUp', index)"
								:disabled="isFirstIndex || disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>arrow_upward</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Move Up</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="$emit('MoveDown', index)"
								:disabled="isLastIndex || disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>arrow_downward</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Move Down</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-divider />
							<v-list-item
								@click="$emit('InsertNewRowAtIndex', index)"
								:disabled="disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>add</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Add Row Above</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="$emit('InsertNewRowAtIndex', index + 1)"
								:disabled="disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>add</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Add Row Below</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="$emit('RemoveRowAtIndex', index)"
								:disabled="disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>remove</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Remove This Row</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
						<!-- </div> -->
						
						<slot name="custom-menu-options"></slot>
						
					</v-list>
				</v-menu>
			</template>
			
			
			
			
			<div style="margin-top: 5px; margin-bottom: 5px; width: 100%; cursor: text; display: flex; align-items: stretch;">
				
				
				<div
					v-if="!Agent && value"
					>
					{{value}} (can't find)
				</div>
				<div 
					v-else-if="Agent"
					@click="SelectNewAgent()"
					:style="{
						flex: '1',
						display: 'flex',
						'align-items': 'flex-end',
						color: disabled ? 'rgba(0, 0, 0, 0.38)' : 'inherit',
					}"
					>
					{{AgentName || 'Empty agent name.'}}
				</div>
				<div
					v-else
					@click="SelectNewAgent()"
					style="flex: 1; text-align: center;"
					class="e2e-agent-select-field-select-or-add-agent"
					>
					<v-btn
						color="primary"
						text
						:disabled="disabled || readonly"
						>
						Select or Add Agent
					</v-btn>
				</div>
				<div
					v-if="Agent && !isDialogue"
					class="d-none d-sm-flex"
					>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								:to="`/section/agents/${value}?tab=Agenda`"
								color="primary"
								label
								outlined
								style="margin: 4px;"
								v-on="on">
								Open
								<v-icon right small>open_in_new</v-icon>
							</v-chip>
						</template>
						<span>Open this agent.</span>
					</v-tooltip>
				</div>
			</div>
			
			
			
			
			<!-- <div style="margin-top: 5px; margin-bottom: 5px; width: 100%; cursor: text;" :class="{
					'text-align-center': !Agent
				}">
				<span v-if="Agent">{{AgentName || 'No agent name.'}}</span>
				<span v-else>
					<v-btn color="primary" text>Select or Add Agent</v-btn>
				</span>
			</div> -->
			
			
			
		</v-input>
	</div>
			
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import FieldBase from './FieldBase';
import AgentList from '@/Components/Lists/AgentsList.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import AgentEditor from '@/Components/Editors/AgentEditor.vue';
import { DateTime } from 'luxon';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AgentList,
		AgentEditor,
	},
	
})

export default class AgentSelectField extends FieldBase {
	
	@Prop({ default: 'Agent' }) declare public readonly label: string | null;
	
	
	public $refs!: {
		input: Vue,
		editor: AgentEditor,
		list: AgentList,
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected newDialogueVisible = false;
	protected newDialogueObject: IAgent | null = null;
	
	
	public mounted(): void {
		//console.log('mounted', this.$refs);
		
		if (null != this.value && !IsNullOrEmpty(this.value)) {

			const agent = Agent.ForId(this.value);
			if (null == agent) {
				const id = this.value;
				SignalRConnection.Ready(() => {
					BillingPermissionsBool.Ready(() => {
						Agent.FetchForId(id);
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
	
	
	get Agent(): IAgent | null {
		
		if (this.value === null || IsNullOrEmpty(this.value)) {
			return null;
		}
		
		return Agent.ForId(this.value);
		
	}
	
	
	get AgentName(): string | null {
		const agent = this.Agent;
		if (!agent) {
			return null;
		}
		
		return agent.json.name || null;
		
	}
	
	protected SelectNewAgent(): void {
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}
		
		console.log('SelectNewAgent()');
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
	
	protected OpenContact(): void {
		console.log('OpenContact()');
		
		if (!IsNullOrEmpty(this.value)) {
			this.$router.push(`/section/agents/${this.value}?tab=Agenda`).catch(((e: Error) => { }));// eslint-disable-line
		} else {
			console.error('can\'t open contact as IsNullOrEmpty(this.value)');
		}
		
	}
	
	protected OnClickEntry(id: string): void {
		console.debug('OnClickEntry', id);
		
		this.$emit('input', id);
		this.selectDialogOpen = false;
	}
	
	protected OpenNewDialogue(): void {
		
		console.debug('OpenNewDialogue()');
		
		this.newDialogueObject = Agent.GetEmpty();
		this.newDialogueVisible = true;
		
	}
	
	protected NewDialogueCancel(): void {
		console.debug('NewDialogueCancel()');
		
		this.$refs.editor.ResetValidation();
		this.newDialogueObject = Agent.GetEmpty();
		this.newDialogueVisible = false;
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected NewDialogueSaveAndAdd(): void {
		console.debug('NewDialogueSaveAndAdd()', this.newDialogueObject);
		
		if (this.newDialogueObject && this.$refs.editor.IsValidated()) {
			
			// First add the agent.
			const state = this.newDialogueObject as IAgent;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				
				const payload: Record<string, IAgent> = {};
				payload[state.id] = state;
				Agent.UpdateIds(payload);
				
				//this.$router.push(`/section/agents/${state.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
				
				// Then choose this new agent.
				this.$emit('input', state.id);
				
				// Close new dialogue.
				this.$refs.editor.ResetValidation();
				this.newDialogueObject = Agent.GetEmpty();
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

Vue.component('AgentSelectField', AgentSelectField);

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