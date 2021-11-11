/* eslint-disable @typescript-eslint/explicit-module-boundary-types */

import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import Vue from 'vue';
import '@/Data/CRM/Assignment/Assignment';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { BillingSubscriptionsProvisioningStatus, IBillingSubscriptionsProvisioningStatus } from '@/Data/Billing/BillingSubscriptionsProvisioningStatus/BillingSubscriptionsProvisioningStatus';
import { BillingPermissionsGroupsMemberships, IBillingPermissionsGroupsMemberships } from '@/Data/Billing/BillingPermissionsGroupsMemberships/BillingPermissionsGroupsMemberships';
import { BillingPermissionsGroups, IBillingPermissionsGroups } from '@/Data/Billing/BillingPermissionsGroups/BillingPermissionsGroups';
import { BillingPermissionsBool, IBillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingPaymentMethod, IBillingPaymentMethod } from '@/Data/Billing/BillingPaymentMethod/BillingPaymentMethod';
import { BillingPaymentFrequencies, IBillingPaymentFrequencies } from '@/Data/Billing/BillingPaymentFrequencies/BillingPaymentFrequencies';
import { BillingPackagesType, IBillingPackagesType } from '@/Data/Billing/BillingPackagesType/BillingPackagesType';
import { BillingPackages, IBillingPackages } from '@/Data/Billing/BillingPackages/BillingPackages';
import { BillingIndustries, IBillingIndustries } from '@/Data/Billing/BillingIndustries/BillingIndustries';
import { BillingCurrency, IBillingCurrency } from '@/Data/Billing/BillingCurrency/BillingCurrency';
import { BillingCouponCodes, IBillingCouponCodes } from '@/Data/Billing/BillingCouponCodes/BillingCouponCodes';
import { BillingSubscriptions, IBillingSubscriptions } from '@/Data/Billing/BillingSubscriptions/BillingSubscriptions';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingCompanies, IBillingCompanies } from '@/Data/Billing/BillingCompanies/BillingCompanies';
import { BillingJournalEntries, IBillingJournalEntries } from '@/Data/Billing/BillingJournalEntries/BillingJournalEntries';
import { BillingInvoices, IBillingInvoices } from '@/Data/Billing/BillingInvoices/BillingInvoices';
import { ISkill, Skill } from '@/Data/CRM/Skill/Skill';
import { ISettingsUser, SettingsUser } from '@/Data/CRM/SettingsUser/SettingsUser';
import { ISettingsProvisioning, SettingsProvisioning } from '@/Data/CRM/SettingsProvisioning/SettingsProvisioning';
import { ISettingsDefault, SettingsDefault } from '@/Data/CRM/SettingsDefault/SettingsDefault';
import { BillingSessions, IBillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import { AssignmentStatus, IAssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import { EmploymentStatus, IEmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import { EstimatingManHours, IEstimatingManHours } from '@/Data/CRM/EstimatingManHours/EstimatingManHours';
import { ILabourSubtypeHoliday, LabourSubtypeHoliday } from '@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { ILabourSubtypeException, LabourSubtypeException } from '@/Data/CRM/LabourSubtypeException/LabourSubtypeException';
import { ILabourSubtypeNonBillable, LabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import { ILabourType, LabourType } from '@/Data/CRM/LabourType/LabourType';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import { IProjectNote, ProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { IProjectStatus, ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { IProduct, Product } from '@/Data/CRM/Product/Product';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import { IOnCallAutoAttendant, OnCallAutoAttendant } from '@/Data/CRM/OnCallAutoAttendant/OnCallAutoAttendant';
import { Calendar, ICalendar } from '@/Data/CRM/Calendar/Calendar';
import { IVoicemail, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import { IRecording, Recording } from '@/Data/CRM/Recording/Recording';

export default {
	state: {
		// Billing
		billingCompanies: {},
		billingContacts: {},
		billingCouponCodes: {},
		billingCurrency: {},
		billingIndustries: {},
		billingInvoices: {},
		billingJournalEntries: {},
		billingJournalEntriesType: {},
		billingPackages: {},
		billingPackagesType: {},
		billingPaymentFrequencies: {},
		billingPaymentMethod: {},
		billingPermissionsBool: {},
		billingPermissionsGroups: {},
		billingPermissionsGroupsMemberships: {},
		billingSessions: {},
		billingSubscriptions: {},
		billingSubscriptionsProvisioningStatus: {},
		
		// Dispatch Pulse
		agents: {},
		assignments: {},
		assignmentStatus: {},
		companies: {},
		agentsEmploymentStatus: {},
		contacts: {},
		estimatingManHours: {},
		labourSubtypeHolidays: {},
		labourSubtypeException: {},
		labourSubtypeNonBillable: {},
		labour: {},
		labourTypes: {},
		materials: {},
		projectNotes: {},
		projects: {},
		projectStatus: {},
		settingsDefault: {},
		settingsProvisioning: {},
		settingsUser: {},
		products: {},
		skills: {},
		dids: {},
		calendars: {},
		onCallAutoAttendants: {},
		voicemails: {},
		recordings: {},
	},
	getters: {
		SortedDeduplicatedContactTitles(state: any) {
			
			const ret: string[] = [ 'Apprentice', 'Journeyman', 'Client', 'Owner', 'Manager', 'Sales'];
			
			// eslint-disable-next-line @typescript-eslint/no-unused-vars
			for (const [key, value] of Object.entries(state.contacts as Record<string, IContact>)) {
				
				const title = value.json.title;
				if (null == title || IsNullOrEmpty(title)) {
					continue;
				}
				
				if (ret.indexOf(title) !== -1) {
					continue;
				}
				
				ret.push(title);
			}
			
			
			return _.sortBy(ret, (o: string) => o.toLowerCase());
		},
	},
	mutations: {
		
		
		
		
		//
		// Billing
		//
		
		UpdateBillingContactsRemote(state: any, payload: Record<string, IBillingContacts>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				value.applicationData = JSON.parse(value.applicationData as unknown as string);
				Vue.set(state.billingContacts, key, BillingContacts.ValidateObject(value));
			}
		},
		DeleteBillingContactsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingContacts, key);
			}
		},



		UpdateBillingCompaniesRemote(state: any, payload: Record<string, IBillingCompanies>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingCompanies, key, BillingCompanies.ValidateObject(value));
			}
		},
		DeleteBillingCompaniesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingCompanies, key);
			}
		},
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingCouponCodesRemote(state: any, payload: Record<string, IBillingCouponCodes>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingCouponCodes, key, BillingCouponCodes.ValidateObject(value));
			}
		},
		
		// UpdateBillingCouponCodes(state: any, payload: Record<string, IBillingCouponCodes>) {
		// 	console.debug(`UpdateBillingCouponCodes`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingCouponCodes, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingCouponCodesForSession(payload);
		// },
		
		DeleteBillingCouponCodesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingCouponCodes, key);
			}
		},
		// DeleteBillingCouponCodes(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingCouponCodes, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingCurrencyRemote(state: any, payload: Record<string, IBillingCurrency>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingCurrency, key, BillingCurrency.ValidateObject(value));
			}
		},
		
		// UpdateBillingCurrency(state: any, payload: Record<string, IBillingCurrency>) {
		// 	console.debug(`UpdateBillingCurrency`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingCurrency, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingCurrencyForSession(payload);
		// },
		DeleteBillingCurrencyRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingCurrency, key);
			}
		},
		// DeleteBillingCurrency(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingCurrency, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingIndustriesRemote(state: any, payload: Record<string, IBillingIndustries>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingIndustries, key, BillingIndustries.ValidateObject(value));
			}
		},
		
		// UpdateBillingIndustries(state: any, payload: Record<string, IBillingIndustries>) {
		// 	console.debug(`UpdateBillingIndustries`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingIndustries, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingIndustriesForSession(payload);
		// },
		DeleteBillingIndustriesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingIndustries, key);
			}
		},
		// DeleteBillingIndustries(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingIndustries, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		UpdateBillingInvoicesRemote(state: any, payload: Record<string, IBillingInvoices>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingInvoices, key, BillingInvoices.ValidateObject(value));
			}
		},
		
		// UpdateBillingInvoices(state: any, payload: Record<string, IBillingInvoices>) {
		// 	console.debug(`UpdateBillingInvoices`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingInvoices, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingInvoicesForSession(payload);
		// },
		DeleteBillingInvoicesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingInvoices, key);
			}
		},
		// DeleteBillingInvoices(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingInvoices, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingJournalEntriesRemote(state: any, payload: Record<string, IBillingJournalEntries>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingJournalEntries, key, BillingJournalEntries.ValidateObject(value));
			}
		},
		
		// UpdateBillingJournalEntries(state: any, payload: Record<string, IBillingJournalEntries>) {
		// 	console.debug(`UpdateBillingJournalEntries`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingJournalEntries, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingJournalEntriesForSession(payload);
		// },
		DeleteBillingJournalEntriesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingJournalEntries, key);
			}
		},
		// DeleteBillingJournalEntries(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingJournalEntries, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingPackagesRemote(state: any, payload: Record<string, IBillingPackages>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPackages, key, BillingPackages.ValidateObject(value));
			}
		},
		
		// UpdateBillingPackages(state: any, payload: Record<string, IBillingPackages>) {
		// 	console.debug(`UpdateBillingPackages`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPackages, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPackagesForSession(payload);
		// },
		DeleteBillingPackagesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPackages, key);
			}
		},
		// DeleteBillingPackages(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPackages, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingPackagesTypeRemote(state: any, payload: Record<string, IBillingPackagesType>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPackagesType, key, BillingPackagesType.ValidateObject(value));
			}
		},
		
		// UpdateBillingPackagesType(state: any, payload: Record<string, IBillingPackagesType>) {
		// 	console.debug(`UpdateBillingPackagesType`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPackagesType, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPackagesTypeForSession(payload);
		// },
		DeleteBillingPackagesTypeRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPackagesType, key);
			}
		},
		// DeleteBillingPackagesType(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPackagesType, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingPaymentFrequenciesRemote(state: any, payload: Record<string, IBillingPaymentFrequencies>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPaymentFrequencies, key, BillingPaymentFrequencies.ValidateObject(value));
			}
		},
		
		// UpdateBillingPaymentFrequencies(state: any, payload: Record<string, IBillingPaymentFrequencies>) {
		// 	console.debug(`UpdateBillingPaymentFrequencies`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPaymentFrequencies, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPaymentFrequenciesForSession(payload);
		// },
		DeleteBillingPaymentFrequenciesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPaymentFrequencies, key);
			}
		},
		// DeleteBillingPaymentFrequencies(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPaymentFrequencies, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		UpdateBillingPaymentMethodRemote(state: any, payload: Record<string, IBillingPaymentMethod>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPaymentMethod, key, BillingPaymentMethod.ValidateObject(value));
			}
		},
		
		// UpdateBillingPaymentMethod(state: any, payload: Record<string, IBillingPaymentMethod>) {
		// 	console.debug(`UpdateBillingPaymentMethod`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPaymentMethod, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPaymentMethodForSession(payload);
		// },
		DeleteBillingPaymentMethodRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPaymentMethod, key);
			}
		},
		// DeleteBillingPaymentMethod(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPaymentMethod, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingPermissionsBoolRemote(state: any, payload: Record<string, IBillingPermissionsBool>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPermissionsBool, key, BillingPermissionsBool.ValidateObject(value));
			}
		},
		
		// UpdateBillingPermissionsBool(state: any, payload: Record<string, IBillingPermissionsBool>) {
		// 	console.debug(`UpdateBillingPermissionsBool`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPermissionsBool, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPermissionsBoolForSession(payload);
		// },
		DeleteBillingPermissionsBoolRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPermissionsBool, key);
			}
		},
		// DeleteBillingPermissionsBool(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPermissionsBool, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingPermissionsGroupsRemote(state: any, payload: Record<string, IBillingPermissionsGroups>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPermissionsGroups, key, BillingPermissionsGroups.ValidateObject(value));
			}
		},
		
		// UpdateBillingPermissionsGroups(state: any, payload: Record<string, IBillingPermissionsGroups>) {
		// 	console.debug(`UpdateBillingPermissionsGroups`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPermissionsGroups, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPermissionsGroupsForSession(payload);
		// },
		DeleteBillingPermissionsGroupsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPermissionsGroups, key);
			}
		},
		// DeleteBillingPermissionsGroups(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPermissionsGroups, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingPermissionsGroupsMembershipsRemote(
			state: any, 
			payload: Record<string, IBillingPermissionsGroupsMemberships>) {
				
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingPermissionsGroupsMemberships, key, BillingPermissionsGroupsMemberships.ValidateObject(value));
			}
		},
		
		// UpdateBillingPermissionsGroupsMemberships(
		// 	state: any, 
		// 	payload: Record<string, IBillingPermissionsGroupsMemberships>) {
		// 	console.debug(`UpdateBillingPermissionsGroupsMemberships`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingPermissionsGroupsMemberships, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingPermissionsGroupsMembershipsForSession(payload);
		// },
		DeleteBillingPermissionsGroupsMembershipsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingPermissionsGroupsMemberships, key);
			}
		},
		// DeleteBillingPermissionsGroupsMemberships(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingPermissionsGroupsMemberships, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		UpdateBillingSessionsRemote(state: any, payload: Record<string, IBillingSessions>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingSessions, key, BillingSessions.ValidateObject(value));
			}
		},
		
		// UpdateBillingSessions(state: any, payload: Record<string, IBillingSessions>) {
		// 	console.debug(`UpdateBillingSessions`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingSessions, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingSessionsForSession(payload);
		// },
		DeleteBillingSessionsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingSessions, key);
			}
		},
		// DeleteBillingSessions(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingSessions, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingSubscriptionsRemote(state: any, payload: Record<string, IBillingSubscriptions>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingSubscriptions, key, BillingSubscriptions.ValidateObject(value));
			}
		},
		
		// UpdateBillingSubscriptions(state: any, payload: Record<string, IBillingSubscriptions>) {
		// 	console.debug(`UpdateBillingSubscriptions`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingSubscriptions, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingSubscriptionsForSession(payload);
		// },
		DeleteBillingSubscriptionsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingSubscriptions, key);
			}
		},
		// DeleteBillingSubscriptions(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingSubscriptions, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateBillingSubscriptionsProvisioningStatusRemote(
			state: any, 
			payload: Record<string, IBillingSubscriptionsProvisioningStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.billingSubscriptionsProvisioningStatus, key, 
					BillingSubscriptionsProvisioningStatus.ValidateObject(value));
			}
		},
		
		// UpdateBillingSubscriptionsProvisioningStatus(
		// 	state: any, 
		// 	payload: Record<string, IBillingSubscriptionsProvisioningStatus>) {
				
		// 	console.debug(`UpdateBillingSubscriptionsProvisioningStatus`, payload);
		// 	for (const [key, value] of Object.entries(payload)) {
		// 		//value.json.lastModifiedBillingId = CurrentBillingContactId();
		// 		//value.lastModifiedISO8601 = DateTime.utc().toISO();
		// 		Vue.set(state.billingSubscriptionsProvisioningStatus, key, value);
		// 	}
			
		// 	SignalRConnection.PushBillingSubscriptionsProvisioningStatusForSession(payload);
		// },
		DeleteBillingSubscriptionsProvisioningStatusRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.billingSubscriptionsProvisioningStatus, key);
			}
		},
		// DeleteBillingSubscriptionsProvisioningStatus(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
		// 	console.error(`Don't call this function.`);
		// 	/*for (const key of payload) {
		// 		Vue.delete(state.billingSubscriptionsProvisioningStatus, key);
		// 	}*/
		// },
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		//
		// Dispatch Pulse
		//
		UpdateAgentsRemote(state: any, payload: Record<string, IAgent>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.agents, key, Agent.ValidateObject(value));
			}
		},
		UpdateAgents(state: any, payload: Record<string, IAgent>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.agents, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Agent.PushAgents.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				agents: plCopy,
			});
			
			
		},
		DeleteAgentsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.agents, key);
				
			}
		},
		DeleteAgents(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.agents, key);
				
			}
			
			Agent.DeleteAgents.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				agentsDelete: payload,
			});
			
		},
		
		
		
		
		
		UpdateAssignmentsRemote(state: any, payload: Record<string, IAssignment>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.assignments, key, Assignment.ValidateObject(value));
			}
		},
		UpdateAssignments(state: any, payload: Record<string, IAssignment>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.assignments, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Assignment.PushAssignments.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				assignments: plCopy,
			});
			
		},
		DeleteAssignmentsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.assignments, key);
			}
		},
		DeleteAssignments(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.assignments, key);
			}
			
			Assignment.DeleteAssignments.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				assignmentsDelete: payload,
			});
			
		},
		
		
		UpdateAssignmentStatusRemote(state: any, payload: Record<string, IAssignmentStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.assignmentStatus, key, AssignmentStatus.ValidateObject(value));
			}
		},
		UpdateAssignmentStatus(state: any, payload: Record<string, IAssignmentStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.assignmentStatus, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			AssignmentStatus.PushAssignmentStatus.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				assignmentStatus: plCopy,
			});
		},
		DeleteAssignmentStatusRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.assignmentStatus, key);
			}
		},
		DeleteAssignmentStatus(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.assignmentStatus, key);
			}
			
			AssignmentStatus.DeleteAssignmentStatus.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				assignmentStatusDelete: payload,
			});
			
		},
		
		
		
		
		
		
		
		UpdateCompaniesRemote(state: any, payload: Record<string, ICompany>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.companies, key, Company.ValidateObject(value));
			}
		},
		UpdateCompanies(state: any, payload: Record<string, ICompany>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.companies, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Company.PushCompanies.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				companies: plCopy,
			});
		},
		DeleteCompaniesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.companies, key);
			}
		},
		DeleteCompanies(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.companies, key);
			}
			
			Company.DeleteCompanies.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				companiesDelete: payload,
			});
		},
		
		
		
		UpdateAgentsEmploymentStatusRemote(state: any, payload: Record<string, IEmploymentStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.agentsEmploymentStatus, key, EmploymentStatus.ValidateObject(value));
			}
		},
		UpdateAgentsEmploymentStatus(state: any, payload: Record<string, IEmploymentStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.agentsEmploymentStatus, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			EmploymentStatus.PushAgentsEmploymentStatus.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				agentsEmploymentStatus: plCopy,
			});
		},
		DeleteAgentsEmploymentStatusRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.agentsEmploymentStatus, key);
			}
		},
		DeleteAgentsEmploymentStatus(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.agentsEmploymentStatus, key);
			}
			
			EmploymentStatus.DeleteAgentsEmploymentStatus.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				agentsEmploymentStatusDelete: payload,
			});
		},
		
		
		
		UpdateContactsRemote(state: any, payload: Record<string, IContact>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.contacts, key, Contact.ValidateObject(value));
			}
		},
		UpdateContacts(state: any, payload: Record<string, IContact>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.contacts, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Contact.PushContacts.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				contacts: plCopy,
			});
		},
		DeleteContactsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.contacts, key);
			}
		},
		DeleteContacts(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.contacts, key);
			}
			Contact.DeleteContacts.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				contactsDelete: payload,
			});
		},
		
		
		UpdateEstimatingManHoursRemote(state: any, payload: Record<string, IEstimatingManHours>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.estimatingManHours, key, EstimatingManHours.ValidateObject(value));
			}
		},
		UpdateEstimatingManHours(state: any, payload: Record<string, IEstimatingManHours>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.estimatingManHours, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			EstimatingManHours.PushEstimatingManHours.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				estimatingManHours: plCopy,
			});
		},
		DeleteEstimatingManHoursRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.estimatingManHours, key);
			}
		},
		DeleteEstimatingManHours(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.estimatingManHours, key);
			}
			
			EstimatingManHours.DeleteEstimatingManHours.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				estimatingManHoursDelete: payload,
			});
		},
		
		
		UpdateLabourSubtypeHolidaysRemote(state: any, payload: Record<string, ILabourSubtypeHoliday>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.labourSubtypeHolidays, key, LabourSubtypeHoliday.ValidateObject(value));
			}
		},
		UpdateLabourSubtypeHolidays(state: any, payload: Record<string, ILabourSubtypeHoliday>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.labourSubtypeHolidays, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			LabourSubtypeHoliday.PushLabourSubtypeHolidays.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourSubtypeHolidays: plCopy,
			});
		},
		DeleteLabourSubtypeHolidaysRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourSubtypeHolidays, key);
			}
		},
		DeleteLabourSubtypeHolidays(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourSubtypeHolidays, key);
			}
			
			LabourSubtypeHoliday.DeleteLabourSubtypeHolidays.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourSubtypeHolidaysDelete: payload,
			});
		},
		
		
		UpdateLabourSubtypeExceptionRemote(state: any, payload: Record<string, ILabourSubtypeException>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.labourSubtypeException, key, LabourSubtypeException.ValidateObject(value));
			}
		},
		UpdateLabourSubtypeException(state: any, payload: Record<string, ILabourSubtypeException>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.labourSubtypeException, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			LabourSubtypeException.PushLabourSubtypeException.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourSubtypeException: plCopy,
			});
		},
		DeleteLabourSubtypeExceptionRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourSubtypeException, key);
			}
		},
		DeleteLabourSubtypeException(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourSubtypeException, key);
			}
			LabourSubtypeException.DeleteLabourSubtypeException.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourSubtypeExceptionDelete: payload,
			});
		},
		
		
		UpdateLabourSubtypeNonBillableRemote(state: any, payload: Record<string, ILabourSubtypeNonBillable>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.labourSubtypeNonBillable, key, LabourSubtypeNonBillable.ValidateObject(value));
			}
		},
		UpdateLabourSubtypeNonBillable(state: any, payload: Record<string, ILabourSubtypeNonBillable>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.labourSubtypeNonBillable, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			LabourSubtypeNonBillable.PushLabourSubtypeNonBillable.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourSubtypeNonBillable: plCopy,
			});
		},
		DeleteLabourSubtypeNonBillableRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourSubtypeNonBillable, key);
			}
		},
		DeleteLabourSubtypeNonBillable(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourSubtypeNonBillable, key);
			}
			LabourSubtypeNonBillable.DeleteLabourSubtypeNonBillable.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourSubtypeNonBillableDelete: payload,
			});
		},
		
		
		
		
		UpdateLabourRemote(state: any, payload: Record<string, ILabour>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.labour, key, Labour.ValidateObject(value));
			}
		},
		UpdateLabour(state: any, payload: Record<string, ILabour>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.labour, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Labour.PushLabour.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labour: plCopy,
			});
		},
		DeleteLabourRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labour, key);
			}
		},
		DeleteLabour(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labour, key);
			}
			Labour.DeleteLabour.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourDelete: payload,
			});
		},
		
		
		UpdateLabourTypesRemote(state: any, payload: Record<string, ILabourType>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.labourTypes, key, LabourType.ValidateObject(value));
			}
		},
		UpdateLabourTypes(state: any, payload: Record<string, ILabourType>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.labourTypes, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			LabourType.PushLabourTypes.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourTypes: plCopy,
			});
		},
		DeleteLabourTypesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourTypes, key);
			}
		},
		DeleteLabourTypes(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.labourTypes, key);
			}
			LabourType.DeleteLabourTypes.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				labourTypesDelete: payload,
			});
		},
		
		
		
		UpdateMaterialsRemote(state: any, payload: Record<string, IMaterial>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.materials, key, Material.ValidateObject(value));
			}
		},
		UpdateMaterials(state: any, payload: Record<string, IMaterial>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.materials, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Material.PushMaterials.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				materials: plCopy,
			});
		},
		DeleteMaterialsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.materials, key);
			}
		},
		DeleteMaterials(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.materials, key);
			}
			
			Material.DeleteMaterials.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				materialsDelete: payload,
			});
			
		},
		
		
		
		UpdateProjectNotesRemote(state: any, payload: Record<string, IProjectNote>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.projectNotes, key, ProjectNote.ValidateObject(value));
			}
		},
		UpdateProjectNotes(state: any, payload: Record<string, IProjectNote>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.projectNotes, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			ProjectNote.PushProjectNotes.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				projectNotes: plCopy,
			});
		},
		DeleteProjectNotesRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.projectNotes, key);
			}
		},
		DeleteProjectNotes(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.projectNotes, key);
			}
			
			ProjectNote.DeleteProjectNotes.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				projectNotesDelete: payload,
			});
		},
		
		
		
		UpdateProjectsRemote(state: any, payload: Record<string, IProject>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.projects, key, Project.ValidateObject(value));
			}
		},
		UpdateProjects(state: any, payload: Record<string, IProject>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.projects, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Project.PushProjects.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				projects: plCopy,
			});
		},
		DeleteProjectsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.projects, key);
			}
		},
		DeleteProjects(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.projects, key);
			}
			Project.DeleteProjects.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				projectsDelete: payload,
			});
		},
		
		
		
		UpdateProjectStatusRemote(state: any, payload: Record<string, IProjectStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.projectStatus, key, ProjectStatus.ValidateObject(value));
			}
		},
		UpdateProjectStatus(state: any, payload: Record<string, IProjectStatus>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.projectStatus, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			ProjectStatus.PushProjectStatus.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				projectStatus: plCopy,
			});
		},
		DeleteProjectStatusRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.projectStatus, key);
			}
		},
		DeleteProjectStatus(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.projectStatus, key);
			}
			
			ProjectStatus.DeleteProjectStatus.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				projectStatusDelete: payload,
			});
		},
		
		
		UpdateSettingsDefaultRemote(state: any, payload: Record<string, ISettingsDefault>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.settingsDefault, key, SettingsDefault.ValidateObject(value));
			}
		},
		UpdateSettingsDefault(state: any, payload: Record<string, ISettingsDefault>) {// eslint-disable-line @typescript-eslint/no-unused-vars
			console.error(`Don't call this function.`);
			/*for (const [key, value] of Object.entries(payload)) {
				//value.json.lastModifiedBillingId = CurrentBillingContactId();
				//value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.settingsDefault, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			SignalRConnection.PushUpdate({
				settingsDefault: plCopy,
			}, 'settingsDefault');*/
		},
		DeleteSettingsDefaultRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.settingsDefault, key);
			}
		},
		DeleteSettingsDefault(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
			console.error(`Don't call this function.`);
			/*for (const key of payload) {
				Vue.delete(state.settingsDefault, key);
			}
			SignalRConnection.PushUpdate({
				settingsDefaultDelete: payload,
			}, 'settingsDefault');*/
		},
		
		
		UpdateSettingsProvisioningRemote(state: any, payload: Record<string, ISettingsProvisioning>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.settingsProvisioning, key, SettingsProvisioning.ValidateObject(value));
			}
		},
		UpdateSettingsProvisioning(state: any, payload: Record<string, ISettingsProvisioning>) {// eslint-disable-line @typescript-eslint/no-unused-vars
			console.error(`Don't call this function.`);
			/*for (const [key, value] of Object.entries(payload)) {
				//value.json.lastModifiedBillingId = CurrentBillingContactId();
				//value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.settingsProvisioning, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			SignalRConnection.PushUpdate({
				settingsProvisioning: plCopy,
			}, 'settingsProvisioning');*/
		},
		DeleteSettingsProvisioningRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.settingsProvisioning, key);
			}
		},
		DeleteSettingsProvisioning(state: any, payload: string[]) {// eslint-disable-line @typescript-eslint/no-unused-vars
			console.error(`Don't call this function.`);
			/*for (const key of payload) {
				Vue.delete(state.settingsProvisioning, key);
			}
			SignalRConnection.PushUpdate({
				settingsProvisioningDelete: payload,
			}, 'settingsProvisioning');*/
		},
		
		
		UpdateSettingsUserRemote(state: any, payload: Record<string, ISettingsUser>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.settingsUser, key, SettingsUser.ValidateObject(value));
			}
		},
		UpdateSettingsUser(state: any, payload: Record<string, ISettingsUser>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.settingsUser, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			SettingsUser.PushSettingsUser.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				settingsUser: plCopy,
			});
		},
		DeleteSettingsUserRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.settingsUser, key);
			}
		},
		DeleteSettingsUser(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.settingsUser, key);
			}
			
			SettingsUser.DeleteSettingsUser.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				settingsUserDelete: payload,
			});
		},
		
		
		
		
		
		
		
		
		
		
		
		
		UpdateProductsRemote(state: any, payload: Record<string, IProduct>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.products, key, Product.ValidateObject(value));
			}
		},
		UpdateProducts(state: any, payload: Record<string, IProduct>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.products, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Product.PushProducts.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				products: plCopy,
			});
		},
		DeleteProductsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.products, key);
			}
		},
		DeleteProducts(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.products, key);
			}
			Product.DeleteProducts.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				productsDelete: payload,
			});
		},
		
		
		
		
		
		
		
		
		
		
		UpdateSkillsRemote(state: any, payload: Record<string, ISkill>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.skills, key, Skill.ValidateObject(value));
			}
		},
		UpdateSkills(state: any, payload: Record<string, ISkill>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.skills, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Skill.PushSkills.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				skills: plCopy,
			});
		},
		DeleteSkillsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.skills, key);
			}
		},
		DeleteSkills(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.skills, key);
			}
			Skill.DeleteSkills.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				skillsDelete: payload,
			});
		},
		
		
		
		
		
		
		
		UpdateDIDsRemote(state: any, payload: Record<string, IDID>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.dids, key, DID.ValidateObject(value));
			}
		},
		UpdateDIDs(state: any, payload: Record<string, IDID>) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.dids, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			DID.PushDIDs.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				dids: plCopy,
			});
		},
		DeleteDIDsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.dids, key);
			}
		},
		DeleteDIDs(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.dids, key);
			}
			DID.DeleteDIDs.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				didsDelete: payload,
			});
		},
		
		
		
		UpdateCalendarsRemote(state: any, payload: { [id: string]: ICalendar; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.calendars, key, Calendar.ValidateObject(value));
			}
		},
		UpdateCalendars(state: any, payload: { [id: string]: ICalendar; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.calendars, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Calendar.PushCalendars.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				calendars: plCopy,
			});
		},
		DeleteCalendarsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.calendars, key);
			}
		},
		DeleteCalendars(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.calendars, key);
			}
			Calendar.DeleteCalendars.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				calendarsDelete: payload,
			});
		},
		
		
		
		
		
		UpdateOnCallAutoAttendantsRemote(state: any, payload: { [id: string]: IOnCallAutoAttendant; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.onCallAutoAttendants, key, OnCallAutoAttendant.ValidateObject(value));
			}
		},
		UpdateOnCallAutoAttendants(state: any, payload: { [id: string]: IOnCallAutoAttendant; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.onCallAutoAttendants, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			OnCallAutoAttendant.PushOnCallAutoAttendants.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				onCallAutoAttendants: plCopy,
			});
		},
		DeleteOnCallAutoAttendantsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.onCallAutoAttendants, key);
			}
		},
		DeleteOnCallAutoAttendants(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.onCallAutoAttendants, key);
			}
			OnCallAutoAttendant.DeleteOnCallAutoAttendants.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				onCallAutoAttendantsDelete: payload,
			});
		},
		
		
		
		
		
		UpdateVoicemailsRemote(state: any, payload: { [id: string]: IVoicemail; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.voicemails, key, Voicemail.ValidateObject(value));
			}
		},
		UpdateVoicemails(state: any, payload: { [id: string]: IVoicemail; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.voicemails, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Voicemail.PushVoicemails.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				voicemails: plCopy,
			});
		},
		DeleteVoicemailsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.voicemails, key);
			}
		},
		DeleteVoicemails(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.voicemails, key);
			}
			Voicemail.DeleteVoicemails.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				voicemailsDelete: payload,
			});
		},



		UpdateRecordingsRemote(state: any, payload: { [id: string]: IRecording; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.json = JSON.parse(value.json as unknown as string);
				Vue.set(state.recordings, key, Recording.ValidateObject(value));
			}
		},
		UpdateRecordings(state: any, payload: { [id: string]: IRecording; }) {
			for (const [key, value] of Object.entries(payload)) {
				value.lastModifiedISO8601 = DateTime.utc().toISO();
				Vue.set(state.recordings, key, value);
			}
			
			// we'll need to go through and replace the json object with a stringified version
			const plCopy = _.cloneDeep(payload);
			for (const [key, value] of Object.entries(plCopy)) { // eslint-disable-line @typescript-eslint/no-unused-vars
				(value as any).json = JSON.stringify(value.json);
			}
			
			Recording.PushRecordings.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				recordings: plCopy,
			});
		},
		DeleteRecordingsRemote(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.recordings, key);
			}
		},
		DeleteRecordings(state: any, payload: string[]) {
			for (const key of payload) {
				Vue.delete(state.recordings, key);
			}
			
			Recording.DeleteRecordings.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				recordingsDelete: payload,
			});
			
		},












		
	},
	actions: {
		
	},
};


