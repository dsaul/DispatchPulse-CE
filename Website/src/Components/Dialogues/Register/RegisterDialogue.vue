<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Register</v-card-title>
			<v-divider></v-divider>
			<v-card-text>
				
				<v-alert type="info" colored-border elevation="2" border="bottom" style="margin-top: 20px;">
					Hi There! Feel free to give us a call at <a href="tel:+18447778556">(844) 777-8556</a>, we'd love to set this up for you! If you want to do it yourself however, please fill out the below form:
				</v-alert>
				
				<v-stepper v-model="CurrentDialogueStep" vertical  style="-webkit-box-shadow: inherit; box-shadow: inherit;">
					<v-stepper-step :complete="RegisterCompanyComplete" step="1">
						Create Your Company
						<small>We'll need to see if your company name and abbreviation are avaliable.</small>
					</v-stepper-step>
					
					<v-stepper-content step="1">
						<v-form
							autocomplete="newpassword"
							ref="companyForm"
							>
							<v-container>
								<v-row>
									<v-col cols="12">
										<div class="title">How should we call you?</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyName"
											autocomplete="newpassword"
											label="Company Name"
											hint="The full name of your company, for example &quot;Dispatch Pulse&quot;."
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyAbbreviation"
											autocomplete="newpassword"
											label="Company Abbreviation"
											hint="An abbreviation for your company, for example &quot;DP&quot;."
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
										
										<v-alert
											v-if="CompanyAbbreviationInUse"
											border="bottom"
											colored-border
											type="error"
											elevation="2"
											style="margin-top: 10px;"
											>
											The abbreviation "{{CompanyAbbreviation}}" is already in use.
										</v-alert>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12">
										<div class="title">A bit about your company&hellip;</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyIndustry"
											autocomplete="newpassword"
											label="Industry (optional)"
											hint="What industry is your company in?"
											:rules="[  ]"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyMarketingCampaign"
											autocomplete="newpassword"
											label="How did you hear about us? (optional)"
											hint=""
											:rules="[  ]"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12">
										<div class="title">Where are you?</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyAddressLine1"
											autocomplete="newpassword"
											label="Address Line 1"
											hint=""
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyAddressLine2"
											autocomplete="newpassword"
											label="Address Line 2 (optional)"
											hint=""
											:rules="[  ]"
											>
										</v-text-field>
									</v-col>
									<v-col cols="6" style="padding-right: 5px;">
										<v-text-field
											ref="item"
											v-model="CompanyAddressCity"
											autocomplete="newpassword"
											label="City"
											hint=""
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
									<v-col cols="6" style="padding-left: 5px;">
										<v-text-field
											ref="item"
											v-model="CompanyAddressProvince"
											autocomplete="newpassword"
											label="Province / State"
											hint=""
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
									<v-col cols="6" style="padding-right: 5px;">
										<v-text-field
											ref="item"
											v-model="CompanyAddressPostalCode"
											autocomplete="newpassword"
											label="Postal Code / Zip Code"
											hint=""
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
									<v-col cols="6" style="padding-left: 5px;">
										<v-text-field
											ref="item"
											v-model="CompanyAddressCountry"
											autocomplete="newpassword"
											label="Country"
											hint=""
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12">
										<div class="title">What is your contact information?</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="MainContactEMail"
											autocomplete="newpassword"
											label="Main Contact E-Mail"
											hint="What e-mail can we use to contact you?"
											:rules="[ ValidateRequiredField ]"
											type="email"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="MainContactPhoneNumber"
											autocomplete="newpassword"
											label="Main Contact Phone #"
											hint="What phone number can we use to contact you?"
											type="phone"
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
								</v-row>
							</v-container>
						</v-form>
						
						<div style="text-align: right; padding-bottom: 10px; padding-right: 10px;">
							<v-btn color="primary" @click="RegisterCompany()">Register Company</v-btn>
						</div>
					</v-stepper-content>
					
					<v-stepper-step :complete="CurrentDialogueStep > 2" step="2">
						Main Account
						<small>What you use to login, as well as how we should contact you.</small>
					</v-stepper-step>
					
					<v-stepper-content step="2">
						
						
						<v-form
							autocomplete="newpassword"
							ref="mainLoginForm"
							>
							<v-container>
								<v-row>
									<v-col cols="12">
										<div class="title">General</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="MainContactFullName"
											autocomplete="newpassword"
											label="Main Contact's Full Name"
											hint="The full name for the main contact, for example, John Doe."
											:rules="[ ValidateRequiredField ]"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12">
										<div class="title">Login Information</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="CompanyAbbreviation"
											autocomplete="newpassword"
											label="Company Abbreviation"
											hint="An abbreviation for your company, for example &quot;DP&quot;."
											:rules="[ ValidateRequiredField ]"
											readonly="readonly"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="MainContactEMail"
											autocomplete="newpassword"
											label="Login E-Mail"
											hint="You always log in with your email address"
											:rules="[ ValidateRequiredField ]"
											readonly="readonly"
											type="email"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="MainContactPassword1"
											autocomplete="newpassword"
											label="Password"
											hint="The password you'll use to login."
											:rules="[ ValidateRequiredField ]"
											type="password"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-text-field
											ref="item"
											v-model="MainContactPassword2"
											autocomplete="newpassword"
											label="Password, Again"
											hint="Type the password again, so we're sure we got it right."
											:rules="[ ValidateRequiredField ]"
											type="password"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12">
										<div class="title">Contact Preferences</div>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-switch
											v-model="MainContactEMailMarketing"
											label="Marketing Emails"
											style="margin-top:0px;"
											>
										</v-switch>
										<p>Your e-mail will not be sold, we will only use this to inform you of new products we've launched. Max 1 marketing email per month.</p>
									</v-col>
								</v-row>
								<v-row no-gutters>
									<v-col cols="12">
										<v-switch
											v-model="MainContactEMailTutorials"
											label="Tutorial Emails"
											style="margin-top:0px;"
											>
										</v-switch>
										<p>Your e-mail will not be sold, we will send you tutorials at a rate of 1 per week.</p>
									</v-col>
								</v-row>
							</v-container>
						</v-form>
						
						
						
						
						
						
						
						
						
						
						
						<div style="text-align: right; padding-bottom: 10px; padding-right: 10px;">
							<v-btn color="primary" @click="SaveMainUser()">Save and Continue</v-btn>
						</div>
					</v-stepper-content>
					
					<v-stepper-step :complete="CurrentDialogueStep > 3" step="3">
						Other Accounts
						<small>How many other people will be using this program?</small>
					</v-stepper-step>
					
					<v-stepper-content step="3">
						
						<p style="color: black;">Dispatch Pulse is billed based on how many people can use it at the same time. </p>
						
						<p style="color: black;">
							You always have your main account. If it is just you, or you only have one 
							administrative assistant entering time, and agents don't need to access project data
							then one account should be all you need.
						</p>
						
						<p style="color: black;">
							If more agents will need to use this app at the same time, for example, entering 
							their own time, or using any of the collaborative features, they should have 
							their own accounts. 
						</p>
						
						<p style="color: black;">Accounts cost ${{IPPerUserCost}} {{IPCurrency}} per account per month.</p>
						
						<p style="color: black;">
							There are volume discounts if you need many accounts, if you will order more than 5 
							additional accounts, complete this and then please give us a 
							call or send us an email and we'll adjust the billing.
						</p>
						
						<div v-for="(obj, index) of OtherAccountsToAdd" :key="index">
							<RegisterAdditionalAccount
								:value="OtherAccountsToAdd[index]"
								@input="ModifyRegisterAdditionalAccount(index, $event)"
								@remove="OnRemoveRegisterAdditionalAccount($event)"
								/>
						</div>
						<v-btn text @click="AddAnotherOtherAccount()">Add Another Account</v-btn>
						
						
						<div style="text-align: right; padding-bottom: 10px; padding-right: 10px; margin-top: 10px;">
							<v-btn color="primary" @click="SaveOtherAccounts()">Save and Continue</v-btn>
						</div>
						
						
						
					</v-stepper-content>
					
					<v-stepper-step :complete="CurrentDialogueStep > 4" step="4">Summary &amp; Payment Method</v-stepper-step>
					
					<v-stepper-content step="4">
						<p>How often do you want to pay?</p>
						<v-radio-group v-model="BillingFrequency" style="margin-left: 10px;">
							<v-radio
								label="Monthly"
								value="Monthly"
								>
							</v-radio>
							<v-radio
								label="Quarterly"
								value="Quarterly"
								>
							</v-radio>
							<v-radio
								label="Annually"
								value="Annually"
								>
							</v-radio>
						</v-radio-group>
						
						<template>
							<v-data-table dense :headers="summaryHeaders" :items="summaryValues" item-key="name"></v-data-table>
						</template>
						
						<p>
							Subtotal: <strong>{{SummaryRunningTotal}} {{ModelState.currency}}</strong> plus tax.
						</p>
						<p>
							We'll apply your appropriate sales tax rate on your first invoice.
						</p>
						<p>
							We do our invoicing via credit card payments provided by Stripe. You will 
							recieve an invoice via email with a link to pay your main email above. Our 
							service is pre-paid, with 2 weeks grace period after invoice arrival.
						</p>
						<p>
							If you have a quoted rate, complete the setup anyway as close as possible, 
							then let us know and we'll adjust your packaging.
						</p>
						
						
						
						
						
						
						
						<!--<form action="/charge" method="post" id="payment-form" ref="paymentForm">
						
							Cardholder name: <input id="cardholder-name" />
							
							
							<div class="form-row">
								<label for="card-element">
								Credit or debit card
								</label>
								<div id="card-element">
								</div>
								<div id="card-errors" role="alert"></div>
							</div>

							<button id="card-button">Submit Payment</button>
						</form>
						
						
						
						
						
						I authorise Dispatch Pulse/Saul and Partners to send instructions to the 
						financial institution that issued my card to take payments from my card 
						account in accordance with the terms of my agreement with you.-->
						
						
						<div style="text-align: right; padding-bottom: 10px; padding-right: 10px;">
							<v-btn color="primary" @click="SaveSummaryAndPaymentMethod()">Save and Continue</v-btn>
						</div>
					</v-stepper-content>
					
					<v-stepper-step :complete="CurrentDialogueStep > 5" step="5">Service Agreement</v-stepper-step>
					<v-stepper-content step="5">
						<div style="height: 200px; overflow: auto; border: 1px solid rgba(0, 0, 0, 0.2); padding: 10px;" ref="agreementText">
							<SLAContent />
						</div>
						<div style="color: rgba(0, 0, 0, 0.6); font-size: 12px;">Signature</div>
						<div><canvas width="300" height="150" ref="signatureCanvas" style="border: 1px solid rgba(0, 0, 0, 0.2);"></canvas></div>
						<div style="display: flex; padding-bottom: 10px; padding-right: 10px; margin-top: 20px;">
							<v-btn color='red darken-1' text @click='ClearSignature()'>Clear Signature</v-btn>
							<v-spacer />
							<v-btn color="primary" @click="SaveServiceAgreement()">Agree and Continue</v-btn>
						</div>
					</v-stepper-content>
					<v-stepper-step :complete="CurrentDialogueStep > 6" step="6">Setting up Services</v-stepper-step>
					<v-stepper-content step="6">
						<v-progress-circular
							indeterminate
							color="primary"
							>
						</v-progress-circular>
					</v-stepper-content>
				</v-stepper>

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-btn color='red darken-1' text @click='Cancel()'>Back to Login</v-btn>
				<v-spacer />
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang='ts'>
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import GenerateID from '@/Utility/GenerateID';
import { IRegisterAdditionalUser, IRegisterDialogueModelState, RegisterDialogueModelState } from '@/Data/Models/RegisterDialogueModelState/RegisterDialogueModelState';
import _ from 'lodash';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import SignalRConnection from '@/RPC/SignalRConnection';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import bcrypt from 'bcryptjs';
import SignaturePad from 'signature_pad';
import RegisterAdditionalAccount from './RegisterAdditionalAccount.vue';
import SLAContent from '@/Components/SLAContent.vue';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { BillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingPermissionsGroups } from '@/Data/Billing/BillingPermissionsGroups/BillingPermissionsGroups';
import { BillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingCurrency } from '@/Data/Billing/BillingCurrency/BillingCurrency';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingPaymentMethod } from '@/Data/Billing/BillingPaymentMethod/BillingPaymentMethod';
import { IPerformGetIPCurrencyPerUserCostCB } from '@/Data/Billing/BillingCurrency/RPCPerformGetIPCurrencyPerUserCost';
import { VForm } from '@/Components/Editors/EditorBase';
import { IPerformRegisterNewCompanyCB } from '@/Data/Billing/BillingCompanies/RPCPerformRegisterNewCompany';
import { IPerformRegisterMainUserCB } from '@/Data/Billing/BillingContacts/RPCPerformRegisterMainUser';
import { IPerformRegisterAdditionalUsersCB } from '@/Data/Billing/BillingContacts/RPCPerformRegisterAdditionalUsers';
import { IPerformRegisterPaymentInformationCB } from '@/Data/Billing/BillingPaymentMethod/RPCPerformRegisterPaymentInformation';
import { IPerformRegisterSaveServiceAgreementCB } from '@/Data/Billing/BillingCompanies/RPCPerformRegisterSaveServiceAgreement';
import { IPerformRegisterCreateDPDatabaseCB } from '@/Data/Billing/BillingCompanies/RPCPerformRegisterCreateDPDatabase';
import { IRequestCompanyNameInUseCB } from '@/Data/Billing/BillingCompanies/RPCRequestCompanyNameInUse';

interface IRegisterSummaryRow {
	product: string;
	quantity: string;
	unitPrice: string;
	unitTotal: string;
	runningTotal: string;
}

@Component({
	components: {
		RegisterAdditionalAccount,
		SLAContent,
	},
})
export default class RegisterDialogue extends DialogueBase {
	
	public $refs!: {
		companyForm: VForm,
		mainLoginForm: VForm,
		signatureCanvas: HTMLCanvasElement,
		paymentForm: HTMLFormElement,
		agreementText: HTMLDivElement,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	protected signaturePad: SignaturePad | null = null;
	
	protected summaryValues: IRegisterSummaryRow[] = [];
	protected summaryHeaders = [
		{
			text: 'Product',
			align: 'start',
			value: 'product',
			sortable: false,
		},
		{
			text: 'Quantity',
			align: 'end',
			value: 'quantity',
			sortable: false,
		},
		{
			text: 'Unit Price',
			align: 'end',
			value: 'unitPrice',
			sortable: false,
		},
		{
			text: 'Unit Total',
			align: 'end',
			value: 'unitTotal',
			sortable: false,
		},
		{
			text: 'Running Total',
			align: 'end',
			value: 'runningTotal',
			sortable: false,
		},
	];
	
	
	constructor() {
		super();
		this.ModelState = RegisterDialogueModelState.GetEmpty();
		
	}
	
	public mounted(): void {
		// https://github.com/szimek/signature_pad
		
		const state = this.ModelState as IRegisterDialogueModelState;
		if (!state.perUserCost || !state.ip || !state.currency) {
			SignalRConnection.Ready(() => {
				const rtr = BillingCurrency.PerformGetIPCurrencyPerUserCost.Send({
					sessionId: BillingSessions.CurrentSessionId(),
					idempotencyToken: GenerateID(),
				});
				
				if (rtr.completeRequestPromise) {
					rtr.completeRequestPromise.then((payload: IPerformGetIPCurrencyPerUserCostCB) => {
						
						const state2 = this.ModelState as IRegisterDialogueModelState;
						state2.ip = payload.ip;
						state2.currency = payload.currency;
						state2.perUserCost = payload.perUserCost;
						this.ModelState = state2;
						
					});
				}
			});
		}
		
		
		
		//console.log(StripeManager);
		
		
		
		
		//
		//
	}
	
	public updated(): void {
		this.$nextTick(() => {
			
			
			
			if (this.$refs.signatureCanvas) {
				this.signaturePad = new SignaturePad(this.$refs.signatureCanvas);
				this.signaturePad.on();
			}
			
			
			
			
			
			
		});
	}
	
	
	
	public beforeDestroy(): void {
		if (this.signaturePad) {
			this.signaturePad.off();
		}
	}
	
	protected ClearSignature(): void {
		if (this.signaturePad) {
			this.signaturePad.clear();
		}
		
	}
	
	
	protected test(p: any, b: any): void { // eslint-disable-line
		console.log('test', p, b, this);
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	protected AddAnotherOtherAccount(): void {
		const accounts = this.OtherAccountsToAdd;
		accounts.push({
			id: GenerateID(),
			fullName: null,
			email: null,
			phoneNumber: null,
			password1: null,
			password2: null,
			passwordHash: null,
		});
		this.OtherAccountsToAdd = accounts;
	}
	
	protected ModifyRegisterAdditionalAccount(index: number, obj: IRegisterAdditionalUser): void {
		const accounts = this.OtherAccountsToAdd;
		accounts[index] = obj;
		this.OtherAccountsToAdd = accounts;
	}
	
	protected OnRemoveRegisterAdditionalAccount(payload: IRegisterAdditionalUser): void {
		const accounts = this.OtherAccountsToAdd;
		const idx = _.indexOf(accounts, payload);
		accounts.splice(idx, 1);
		this.OtherAccountsToAdd = accounts;
	}
	
	
	
	protected RegisterCompany(): void {
		
		console.debug('register company');
		
		if (!this.$refs.companyForm.validate()) {
			
			Notifications.AddNotification({
				severity: 'error',
				message: 'Some of the form fields didn\'t pass validation.',
				autoClearInSeconds: 10,
			});
			
			return;
		}
		
		const state = this.ModelState as IRegisterDialogueModelState;
		
		const rtr = BillingCompanies.PerformRegisterNewCompany.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			idempotencyToken: GenerateID(),
			name: state.companyName,
			abbreviation: state.companyAbbreviation,
			industry: state.companyIndustry,
			marketingCampaign: state.companyMarketingCampaign,
			addressLine1: state.companyAddressLine1,
			addressLine2: state.companyAddressLine2,
			addressCity: state.companyAddressCity,
			addressProvince: state.companyAddressProvince,
			addressPostalCode: state.companyAddressPostalCode,
			addressCountry: state.companyAddressCountry,
			mainContactEMail: state.mainContactEMail,
			mainContactPhoneNumber: state.mainContactPhoneNumber,
		});
		
		if (!rtr || !rtr.completeRequestPromise) {
			return;
		}
		
		rtr.completeRequestPromise.then((payload: IPerformRegisterNewCompanyCB) => {
			
			console.debug('complete payload', payload);
			
			if (payload.isError) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: payload.errorMessage,
					autoClearInSeconds: 10,
				});
				return;
			}
			if (payload.created === false) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to register, please contact support. #1',
					autoClearInSeconds: 10,
				});
				return;
			}
			if (payload.billingCompanyId === null) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to register, please contact support. #2',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			const state2 = this.ModelState as IRegisterDialogueModelState;
			state2.registeredBillingCompanyId = payload.billingCompanyId;
			state2.registeredBillingContactId = payload.billingContactId;
			state2.registeredBillingSessionId = payload.billingSessionId;
			state2.stripeIntentClientSecret = payload.stripeIntentClientSecret;
			this.ModelState = state2;
			
			
			this.CurrentDialogueStep = 2;
		});
		
		
		
		
		
		
		
		
	} // RegisterCompany
	
	protected SaveMainUser(): void {
		
		console.debug('SaveMainUser');
		
		if (!this.$refs.mainLoginForm.validate()) {
			
			Notifications.AddNotification({
				severity: 'error',
				message: 'Some parts of the form didn\'t validate.',
				autoClearInSeconds: 10,
			});
			
			return;
		}
		
		const state = this.ModelState as IRegisterDialogueModelState;
		
		if (state.mainContactPassword1 !== state.mainContactPassword2) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'The two passwords don\'t match.',
				autoClearInSeconds: 10,
			});
			
			return;
		}
		
		
		
		
		
		
		const rtr = BillingContacts.PerformRegisterMainUser.Send({
			idempotencyToken: GenerateID(),
			fullName: state.mainContactFullName,
			sessionId: state.registeredBillingSessionId,
			contactId: state.registeredBillingContactId,
			passwordHash: state.mainContactPasswordHash,
			eMailMarketing: state.mainContactEMailMarketing,
			eMailTutorials: state.mainContactEMailTutorials,
			
		});
		
		if (!rtr || !rtr.completeRequestPromise) {
			return;
		}
		
		rtr.completeRequestPromise.then((payload: IPerformRegisterMainUserCB) => {
			
			console.debug('complete payload', payload);
			
			if (payload.isError) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: payload.errorMessage,
					autoClearInSeconds: 10,
				});
				
				return;
			}
			if (payload.saved === false) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to save, please contact support. #1',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			
			this.CurrentDialogueStep = 3;
		});
		
		
		
	} // SaveMainUser
	
	protected SaveOtherAccounts(): void {
		
		console.debug('SaveOtherAccounts');
		
		let state = this.ModelState as IRegisterDialogueModelState;
		
		for (const info of state.otherAccountsToAdd) {
			
			if (
				!info.fullName || IsNullOrEmpty(info.fullName) ||
				!info.email || IsNullOrEmpty(info.email) ||
				!info.phoneNumber || IsNullOrEmpty(info.phoneNumber) ||
				!info.passwordHash || IsNullOrEmpty(info.passwordHash) ||
				info.password1 !== info.password2
				) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: 'One of the additional accounts has information that didn\'t validate.',
					autoClearInSeconds: 10,
				});
				return;
			}
			
		}
		
		
		const rtr = BillingContacts.PerformRegisterAdditionalUsers.Send({
			sessionId: state.registeredBillingSessionId,
			otherAccountsToAdd: state.otherAccountsToAdd,
		});
		if (!rtr || !rtr.completeRequestPromise) {
			return;
		}
		
		rtr.completeRequestPromise.then((payload: IPerformRegisterAdditionalUsersCB) => {
			
			console.debug('complete payload', payload);
			
			if (payload.isError) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: payload.errorMessage,
					autoClearInSeconds: 10,
				});
				
				return;
			}
			if (payload.created === false) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to create additional accounts, please contact support. #1',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			this.CreateBillingSummary();
			this.CurrentDialogueStep = 4;
			
		});
		
		
		
		
		
		
		
		
		//this.CurrentDialogueStep = 4;
		
		// Setup stripe
		this.$nextTick(() => {
			
			state = this.ModelState as IRegisterDialogueModelState;
			
			/*if (this.$refs.paymentForm && StripeManager.stripe) {
				
				const elements = StripeManager.stripe.elements();
				const cardElement = elements.create('card');
				cardElement.mount('#card-element');
				
				const cardholderName: HTMLInputElement = document.getElementById('cardholder-name') as HTMLInputElement;
				const cardButton = document.getElementById('card-button');
				const clientSecret = state.stripeIntentClientSecret;
				
				if (cardButton && cardholderName) {
					cardButton.addEventListener('click', async (ev) => {
					
					ev.preventDefault();
					
					const {setupIntent, error} = await StripeManager.stripe.confirmCardSetup(
						clientSecret,
						{
						payment_method: {
							card: cardElement,
							billing_details: {
							name: cardholderName.value,
							},
						},
						},
					);

					if (error) {
						// Display error.message in your UI.
					} else {
						if (setupIntent.status === 'succeeded') {
						
						console.log('stripe success!');
						
						// The setup has succeeded. Display a success message. Send
						// setupIntent.payment_method to your server to save the card to a Customer
						}
					}
					});
				}
				
				
				
			}*/
		});
		
	} // SaveOtherAccounts
	
	protected CreateBillingSummary(): void {
		
		//summaryValues
		
		const state = this.ModelState as IRegisterDialogueModelState;
		const otherAccounts = state.otherAccountsToAdd;
		
		let unitPrice = state.perUserCost || 50;
		const unitCurrency = state.perUserCost ? state.currency : 'USD';
		let runningTotal = 0;
		
		switch (this.BillingFrequency) {
			case 'Quarterly':
				unitPrice *= 3;
				break;
			case 'Annually':
				unitPrice *= 12;
				break;
			case 'Monthly':
			default:
				break;
		}
		
		
		
		this.summaryValues = [];
		
		// Always have the primary account.
		runningTotal += unitPrice;
		
		this.summaryValues.push({
			product: `Dispatch Pulse Seat, ${this.BillingFrequency}, 1st`,
			quantity: '1',
			unitPrice: `$${unitPrice.toFixed(2)} ${unitCurrency}`,
			unitTotal: `$${unitPrice.toFixed(2)} ${unitCurrency}`,
			runningTotal: `$${runningTotal.toFixed(2)} ${unitCurrency}`,
		});
		
		// Additional accounts.
		if (otherAccounts.length > 0) {
			runningTotal += unitPrice * otherAccounts.length;
			
			this.summaryValues.push({
				product: `Dispatch Pulse Seat, ${this.BillingFrequency}, Additional`,
				quantity: `${otherAccounts.length}`,
				unitPrice: `$${unitPrice.toFixed(2)} ${unitCurrency}`,
				unitTotal: `$${(unitPrice * otherAccounts.length).toFixed(2)} ${unitCurrency}`,
				runningTotal: `$${runningTotal.toFixed(2)} ${unitCurrency}`,
			});
		}
		
		state.summaryRunningTotal = runningTotal;
		this.ModelState = state;
	}
	
	
	
	
	protected SaveSummaryAndPaymentMethod(): void {
		
		console.debug('SaveSummaryAndPaymentMethod');
		
		const state = this.ModelState as IRegisterDialogueModelState;
		
		if (null == state || 
			null == state.registeredBillingSessionId) {
			
			Notifications.AddNotification({
				severity: 'error',
				message: 'No session, can\'t continue.',
				autoClearInSeconds: 10,
			});
			return;
			
		}
		
		if (null == state.billingFrequency) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'You must select a billing/payment frequency.',
				autoClearInSeconds: 10,
			});
			return;
		}
		
		
		
		const rtr = BillingPaymentMethod.PerformRegisterPaymentInformation.Send({
			idempotencyToken: GenerateID(),
			sessionId: state.registeredBillingSessionId,
			
			paymentFrequency: state.billingFrequency,
			
			numberOfSeats: (1 + state.otherAccountsToAdd.length),
			currency: state.currency || 'USD',
		});
		if (!rtr || !rtr.completeRequestPromise) {
			return;
		}
		
		rtr.completeRequestPromise.then((payload: IPerformRegisterPaymentInformationCB) => {
			
			console.debug('complete payload', payload);
			
			if (payload.isError) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: payload.errorMessage,
					autoClearInSeconds: 10,
				});
				
				return;
			}
			if (payload.saved === false) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to save your payment information.',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			this.CurrentDialogueStep = 5;
			
		});
		
		
		
		
		
		
		
		
		
		
		
	}
	
	
	protected SaveServiceAgreement(): void {
		console.debug('SaveServiceAgreement');
		
		if (!this.signaturePad) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Can\'t access the signature pad.',
				autoClearInSeconds: 10,
			});
			return;
		}
		
		if (this.signaturePad.isEmpty()) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'You must agree to the service agreement by signing below.',
				autoClearInSeconds: 10,
			});
			return;
		}
		
		if (!this.$refs.agreementText) {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Can\'t access the agreement text.',
				autoClearInSeconds: 10,
			});
			return;
		}
		
		const svg = this.signaturePad.toDataURL('image/svg+xml');
		
		const state = this.ModelState as IRegisterDialogueModelState;
		
		if (null == state || 
			null == state.registeredBillingSessionId) {
			
			Notifications.AddNotification({
				severity: 'error',
				message: 'No session, can\'t continue.',
				autoClearInSeconds: 10,
			});
			return;
			
		}
		
		const rtr = BillingCompanies.PerformRegisterSaveServiceAgreement.Send({
			idempotencyToken: GenerateID(),
			sessionId: state.registeredBillingSessionId,
			agreementText: '' + this.$refs.agreementText.textContent,
			signatureSVG: svg,
		});
		if (!rtr || !rtr.completeRequestPromise) {
			return;
		}
		
		rtr.completeRequestPromise.then((payload: IPerformRegisterSaveServiceAgreementCB) => {
			
			console.debug('complete payload', payload);
			
			if (payload.isError) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: payload.errorMessage,
					autoClearInSeconds: 10,
				});
				
				return;
			}
			if (payload.saved === false) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to save your contract.',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			this.CurrentDialogueStep = 6;
			
			
			
			
			this.RequestProvisioningStart();
			
		});
		
		
		
		
		
		
		
		
		
		
		
		
		
		//this.$refs.signaturePad.undoSignature();
		
		//const { isEmpty, data } = this.$refs.signaturePad.saveSignature();
		//console.log(isEmpty);
		//console.log(data);
	}
	
	protected RequestProvisioningStart(): void {
		
		console.log('RequestProvisioningStart');
		
		const state = this.ModelState as IRegisterDialogueModelState;
		
		if (null == state || 
			null == state.registeredBillingSessionId) {
			
			Notifications.AddNotification({
				severity: 'error',
				message: 'No session, can\'t continue.',
				autoClearInSeconds: 10,
			});
			return;
			
		}
		
		const rtr = BillingCompanies.PerformRegisterCreateDPDatabase.Send({
			idempotencyToken: GenerateID(),
			sessionId: state.registeredBillingSessionId,
		});
		if (!rtr || !rtr.completeRequestPromise) {
			return;
		}
		
		//console.log('##########');
		
		rtr.completeRequestPromise.then((payload: IPerformRegisterCreateDPDatabaseCB) => {
			
			console.debug('complete payload dp create', payload);
			
			if (payload.isError) {
				
				Notifications.AddNotification({
					severity: 'error',
					message: payload.errorMessage,
					autoClearInSeconds: 10,
				});
				
				return;
			}
			if (payload.created === false) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Unable to create your database.',
					autoClearInSeconds: 10,
				});
				return;
			}
			
			// Finished.
			
			const state2 = this.ModelState as IRegisterDialogueModelState;
			if (state2.registeredBillingSessionId) {
				this.$store.commit('SetSession', state2.registeredBillingSessionId);
				localStorage.setItem('SessionUUID', state2.registeredBillingSessionId);
				
				const promises: Array<Promise<any>> = [];
				
				const rtr1 = BillingPermissionsBool.RequestBillingPermissionsBoolForCurrentSession.Send({
					sessionId: BillingSessions.CurrentSessionId(),
				});
				if (rtr1.completeRequestPromise) {
					promises.push(rtr1.completeRequestPromise);
				}
				
				const rtr2 = BillingPermissionsGroups.RequestBillingPermissionsGroupsForCurrentSession.Send({
					sessionId: BillingSessions.CurrentSessionId(),
				});
				if (rtr2.completeRequestPromise) {
					promises.push(rtr2.completeRequestPromise);
				}
				
				const rtr3 = BillingPermissionsGroupsMemberships.
					RequestBillingPermissionsGroupsMembershipsForCurrentSession.Send({
						sessionId: BillingSessions.CurrentSessionId(),
					});
				if (rtr3.completeRequestPromise) {
					promises.push(rtr3.completeRequestPromise);
				}
				
				
				if (promises.length > 0) {
					Promise.all(promises).finally(() => {
						
						Dialogues.Close(this.DialogueName);
						this.ModelState = RegisterDialogueModelState.GetEmpty();
						
					});
				}
				
				//SignalRConnection.Ready(() => {
				//	BillingContacts.RequestBillingContactsForCurrentSession.Send();
				//	SignalRConnection.RequestBillingSessionsForCurrentSession();
				//});
			}
			
			
			
			
		});
		
		
		
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public get RegisterCompanyComplete(): boolean {
		const state = this.ModelState as IRegisterDialogueModelState;
		return state.registeredBillingCompanyId != null;
	}
	
	
	
	protected get CurrentDialogueStep(): number {
		return (this.ModelState as IRegisterDialogueModelState).currentDialogueStep;
	}
	
	protected set CurrentDialogueStep(payload: number) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.currentDialogueStep = payload;
		this.ModelState = state;
	}
	
	
	protected get CompanyName(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyName;
	}
	
	protected set CompanyName(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyName = payload;
		this.ModelState = state;
	}
	
	protected get CompanyAbbreviation(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAbbreviation;
	}
	
	protected set CompanyAbbreviation(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAbbreviation = payload;
		
		if (payload && !IsNullOrEmpty(payload)) {
			const promises = BillingCompanies.RequestCompanyNameInUse.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				idempotencyToken: GenerateID(),
				abbreviation: payload,
			});
			
			if (promises && promises.completeRequestPromise) {
				promises.completeRequestPromise.then((pl: IRequestCompanyNameInUseCB) => {
					this.CompanyAbbreviationInUse = pl.inUse;
				});
			}
		}

		
		
		this.ModelState = state;
	}
	
	protected get CompanyAbbreviationInUse(): boolean {
		return (this.ModelState as IRegisterDialogueModelState).companyAbbreviationInUse;
	}
	
	protected set CompanyAbbreviationInUse(payload: boolean) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAbbreviationInUse = payload;
		this.ModelState = state;
	}
	
	protected get CompanyIndustry(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyIndustry;
	}
	
	protected set CompanyIndustry(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyIndustry = payload;
		this.ModelState = state;
	}
	
	protected get CompanyMarketingCampaign(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyMarketingCampaign;
	}
	
	protected set CompanyMarketingCampaign(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyMarketingCampaign = payload;
		this.ModelState = state;
	}
	
	
	protected get CompanyAddressLine1(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAddressLine1;
	}
	
	protected set CompanyAddressLine1(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAddressLine1 = payload;
		this.ModelState = state;
	}
	
	protected get CompanyAddressLine2(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAddressLine2;
	}
	
	protected set CompanyAddressLine2(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAddressLine2 = payload;
		this.ModelState = state;
	}
	
	protected get CompanyAddressCity(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAddressCity;
	}
	
	protected set CompanyAddressCity(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAddressCity = payload;
		this.ModelState = state;
	}
	
	
	protected get CompanyAddressProvince(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAddressProvince;
	}
	
	protected set CompanyAddressProvince(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAddressProvince = payload;
		this.ModelState = state;
	}

	
	protected get CompanyAddressPostalCode(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAddressPostalCode;
	}
	
	protected set CompanyAddressPostalCode(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAddressPostalCode = payload;
		this.ModelState = state;
	}

	
	protected get CompanyAddressCountry(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).companyAddressCountry;
	}
	
	protected set CompanyAddressCountry(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.companyAddressCountry = payload;
		this.ModelState = state;
	}

	protected get MainContactEMail(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).mainContactEMail;
	}
	
	protected set MainContactEMail(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactEMail = payload;
		this.ModelState = state;
	}
	
	protected get MainContactPhoneNumber(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).mainContactPhoneNumber;
	}
	
	protected set MainContactPhoneNumber(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactPhoneNumber = payload;
		this.ModelState = state;
	}
	
	protected get MainContactPassword1(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).mainContactPassword1;
	}
	
	protected set MainContactPassword1(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactPassword1 = payload;
		
		const salt = bcrypt.genSaltSync(11);
		state.mainContactPasswordHash = payload == null || IsNullOrEmpty(payload) ? null : bcrypt.hashSync(payload, salt);
		
		this.ModelState = state;
	}
	
	protected get MainContactPassword2(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).mainContactPassword2;
	}
	
	protected set MainContactPassword2(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactPassword2 = payload;
		this.ModelState = state;
	}
	
	protected get MainContactEMailMarketing(): boolean {
		return (this.ModelState as IRegisterDialogueModelState).mainContactEMailMarketing;
	}
	
	protected set MainContactEMailMarketing(payload: boolean) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactEMailMarketing = payload;
		this.ModelState = state;
	}
	
	protected get MainContactEMailTutorials(): boolean {
		return (this.ModelState as IRegisterDialogueModelState).mainContactEMailTutorials;
	}
	
	protected set MainContactEMailTutorials(payload: boolean) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactEMailTutorials = payload;
		this.ModelState = state;
	}
	
	
	protected get MainContactFullName(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).mainContactFullName;
	}
	
	protected set MainContactFullName(payload: string | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.mainContactFullName = payload;
		this.ModelState = state;
	}
	
	protected get OtherAccountsToAdd(): IRegisterAdditionalUser[] {
		return (this.ModelState as IRegisterDialogueModelState).otherAccountsToAdd;
	}
	
	protected set OtherAccountsToAdd(payload: IRegisterAdditionalUser[]) {
		
		console.log('OtherAccountsToAdd set');
		
		const state = this.ModelState as IRegisterDialogueModelState;
		state.otherAccountsToAdd = payload;
		this.ModelState = state;
	}
	
	
	
	
	protected get IPCurrency(): string | null {
		return (this.ModelState as IRegisterDialogueModelState).currency;
	}
	
	protected get IPPerUserCost(): number | null {
		return (this.ModelState as IRegisterDialogueModelState).perUserCost;
	}
	
	protected get SummaryRunningTotal(): number | null {
		return (this.ModelState as IRegisterDialogueModelState).summaryRunningTotal;
	}
	
	
	
	
	protected get BillingFrequency(): 'Monthly' | 'Quarterly' | 'Annually' | null {
		return (this.ModelState as IRegisterDialogueModelState).billingFrequency;
	}
	
	protected set BillingFrequency(payload: 'Monthly' | 'Quarterly' | 'Annually' | null) {
		const state = this.ModelState as IRegisterDialogueModelState;
		state.billingFrequency = payload;
		this.ModelState = state;
		
		this.CreateBillingSummary();
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	get DialogueName(): string {
		return 'RegisterDialogue';
	}
	
	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = RegisterDialogueModelState.GetEmpty();
	}
	
	protected Next(): void {
		console.log('Next');
		
		Dialogues.Close(this.DialogueName);
		this.ModelState = RegisterDialogueModelState.GetEmpty();
		
	}
	
	
	
	
}
</script>
<style scoped>

.StripeElement {
  box-sizing: border-box;

  height: 40px;

  padding: 10px 12px;

  border: 1px solid transparent;
  border-radius: 4px;
  background-color: white;

  box-shadow: 0 1px 3px 0 #e6ebf1;
  -webkit-transition: box-shadow 150ms ease;
  transition: box-shadow 150ms ease;
}

.StripeElement--focus {
  box-shadow: 0 1px 3px 0 #cfd7df;
}

.StripeElement--invalid {
  border-color: #fa755a;
}

.StripeElement--webkit-autofill {
  background-color: #fefde5 !important;
}

</style>