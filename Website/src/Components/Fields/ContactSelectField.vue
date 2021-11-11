<template>
	<div class="outerdiv" style="border-bottom: ">
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
				<v-card-title>Select a Contact</v-card-title>
				<v-divider></v-divider>
				<v-card-text >
					
					<ContactList
						:openEntryOnClick="false"
						:showMenuButton="false"
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
							<v-card-title>Create and Select New Contact</v-card-title>
							<v-divider></v-divider>
							<v-card-text>
								<ContactEditor 
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
							@click="SelectNewContact()"
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
					v-if="!Contact && value"
					>
					{{value}} (can't find)
				</div>
				<div 
					v-else-if="ContactForId(value)"
					@click="SelectNewContact()"
					:style="{
						flex: '1',
						display: 'flex',
						'align-items': 'flex-end',
						color: disabled ? 'rgba(0, 0, 0, 0.38)' : 'inherit',
					}"
					>
					{{ContactNameForId(value) || 'Empty contact name.'}}
				</div>
				<div
					v-else
					@click="SelectNewContact()"
					:disabled="disabled || readonly"
					style="flex: 1; text-align: center;"
					>
					<v-btn color="primary" text>Select or Add Contact</v-btn>
				</div>
				<div
					v-if="ContactForId(value) && !isDialogue"
					class="d-none d-sm-flex"
					>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								:to="`/section/contacts/${value}?tab=General`"
								color="primary"
								label
								outlined
								style="margin: 4px;"
								v-on="on">
								Open
								<v-icon right small>open_in_new</v-icon>
							</v-chip>
						</template>
						<span>Open this contact.</span>
					</v-tooltip>
				</div>
			</div>
			
			
			
			
			
			
			
			
			<!-- <div style="margin-top: 5px; margin-bottom: 5px; width: 100%; cursor: text;" :class="{
					'text-align-center': !ContactForId(value)
				}">
				<span v-if="ContactForId(value)">{{ContactNameForId(value) || 'No contact name.'}}</span>
				<span v-else>
					<v-btn color="primary" text>Select or Add Contact</v-btn>
				</span>
			</div> -->
		</v-input>
		
		<div v-if="showDetails && ContactForId(value) && ContactForId(value).json.phoneNumbers && ContactForId(value).json.phoneNumbers.length > 0" style="margin-bottom: 20px;">
			<div class="subtitle-1" style="font-weight: bold;">Phone Numbers</div>
			<table>
				<tr v-for="(obj) in ContactForId(value).json.phoneNumbers" :key="obj.id">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
						<span v-if="obj.label">{{obj.label}}:</span>
					</td>
					<td style="font-size: 14px;"><a :href="`tel:${obj.value.replace(/\D/g, '')}`">{{obj.value}}</a></td>
				</tr>
			</table>
		</div>
		
		<div v-if="showDetails && ContactForId(value) && ContactForId(value).json.addresses && ContactForId(value).json.addresses.length > 0" style="margin-bottom: 20px;">
			<div class="subtitle-1" style="font-weight: bold;">Addresses</div>
			<table>
				<tr v-for="(obj) in ContactForId(value).json.addresses" :key="obj.id">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
						<span v-if="obj.label">{{obj.label}}:</span>
					</td>
					<td style="font-size: 14px;">
						<a :href="`https://www.google.com/maps/dir/current+location/${obj.value.replace(/[\r\n\x0B\x0C\u0085\u2028\u2029\/]+/g, ' ')}`" target="_blank">{{obj.value}}</a>
					</td>
				</tr>
			</table>
		</div>
		
		<div v-if="showDetails && ContactForId(value) && ContactForId(value).json.emails && ContactForId(value).json.emails.length > 0" style="margin-bottom: 20px;">
			<div class="subtitle-1" style="font-weight: bold;">E-Mails</div>
			<table>
				<tr v-for="(obj) in ContactForId(value).json.emails" :key="obj.id">
					<td style="font-size: 14px; padding-right:5px; white-space:nowrap;">
						<span v-if="obj.label">{{obj.label}}:</span>
					</td>
					<td style="font-size: 14px;"><a :href="`mailto:${obj.value}`">{{obj.value}}</a></td>
				</tr>
			</table>
		</div>
		
	</div>
			
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import FieldBase from './FieldBase';
import ContactList from '@/Components/Lists/ContactList.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ContactEditor from '@/Components/Editors/ContactEditor.vue';
import { DateTime } from 'luxon';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		ContactList,
		ContactEditor,
	},
	
})

export default class ContactSelectField extends FieldBase {
	
	@Prop({ default: 'Contact' }) declare public readonly label: string | null;
	@Prop({ default: false }) public readonly showCompanyContactsBelow!: boolean | null;
	
	public $refs!: {
		input: Vue,
		editor: ContactEditor,
		list: ContactList,
	};
	
	protected ContactForId = Contact.ForId;
	protected ContactNameForId = Contact.NameForId;
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected newDialogueVisible = false;
	protected newDialogueObject: IContact | null = null;
	
	public mounted(): void {
		//console.log('mounted', this.$refs);
		
		if (null != this.value && !IsNullOrEmpty(this.value)) {

			const contact = Contact.ForId(this.value);
			if (null == contact) {
				const id = this.value;
				SignalRConnection.Ready(() => {
					BillingPermissionsBool.Ready(() => {
						Contact.FetchForId(id);
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
	
	protected get Contact(): IContact | null {
		const contact = Contact.ForId(this.value);
		if (!contact) {
			return null;
		}
		return contact;
	}
	
	protected SelectNewContact(): void {
		
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}
		
		console.log('SelectNewContact()');
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
			this.$router.push(`/section/contacts/${this.value}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
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
		
		this.newDialogueObject = Contact.GetEmpty();
		this.newDialogueVisible = true;
		
	}
	
	protected NewDialogueCancel(): void {
		console.debug('NewDialogueCancel()');
		
		this.$refs.editor.ResetValidation();
		this.newDialogueObject = Contact.GetEmpty();
		this.newDialogueVisible = false;
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected NewDialogueSaveAndAdd(): void {
		console.debug('NewDialogueSaveAndAdd()', this.newDialogueObject);
		
		if (this.newDialogueObject && this.$refs.editor.IsValidated()) {
			
			// First add the agent.
			const state = this.newDialogueObject as IContact;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				
				const payload: Record<string, IContact> = {};
				payload[state.id] = state;
				Contact.UpdateIds(payload);
				
				
				// Then choose this new agent.
				this.$emit('input', state.id);
				
				// Close new dialogue.
				this.$refs.editor.ResetValidation();
				this.newDialogueObject = Contact.GetEmpty();
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

Vue.component('ContactSelectField', ContactSelectField);

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