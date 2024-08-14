import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
export default class CRMBackups {
	public static PermCRMBackupsRunLocal(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.backups.run-local") !== -1;
		return ret;
	}

	public static PermCRMBackupsRunServer(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.backups.run-server") !== -1;
		return ret;
	}
}
