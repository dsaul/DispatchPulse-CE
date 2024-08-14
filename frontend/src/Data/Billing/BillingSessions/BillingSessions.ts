import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import store from "@/plugins/store/store";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCPushBillingSessionAgentId } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionAgentId";
import { RPCPushBillingSessionContactId } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionContactId";
import { RPCPushBillingSessionEMail } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionEMail";
import { RPCPushBillingSessionEMailListMarketing } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionEMailListMarketing";
import { RPCPushBillingSessionEMailListTutorials } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionEMailListTutorials";
import { RPCPushBillingSessionMarketingCampaign } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionMarketingCampaign";
import { RPCPushBillingSessionName } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionName";
import { RPCPushBillingSessionPhone } from "@/Data/Billing/BillingSessions/RPCPushBillingSessionPhone";
import { RPCRequestBillingSessionsForCurrentSession } from "@/Data/Billing/BillingSessions/RPCRequestBillingSessionsForCurrentSession";
import { RPCPerformCreateBillingSessionForCredentials } from "@/Data/Billing/BillingSessions/RPCPerformCreateBillingSessionForCredentials";
import { RPCPerformLogOutSession } from "@/Data/Billing/BillingSessions/RPCPerformLogOutSession";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";

export interface IBillingSessions {
	uuid: string;
	contactId: string;
	agentDescription: string;
	ipAddress: string | null;
	createdUtc: string;
	lastAccessUtc: string;
	json: Record<string, any>;
}

export class BillingSessions {
	// RPC Methods
	public static PushBillingSessionAgentId = RPCMethod.Register<
		RPCPushBillingSessionAgentId
	>(new RPCPushBillingSessionAgentId());
	public static PushBillingSessionContactId = RPCMethod.Register<
		RPCPushBillingSessionContactId
	>(new RPCPushBillingSessionContactId());
	public static PushBillingSessionEMail = RPCMethod.Register<
		RPCPushBillingSessionEMail
	>(new RPCPushBillingSessionEMail());
	public static PushBillingSessionEMailListMarketing = RPCMethod.Register<
		RPCPushBillingSessionEMailListMarketing
	>(new RPCPushBillingSessionEMailListMarketing());
	public static PushBillingSessionEMailListTutorials = RPCMethod.Register<
		RPCPushBillingSessionEMailListTutorials
	>(new RPCPushBillingSessionEMailListTutorials());
	public static PushBillingSessionMarketingCampaign = RPCMethod.Register<
		RPCPushBillingSessionMarketingCampaign
	>(new RPCPushBillingSessionMarketingCampaign());
	public static PushBillingSessionName = RPCMethod.Register<
		RPCPushBillingSessionName
	>(new RPCPushBillingSessionName());
	public static PushBillingSessionPhone = RPCMethod.Register<
		RPCPushBillingSessionPhone
	>(new RPCPushBillingSessionPhone());
	public static RequestBillingSessionsForCurrentSession = RPCMethod.Register<
		RPCRequestBillingSessionsForCurrentSession
	>(new RPCRequestBillingSessionsForCurrentSession());
	public static PerformCreateBillingSessionForCredentials = RPCMethod.Register<
		RPCPerformCreateBillingSessionForCredentials
	>(new RPCPerformCreateBillingSessionForCredentials());
	public static PerformLogOutSession = RPCMethod.Register<
		RPCPerformLogOutSession
	>(new RPCPerformLogOutSession());

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingSessions {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingSessions {
		const id = GenerateID();
		const ret: IBillingSessions = {
			uuid: id,
			contactId: "",
			agentDescription: "",
			ipAddress: null,
			createdUtc: "",
			lastAccessUtc: "",
			json: {}
		};

		return ret;
	}

	public static CurrentSessionId(): string | null {
		return store.state.Sessions.sessionId;
	}

	public static ValidateObject(o: IBillingSessions): IBillingSessions {
		return o;
	}

	public static PermBillingSessionsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.sessions.read-any") !== -1 ||
			perms.indexOf("billing.sessions.read-company") !== -1 ||
			perms.indexOf("billing.sessions.read-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

(window as any).DEBUG_BillingSessions = BillingSessions;

export default {};
