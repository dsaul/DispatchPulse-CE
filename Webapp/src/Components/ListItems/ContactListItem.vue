<template>
	<div class="e2e-contact-list-item">
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>person</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title  style="white-space: normal;">{{value.json.name || 'No Name'}}</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">
					
					<v-tooltip
						v-if="value != null && value.json.companyId && CompanyForId(value.json.companyId)"
						top
						>
						
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<!--<v-avatar left>
									<v-icon small>fa-hourglass-end</v-icon>
								</v-avatar>-->
								{{CompanyForId(value.json.companyId).json.name}}
							</v-chip>
						</template>
						<span>Company</span>
					</v-tooltip>
					
					<v-tooltip
						v-if="value != null && value.json.title"
						top
						>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip
								label
								outlined
								small
								style="margin-right: 5px;"
								v-on="on"
								>
								<!--<v-avatar left>
									<v-icon small>fa-hourglass-end</v-icon>
								</v-avatar>-->
								{{value.json.title}}
							</v-chip>
						</template>
						<span>Title</span>
					</v-tooltip>
					
					
				</v-list-item-subtitle>
			</v-list-item-content>

			<v-list-item-action v-if="showMenuButton">
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
							:disabled="isDialogue || disabled"
							@click="$emit('OpenEntry', value.id)"
							>
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item
							@click="$emit('DeleteEntry', value.id)"
							:disabled="disabled || !PermContactsCanDelete()"
							>
							<v-list-item-icon>
								<v-icon>delete</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Delete…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						
						<div v-if="PhoneNumbers.length > 0">
							<v-divider />
							
							<v-subheader style="height: 20px; padding-top: 10px;">Call at…</v-subheader>
							
							<v-list-item
								v-for="number in PhoneNumbers "
								:key="number.value"
								@click.stop="CallNum(number.value)"
								:disabled="disabled"
								>
								<v-list-item-icon>
									<v-icon>phone</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>{{number.value}} ({{number.label}})</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
						</div>
						
						<div v-if="PhoneNumbers.length > 0">
							<v-divider />
							
							<v-subheader style="height: 20px; padding-top: 10px;">Text at…</v-subheader>
							
							<v-list-item
								v-for="number in PhoneNumbers "
								:key="number.value"
								@click.stop="SMSNum(number.value)"
								:disabled="disabled"
								>
								<v-list-item-icon>
									<v-icon>textsms</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>{{number.value}} ({{number.label}})</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
						</div>
						
						<div v-if="EMail.length > 0">
							<v-divider />
							
							<v-subheader style="height: 20px; padding-top: 10px;">E-Mail at…</v-subheader>
							
							<v-list-item
								v-for="email in EMail"
								:key="email.value"
								@click.stop="SendEmail(email.value)"
								:disabled="disabled"
								>
								<v-list-item-icon>
									<v-icon>email</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>{{email.value}} ({{email.label}})</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
						</div>
						
					</v-list>
				</v-menu>
			</v-list-item-action>
		</v-list-item>
	</div>

</template>

<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import ListItemBase from './ListItemBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Company } from '@/Data/CRM/Company/Company';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import SignalRConnection from '@/RPC/SignalRConnection';
import _ from 'lodash';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { IEMail } from '@/Data/Models/EMail/EMail';
import { IAddress } from '@/Data/Models/Address/Address';
import { IPhoneNumber } from '@/Data/Models/PhoneNumber/PhoneNumber';

@Component({
	
})
export default class ContactListItem extends ListItemBase {
	
	@Prop({ default: null }) declare public readonly value: IContact;
	
	protected CompanyForId = Company.ForId;
	protected PermContactsCanDelete = Contact.PermContactsCanDelete;
	
	protected loadingData = false;
	
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				if (this.value == null) {
					return;
				}
				
				const promises: Array<Promise<any>> = [];
				
				if (null != this.value.json.companyId && !IsNullOrEmpty(this.value.json.companyId)) {
					
					const company = Company.ForId(this.value.json.companyId);
					if (null == company && Company.PermCompaniesCanRequest()) {
						
						const rtr = Company.FetchForId(this.value.json.companyId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
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
		
		
	}
	
	
	
	protected get PhoneNumbers(): IPhoneNumber[] {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.phoneNumbers ||
			this.value.json.phoneNumbers.length === 0) {
			return [];
		}
		
		const copy = _.filter(this.value.json.phoneNumbers, (value) => {
			return !IsNullOrEmpty(value.value);
		});
		
		return copy;
	}
	
	protected get Addresses(): IAddress[] {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.addresses ||
			this.value.json.addresses.length === 0) {
			return [];
		}
		
		const copy = _.filter(this.value.json.addresses, (value) => {
			return !IsNullOrEmpty(value.value);
		});
		
		
		return copy;
	}
	
	protected get EMail(): IEMail[] {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.emails ||
			this.value.json.emails.length === 0) {
			return [];
		}
		
		const copy = _.filter(this.value.json.emails, (value) => {
			return !IsNullOrEmpty(value.value);
		});
		
		
		return copy;
	}
	
	protected CallNum(num: string): void {
		
		const mod = num.replace(/\D/g, '');
		window.location.href = `tel:${mod}`;
	}
	
	protected SMSNum(num: string): void {
		const mod = num.replace(/\D/g, '');
		(window as any).location = `sms:${mod}`;
	}
	
	protected SendEmail(email: string): void {
		(window as any).location = `mailto:${email}`;
	}
	
	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}
	
}

</script>