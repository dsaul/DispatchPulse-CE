export default (key: string): string => {
	switch (key) {
		case "billing.contacts.read-company":
			return "See company contacts.";
		case "billing.permissions-bool.read-self":
			return "See their own permissions #2.";
		case "billing.sessions.read-self":
			return "See their own sessions.";
		case "billing.permissions-groups-memberships.read-self":
			return "See their own group membership.";
		case "billing.subscription-provisioning-status.request-company":
			return "See how the app is configured.";
		case "billing.contacts.add-company":
			return "Add company accounts.";
		case "billing.contacts.modify-company":
			return "Modify company accounts.";
		case "billing.contacts.delete-company":
			return "Delete company accounts.";
		case "billing.coupon-codes.read-company":
			return "See DP coupon codes (company).";
		case "billing.industries.read-company":
			return "See DP industries (company).";
		case "billing.invoices.read-company":
			return "See DP invoices (company).";
		case "billing.journal-entries.read-company":
			return "See DP journal entries (company).";
		case "billing.packages.read-company":
			return "See DP packages (company).";
		case "billing.packages-type.read-company":
			return "See DP packages type (company).";
		case "billing.payment-frequencies.read-company":
			return "See DP payment frequencies (company).";
		case "billing.payment-method.read-company":
			return "See DP payment method (company).";
		case "billing.permissions-bool.read-company":
			return "See company account permissions.";
		case "billing.permissions-bool.modify-company":
			return "Modify company account permissions.";
		case "billing.permissions-bool.delete-company":
			return "Delete company account permissions.";
		case "billing.sessions.read-company":
			return "See company account sessions.";
		case "billing.sessions.add-company":
			return "Add company account sessions.";
		case "billing.sessions.modify-company":
			return "Modify company account sessions.";
		case "billing.subscription.read-company":
			return "See DP subscriptions (company).";
		case "billing.permissions-groups.read-company":
			return "See company permissions groups.";
		case "billing.permissions-groups-memberships.read-company":
			return "See company permissions group memberships.";
		case "crm.agents.request-company":
			return "See agents (company).";
		case "crm.assignments.request-company":
			return "See assignments (company).";
		case "crm.assignments-status.request-company":
			return "See assignment's status (company).";
		case "crm.companies.request-company":
			return "See companies (company).";
		case "crm.contacts.request-company":
			return "See contacts (company).";
		case "crm.labour.request-company":
			return "See labour (company).";
		case "crm.labour-subtype-exception.request-company":
			return "See labour subtype exception (company).";
		case "crm.labour-subtype-holidays.request-company":
			return "See labour subtype holidays (company).";
		case "crm.labour-subtype-non-billable.request-company":
			return "See labour subtype non-billable (company).";
		case "crm.labour-types.request-company":
			return "See labour types (company).";
		case "crm.products.request-company":
			return "See products (company).";
		case "crm.projects.request-company":
			return "See projects (company).";
		case "crm.project-status.request-company":
			return "See project's status.";
		case "crm.settings-provisioning.request-company":
			return "See provisioning (company).";
		case "crm.settings-user.request-company":
			return "See user settngs (company).";
		case "crm.skills.request-company":
			return "See skills (company).";
		case "crm.project-notes.request-company":
			return "See project notes (company).";
		case "crm.agents-employment-status.delete-company":
			return "Delete agent's employment status (company).";
		case "crm.agents-employment-status.push-company":
			return "Modify agent's employment status (company).";
		case "crm.agents-employment-status.request-company":
			return "See agent's employment status (company).";
		case "crm.assignments.delete-company":
			return "Delete assignments (company).";
		case "crm.assignments.push-company":
			return "Modify assignments (company).";
		case "crm.assignments-status.delete-company":
			return "Delete assignment's status (company).";
		case "crm.assignments-status.push-company":
			return "Modify assignment's status (company).";
		case "crm.companies.delete-company":
			return "Delete companies (company).";
		case "crm.companies.push-company":
			return "Modify companies (company).";
		case "crm.contacts.delete-company":
			return "Delete company (company).";
		case "crm.contacts.push-company":
			return "Modify company (company).";
		case "crm.estimating-man-hours.delete-company":
			return "Delete man hours (company).";
		case "crm.estimating-man-hours.request-company":
			return "See man hours (company).";
		case "crm.labour.delete-company":
			return "Delete labour (company).";
		case "crm.labour.push-company":
			return "Modify labour (company).";
		case "crm.labour-subtype-exception.push-company":
			return "Modify labour subtype exception (company).";
		case "crm.labour-subtype-holidays.delete-company":
			return "Delete labour subtype holidays (company).";
		case "crm.labour-subtype-holidays.push-company":
			return "Modify labour subtype holidays (company).";
		case "crm.labour-subtype-non-billable.delete-company":
			return "Delete labour subtype non billable (company).";
		case "crm.labour-subtype-non-billable.push-company":
			return "Modify labour subtype non billable (company).";
		case "crm.labour-types.delete-company":
			return "Delete labour types (company).";
		case "crm.labour-types.push-company":
			return "Modify labour types (company).";
		case "crm.materials.delete-company":
			return "Delete materials (company).";
		case "crm.materials.push-company":
			return "Modify materials (company).";
		case "crm.materials.request-company":
			return "See materials (company).";
		case "crm.products.delete-company":
			return "Delete products (company).";
		case "crm.products.push-company":
			return "Modify products (company).";
		case "crm.project-notes.delete-company":
			return "Delete product notes (company).";
		case "crm.project-notes.push-company":
			return "Modify product notes (company).";
		case "crm.projects.push-company":
			return "Modify company (company).";
		case "crm.project-status.delete-company":
			return "Delete company (company).";
		case "crm.project-status.push-company":
			return "Modify project status (company).";
		case "crm.settings-default.delete-company":
			return "Delete settings default (company).";
		case "crm.settings-default.push-company":
			return "Modify settings default (company).";
		case "crm.settings-default.request-company":
			return "See settings default (company).";
		case "crm.settings-provisioning.push-company":
			return "Modify settings provisioning (company).";
		case "crm.settings-user.delete-company":
			return "Modify user settings (company).";
		case "crm.skills.push-company":
			return "Modify skills (company)";
		case "crm.agents.delete-company":
			return "Delete agents (company).";
		case "crm.agents.push-company":
			return "Modify agents (company).";
		case "crm.settings-user.push-company":
			return "Modify user settings (company).";
		case "crm.estimating-man-hours.push-company":
			return "Modify man hours (company).";
		case "crm.labour-subtype-exception.delete-company":
			return "Delete labour subtype exception (company).";
		case "billing.currency.read-company":
			return "See DP Currency (company)";
		case "billing.permissions-bool.add-company":
			return "Add DP permissions (company) #2";
		case "billing.sessions.delete-company":
			return "Delete DP Sessions (company).";
		case "billing.permissions-groups-memberships.modify-company":
			return "Modify DP permissions group memberships (company).";
		case "billing.permissions-groups-memberships.delete-company":
			return "Delete DP permissions group memberships (company).";
		case "billing.journal-entries-type.read-company":
			return "See DP journal entries type (company).";
		case "crm.projects.delete-company":
			return "Delete projects (company).";
		case "crm.settings-provisioning.delete-company":
			return "Delete settings provisioning (company).";
		case "crm.skills.delete-company":
			return "Delete skills (company).";
		case "crm.backups.run-local":
			return "Run local backups.";
		case "crm.backups.run-server":
			return "Run server backups.";
		default:
			return key;
	}
};
