import GenerateID from "@/Utility/GenerateID";
import _ from "lodash";
import { guid } from "@/Utility/GlobalTypes";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { BillingContacts } from "../BillingContacts/BillingContacts";
import { RPCRequestBillingCompanyForCurrentSession } from "@/Data/Billing/BillingCompanies/RPCRequestBillingCompanyForCurrentSession";
import { RPCRequestCompanyNameInUse } from "@/Data/Billing/BillingCompanies/RPCRequestCompanyNameInUse";
import { RPCPerformCheckCompanyPhoneIdInUse } from "@/Data/Billing/BillingCompanies/RPCPerformCheckCompanyPhoneIdInUse";
import { RPCPerformRegisterCreateDPDatabase } from "@/Data/Billing/BillingCompanies/RPCPerformRegisterCreateDPDatabase";
import { RPCPerformRegisterNewCompany } from "@/Data/Billing/BillingCompanies/RPCPerformRegisterNewCompany";
import { RPCPerformRegisterSaveServiceAgreement } from "@/Data/Billing/BillingCompanies/RPCPerformRegisterSaveServiceAgreement";
import { RPCPerformUpdatePhoneId } from "@/Data/Billing/BillingCompanies/RPCPerformUpdatePhoneId";
import { RPCMethod } from "@/RPC/RPCMethod";

export interface IBillingCompanies {
	uuid: string;
	fullName: string | null;
	abbreviation: string | null;
	industry: string | null;
	marketingCampaign: string | null;
	addressCity: string | null;
	addressCountry: string | null;
	addressLine1: string | null;
	addressLine2: string | null;
	addressPostalCode: string | null;
	addressProvince: string | null;
	stripeCustomerId: string | null;
	paymentMethod: "Invoice" | "Square Pre-Authorized" | null;
	invoiceContactId: string | null;
	paymentFrequency: string | null;
	json: {
		phoneId: string | null;
	};
}

export class BillingCompanies {
	// RPC Methods
	public static RequestBillingCompanyForCurrentSession = RPCMethod.Register<
		RPCRequestBillingCompanyForCurrentSession
	>(new RPCRequestBillingCompanyForCurrentSession());
	public static RequestCompanyNameInUse = RPCMethod.Register<
		RPCRequestCompanyNameInUse
	>(new RPCRequestCompanyNameInUse());
	public static PerformCheckCompanyPhoneIdInUse = RPCMethod.Register<
		RPCPerformCheckCompanyPhoneIdInUse
	>(new RPCPerformCheckCompanyPhoneIdInUse());
	public static PerformRegisterCreateDPDatabase = RPCMethod.Register<
		RPCPerformRegisterCreateDPDatabase
	>(new RPCPerformRegisterCreateDPDatabase());
	public static PerformRegisterNewCompany = RPCMethod.Register<
		RPCPerformRegisterNewCompany
	>(new RPCPerformRegisterNewCompany());
	public static PerformRegisterSaveServiceAgreement = RPCMethod.Register<
		RPCPerformRegisterSaveServiceAgreement
	>(new RPCPerformRegisterSaveServiceAgreement());
	public static PerformUpdatePhoneId = RPCMethod.Register<
		RPCPerformUpdatePhoneId
	>(new RPCPerformUpdatePhoneId());

	// public static _RefreshTracker: { [id: string]: ITracker } = {};

	// public static FetchForId(id: guid): IRoundTripRequest {

	// 	const ret: IRoundTripRequest = {
	// 		roundTripRequestId: GenerateID(),
	// 		outboundRequestPromise: null,
	// 		completeRequestPromise: null,
	// 		_completeRequestPromiseResolve: null,
	// 		_completeRequestPromiseReject: null,
	// 	};

	// 	// If we have no id, reject.
	// 	if (!id || IsNullOrEmpty(id)) {
	// 		ret.outboundRequestPromise = Promise.reject();
	// 		ret.completeRequestPromise = Promise.reject();
	// 		return ret;
	// 	}

	// 	// Remove all trackers that are complete and older than 5 seconds.
	// 	const keys = Object.keys(this._RefreshTracker);
	// 	for (const key of keys) {
	// 		const tracker: ITracker = this._RefreshTracker[key];
	// 		if (!tracker.isComplete) {
	// 			continue;
	// 		}

	// 		if (DateTime.utc() > tracker.lastRequestTimeUtc.plus({seconds: 5})) {
	// 			delete this._RefreshTracker[key];
	// 		}
	// 	}

	// 	// Check and see if we already have a request pending.
	// 	const existing = this._RefreshTracker[id];
	// 	if (existing) {
	// 		return existing.rtr;
	// 	}

	// 	const billingCompanies = BillingCompanies.ForId(id);
	// 	if (billingCompanies) {
	// 		ret.outboundRequestPromise = Promise.resolve();
	// 		ret.completeRequestPromise = Promise.resolve(billingCompanies);
	// 		return ret;
	// 	}

	// 	// We'll need to request this.
	// 	const rtrNew = SignalRConnection.RequestBillingCompanyForCurrentSession({
	// 		limitToIds: [id],
	// 	});

	// 	ret.outboundRequestPromise = rtrNew.outboundRequestPromise;

	// 	ret.completeRequestPromise = new Promise((resolve, reject) => {
	// 		ret._completeRequestPromiseResolve = resolve;
	// 		ret._completeRequestPromiseReject = reject;
	// 	});

	// 	// Handlers once we get a response.
	// 	if (rtrNew.completeRequestPromise) {

	// 		rtrNew.completeRequestPromise.then(() => {
	// 			if (ret._completeRequestPromiseResolve) {
	// 				ret._completeRequestPromiseResolve(Agent.ForId(id));
	// 			}
	// 		});

	// 		rtrNew.completeRequestPromise.catch((e: Error) => {
	// 			if (ret._completeRequestPromiseReject) {
	// 				ret._completeRequestPromiseReject(e);
	// 			}
	// 		});

	// 		rtrNew.completeRequestPromise.finally(() => {
	// 			this._RefreshTracker[id].isComplete = true;
	// 		});
	// 	}

	// 	this._RefreshTracker[id] = {
	// 		lastRequestTimeUtc: DateTime.utc(),
	// 		isComplete: false,
	// 		rtr: rtrNew,
	// 	};

	// 	return ret;
	// }

	public static GetMerged(
		mergeValues: Record<string, any>
	): IBillingCompanies {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IBillingCompanies {
		const id = GenerateID();
		const ret: IBillingCompanies = {
			uuid: id,
			fullName: null,
			abbreviation: null,
			industry: null,
			marketingCampaign: null,
			addressCity: null,
			addressCountry: null,
			addressLine1: null,
			addressLine2: null,
			addressPostalCode: null,
			addressProvince: null,
			stripeCustomerId: null,
			paymentMethod: null,
			invoiceContactId: null,
			paymentFrequency: null,
			json: {
				phoneId: null
			}
		};

		return ret;
	}

	public static All(): Record<string, IBillingCompanies> | null {
		const billingCompanies = store.state.Database
			.billingCompanies as Record<string, IBillingCompanies>;
		if (!billingCompanies || Object.keys(billingCompanies).length === 0) {
			return null;
		}
		return billingCompanies;
	}

	public static ForId(id: guid | null): IBillingCompanies | null {
		if (!id) {
			return null;
		}

		const billingCompanies = this.All();
		if (!billingCompanies || Object.keys(billingCompanies).length === 0) {
			return null;
		}

		let billingCompany = billingCompanies[id];
		if (!billingCompany) {
			billingCompany = CaseInsensitivePropertyGet(billingCompanies, id);
		}
		if (!billingCompany) {
			return null;
		}

		return billingCompany;
	}

	public static CompanyForCurrentBillingContact(): IBillingCompanies | null {
		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			console.error("!contact");
			return null;
		}

		if (!contact.companyId || IsNullOrEmpty(contact.companyId)) {
			console.error(
				"!contact.companyId || IsNullOrEmpty(contact.companyId)"
			);
			return null;
		}

		const company = BillingCompanies.ForId(contact.companyId);
		if (!company) {
			console.error("!company");
			return null;
		}

		if (!company.json) {
			console.error("!company.json");
			return null;
		}

		return company;
	}

	public static CompanyPhoneIdForCurrentBillingContact(): string | "" {
		const company = BillingCompanies.CompanyForCurrentBillingContact();
		if (!company) {
			return "";
		}

		return company.json.phoneId || "";
	}

	public static ValidateObject(o: IBillingCompanies): IBillingCompanies {
		return o;
	}

	public static PermBillingCompaniesCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.companies.read-any") !== -1 ||
			perms.indexOf("billing.companies.read-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermBillingCompaniesCanModifyPhoneId(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.companies.modify-company-phone-id") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermBillingCompaniesCanModify(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("billing.companies.modify-any") !== -1 ||
			perms.indexOf("billing.companies.modify-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

(window as any).DEBUG_BillingCompanies = BillingCompanies;

export default {};
