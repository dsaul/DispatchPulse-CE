import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { Project } from '@/Data/CRM/Project/Project';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
export default class CRMViews {
	
	public static PermCRMViewDashboardDispatchTab(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf('crm.view.dashboard.dispatch-tab') !== -1;
		return ret;
	}
	
	public static PermCRMViewDashboardBillingTab(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf('crm.view.dashboard.billing-tab') !== -1;
		return ret;
	}
	
	public static PermCRMViewDashboardManagementTab(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf('crm.view.dashboard.management-tab') !== -1;
		return ret;
	}
	
	public static PermCRMViewProjectIndexMergeProjects(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const canMerge = perms.indexOf('crm.view.billing-index.merge-projects') !== -1;
		const canPush = Project.PermProjectsCanPush();
		const canRequest = Project.PermProjectsCanRequest();
		const canDelete = Project.PermProjectsCanDelete();
		
		// console.log('PermCRMViewProjectIndexMergeProjects', 
		// 	{canMerge, canPush, canRequest, canDelete});
		
		return canMerge && canPush && canRequest && canDelete;
	}
	
	
	
	
}






