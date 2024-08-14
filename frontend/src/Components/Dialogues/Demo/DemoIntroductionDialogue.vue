<template>
	<v-dialog v-model="IsOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
		<v-card>
			<v-card-title>Dispatch Pulse Demo</v-card-title>
			<v-divider></v-divider>
			<v-card-text>

				<p style='margin-top: 12px;'>Thank you for giving our demo a try, a few notes before we begin:</p>
				<ul>
					<li>The content of the demo is re-done every time you open it.</li>
					<li>Any changes you put in are lost when it is closed.</li>
					<li>You can export anything you put into the demo in settings, but you won't be able to import it
						until you have a activated account.</li>
				</ul>
				<p style='margin-top: 12px;'>With that said, click Launch Demo to begin!</p>

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-spacer />
				<v-btn color='red darken-1' text @click='Cancel()'>Back to Login</v-btn>
				<v-btn color='green darken-1' text @click='LaunchDemo()'>Launch Demo</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang='ts'>
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import EmploymentStatusEditor from '@/Components/Editors/EmploymentStatusEditor.vue';
import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import { DemoIntroductionDialogueModelState } from '@/Data/Models/DemoIntroductionDialogueModelState/DemoIntroductionDialogueModelState';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import { AssignmentStatus, IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import { EmploymentStatus, IEmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';
import { ILabourSubtypeHoliday, LabourSubtypeHoliday } from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { ILabourSubtypeException, LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
import { ILabourSubtypeNonBillable, LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
import { ILabourType, LabourType } from '@/Data/CRM/LabourType/LabourType';
import { IProjectStatus, ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import _ from 'lodash';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import Dialogues from '@/Utility/Dialogues';
import { BillingPermissionsGroups, IBillingPermissionsGroups } from '@/Data/Billing/BillingPermissionsGroups/BillingPermissionsGroups';
import { BillingPermissionsGroupsMemberships, IBillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingPermissionsBool, IBillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { IBillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import { IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { IBillingCouponCodes } from '@/Data/Billing/BillingCouponCodes/BillingCouponCodes';
import { IBillingCurrency } from '@/Data/Billing/BillingCurrency/BillingCurrency';
import { IBillingIndustries } from '@/Data/Billing/BillingIndustries/BillingIndustries';
import { IBillingInvoices } from '@/Data/Billing/BillingInvoices/BillingInvoices';
import { IBillingJournalEntries } from '@/Data/Billing/BillingJournalEntries/BillingJournalEntries';
//import { IBillingJournalEntriesType } from '@/Data/Billing/BillingJournalEntriesType/BillingJournalEntriesType';
import { IBillingPackages } from '@/Data/Billing/BillingPackages/BillingPackages';
import { IBillingPackagesType } from '@/Data/Billing/BillingPackagesType/BillingPackagesType';
import { IBillingPaymentFrequencies } from '@/Data/Billing/BillingPaymentFrequencies/BillingPaymentFrequencies';
import { IBillingPaymentMethod } from '@/Data/Billing/BillingPaymentMethod/BillingPaymentMethod';
import { IBillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { IBillingSubscriptions } from '@/Data/Billing/BillingSubscriptions/BillingSubscriptions';
import { IBillingSubscriptionsProvisioningStatus } from '@/Data/Billing/BillingSubscriptionsProvisioningStatus/BillingSubscriptionsProvisioningStatus';
import { IEstimatingManHours } from '@/Data/CRM/EstimatingManHours/EstimatingManHours';
import { IMaterial } from '@/Data/CRM/Material/Material';
import { ISettingsDefault } from '@/Data/CRM/SettingsDefault/SettingsDefault';
import { ISettingsProvisioning } from '@/Data/CRM/SettingsProvisioning/SettingsProvisioning';
import { ISettingsUser } from '@/Data/CRM/SettingsUser/SettingsUser';
import { IProduct } from '@/Data/CRM/Product/Product';
import { ISkill } from '@/Data/CRM/Skill/Skill';
import { IProjectNoteStyledText } from '@/Data/CRM/ProjectNoteStyledText/ProjectNoteStyledText';

@Component({
	components: {
		EmploymentStatusEditor,
	},
})
export default class DemoIntroductionDialogue extends DialogueBase {

	//public $refs!: {
	//	editor: EmploymentStatusEditor,
	//};

	protected MobileDeviceWidth = MobileDeviceWidth;

	constructor() {
		super();
		this.ModelState = DemoIntroductionDialogueModelState.GetEmpty();
	}



	get DialogueName(): string {
		return 'DemoIntroductionDialogue';
	}

	protected Cancel(): void {
		Dialogues.Close(this.DialogueName);
		this.ModelState = DemoIntroductionDialogueModelState.GetEmpty();
		this.$store.commit('SetDemoMode', false);
	}

	protected LaunchDemo(): void {
		console.log('Launch Demo');

		this.$store.commit('SetDemoMode', true);

		this.BuildFakeDatabase();

		Dialogues.Close(this.DialogueName);
		this.ModelState = DemoIntroductionDialogueModelState.GetEmpty();


	}


	protected BuildFakeDatabase(): void {


		const billingContactId = GenerateID();
		const billingCompanyId = GenerateID();

		// database objects to save
		const billingCompanies: Record<string, IBillingCompanies> = {};
		const billingContacts: Record<string, IBillingContacts> = {};
		const billingCouponCodes: Record<string, IBillingCouponCodes> = {}; // No need for us to fill this out.
		const billingCurrency: Record<string, IBillingCurrency> = {};
		const billingIndustries: Record<string, IBillingIndustries> = {};
		const billingInvoices: Record<string, IBillingInvoices> = {};
		const billingJournalEntries: Record<string, IBillingJournalEntries> = {};
		//const billingJournalEntriesType: Record<string, IBillingJournalEntriesType> = {};
		const billingPackages: Record<string, IBillingPackages> = {};
		const billingPackagesType: Record<string, IBillingPackagesType> = {};
		const billingPaymentFrequencies: Record<string, IBillingPaymentFrequencies> = {};
		const billingPaymentMethod: Record<string, IBillingPaymentMethod> = {};
		const billingPermissionsBool: Record<string, IBillingPermissionsBool> = {};
		const billingPermissionsGroups: Record<string, IBillingPermissionsGroups> = {};
		const billingPermissionsGroupsMemberships: Record<string, IBillingPermissionsGroupsMemberships> = {};
		const billingSessions: Record<string, IBillingSessions> = {};
		const billingSubscriptions: Record<string, IBillingSubscriptions> = {};
		const billingSubscriptionsProvisioningStatus: Record<string, IBillingSubscriptionsProvisioningStatus> = {};

		const agents: Record<string, IAgent> = {};
		const assignments: Record<string, IAssignment> = {};
		const assignmentStatus: Record<string, IAssignmentStatus> = {};
		const companies: Record<string, ICompany> = {};
		const agentsEmploymentStatus: Record<string, IEmploymentStatus> = {};
		const contacts: Record<string, IContact> = {};
		const estimatingManHours: Record<string, IEstimatingManHours> = {};
		const labourSubtypeHolidays: Record<string, ILabourSubtypeHoliday> = {};
		const labourSubtypeException: Record<string, ILabourSubtypeException> = {};
		const labourSubtypeNonBillable: Record<string, ILabourSubtypeNonBillable> = {};
		const labour: Record<string, ILabour> = {};
		const labourTypes: Record<string, ILabourType> = {};
		const materials: Record<string, IMaterial> = {};
		const projectNotes: Record<string, IProjectNote> = {};
		const projects: Record<string, IProject> = {};
		const projectStatus: Record<string, IProjectStatus> = {};
		const settingsDefault: Record<string, ISettingsDefault> = {};
		const settingsProvisioning: Record<string, ISettingsProvisioning> = {};
		const settingsUser: Record<string, ISettingsUser> = {};
		const products: Record<string, IProduct> = {};
		const skills: Record<string, ISkill> = {};

		// Begin generated objects


		const dpAgent1 = Agent.GetEmpty();
		if (dpAgent1.id) {
			agents[dpAgent1.id] = dpAgent1;
		}


		const dpContact1 = Contact.GetEmpty();
		if (dpContact1.id) {
			contacts[dpContact1.id] = dpContact1;
		}


		const dpCompany1 = Company.GetEmpty();
		if (dpCompany1.id) {
			companies[dpCompany1.id] = dpCompany1;
		}


		const billingContact: IBillingContacts = {
			fullName: 'James Mackenzie',
			email: 'jmackenzie@example.com',
			emailListMarketing: false,
			emailListTutorials: false,
			marketingCampaign: 'N/A',
			phone: '(204) 555-5555',
			uuid: billingContactId,
			companyId: billingCompanyId,
			applicationData: {
				dispatchPulseContactId: dpContact1.id || null,
				dispatchPulseAgentId: dpAgent1.id || null,
			},
			json: {},
		};
		billingContacts[billingContact.uuid] = billingContact;

		// Permissions, grant company owner to demo.

		// Create group owner

		const grpOwner = BillingPermissionsGroups.GetEmpty();
		grpOwner.id = GenerateID();
		grpOwner.name = 'Demo Group';
		grpOwner.json = { hidden: false };
		billingPermissionsGroups[grpOwner.id] = grpOwner;

		const grpMembership = BillingPermissionsGroupsMemberships.GetEmpty();
		grpMembership.id = GenerateID();
		grpMembership.groupId = grpOwner.id;
		grpMembership.contactId = billingContact.uuid;
		grpMembership.json = {};
		billingPermissionsGroupsMemberships[grpMembership.id] = grpMembership;


		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-groups.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-groups-memberships.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.employment-status.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments-status.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments-status.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments-status.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.companies.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.companies.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.contacts.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.contacts.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.contacts.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.estimating-man-hours.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.estimating-man-hours.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-exception.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-exception.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-holidays.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-holidays.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-non-billable.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-non-billable.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-non-billable.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-types.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-types.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.materials.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.materials.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.materials.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.products.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.products.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.project-notes.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.project-notes.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.project-notes.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.projects.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.projects.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.project-status.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.project-status.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-default.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-default.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-default.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-provisioning.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-provisioning.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-user.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-user.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.skills.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.skills.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.employment-status.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.employment-status.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.agents.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-user.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions.add-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions.modify-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.contacts.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.contacts.modify-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.currency.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.industries.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.invoices.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.journal-entries.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.contacts.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.packages-type.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.payment-method.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-bool.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-bool.add-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-bool.modify-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.sessions.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.sessions.add-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.sessions.modify-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.sessions.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.subscription.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-groups-memberships.modify-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-groups-memberships.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.packages.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.agents.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.agents.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.companies.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.estimating-man-hours.push-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-exception.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-subtype-holidays.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour-types.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.products.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.projects.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.project-status.request-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.settings-provisioning.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.skills.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.coupon-codes.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.journal-entries-type.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.payment-frequencies.read-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.permissions-bool.delete-company';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.companies-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.report.projects-pdf';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.report.assignments-pdf';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.agents.display-own';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.view.dashboard.dispatch-tab';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.view.dashboard.billing-tab';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm-view.dashboard.management-tab';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour.manual-entries';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.address-book';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-contacts';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-companies';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.projects';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-projects';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-assignments';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-material-entries';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.project-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.product-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.assignment-status-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.man-hours-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.project-status-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.agents';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-agents';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-labour-entries';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.agents-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.employment-status-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.labour-exception-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.labour-holidays-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.labour-non-billable-definitions';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.reports';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.all-reports';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.navigation.show.settings';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.report.contacts-pdf';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.contacts-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.report.companies-pdf';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.assignments-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.report.materials-pdf';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.materials-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.product-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.assignment-status-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.man-hours-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.project-status-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.report.labour-pdf';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.labour-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.employment-status-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.labour-exception-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.labour-holidays-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.labour-non-billable-definitions-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.view.billing-index.merge-projects';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.labour.push-self';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.assignments.request-self';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.export.projects-csv';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.backups.run-local';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'crm.backups.run-server';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}

		{
			const perm = BillingPermissionsBool.GetEmpty();
			perm.uuid = GenerateID();
			perm.key = 'billing.companies.modify-company-phone-id';
			perm.contactId = null;
			perm.value = true;
			perm.json = {};
			perm.groupId = grpOwner.id;
			billingPermissionsBool[perm.uuid] = perm;
		}




		const billingCompany: IBillingCompanies = {
			uuid: billingCompanyId,
			fullName: 'A/C Electrical',
			abbreviation: 'ACE',
			industry: 'Electrical Contractors',
			marketingCampaign: 'N/A',
			addressCity: 'Winnipeg',
			addressCountry: 'Canada',
			addressLine1: '123 Industrial Road',
			addressLine2: '',
			addressPostalCode: 'R2M 2N1',
			addressProvince: 'MB',
			stripeCustomerId: '',
			paymentMethod: 'Square Pre-Authorized',
			invoiceContactId: billingContactId,
			paymentFrequency: 'yearly',
			json: {
				phoneId: null,
			},
		};
		billingCompanies[billingCompany.uuid] = billingCompany;

		dpAgent1.json.name = billingContact.fullName;
		dpAgent1.json.title = 'Company Owner';
		//dpAgent1.employmentStatusId
		dpAgent1.json.hourlyWage = 36.50;
		dpAgent1.json.notificationSMSNumber = '5555555555';

		dpContact1.json.name = billingContact.fullName;
		dpContact1.json.title = dpAgent1.json.title;
		dpContact1.json.companyId = dpCompany1.id || null;
		dpContact1.json.notes = '';
		dpContact1.json.phoneNumbers = [
			{
				id: GenerateID(),
				label: 'Mobile',
				value: billingContact.phone,
			},
		];
		dpContact1.json.emails = [
			{
				id: GenerateID(),
				label: 'Work',
				value: billingContact.email,
			},
		];
		dpContact1.json.addresses = [
			{
				id: GenerateID(),
				label: 'Home',
				value: '123 Smith Street',
			},
		];



		dpCompany1.json.name = billingCompany.fullName;
		//dpCompany1.json.logoURI = '';
		//dpCompany1.json.websiteURI = '';


		// billingCurrency
		{
			const currency1: IBillingCurrency = {
				uuid: GenerateID(),
				currency: 'CAD',
				json: {},
			};
			billingCurrency[currency1.uuid] = currency1;

			const currency2: IBillingCurrency = {
				uuid: GenerateID(),
				currency: 'USD',
				json: {},
			};
			billingCurrency[currency2.uuid] = currency2;
		}


		// billingIndustries
		{
			const industry1: IBillingIndustries = {
				uuid: GenerateID(),
				value: 'Electrical',
				json: {},
			};
			billingIndustries[industry1.uuid] = industry1;
		}

		// billingInvoices
		// billingJournalEntries
		// billingJournalEntriesType
		// billingPackages
		// billingPackagesType
		// billingPaymentFrequencies
		// billingPaymentMethod
		// billingPermissionsBool
		// billingPermissionsGroups
		// billingPermissionsGroupsMemberships

		// billingSessions
		{
			const session1: IBillingSessions = {
				uuid: GenerateID(),
				contactId: billingContact.uuid,
				agentDescription: 'Fake Demo Sesion',
				ipAddress: '127.0.0.1',
				createdUtc: DateTime.utc().toISO(),
				lastAccessUtc: DateTime.utc().toISO(),
				json: {},
			};
			billingSessions[session1.uuid] = session1;
			this.$store.commit('SetSession', session1.uuid);
		}

		// billingSubscriptions
		// billingSubscriptionsProvisioningStatus


		// Supporting tables.

		// assignmentStatus
		let assignmentStatusAssigned;
		let assignmentStatusToBeScheduled;
		{
			const assignmentStatusBillable = AssignmentStatus.GetEmpty();
			if (assignmentStatusBillable && assignmentStatusBillable.id) {
				assignmentStatusBillable.json.name = 'Billable';
				assignmentStatusBillable.json.isBillable = true;
				assignmentStatusBillable.json.isDefault = true;
				assignmentStatus[assignmentStatusBillable.id] = assignmentStatusBillable;
			}

			assignmentStatusAssigned = AssignmentStatus.GetEmpty();
			if (assignmentStatusAssigned && assignmentStatusAssigned.id) {
				assignmentStatusAssigned.json.isOpen = true;
				assignmentStatusAssigned.json.name = 'Assigned';
				assignmentStatusAssigned.json.isAssigned = true;
				assignmentStatusAssigned.json.isDefault = true;
				assignmentStatus[assignmentStatusAssigned.id] = assignmentStatusAssigned;
			}

			const assignmentStatusWoV = AssignmentStatus.GetEmpty();
			if (assignmentStatusWoV && assignmentStatusWoV.id) {
				assignmentStatusWoV.json.isOpen = true;
				assignmentStatusWoV.json.name = 'Waiting on Vendor';
				assignmentStatusWoV.json.isWaitingOnVendor = true;
				assignmentStatusWoV.json.isDefault = true;
				assignmentStatus[assignmentStatusWoV.id] = assignmentStatusWoV;
			}


			const assignmentStatusInProgress = AssignmentStatus.GetEmpty();
			if (assignmentStatusInProgress && assignmentStatusInProgress.id) {
				assignmentStatusInProgress.json.name = 'In Progress';
				assignmentStatusInProgress.json.isOpen = true;
				assignmentStatusInProgress.json.isAssigned = true;
				assignmentStatusInProgress.json.isDefault = true;
				assignmentStatusInProgress.json.isInProgress = true;
				assignmentStatus[assignmentStatusInProgress.id] = assignmentStatusInProgress;
			}


			const assignmentStatusBillableReview = AssignmentStatus.GetEmpty();
			if (assignmentStatusBillableReview && assignmentStatusBillableReview.id) {
				assignmentStatusBillableReview.json.name = 'Billable Review';
				assignmentStatusBillableReview.json.isBillableReview = true;
				assignmentStatusBillableReview.json.isDefault = true;
				assignmentStatus[assignmentStatusBillableReview.id] = assignmentStatusBillableReview;
			}


			const assignmentStatusWoC = AssignmentStatus.GetEmpty();
			if (assignmentStatusWoC && assignmentStatusWoC.id) {
				assignmentStatusWoC.json.name = 'Waiting on Client';
				assignmentStatusWoC.json.isOpen = true;
				assignmentStatusWoC.json.isWaitingOnClient = true;
				assignmentStatusWoC.json.isDefault = true;
				assignmentStatus[assignmentStatusWoC.id] = assignmentStatusWoC;
			}


			const assignmentStatusToBePicked = AssignmentStatus.GetEmpty();
			if (assignmentStatusToBePicked && assignmentStatusToBePicked.id) {
				assignmentStatusToBePicked.json.name = 'To Be Picked';
				assignmentStatusToBePicked.json.isOpen = true;
				assignmentStatusToBePicked.json.isDefault = true;
				assignmentStatus[assignmentStatusToBePicked.id] = assignmentStatusToBePicked;
			}


			const assignmentStatusReOpened = AssignmentStatus.GetEmpty();
			if (assignmentStatusReOpened && assignmentStatusReOpened.id) {
				assignmentStatusReOpened.json.name = 'Re-opened';
				assignmentStatusReOpened.json.isOpen = true;
				assignmentStatusReOpened.json.isReOpened = true;
				assignmentStatusReOpened.json.isAssigned = true;
				assignmentStatusReOpened.json.isDefault = true;
				assignmentStatus[assignmentStatusReOpened.id] = assignmentStatusReOpened;
			}


			const assignmentStatusNonBillable = AssignmentStatus.GetEmpty();
			if (assignmentStatusNonBillable && assignmentStatusNonBillable.id) {
				assignmentStatusNonBillable.json.name = 'Non Billable';
				assignmentStatusNonBillable.json.isNonBillable = true;
				assignmentStatusNonBillable.json.isDefault = true;
				assignmentStatus[assignmentStatusNonBillable.id] = assignmentStatusNonBillable;
			}


			const assignmentStatusScheduled = AssignmentStatus.GetEmpty();
			if (assignmentStatusScheduled && assignmentStatusScheduled.id) {
				assignmentStatusScheduled.json.name = 'Scheduled';
				assignmentStatusScheduled.json.isOpen = true;
				assignmentStatusScheduled.json.isScheduled = true;
				assignmentStatusScheduled.json.isDefault = true;
				assignmentStatus[assignmentStatusScheduled.id] = assignmentStatusScheduled;
			}


			const assignmentStatusClosed = AssignmentStatus.GetEmpty();
			if (assignmentStatusClosed && assignmentStatusClosed.id) {
				assignmentStatusClosed.json.name = 'Closed';
				assignmentStatusClosed.json.isDefault = true;
				assignmentStatus[assignmentStatusClosed.id] = assignmentStatusClosed;
			}


			assignmentStatusToBeScheduled = AssignmentStatus.GetEmpty();
			if (assignmentStatusToBeScheduled && assignmentStatusToBeScheduled.id) {
				assignmentStatusToBeScheduled.json.name = 'To Be Scheduled';
				assignmentStatusToBeScheduled.json.isOpen = true;
				assignmentStatusToBeScheduled.json.isToBeScheduled = true;
				assignmentStatusToBeScheduled.json.isDefault = true;
				assignmentStatus[assignmentStatusToBeScheduled.id] = assignmentStatusToBeScheduled;
			}

		}


		// agentsEmploymentStatus
		let agentsEmploymentStatusCurrentEmployee;
		{
			const agentsEmploymentStatusNonEmployee = EmploymentStatus.GetEmpty();
			if (agentsEmploymentStatusNonEmployee && agentsEmploymentStatusNonEmployee.id) {
				agentsEmploymentStatusNonEmployee.json.name = 'Non Employee';
				agentsEmploymentStatus[agentsEmploymentStatusNonEmployee.id] = agentsEmploymentStatusNonEmployee;
			}


			const agentsEmploymentStatusCurrentContractor = EmploymentStatus.GetEmpty();
			if (agentsEmploymentStatusCurrentContractor && agentsEmploymentStatusCurrentContractor.id) {
				agentsEmploymentStatusCurrentContractor.json.name = 'Current Contractor';
				agentsEmploymentStatus[agentsEmploymentStatusCurrentContractor.id] = agentsEmploymentStatusCurrentContractor;
			}


			const agentsEmploymentStatusFormerEmployee = EmploymentStatus.GetEmpty();
			if (agentsEmploymentStatusFormerEmployee && agentsEmploymentStatusFormerEmployee.id) {
				agentsEmploymentStatusFormerEmployee.json.name = 'Former Employee';
				agentsEmploymentStatus[agentsEmploymentStatusFormerEmployee.id] = agentsEmploymentStatusFormerEmployee;
			}


			agentsEmploymentStatusCurrentEmployee = EmploymentStatus.GetEmpty();
			if (agentsEmploymentStatusCurrentEmployee && agentsEmploymentStatusCurrentEmployee.id) {
				agentsEmploymentStatusCurrentEmployee.json.name = 'Current Employee';
				agentsEmploymentStatus[agentsEmploymentStatusCurrentEmployee.id] = agentsEmploymentStatusCurrentEmployee;
			}


			const agentsEmploymentStatusFormerContractor = EmploymentStatus.GetEmpty();
			if (agentsEmploymentStatusFormerContractor && agentsEmploymentStatusFormerContractor.id) {
				agentsEmploymentStatusFormerContractor.json.name = 'Former Contractor';
				agentsEmploymentStatus[agentsEmploymentStatusFormerContractor.id] = agentsEmploymentStatusFormerContractor;
			}

		}

		// labourSubtypeHolidays
		{
			const christmas = LabourSubtypeHoliday.GetEmpty();
			if (christmas.id) {
				_.merge(christmas, {
					json: {
						name: 'Christmas',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, held on December 25.',
						isStaticDate: true,
						staticDateMonth: 12,
						staticDateDay: 25,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[christmas.id] = christmas;
			}

			const goodFriday = LabourSubtypeHoliday.GetEmpty();
			if (goodFriday.id) {
				_.merge(goodFriday, {
					json: {
						name: 'Good Friday',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, 2 days before Easter Sunday.',
						isStaticDate: false,
						staticDateMonth: null,
						staticDateDay: null,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: true,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[goodFriday.id] = goodFriday;
			}


			const victoriaDay = LabourSubtypeHoliday.GetEmpty();
			if (victoriaDay.id) {
				_.merge(victoriaDay, {
					json: {
						name: 'Victoria Day',
						icon: 'fa-calendar-day',
						description: 'Federal Holiday, held on last Monday preceding May 25.',
						isStaticDate: false,
						staticDateMonth: null,
						staticDateDay: null,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: true,
						mondayBeforeDateMonth: 5,
						mondayBeforeDateDay: 25,
					}
				});
				labourSubtypeHolidays[victoriaDay.id] = victoriaDay;
			}


			const rememberanceDay = LabourSubtypeHoliday.GetEmpty();
			if (rememberanceDay.id) {
				_.merge(rememberanceDay, {
					json: {
						name: 'Rememberance Day',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, except NS MB ON QC, held on November 11th.',
						isStaticDate: true,
						staticDateMonth: 11,
						staticDateDay: 11,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[rememberanceDay.id] = rememberanceDay;
			}


			const newYearsDay = LabourSubtypeHoliday.GetEmpty();
			if (newYearsDay.id) {
				_.merge(newYearsDay, {
					json: {
						name: 'New Year\'s Day',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, held on January 1.',
						isStaticDate: true,
						staticDateMonth: 1,
						staticDateDay: 1,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[newYearsDay.id] = newYearsDay;
			}


			const boxingDay = LabourSubtypeHoliday.GetEmpty();
			if (boxingDay.id) {
				_.merge(boxingDay, {
					json: {
						name: 'Boxing Day',
						icon: 'fa-calendar-day',
						description: 'Boxing Day, held on December 26.',
						isStaticDate: true,
						staticDateMonth: 12,
						staticDateDay: 26,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[boxingDay.id] = boxingDay;
			}


			const familyDay = LabourSubtypeHoliday.GetEmpty();
			if (familyDay.id) {
				_.merge(familyDay, {
					json: {
						name: 'Louis Riel Day (MB) Family Day (BC, AB, ON, NB, SK) Heritage Day (NS) Islander Day (PE)',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, except QC, NL, and Territories, held on the 3rd Monday in Feburary.',
						isStaticDate: false,
						staticDateMonth: null,
						staticDateDay: null,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: true,
						thirdMondayMonth: 2,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[familyDay.id] = familyDay;
			}


			const canadaDay = LabourSubtypeHoliday.GetEmpty();
			if (canadaDay.id) {
				_.merge(canadaDay, {
					json: {
						name: 'Canada Day',
						icon: 'fa-calendar-day',
						description: 'Federal Holiday, held on July 1.',
						isStaticDate: true,
						staticDateMonth: 7,
						staticDateDay: 1,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[canadaDay.id] = canadaDay;
			}


			const labourDay = LabourSubtypeHoliday.GetEmpty();
			if (labourDay.id) {
				_.merge(labourDay, {
					json: {
						name: 'Labour Day',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, held on the 1st Monday in September.',
						isStaticDate: false,
						staticDateMonth: null,
						staticDateDay: null,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: true,
						firstMondayMonth: 9,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[labourDay.id] = labourDay;
			}


			const civicHoliday = LabourSubtypeHoliday.GetEmpty();
			if (civicHoliday.id) {
				_.merge(civicHoliday, {
					json: {
						name: 'Civic Holiday',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, except NS MB and PEI, held on the 1st Monday in August.',
						isStaticDate: false,
						staticDateMonth: null,
						staticDateDay: null,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: true,
						firstMondayMonth: 8,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: false,
						secondMondayMonth: null,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[civicHoliday.id] = civicHoliday;
			}


			const thanksgiving = LabourSubtypeHoliday.GetEmpty();
			if (thanksgiving.id) {
				_.merge(thanksgiving, {
					json: {
						name: 'Thanksgiving',
						icon: 'fa-calendar-day',
						description: 'Stat Holiday, except Atlantic Provinces, held on the 2nd Monday in October.',
						isStaticDate: false,
						staticDateMonth: null,
						staticDateDay: null,
						isObservationDay: false,
						observationDayStatic: false,
						observationDayStaticMonth: null,
						observationDayStaticDay: null,
						observationDayActivateIfWeekend: false,
						isFirstMondayInMonthDate: false,
						firstMondayMonth: null,
						isGoodFriday: false,
						isThirdMondayInMonthDate: false,
						thirdMondayMonth: null,
						isSecondMondayInMonthDate: true,
						secondMondayMonth: 10,
						isMondayBeforeDate: false,
						mondayBeforeDateMonth: null,
						mondayBeforeDateDay: null,
					}
				});
				labourSubtypeHolidays[thanksgiving.id] = thanksgiving;
			}

		}

		// labourSubtypeException
		{
			const bereavement = LabourSubtypeException.GetEmpty();
			if (bereavement.id) {
				_.merge(bereavement, {
					json: {
						name: 'Bereavement',
						description: 'Time off when someone has died.',
						icon: 'nature_people',
					}
				});
				labourSubtypeException[bereavement.id] = bereavement;
			}


			const sick = LabourSubtypeException.GetEmpty();
			if (sick.id) {
				_.merge(sick, {
					json: {
						name: 'Sick',
						description: 'Time off due to sickness.',
						icon: 'local_hospital',
					}
				});
				labourSubtypeException[sick.id] = sick;
			}


			const personalDay = LabourSubtypeException.GetEmpty();
			if (personalDay.id) {
				_.merge(personalDay, {
					json: {
						name: 'Personal Day',
						description: 'Time off for unspecified reasons.',
						icon: 'bathtub',
					}
				});
				labourSubtypeException[personalDay.id] = personalDay;
			}


			const familyLeave = LabourSubtypeException.GetEmpty();
			if (familyLeave.id) {
				_.merge(familyLeave, {
					json: {
						name: 'Family Leave',
						description: 'Time off to deal with family issues, such as children being sick, etc...',
						icon: 'child_friendly',
					}
				});
				labourSubtypeException[familyLeave.id] = familyLeave;
			}


			const weather = LabourSubtypeException.GetEmpty();
			if (weather.id) {
				_.merge(weather, {
					json: {
						name: 'Weather',
						description: 'Time off due to bad weather.',
						icon: 'ac_unit',
					}
				});
				labourSubtypeException[weather.id] = weather;
			}

		}

		// labourSubtypeNonBillable
		{
			const other = LabourSubtypeNonBillable.GetEmpty();
			if (other.id) {
				_.merge(other, {
					json: {
						name: 'Other',
						description: 'Some other non billable task.',
						icon: 'info',
					}
				});
				labourSubtypeNonBillable[other.id] = other;
			}


			const training = LabourSubtypeNonBillable.GetEmpty();
			if (training.id) {
				_.merge(training, {
					json: {
						name: 'Training',
						description: 'Time spent learning something.',
						icon: 'info',
					}
				});
				labourSubtypeNonBillable[training.id] = training;
			}


			const programming = LabourSubtypeNonBillable.GetEmpty();
			if (programming.id) {
				_.merge(programming, {
					json: {
						name: 'Programming',
						description: 'The writing of computer programs or other technical work.',
						icon: 'info',
					}
				});
				labourSubtypeNonBillable[programming.id] = programming;
			}


			const meeting = LabourSubtypeNonBillable.GetEmpty();
			if (meeting.id) {
				_.merge(meeting, {
					json: {
						name: 'Meeting',
						description: 'A company meeting that cannot be billed to a 3rd party.',
						icon: 'people',
					}
				});
				labourSubtypeNonBillable[meeting.id] = meeting;
			}


			const administration = LabourSubtypeNonBillable.GetEmpty();
			if (administration.id) {
				_.merge(administration, {
					json: {
						name: 'Administration',
						description: 'Clerical tasks, including drafting documents, telephones, scheduling, project management.',
						icon: '',
					}
				});
				labourSubtypeNonBillable[administration.id] = administration;
			}

		}

		// labourTypes
		let billable;
		{
			billable = LabourType.GetEmpty();
			if (billable.id) {
				_.merge(billable, {
					json: {
						name: 'Billable',
						icon: 'attach_money',
						description: 'Time that will be billed to a client.',
						default: true,
						isBillable: true,
						isHoliday: false,
						isNonBillable: false,
						isException: false,
						isPayOutBanked: false,
					}
				});
				labourTypes[billable.id] = billable;
			}


			const exception = LabourType.GetEmpty();
			if (exception.id) {
				_.merge(exception, {
					json: {
						name: 'Exception',
						icon: 'info',
						description: 'Non planned events.',
						default: true,
						isBillable: false,
						isHoliday: false,
						isNonBillable: false,
						isException: true,
						isPayOutBanked: false,
					}
				});
				labourTypes[exception.id] = exception;
			}


			const holiday = LabourType.GetEmpty();
			if (holiday.id) {
				_.merge(holiday, {
					json: {
						name: 'Holiday',
						icon: 'beach_access',
						description: 'Re-occuring standard days off, paid and not paid.',
						default: true,
						isBillable: false,
						isHoliday: true,
						isNonBillable: false,
						isException: false,
						isPayOutBanked: false,
					}
				});
				labourTypes[holiday.id] = holiday;
			}


			const payOutBanked = LabourType.GetEmpty();
			if (payOutBanked.id) {
				_.merge(payOutBanked, {
					json: {
						name: 'Pay Out Banked',
						icon: '360',
						description: '',
						default: true,
						isBillable: false,
						isHoliday: false,
						isNonBillable: false,
						isException: false,
						isPayOutBanked: true,
					}
				});
				labourTypes[payOutBanked.id] = payOutBanked;
			}


			const nonBillable = LabourType.GetEmpty();
			if (nonBillable.id) {
				_.merge(nonBillable, {
					json: {
						name: 'Non Billable',
						icon: 'money_off',
						description: 'Time that will be billed to your company.',
						default: true,
						isBillable: false,
						isHoliday: false,
						isNonBillable: true,
						isException: false,
						isPayOutBanked: false,
					}
				});
				labourTypes[nonBillable.id] = nonBillable;
			}

		}

		// projectStatus
		let quoting;
		let awaitingPayment;
		let inProgress;
		let created;
		{
			const writtenOff = ProjectStatus.GetEmpty();
			if (writtenOff.id) {
				_.merge(writtenOff, {
					json: {
						name: 'Written Off',
						isOpen: false,
						isAwaitingPayment: false,
						isClosed: true,
						isNewProjectStatus: false,
					}
				});
				projectStatus[writtenOff.id] = writtenOff;
			}


			const arears = ProjectStatus.GetEmpty();
			if (arears.id) {
				_.merge(arears, {
					json: {
						name: 'Arears',
						isOpen: false,
						isAwaitingPayment: true,
						isClosed: false,
						isNewProjectStatus: false,
					}
				});
				projectStatus[arears.id] = arears;
			}


			const terminated = ProjectStatus.GetEmpty();
			if (terminated.id) {
				_.merge(terminated, {
					json: {
						name: 'Terminated',
						isOpen: false,
						isAwaitingPayment: false,
						isClosed: true,
						isNewProjectStatus: false,
					}
				});
				projectStatus[terminated.id] = terminated;
			}


			awaitingPayment = ProjectStatus.GetEmpty();
			if (awaitingPayment.id) {
				_.merge(awaitingPayment, {
					json: {
						name: 'Awaiting Payment',
						isOpen: false,
						isAwaitingPayment: true,
						isClosed: false,
						isNewProjectStatus: false,
					}
				});
				projectStatus[awaitingPayment.id] = awaitingPayment;
			}


			const complete = ProjectStatus.GetEmpty();
			if (complete.id) {
				_.merge(complete, {
					json: {
						name: 'Complete',
						isOpen: false,
						isAwaitingPayment: false,
						isClosed: true,
						isNewProjectStatus: false,
					}
				});
				projectStatus[complete.id] = complete;
			}


			inProgress = ProjectStatus.GetEmpty();
			if (inProgress.id) {
				_.merge(inProgress, {
					json: {
						name: 'In Progress',
						isOpen: true,
						isAwaitingPayment: false,
						isClosed: false,
						isNewProjectStatus: false,
					}
				});
				projectStatus[inProgress.id] = inProgress;
			}


			quoting = ProjectStatus.GetEmpty();
			if (quoting.id) {
				_.merge(quoting, {
					json: {
						name: 'Quoting',
						isOpen: true,
						isAwaitingPayment: false,
						isClosed: false,
						isNewProjectStatus: false,
					}
				});
				projectStatus[quoting.id] = quoting;
			}


			const roughIn = ProjectStatus.GetEmpty();
			if (roughIn.id) {
				_.merge(roughIn, {
					json: {
						name: 'Rough In',
						isOpen: true,
						isAwaitingPayment: false,
						isClosed: false,
						isNewProjectStatus: false,
					}
				});
				projectStatus[roughIn.id] = roughIn;
			}


			const finishing = ProjectStatus.GetEmpty();
			if (finishing.id) {
				_.merge(finishing, {
					json: {
						name: 'Finishing',
						isOpen: true,
						isAwaitingPayment: false,
						isClosed: false,
						isNewProjectStatus: false,
					}
				});
				projectStatus[finishing.id] = finishing;
			}


			created = ProjectStatus.GetEmpty();
			if (created.id) {
				_.merge(created, {
					json: {
						name: 'Created',
						isOpen: true,
						isAwaitingPayment: false,
						isClosed: false,
						isNewProjectStatus: true,
					}
				});
				projectStatus[created.id] = created;
			}

		}

		// agents

		// dpAgent1 already exists for company owner.
		let agentSeanMartin;
		let agentStanleyRiley;
		{
			agentSeanMartin = Agent.GetEmpty();
			if (agentSeanMartin.id) {
				agentSeanMartin.json.name = 'Sean D. Martin';
				agentSeanMartin.json.title = 'Electrician';
				agentSeanMartin.json.employmentStatusId = agentsEmploymentStatusCurrentEmployee.id || null;
				agentSeanMartin.json.hourlyWage = 32;
				agentSeanMartin.json.notificationSMSNumber = '(337) 519-6737';
				agents[agentSeanMartin.id] = agentSeanMartin;
			}


			agentStanleyRiley = Agent.GetEmpty();
			if (agentStanleyRiley.id) {
				agentStanleyRiley.json.name = 'Stanley C. Riley';
				agentStanleyRiley.json.title = 'Electrician Apprentice';
				agentStanleyRiley.json.employmentStatusId = agentsEmploymentStatusCurrentEmployee.id || null;
				agentStanleyRiley.json.hourlyWage = 22;
				agentStanleyRiley.json.notificationSMSNumber = '(337) 519-6875';
				agents[agentStanleyRiley.id] = agentStanleyRiley;
			}

		}

		// companies
		let kitchenContractors;
		let honestDave;
		let raiseTheRoof;
		let bobBuilders;
		let ohCrap;
		let throneMasters;
		let getFramed;
		let thinkInsideTheBox;
		let ductsUnlimited;
		let getVented;
		let justFineDesign;
		{
			kitchenContractors = Company.GetEmpty();
			if (kitchenContractors.id) {
				kitchenContractors.json.name = 'Kitchen Contractors';
				companies[kitchenContractors.id] = kitchenContractors;
			}


			honestDave = Company.GetEmpty();
			if (honestDave.id) {
				honestDave.json.name = 'Honest Dave\'s Construction';
				companies[honestDave.id] = honestDave;
			}


			raiseTheRoof = Company.GetEmpty();
			if (raiseTheRoof.id) {
				raiseTheRoof.json.name = 'RAISE THE ROOF Builders';
				companies[raiseTheRoof.id] = raiseTheRoof;
			}


			bobBuilders = Company.GetEmpty();
			if (bobBuilders.id) {
				bobBuilders.json.name = 'Bob and his Builders';
				companies[bobBuilders.id] = bobBuilders;
			}


			ohCrap = Company.GetEmpty();
			if (ohCrap.id) {
				ohCrap.json.name = 'Oh Crap Plumbing';
				companies[ohCrap.id] = ohCrap;
			}


			throneMasters = Company.GetEmpty();
			if (throneMasters.id) {
				throneMasters.json.name = 'Throne Masters';
				companies[throneMasters.id] = throneMasters;
			}


			getFramed = Company.GetEmpty();
			if (getFramed.id) {
				getFramed.json.name = 'Get Framed Incorporated';
				companies[getFramed.id] = getFramed;
			}


			thinkInsideTheBox = Company.GetEmpty();
			if (thinkInsideTheBox.id) {
				thinkInsideTheBox.json.name = 'Think inside the Box Co.';
				companies[thinkInsideTheBox.id] = thinkInsideTheBox;
			}


			ductsUnlimited = Company.GetEmpty();
			if (ductsUnlimited.id) {
				ductsUnlimited.json.name = 'Ducts Unlimited';
				companies[ductsUnlimited.id] = ductsUnlimited;
			}


			getVented = Company.GetEmpty();
			if (getVented.id) {
				getVented.json.name = 'Get Vented';
				companies[getVented.id] = getVented;
			}


			justFineDesign = Company.GetEmpty();
			if (justFineDesign.id) {
				justFineDesign.json.name = 'Just Fine Design';
				companies[justFineDesign.id] = justFineDesign;
			}

		}

		// contacts
		let contactSeanMartin;
		let contactStanleyRiley;
		{
			contactSeanMartin = Contact.GetEmpty();
			if (contactSeanMartin.id) {
				contactSeanMartin.json.name = agentSeanMartin.json.name;
				contactSeanMartin.json.title = agentSeanMartin.json.title;
				contactSeanMartin.json.companyId = dpCompany1.id || null;
				contactSeanMartin.json.notes = '';
				contactSeanMartin.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: agentSeanMartin.json.notificationSMSNumber,
					},
				];
				contacts[contactSeanMartin.id] = contactSeanMartin;
			}


			contactStanleyRiley = Contact.GetEmpty();
			if (contactStanleyRiley.id) {
				contactStanleyRiley.json.name = agentStanleyRiley.json.name;
				contactStanleyRiley.json.title = agentStanleyRiley.json.title;
				contactStanleyRiley.json.companyId = dpCompany1.id || null;
				contactStanleyRiley.json.notes = '';
				contactStanleyRiley.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: agentStanleyRiley.json.notificationSMSNumber,
					},
				];
				contacts[contactStanleyRiley.id] = contactStanleyRiley;
			}

		}

		// kitchenContractors contacts
		let contactJamesMalone;
		let contactCodyTalty;
		let contactRayWare;
		{
			contactJamesMalone = Contact.GetEmpty();
			if (contactJamesMalone.id) {
				contactJamesMalone.json.name = 'James M. Malone';
				contactJamesMalone.json.title = 'Project Manager';
				contactJamesMalone.json.companyId = kitchenContractors.id || null;
				contactJamesMalone.json.notes = '';
				contactJamesMalone.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(970) 638-7249',
					},
				];
				contacts[contactJamesMalone.id] = contactJamesMalone;
			}


			contactCodyTalty = Contact.GetEmpty();
			if (contactCodyTalty.id) {
				contactCodyTalty.json.name = 'Cody Talty';
				contactCodyTalty.json.title = 'Carpenter';
				contactCodyTalty.json.companyId = kitchenContractors.id || null;
				contactCodyTalty.json.notes = '';
				contactCodyTalty.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(867) 897-2903',
					},
				];
				contacts[contactCodyTalty.id] = contactCodyTalty;
			}


			contactRayWare = Contact.GetEmpty();
			if (contactRayWare.id) {
				contactRayWare.json.name = 'Ray S. Ware';
				contactRayWare.json.title = 'Carpenter';
				contactRayWare.json.companyId = kitchenContractors.id || null;
				contactRayWare.json.notes = '';
				contactRayWare.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(613) 722-0578',
					},
				];
				contacts[contactRayWare.id] = contactRayWare;
			}

		}

		// honestDave contacts
		let contactDaveWheeler;
		{
			contactDaveWheeler = Contact.GetEmpty();
			if (contactDaveWheeler.id) {
				contactDaveWheeler.json.name = 'Dave Wheeler';
				contactDaveWheeler.json.title = 'Project Manager';
				contactDaveWheeler.json.companyId = honestDave.id || null;
				contactDaveWheeler.json.notes = '';
				contactDaveWheeler.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(423) 965-0374',
					},
				];
				contacts[contactDaveWheeler.id] = contactDaveWheeler;
			}

		}

		// raiseTheRoof contacts
		let contactBennyHeidelberg;
		{
			contactBennyHeidelberg = Contact.GetEmpty();
			if (contactBennyHeidelberg.id) {
				contactBennyHeidelberg.json.name = 'Benny D. Heidelberg';
				contactBennyHeidelberg.json.title = 'Project Manager';
				contactBennyHeidelberg.json.companyId = raiseTheRoof.id || null;
				contactBennyHeidelberg.json.notes = '';
				contactBennyHeidelberg.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(530) 430-3458',
					},
				];
				contacts[contactBennyHeidelberg.id] = contactBennyHeidelberg;
			}

		}

		// bobBuilders contacts
		let contactFredricKeller;
		{
			contactFredricKeller = Contact.GetEmpty();
			if (contactFredricKeller.id) {
				contactFredricKeller.json.name = 'Fredric B. Keller';
				contactFredricKeller.json.title = 'Project Manager';
				contactFredricKeller.json.companyId = bobBuilders.id || null;
				contactFredricKeller.json.notes = '';
				contactFredricKeller.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(705) 447-2299',
					},
				];
				contacts[contactFredricKeller.id] = contactFredricKeller;
			}

		}

		// ohCrap contacts
		let contactChristopherMurray;
		{
			contactChristopherMurray = Contact.GetEmpty();
			if (contactChristopherMurray.id) {
				contactChristopherMurray.json.name = 'Christopher M. Murray';
				contactChristopherMurray.json.title = 'Project Manager';
				contactChristopherMurray.json.companyId = ohCrap.id || null;
				contactChristopherMurray.json.notes = '';
				contactChristopherMurray.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(902) 530-0522',
					},
				];
				contacts[contactChristopherMurray.id] = contactChristopherMurray;
			}

		}

		// throneMasters contacts
		let contactCliffordFelix;
		let contactCharlaFoley;
		let contactJamesGarcia;
		{
			contactCliffordFelix = Contact.GetEmpty();
			if (contactCliffordFelix.id) {
				contactCliffordFelix.json.name = 'Clifford P. Felix';
				contactCliffordFelix.json.title = 'Project Manager';
				contactCliffordFelix.json.companyId = throneMasters.id || null;
				contactCliffordFelix.json.notes = '';
				contactCliffordFelix.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(613) 622-2296',
					},
				];
				contacts[contactCliffordFelix.id] = contactCliffordFelix;
			}


			contactCharlaFoley = Contact.GetEmpty();
			if (contactCharlaFoley.id) {
				contactCharlaFoley.json.name = 'Charla E. Foley';
				contactCharlaFoley.json.title = 'Plumber';
				contactCharlaFoley.json.companyId = throneMasters.id || null;
				contactCharlaFoley.json.notes = '';
				contactCharlaFoley.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(905) 354-1337',
					},
				];
				contacts[contactCharlaFoley.id] = contactCharlaFoley;
			}


			contactJamesGarcia = Contact.GetEmpty();
			if (contactJamesGarcia.id) {
				contactJamesGarcia.json.name = 'James Y. Garcia';
				contactJamesGarcia.json.title = 'Plumber';
				contactJamesGarcia.json.companyId = throneMasters.id || null;
				contactJamesGarcia.json.notes = '';
				contactJamesGarcia.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(418) 314-8577',
					},
				];
				contacts[contactJamesGarcia.id] = contactJamesGarcia;
			}

		}

		// getFramed contacts
		let contactHaroldMcDonald;
		let contactGuyGrounds;
		{
			contactHaroldMcDonald = Contact.GetEmpty();
			if (contactHaroldMcDonald.id) {
				contactHaroldMcDonald.json.name = 'Harold M. McDonald';
				contactHaroldMcDonald.json.title = 'Project Manager';
				contactHaroldMcDonald.json.companyId = getFramed.id || null;
				contactHaroldMcDonald.json.notes = '';
				contactHaroldMcDonald.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(416) 861-2543',
					},
				];
				contacts[contactHaroldMcDonald.id] = contactHaroldMcDonald;
			}


			contactGuyGrounds = Contact.GetEmpty();
			if (contactGuyGrounds.id) {
				contactGuyGrounds.json.name = 'Guy S. Grounds';
				contactGuyGrounds.json.title = 'Carpenter';
				contactGuyGrounds.json.companyId = getFramed.id || null;
				contactGuyGrounds.json.notes = '';
				contactGuyGrounds.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(514) 241-1489',
					},
				];
				contacts[contactGuyGrounds.id] = contactGuyGrounds;
			}

		}


		// thinkInsideTheBox contacts
		let contactJohnKrieger;
		{
			contactJohnKrieger = Contact.GetEmpty();
			if (contactJohnKrieger.id) {
				contactJohnKrieger.json.name = 'John L. Krieger';
				contactJohnKrieger.json.title = 'Project Manager';
				contactJohnKrieger.json.companyId = thinkInsideTheBox.id || null;
				contactJohnKrieger.json.notes = '';
				contactJohnKrieger.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(416) 861-2543',
					},
				];
				contacts[contactJohnKrieger.id] = contactJohnKrieger;
			}

		}


		// ductsUnlimited contacts
		let contactChristopherSims;
		{
			contactChristopherSims = Contact.GetEmpty();
			if (contactChristopherSims.id) {
				contactChristopherSims.json.name = 'Christopher A. Sims';
				contactChristopherSims.json.title = 'Project Manager';
				contactChristopherSims.json.companyId = ductsUnlimited.id || null;
				contactChristopherSims.json.notes = '';
				contactChristopherSims.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(416) 291-6585',
					},
				];
				contacts[contactChristopherSims.id] = contactChristopherSims;
			}

		}


		// getVented contacts
		let contactRudolphMorris;
		{
			contactRudolphMorris = Contact.GetEmpty();
			if (contactRudolphMorris.id) {
				contactRudolphMorris.json.name = 'Rudolph R. Morris';
				contactRudolphMorris.json.title = 'Project Manager';
				contactRudolphMorris.json.companyId = getVented.id || null;
				contactRudolphMorris.json.notes = '';
				contactRudolphMorris.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(250) 221-6992',
					},
				];
				contacts[contactRudolphMorris.id] = contactRudolphMorris;
			}

		}


		// justFineDesign contacts
		let contactWilliamWoodford;
		{
			contactWilliamWoodford = Contact.GetEmpty();
			if (contactWilliamWoodford.id) {
				contactWilliamWoodford.json.name = 'William J. Woodford';
				contactWilliamWoodford.json.title = 'Project Manager';
				contactWilliamWoodford.json.companyId = justFineDesign.id || null;
				contactWilliamWoodford.json.notes = '';
				contactWilliamWoodford.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(416) 744-7676',
					},
				];
				contacts[contactWilliamWoodford.id] = contactWilliamWoodford;
			}

		}


		// homeowners

		let contactHaroldCurry;
		let contactGeorgianaAtkinson;
		let contactLindaPhaneuf;
		let contactRobinBergman;
		let contactJordanMcPeak;
		{
			contactHaroldCurry = Contact.GetEmpty();
			if (contactHaroldCurry.id) {
				contactHaroldCurry.json.name = 'Harold Curry';
				contactHaroldCurry.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(819) 689-9701',
					},
				];
				contacts[contactHaroldCurry.id] = contactHaroldCurry;
			}


			contactGeorgianaAtkinson = Contact.GetEmpty();
			if (contactGeorgianaAtkinson.id) {
				contactGeorgianaAtkinson.json.name = 'Georgiana Atkinson';
				contactGeorgianaAtkinson.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(780) 525-1779',
					},
				];
				contacts[contactGeorgianaAtkinson.id] = contactGeorgianaAtkinson;
			}


			contactLindaPhaneuf = Contact.GetEmpty();
			if (contactLindaPhaneuf.id) {
				contactLindaPhaneuf.json.name = 'Linda Phaneuf';
				contactLindaPhaneuf.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(514) 773-7358',
					},
				];
				contacts[contactLindaPhaneuf.id] = contactLindaPhaneuf;
			}


			contactRobinBergman = Contact.GetEmpty();
			if (contactRobinBergman.id) {
				contactRobinBergman.json.name = 'Robin Bergman';
				contactRobinBergman.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(781) 261-4366',
					},
				];
				contacts[contactRobinBergman.id] = contactRobinBergman;
			}




			contactJordanMcPeak = Contact.GetEmpty();
			if (contactJordanMcPeak.id) {
				contactJordanMcPeak.json.name = 'Jordan McPeak';
				contactJordanMcPeak.json.phoneNumbers = [
					{
						id: GenerateID(),
						label: 'Mobile',
						value: '(698) 261-4366',
					},
				];
				contacts[contactJordanMcPeak.id] = contactJordanMcPeak;
			}

		}



		// ALL PROJECTS
		{
			// projects
			// projectNotes
			// assignments


			const todayLocal = DateTime.local();

			const todayPlus1WeekMonday830 = DateTime.fromObject(
				{
					weekNumber: todayLocal.weekNumber + 1,
					weekday: 1,
					hour: 8,
					minute: 30,
				},
				{
					zone: 'local',
				},
			);


			const kitchenReno1 = Project.GetEmpty();
			if (kitchenReno1.id) {
				kitchenReno1.json.name = 'Kitchen Reno';
				kitchenReno1.json.statusId = quoting.id || null;
				kitchenReno1.json.addresses = [
					{
						id: GenerateID(),
						label: 'Jobsite',
						value: '4506 Islington Ave',
					},
				];
				kitchenReno1.json.companies = [
					{
						id: GenerateID(),
						label: 'General Contractor',
						value: kitchenContractors.id || null,
					},
					{
						id: GenerateID(),
						label: 'Plumbing',
						value: throneMasters.id || null,
					},
				];

				kitchenReno1.json.contacts = [
					{
						id: GenerateID(),
						label: 'Manager',
						value: contactJamesMalone.id || null,
					},
					{
						id: GenerateID(),
						label: 'Homeowner',
						value: contactHaroldCurry.id || null,
					},
					{
						id: GenerateID(),
						label: 'Plumber',
						value: contactCliffordFelix.id || null,
					},
					{
						id: GenerateID(),
						label: 'Finishing Carpenter',
						value: contactCodyTalty.id || null,
					},
				];
				kitchenReno1.json.hasStartISO8601 = true;



				kitchenReno1.json.startISO8601 = todayPlus1WeekMonday830.toISO();
				kitchenReno1.json.startTimeMode = 'time';

				const todayP1WeekFruday1630 = DateTime.fromObject({
					weekNumber: todayLocal.weekNumber + 1,
					weekday: 5,
					hour: 16,
					minute: 30,
				},
					{
						zone: 'local',
					},
				);

				kitchenReno1.json.hasEndISO8601 = true;
				kitchenReno1.json.endISO8601 = todayP1WeekFruday1630.toISO();
				kitchenReno1.json.endTimeMode = 'time';
				projects[kitchenReno1.id] = kitchenReno1;
			}




			// Notes
			const kitchenReno1Note1 = ProjectNote.GetEmpty();
			if (kitchenReno1Note1.id) {
				kitchenReno1Note1.lastModifiedISO8601 = todayLocal.minus({ weeks: 2 }).toISO();
				kitchenReno1Note1.json.lastModifiedBillingId = billingContactId;
				kitchenReno1Note1.json.projectId = kitchenReno1.id || null;
				kitchenReno1Note1.json.content = {
					html: 'Client gave the OK to begin on schedule.',
				} as IProjectNoteStyledText;
				kitchenReno1Note1.json.contentType = 'styled-text';
				projectNotes[kitchenReno1Note1.id] = kitchenReno1Note1;
			}


			// Assignments
			const assignments1 = Assignment.GetEmpty();
			if (assignments1.id) {
				assignments1.json.hasStartISO8601 = true;
				assignments1.json.startISO8601 = kitchenReno1.json.startISO8601;
				assignments1.json.startTimeMode = kitchenReno1.json.startTimeMode;
				assignments1.json.hasEndISO8601 = true;
				assignments1.json.endISO8601 = kitchenReno1.json.endISO8601;
				assignments1.json.endTimeMode = kitchenReno1.json.endTimeMode;
				assignments1.json.statusId = assignmentStatusAssigned.id || null;
				assignments1.json.projectId = kitchenReno1.id || null;
				assignments1.json.agentIds = [agentSeanMartin.id || null];
				assignments1.json.workRequested = 'Standard kitchen reno, plans on site.';
				assignments[assignments1.id] = assignments1;
			}


			const assignments2 = Assignment.GetEmpty();
			if (assignments2.id) {
				assignments2.json.hasStartISO8601 = true;
				assignments2.json.startISO8601 = kitchenReno1.json.startISO8601;
				assignments2.json.startTimeMode = kitchenReno1.json.startTimeMode;
				assignments2.json.hasEndISO8601 = true;
				assignments2.json.endISO8601 = kitchenReno1.json.endISO8601;
				assignments2.json.endTimeMode = kitchenReno1.json.endTimeMode;
				assignments2.json.statusId = assignmentStatusAssigned.id || null;
				assignments2.json.projectId = kitchenReno1.id || null;
				assignments2.json.agentIds = [agentStanleyRiley.id || null];
				assignments2.json.workRequested = 'Standard kitchen reno, plans on site.';
				assignments[assignments2.id] = assignments2;
			}

		} // kitchenReno1
		{
			const todayLocal = DateTime.local();

			const todayPlus1WeekMonday830 = DateTime.fromObject(
				{
					weekNumber: todayLocal.weekNumber + 2,
					weekday: 1,
					hour: 8,
					minute: 30,
				},
				{
					zone: 'local',
				},
			);

			// projects
			// projectNotes
			// assignments
			const basementReno = Project.GetEmpty();
			if (basementReno.id) {
				basementReno.json.name = 'Basement Reno';
				basementReno.json.statusId = quoting.id || null;
				basementReno.json.addresses = [
					{
						id: GenerateID(),
						label: 'Jobsite',
						value: '4046 137th Avenue',
					},
				];
				basementReno.json.companies = [
					{
						id: GenerateID(),
						label: 'General Contractor',
						value: getFramed.id || null,
					},
					{
						id: GenerateID(),
						label: 'Plumbing',
						value: ohCrap.id || null,
					},
				];

				basementReno.json.contacts = [
					{
						id: GenerateID(),
						label: 'Manager',
						value: contactHaroldMcDonald.id || null,
					},
					{
						id: GenerateID(),
						label: 'Homeowner',
						value: contactGeorgianaAtkinson.id || null,
					},
					{
						id: GenerateID(),
						label: 'Plumber',
						value: contactChristopherMurray.id || null,
					},
					{
						id: GenerateID(),
						label: 'Carpenter',
						value: contactGuyGrounds.id || null,
					},
				];
				basementReno.json.hasStartISO8601 = true;

				basementReno.json.startISO8601 = todayPlus1WeekMonday830.toISO();
				basementReno.json.startTimeMode = 'time';

				const todayP1WeekFruday1630 = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber + 2,
						weekday: 5,
						hour: 16,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				basementReno.json.hasEndISO8601 = true;
				basementReno.json.endISO8601 = todayP1WeekFruday1630.toISO();
				basementReno.json.endTimeMode = 'time';
				projects[basementReno.id] = basementReno;
			}

			// Notes
			const note1 = ProjectNote.GetEmpty();
			if (note1.id) {
				note1.lastModifiedISO8601 = todayLocal.minus({ weeks: 2 }).toISO();
				note1.json.lastModifiedBillingId = billingContactId;
				note1.json.projectId = basementReno.id || null;
				note1.json.content = {
					html: 'Client gave the OK to begin on schedule.',
				} as IProjectNoteStyledText;
				note1.json.contentType = 'styled-text';
				projectNotes[note1.id] = note1;
			}


			// Assignments
			const assignments1 = Assignment.GetEmpty();
			if (assignments1.id) {
				assignments1.json.hasStartISO8601 = true;
				assignments1.json.startISO8601 = basementReno.json.startISO8601;
				assignments1.json.startTimeMode = basementReno.json.startTimeMode;
				assignments1.json.hasEndISO8601 = true;
				assignments1.json.endISO8601 = basementReno.json.endISO8601;
				assignments1.json.endTimeMode = basementReno.json.endTimeMode;
				assignments1.json.statusId = assignmentStatusAssigned.id || null;
				assignments1.json.projectId = basementReno.id || null;
				assignments1.json.agentIds = [agentSeanMartin.id || null];
				assignments1.json.workRequested = 'Standard basement reno, plans at shop.';
				assignments[assignments1.id] = assignments1;
			}


			const assignments2 = Assignment.GetEmpty();
			if (assignments2.id) {
				assignments2.json.hasStartISO8601 = true;
				assignments2.json.startISO8601 = basementReno.json.startISO8601;
				assignments2.json.startTimeMode = basementReno.json.startTimeMode;
				assignments2.json.hasEndISO8601 = true;
				assignments2.json.endISO8601 = basementReno.json.endISO8601;
				assignments2.json.endTimeMode = basementReno.json.endTimeMode;
				assignments2.json.statusId = assignmentStatusAssigned.id || null;
				assignments2.json.projectId = basementReno.id || null;
				assignments2.json.agentIds = [agentStanleyRiley.id || null];
				assignments2.json.workRequested = 'Standard basement reno, plans at shop.';
				assignments[assignments2.id] = assignments2;
			}

		} // basementReno
		let bathroomReno;
		let bathroomRenoAssignment1;
		{
			const todayLocal = DateTime.local();

			const todayPlus1WeekMonday830 = DateTime.fromObject(
				{
					weekNumber: todayLocal.weekNumber - 2,
					weekday: 1,
					hour: 8,
					minute: 30,
				},
				{
					zone: 'local',
				},
			);

			// projects
			// projectNotes
			// assignments
			bathroomReno = Project.GetEmpty();
			if (bathroomReno.id) {
				bathroomReno.json.name = 'Bathroom Rewire';
				bathroomReno.json.statusId = awaitingPayment.id || null;
				bathroomReno.json.addresses = [
					{
						id: GenerateID(),
						label: 'Jobsite',
						value: '1368 Eglinton Avenue',
					},
				];
				bathroomReno.json.companies = [
					{
						id: GenerateID(),
						label: 'General Contractor',
						value: getFramed.id || null,
					},
					{
						id: GenerateID(),
						label: 'Plumbing',
						value: ohCrap.id || null,
					},
				];

				bathroomReno.json.contacts = [
					{
						id: GenerateID(),
						label: 'Manager',
						value: contactHaroldMcDonald.id || null,
					},
					{
						id: GenerateID(),
						label: 'Homeowner',
						value: contactGeorgianaAtkinson.id || null,
					},
					{
						id: GenerateID(),
						label: 'Plumber',
						value: contactChristopherMurray.id || null,
					},
					{
						id: GenerateID(),
						label: 'Carpenter',
						value: contactGuyGrounds.id || null,
					},
				];
				bathroomReno.json.hasStartISO8601 = true;



				bathroomReno.json.startISO8601 = todayPlus1WeekMonday830.toISO();
				bathroomReno.json.startTimeMode = 'time';

				const todayP1WeekFruday1630 = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber - 2,
						weekday: 5,
						hour: 16,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				bathroomReno.json.hasEndISO8601 = true;
				bathroomReno.json.endISO8601 = todayP1WeekFruday1630.toISO();
				bathroomReno.json.endTimeMode = 'time';
				projects[bathroomReno.id] = bathroomReno;

				console.log('bathroomReno', bathroomReno);
			}


			// Notes
			const note1 = ProjectNote.GetEmpty();
			if (note1.id) {
				note1.lastModifiedISO8601 = todayLocal.minus({ weeks: 2 }).toISO();
				note1.json.lastModifiedBillingId = billingContactId;
				note1.json.projectId = bathroomReno.id || null;
				note1.json.content = {
					html: 'Client gave the OK to begin on schedule.',
				} as IProjectNoteStyledText;
				note1.json.contentType = 'styled-text';
				projectNotes[note1.id] = note1;
			}


			// Assignments
			bathroomRenoAssignment1 = Assignment.GetEmpty();
			if (bathroomRenoAssignment1.id) {
				bathroomRenoAssignment1.json.hasStartISO8601 = true;
				bathroomRenoAssignment1.json.startISO8601 = bathroomReno.json.startISO8601;
				bathroomRenoAssignment1.json.startTimeMode = bathroomReno.json.startTimeMode;
				bathroomRenoAssignment1.json.hasEndISO8601 = true;
				bathroomRenoAssignment1.json.endISO8601 = bathroomReno.json.endISO8601;
				bathroomRenoAssignment1.json.endTimeMode = bathroomReno.json.endTimeMode;
				bathroomRenoAssignment1.json.statusId = assignmentStatusAssigned.id || null;
				bathroomRenoAssignment1.json.projectId = bathroomReno.id || null;
				bathroomRenoAssignment1.json.agentIds = [agentSeanMartin.id || null];
				bathroomRenoAssignment1.json.workRequested = 'Standard basement reno, plans at shop.';
				assignments[bathroomRenoAssignment1.id] = bathroomRenoAssignment1;
			}


			const assignments2 = Assignment.GetEmpty();
			if (assignments2.id) {
				assignments2.json.hasStartISO8601 = true;
				assignments2.json.startISO8601 = bathroomReno.json.startISO8601;
				assignments2.json.startTimeMode = bathroomReno.json.startTimeMode;
				assignments2.json.hasEndISO8601 = true;
				assignments2.json.endISO8601 = bathroomReno.json.endISO8601;
				assignments2.json.endTimeMode = bathroomReno.json.endTimeMode;
				assignments2.json.statusId = assignmentStatusAssigned.id || null;
				assignments2.json.projectId = bathroomReno.id || null;
				assignments2.json.agentIds = [agentStanleyRiley.id || null];
				assignments2.json.workRequested = 'Standard basement reno, plans at shop.';
				assignments[assignments2.id] = assignments2;
			}

		} // bathroomReno
		let garageWiringServiceReplacement;
		let garageServiceReplacementAssignment;
		{
			const todayLocal = DateTime.local();

			const startLocal = DateTime.fromObject(
				{
					weekNumber: todayLocal.weekNumber,
					weekday: 1,
					hour: 8,
					minute: 30,
				},
				{
					zone: 'local',
				},
			);

			// projects
			// projectNotes
			// assignments
			garageWiringServiceReplacement = Project.GetEmpty();
			if (garageWiringServiceReplacement.id) {
				garageWiringServiceReplacement.json.name = 'Garage Wiring & 200A Service Replacement';
				garageWiringServiceReplacement.json.statusId = inProgress.id || null;
				garageWiringServiceReplacement.json.addresses = [
					{
						id: GenerateID(),
						label: 'Jobsite',
						value: '2143 rue Garneau',
					},
				];
				garageWiringServiceReplacement.json.companies = [
					{
						id: GenerateID(),
						label: 'General Contractor',
						value: bobBuilders.id || null,
					},
				];

				garageWiringServiceReplacement.json.contacts = [
					{
						id: GenerateID(),
						label: 'Manager',
						value: contactFredricKeller.id || null,
					},
					{
						id: GenerateID(),
						label: 'Homeowner',
						value: contactLindaPhaneuf.id || null,
					},
					{
						id: GenerateID(),
						label: 'Carpenter',
						value: contactFredricKeller.id || null,
					},
				];
				garageWiringServiceReplacement.json.hasStartISO8601 = true;



				garageWiringServiceReplacement.json.startISO8601 = startLocal.toISO();
				garageWiringServiceReplacement.json.startTimeMode = 'time';

				const endLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 5,
						hour: 16,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				garageWiringServiceReplacement.json.hasEndISO8601 = true;
				garageWiringServiceReplacement.json.endISO8601 = endLocal.toISO();
				garageWiringServiceReplacement.json.endTimeMode = 'time';
				projects[garageWiringServiceReplacement.id] = garageWiringServiceReplacement;
			}


			// Notes
			const note1 = ProjectNote.GetEmpty();
			if (note1.id) {
				note1.lastModifiedISO8601 = todayLocal.minus({ weeks: 2 }).toISO();
				note1.json.lastModifiedBillingId = billingContactId;
				note1.json.projectId = garageWiringServiceReplacement.id || null;
				note1.json.content = {
					html: 'Client gave the OK to begin on schedule.',
				} as IProjectNoteStyledText;
				note1.json.contentType = 'styled-text';
				projectNotes[note1.id] = note1;
			}


			// Assignments
			garageServiceReplacementAssignment = Assignment.GetEmpty();
			if (garageServiceReplacementAssignment.id) {
				garageServiceReplacementAssignment.json.hasStartISO8601 = true;
				garageServiceReplacementAssignment.json.startISO8601 = garageWiringServiceReplacement.json.startISO8601;
				garageServiceReplacementAssignment.json.startTimeMode = garageWiringServiceReplacement.json.startTimeMode;
				garageServiceReplacementAssignment.json.hasEndISO8601 = true;
				garageServiceReplacementAssignment.json.endISO8601 = garageWiringServiceReplacement.json.endISO8601;
				garageServiceReplacementAssignment.json.endTimeMode = garageWiringServiceReplacement.json.endTimeMode;
				garageServiceReplacementAssignment.json.statusId = assignmentStatusAssigned.id || null;
				garageServiceReplacementAssignment.json.projectId = garageWiringServiceReplacement.id || null;
				garageServiceReplacementAssignment.json.agentIds = [dpAgent1.id || null];
				garageServiceReplacementAssignment.json.workRequested = 'Garage replacement and 200A service replacement.';
				assignments[garageServiceReplacementAssignment.id] = garageServiceReplacementAssignment;
			}

		} // garageWiringServiceReplacement

		// Some small oneoff projects.
		let lightingFixtureReplacement;
		{
			lightingFixtureReplacement = Project.GetEmpty();
			if (lightingFixtureReplacement.id) {
				lightingFixtureReplacement.json.name = 'Replace Light Fixture';
				lightingFixtureReplacement.json.statusId = created.id || null;

				lightingFixtureReplacement.json.addresses = [
					{
						id: GenerateID(),
						label: 'Jobsite',
						value: '619 Burke Street',
					},
				];

				lightingFixtureReplacement.json.contacts = [
					{
						id: GenerateID(),
						label: 'Homeowner',
						value: contactRobinBergman.id || null,
					},
				];

				projects[lightingFixtureReplacement.id] = lightingFixtureReplacement;
			}


			const assignment1 = Assignment.GetEmpty();
			if (assignment1.id) {
				assignment1.json.statusId = assignmentStatusToBeScheduled.id || null;
				assignment1.json.projectId = lightingFixtureReplacement.id || null;
				assignment1.json.workRequested = 'Replace Lighting Fixture';
				assignments[assignment1.id] = assignment1;
			}

		}
		let outsidePlug;
		{
			outsidePlug = Project.GetEmpty();
			if (outsidePlug.id) {
				outsidePlug.json.name = 'Outside Plug';
				outsidePlug.json.statusId = created.id || null;

				outsidePlug.json.addresses = [
					{
						id: GenerateID(),
						label: 'Jobsite',
						value: '880 Howard River Rd',
					},
				];

				outsidePlug.json.contacts = [
					{
						id: GenerateID(),
						label: 'Homeowner',
						value: contactJordanMcPeak.id || null,
					},
				];

				projects[outsidePlug.id] = outsidePlug;
			}


			const assignment1 = Assignment.GetEmpty();
			if (assignment1.id) {
				assignment1.json.statusId = assignmentStatusToBeScheduled.id || null;
				assignment1.json.projectId = outsidePlug.id || null;
				assignment1.json.workRequested = 'Install Outside Plug';
				assignments[assignment1.id] = assignment1;
			}

		}


		// estimatingManHours


		// labour
		{
			const todayLocal = DateTime.local();


			// Bathoom Reno
			{
				const startLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 1,
						hour: 8,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				// const endLocal = DateTime.fromObject(
				// 	{
				// 		weekNumber: todayLocal.weekNumber,
				// 		weekday: 1,
				// 		hour: 16,
				// 		minute: 30,
				// 	}, 
				// 	{
				// 		zone: 'local',
				// 	},
				// );

				if (todayLocal > startLocal && todayLocal.day !== startLocal.day) {
					const entry = Labour.GetEmpty();
					if (entry.id) {
						entry.json.projectId = bathroomReno.id || null;
						entry.json.agentId = dpAgent1.id || null;
						entry.json.assignmentId = bathroomRenoAssignment1.id || null;
						entry.json.typeId = billable.id || null;
						entry.json.timeMode = 'date-and-hours';
						entry.json.hours = 5;
						entry.json.startISO8601 = startLocal.toISO();
						entry.json.isActive = false;
						entry.json.locationType = 'on-site';
						entry.json.isEnteredThroughTelephoneCompanyAccess = false;
						labour[entry.id] = entry;
					}

				}

			}

			{
				const startLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 2,
						hour: 8,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				// const endLocal = DateTime.fromObject(
				// 	{
				// 		weekNumber: todayLocal.weekNumber,
				// 		weekday: 2,
				// 		hour: 16,
				// 		minute: 30,
				// 	}, 
				// 	{
				// 		zone: 'local',
				// 	},
				// );

				if (todayLocal > startLocal && todayLocal.day !== startLocal.day) {
					const entry = Labour.GetEmpty();
					if (entry.id) {
						entry.json.projectId = bathroomReno.id || null;
						entry.json.agentId = dpAgent1.id || null;
						entry.json.assignmentId = bathroomRenoAssignment1.id || null;
						entry.json.typeId = billable.id || null;
						entry.json.timeMode = 'date-and-hours';
						entry.json.hours = 8;
						entry.json.startISO8601 = startLocal.toISO();
						entry.json.isActive = false;
						entry.json.locationType = 'on-site';
						entry.json.isEnteredThroughTelephoneCompanyAccess = false;
						labour[entry.id] = entry;
					}

				}

			}

			{
				const startLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 3,
						hour: 8,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				const endLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 3,
						hour: 16,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				if (todayLocal > startLocal && todayLocal.day !== startLocal.day) {
					const entry = Labour.GetEmpty();
					if (entry.id) {
						entry.json.projectId = bathroomReno.id || null;
						entry.json.agentId = dpAgent1.id || null;
						entry.json.assignmentId = bathroomRenoAssignment1.id || null;
						entry.json.typeId = billable.id || null;
						entry.json.timeMode = 'start-stop-timestamp';
						entry.json.startISO8601 = startLocal.toISO();
						entry.json.endISO8601 = endLocal.toISO();
						entry.json.isActive = false;
						entry.json.locationType = 'on-site';
						entry.json.isEnteredThroughTelephoneCompanyAccess = false;
						labour[entry.id] = entry;
					}

				}

			}

			{
				const startLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 4,
						hour: 8,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				const endLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 4,
						hour: 16,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				if (todayLocal > startLocal && todayLocal.day !== startLocal.day) {
					const entry = Labour.GetEmpty();
					if (entry.id) {
						entry.json.projectId = bathroomReno.id || null;
						entry.json.agentId = dpAgent1.id || null;
						entry.json.assignmentId = bathroomRenoAssignment1.id || null;
						entry.json.typeId = billable.id || null;
						entry.json.timeMode = 'start-stop-timestamp';
						entry.json.startISO8601 = startLocal.toISO();
						entry.json.endISO8601 = endLocal.toISO();
						entry.json.isActive = false;
						entry.json.locationType = 'on-site';
						entry.json.isEnteredThroughTelephoneCompanyAccess = false;
						labour[entry.id] = entry;
					}

				}

			}

			{
				const startLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 5,
						hour: 8,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				const endLocal = DateTime.fromObject(
					{
						weekNumber: todayLocal.weekNumber,
						weekday: 5,
						hour: 16,
						minute: 30,
					},
					{
						zone: 'local',
					},
				);

				if (todayLocal > startLocal && todayLocal.day !== startLocal.day) {
					const entry = Labour.GetEmpty();
					if (entry.id) {
						entry.json.projectId = bathroomReno.id || null;
						entry.json.agentId = dpAgent1.id || null;
						entry.json.assignmentId = bathroomRenoAssignment1.id || null;
						entry.json.typeId = billable.id || null;
						entry.json.timeMode = 'start-stop-timestamp';
						entry.json.startISO8601 = startLocal.toISO();
						entry.json.endISO8601 = endLocal.toISO();
						entry.json.isActive = false;
						entry.json.locationType = 'on-site';
						entry.json.isEnteredThroughTelephoneCompanyAccess = false;
						labour[entry.id] = entry;
					}

				}

			}


			// Add an active entry for today.
			{
				const startLocal = todayLocal.minus({ hours: 2.25 });

				const entry = Labour.GetEmpty();
				if (entry.id) {
					entry.json.projectId = garageWiringServiceReplacement.id || null;
					entry.json.agentId = dpAgent1.id || null;
					entry.json.assignmentId = garageServiceReplacementAssignment.id || null;
					entry.json.typeId = billable.id || null;
					entry.json.timeMode = 'start-stop-timestamp';
					entry.json.startISO8601 = startLocal.toISO();
					entry.json.isActive = true;
					entry.json.locationType = 'on-site';
					entry.json.isEnteredThroughTelephoneCompanyAccess = false;
					labour[entry.id] = entry;
				}

			}


		}


















		// materials


		// settingsDefault
		// settingsProvisioning
		// settingsUser
		// products
		// skills



		for (const [key, value] of Object.entries(billingCompanies)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingCompanies[key] = mod;
		}
		for (const [key, value] of Object.entries(billingContacts)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			mod.applicationData = JSON.stringify(mod.applicationData);
			billingContacts[key] = mod;
		}
		for (const [key, value] of Object.entries(billingCouponCodes)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingCouponCodes[key] = mod;
		}
		for (const [key, value] of Object.entries(billingCurrency)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingCurrency[key] = mod;
		}
		for (const [key, value] of Object.entries(billingIndustries)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingIndustries[key] = mod;
		}
		for (const [key, value] of Object.entries(billingInvoices)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingInvoices[key] = mod;
		}
		for (const [key, value] of Object.entries(billingJournalEntries)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingJournalEntries[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPackages)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPackages[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPackagesType)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPackagesType[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPaymentFrequencies)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPaymentFrequencies[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPaymentMethod)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPaymentMethod[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPermissionsBool)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPermissionsBool[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPermissionsGroups)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPermissionsGroups[key] = mod;
		}
		for (const [key, value] of Object.entries(billingPermissionsGroupsMemberships)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingPermissionsGroupsMemberships[key] = mod;
		}
		for (const [key, value] of Object.entries(billingSessions)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingSessions[key] = mod;
		}
		for (const [key, value] of Object.entries(billingSubscriptions)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingSubscriptions[key] = mod;
		}
		for (const [key, value] of Object.entries(billingSubscriptionsProvisioningStatus)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			billingSubscriptionsProvisioningStatus[key] = mod;
		}
		for (const [key, value] of Object.entries(agents)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			agents[key] = mod;
		}
		for (const [key, value] of Object.entries(assignments)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			assignments[key] = mod;
		}
		for (const [key, value] of Object.entries(assignmentStatus)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			assignmentStatus[key] = mod;
		}
		for (const [key, value] of Object.entries(companies)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			companies[key] = mod;
		}
		for (const [key, value] of Object.entries(agentsEmploymentStatus)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			agentsEmploymentStatus[key] = mod;
		}
		for (const [key, value] of Object.entries(contacts)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			contacts[key] = mod;
		}
		for (const [key, value] of Object.entries(estimatingManHours)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			estimatingManHours[key] = mod;
		}
		for (const [key, value] of Object.entries(labourSubtypeHolidays)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			labourSubtypeHolidays[key] = mod;
		}
		for (const [key, value] of Object.entries(labourSubtypeException)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			labourSubtypeException[key] = mod;
		}
		for (const [key, value] of Object.entries(labourSubtypeNonBillable)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			labourSubtypeNonBillable[key] = mod;
		}
		for (const [key, value] of Object.entries(labour)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			labour[key] = mod;
		}
		for (const [key, value] of Object.entries(labourTypes)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			labourTypes[key] = mod;
		}
		for (const [key, value] of Object.entries(materials)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			materials[key] = mod;
		}
		for (const [key, value] of Object.entries(projectNotes)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			projectNotes[key] = mod;
		}
		for (const [key, value] of Object.entries(projects)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			projects[key] = mod;
		}
		for (const [key, value] of Object.entries(projectStatus)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			projectStatus[key] = mod;
		}
		for (const [key, value] of Object.entries(settingsDefault)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			settingsDefault[key] = mod;
		}
		for (const [key, value] of Object.entries(settingsProvisioning)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			settingsProvisioning[key] = mod;
		}
		for (const [key, value] of Object.entries(settingsUser)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			settingsUser[key] = mod;
		}
		for (const [key, value] of Object.entries(products)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			products[key] = mod;
		}
		for (const [key, value] of Object.entries(skills)) {
			const mod = value as any;
			mod.json = JSON.stringify(mod.json);
			skills[key] = mod;
		}



		this.$store.commit('UpdateBillingCompaniesRemote', billingCompanies);
		this.$store.commit('UpdateBillingContactsRemote', billingContacts);
		this.$store.commit('UpdateBillingCouponCodesRemote', billingCouponCodes);
		this.$store.commit('UpdateBillingCurrencyRemote', billingCurrency);
		this.$store.commit('UpdateBillingIndustriesRemote', billingIndustries);
		this.$store.commit('UpdateBillingInvoicesRemote', billingInvoices);
		this.$store.commit('UpdateBillingJournalEntriesRemote', billingJournalEntries);
		this.$store.commit('UpdateBillingPackagesRemote', billingPackages);
		this.$store.commit('UpdateBillingPackagesTypeRemote', billingPackagesType);
		this.$store.commit('UpdateBillingPaymentFrequenciesRemote', billingPaymentFrequencies);
		this.$store.commit('UpdateBillingPaymentMethodRemote', billingPaymentMethod);
		this.$store.commit('UpdateBillingPermissionsBoolRemote', billingPermissionsBool);
		this.$store.commit('UpdateBillingPermissionsGroupsRemote', billingPermissionsGroups);
		this.$store.commit('UpdateBillingPermissionsGroupsMembershipsRemote', billingPermissionsGroupsMemberships);
		this.$store.commit('UpdateBillingSessionsRemote', billingSessions);
		this.$store.commit('UpdateBillingSubscriptionsRemote', billingSubscriptions);
		this.$store.commit('UpdateBillingSubscriptionsProvisioningStatusRemote', billingSubscriptionsProvisioningStatus);
		this.$store.commit('UpdateAgentsRemote', agents);
		this.$store.commit('UpdateAssignmentsRemote', assignments);
		this.$store.commit('UpdateAssignmentStatusRemote', assignmentStatus);
		this.$store.commit('UpdateCompaniesRemote', companies);
		this.$store.commit('UpdateAgentsEmploymentStatusRemote', agentsEmploymentStatus);
		this.$store.commit('UpdateContactsRemote', contacts);
		this.$store.commit('UpdateEstimatingManHoursRemote', estimatingManHours);
		this.$store.commit('UpdateLabourSubtypeHolidaysRemote', labourSubtypeHolidays);
		this.$store.commit('UpdateLabourSubtypeExceptionRemote', labourSubtypeException);
		this.$store.commit('UpdateLabourSubtypeNonBillableRemote', labourSubtypeNonBillable);
		this.$store.commit('UpdateLabourRemote', labour);
		this.$store.commit('UpdateLabourTypesRemote', labourTypes);
		this.$store.commit('UpdateMaterialsRemote', materials);
		this.$store.commit('UpdateProjectNotesRemote', projectNotes);
		this.$store.commit('UpdateProjectsRemote', projects);
		this.$store.commit('UpdateProjectStatusRemote', projectStatus);
		this.$store.commit('UpdateSettingsDefaultRemote', settingsDefault);
		this.$store.commit('UpdateSettingsProvisioningRemote', settingsProvisioning);
		this.$store.commit('UpdateSettingsUserRemote', settingsUser);
		this.$store.commit('UpdateProductsRemote', products);
		this.$store.commit('UpdateSkillsRemote', skills);
	}

}
</script>