import { BillingContacts } from "@/Data/Billing/BillingContacts/BillingContacts";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";

export default class CRMNavigation {
	public static PermCRMNavigationShowAddressBook(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.address-book") !== -1;

		// console.debug('PermCRMNavigationShowAddressBook', ret);

		return ret;
	}

	public static PermCRMNavigationShowAllContacts(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.all-contacts") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllCompanies(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.all-companies") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowProjects(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.projects") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllProjects(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.all-projects") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllAssignments(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.all-assignments") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllMaterialEntries(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.all-material-entries") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowProjectDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.project-definitions") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowProductsDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.product-definitions") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAssignmentStatusDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf(
				"crm.navigation.show.assignment-status-definitions"
			) !== -1;
		return ret;
	}

	public static PermCRMNavigationShowManHoursDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.man-hours-definitions") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowProjectStatusDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.project-status-definitions") !==
			-1;
		return ret;
	}

	public static PermCRMNavigationShowAgents(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.agents") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllAgents(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.all-agents") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllLabourEntries(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.all-labour-entries") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAgentsDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.agents-definitions") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowEmploymentStatusDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf(
				"crm.navigation.show.employment-status-definitions"
			) !== -1;
		return ret;
	}
	public static PermCRMNavigationShowLabourExceptionDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf(
				"crm.navigation.show.labour-exception-definitions"
			) !== -1;
		return ret;
	}
	public static PermCRMNavigationShowLabourHolidaysDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.labour-holidays-definitions") !==
			-1;
		return ret;
	}
	public static PermCRMNavigationShowLabourNonBillableDefinitions(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf(
				"crm.navigation.show.labour-non-billable-definitions"
			) !== -1;
		return ret;
	}

	public static PermCRMNavigationShowReports(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.reports") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowAllReports(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.all-reports") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowSettings(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.settings") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServices(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.navigation.show.phone-services") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServicesPhoneNumbers(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf(
				"crm.navigation.show.phone-services-phone-numbers"
			) !== -1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServicesRecordings(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.phone-services-recordings") !==
			-1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServicesSetup(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.phone-services-setup") !== -1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServicesOnCallResponders(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf(
				"crm.navigation.show.phone-services-on-call-responders"
			) !== -1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServicesCalendars(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.phone-services-calendars") !==
			-1;
		return ret;
	}

	public static PermCRMNavigationShowPhoneServicesVoicemails(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.navigation.show.phone-services-voicemails") !==
			-1;
		return ret;
	}

	public static PermCRMNavigationLicensedProjectsSchedulingTime(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const contact = BillingContacts.ForCurrentSession();
		if (
			!contact ||
			!contact.json ||
			!contact.json.licenseAssignedProjectsSchedulingTime
		) {
			return false;
		}

		return contact.json.licenseAssignedProjectsSchedulingTime;
	}

	public static PermCRMNavigationLicensedOnCall(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const contact = BillingContacts.ForCurrentSession();
		if (!contact || !contact.json || !contact.json.licenseAssignedOnCall) {
			return false;
		}

		return contact.json.licenseAssignedOnCall;
	}
}
