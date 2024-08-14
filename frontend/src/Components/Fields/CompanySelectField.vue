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
				<v-card-title>Select a Company</v-card-title>
				<v-divider></v-divider>
				<v-card-text >
					
					<CompanyList
						:showMenuButton="false"
						:openEntryOnClick="false"
						@ClickEntry="OnClickEntry"
						class="e2e-company-select-list"
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
								<CompanyEditor 
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
							@click="OpenCompany()"
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
							@click="SelectNewCompany()"
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
					v-if="!Company && value"
					>
					{{value}} (can't find)
				</div>
				<div 
					v-else-if="CompanyForId(value)"
					@click="SelectNewCompany()"
					:style="{
						flex: '1',
						display: 'flex',
						'align-items': 'flex-end',
						color: disabled ? 'rgba(0, 0, 0, 0.38)' : 'inherit',
					}"
					>
					{{CompanyNameForId(value) || 'Empty company name.'}}
				</div>
				<div
					v-else
					@click="SelectNewCompany()"
					style="flex: 1; text-align: center;"
					class="e2e-company-select-field-select-or-add-company"
					>
					<v-btn
						color="primary"
						text
						:disabled="disabled || readonly"
						>
						Select or Add Company
					</v-btn>
				</div>
				<div
					v-if="CompanyForId(value) && !isDialogue"
					class="d-none d-sm-flex"
					>
					<v-tooltip
						top
						>
						<template v-slot:activator="{ on }">
							<v-chip
								:to="`/section/companies/${value}?tab=General`"
								color="primary"
								label
								outlined
								style="margin: 4px;"
								v-on="on">
								Open
								<v-icon right small>open_in_new</v-icon>
							</v-chip>
						</template>
						<span>Open this company.</span>
					</v-tooltip>
				</div>
			</div>
			
		</v-input>
		<div v-if="showCompanyContactsBelow == true && value != null">
			<div v-for="contact of ContactsForCompanyId(value)" :key="contact.id" style="font-size:14px;">
				<router-link
					:to="`/section/contacts/${contact.id}`">{{contact.json.name}} <span v-if="contact.json.title"> &mdash; {{contact.json.title}}</span></router-link>
			</div>
			
		</div>
	</div>
			
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import CompanyList from '@/Components/Lists/CompanyList.vue';
import FieldBase from './FieldBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import CompanyEditor from '@/Components/Editors/CompanyEditor.vue';
import { DateTime } from 'luxon';
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import { Contact } from '@/Data/CRM/Contact/Contact';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		CompanyList,
		CompanyEditor,
	},
	
})

export default class CompanySelectField extends FieldBase {
	
	@Prop({ default: 'Company' }) declare public readonly label: string | null;
	@Prop({ default: false }) public readonly showCompanyContactsBelow!: boolean | null;
	
	
	public $refs!: {
		input: Vue,
		editor: CompanyEditor,
		list: CompanyList,
	};
	
	protected ContactsForCompanyId = Contact.ForCompanyId;
	protected CompanyForId = Company.ForId;
	protected CompanyNameForId = Company.NameForId;
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected newDialogueVisible = false;
	protected newDialogueObject: ICompany | null = null;
	
	public mounted(): void {
		//console.log('mounted', this.$refs);
		//console.debug('company select field ', this, this.$store.state.Database.companies);
		
		//console.log(this.value);
		if (null != this.value && !IsNullOrEmpty(this.value)) {

			const company = Company.ForId(this.value);
			if (null == company) {
				const id = this.value;
				SignalRConnection.Ready(() => {
					BillingPermissionsBool.Ready(() => {
						Company.FetchForId(id);
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
	
	protected get Company(): ICompany | null {
		const company = Company.ForId(this.value);
		if (!company) {
			return null;
		}
		return company;
	}
	
	protected SelectNewCompany(): void {
		
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}
		
		
		console.log('SelectNewCompany()');
		
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
	
	protected OpenCompany(): void {
		
		
		
		if (!IsNullOrEmpty(this.value)) {
			this.$router.push(`/section/companies/${this.value}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		} else {
			console.error('Can\'t go to the company as value is null');
		}
	}
	
	protected OnClickEntry(id: string): void {
		//console.debug('OnSelectedId', id);
		
		this.$emit('input', id);
		this.selectDialogOpen = false;
	}
	
	protected OpenNewDialogue(): void {
		
		console.debug('OpenNewDialogue()');
		
		this.newDialogueObject = Company.GetEmpty();
		this.newDialogueVisible = true;
		
	}
	
	protected NewDialogueCancel(): void {
		console.debug('NewDialogueCancel()');
		
		this.$refs.editor.ResetValidation();
		this.newDialogueObject = Company.GetEmpty();
		this.newDialogueVisible = false;
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected NewDialogueSaveAndAdd(): void {
		console.debug('NewDialogueSaveAndAdd()', this.newDialogueObject);
		
		if (this.newDialogueObject && this.$refs.editor.IsValidated()) {
			
			// First add the company.
			const state = this.newDialogueObject as ICompany;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				
				const payload: Record<string, ICompany> = {};
				payload[state.id] = state;
				Company.UpdateIds(payload);
				
				
				// Then choose this new company.
				this.$emit('input', state.id);
				
				// Close new dialogue.
				this.$refs.editor.ResetValidation();
				this.newDialogueObject = Company.GetEmpty();
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

Vue.component('CompanySelectField', CompanySelectField);

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