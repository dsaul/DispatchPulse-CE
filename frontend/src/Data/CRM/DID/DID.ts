import _ from "lodash";
import store from "@/plugins/store/store";
import { guid } from "@/Utility/GlobalTypes";
import { RPCMethod } from "@/RPC/RPCMethod";
import GenerateID from "@/Utility/GenerateID";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import ITracker from "@/Utility/ITracker";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import { DateTime } from "luxon";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestDIDs } from "@/Data/CRM/DID/RPCRequestDIDs";
import { RPCDeleteDIDs } from "@/Data/CRM/DID/RPCDeleteDIDs";
import { RPCPushDIDs } from "@/Data/CRM/DID/RPCPushDIDs";
import { RPCPerformCheckDIDPBXRegistered } from "@/Data/CRM/DID/RPCPerformCheckDIDPBXRegistered";
import { RPCPerformPBXDeRegisterDID } from "@/Data/CRM/DID/RPCPerformPBXDeRegisterDID";
import { RPCPerformPBXRegisterDID } from "@/Data/CRM/DID/RPCPerformPBXRegisterDID";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IDID extends ICRMTable {
	json: {
		DIDNumber: string | null;
		assignToType: "Hangup" | "OnCallAutoAttendant" | null;
		assignToID: guid | null;
		lastModifiedBillingId: string | null;
	};
}

export class DID {
	// RPC Methods
	public static RequestDIDs = RPCMethod.Register<RPCRequestDIDs>(
		new RPCRequestDIDs()
	);
	public static DeleteDIDs = RPCMethod.Register<RPCDeleteDIDs>(
		new RPCDeleteDIDs()
	);
	public static PushDIDs = RPCMethod.Register<RPCPushDIDs>(new RPCPushDIDs());
	public static PerformCheckDIDPBXRegistered = RPCMethod.Register<
		RPCPerformCheckDIDPBXRegistered
	>(new RPCPerformCheckDIDPBXRegistered());
	public static PerformPBXDeRegisterDID = RPCMethod.Register<
		RPCPerformPBXDeRegisterDID
	>(new RPCPerformPBXDeRegisterDID());
	public static PerformPBXRegisterDID = RPCMethod.Register<
		RPCPerformPBXRegisterDID
	>(new RPCPerformPBXRegisterDID());

	public static _RefreshTracker: { [id: string]: ITracker } = {};

	public static FetchForId(id: guid): IRoundTripRequest {
		const ret: IRoundTripRequest = {
			roundTripRequestId: GenerateID(),
			outboundRequestPromise: null,
			completeRequestPromise: null,
			_completeRequestPromiseResolve: null,
			_completeRequestPromiseReject: null
		};

		// If we have no id, reject.
		if (!id || IsNullOrEmpty(id)) {
			ret.outboundRequestPromise = Promise.reject();
			ret.completeRequestPromise = Promise.reject();
			return ret;
		}

		// Remove all trackers that are complete and older than 5 seconds.
		const keys = Object.keys(this._RefreshTracker);
		for (const key of keys) {
			const tracker: ITracker = this._RefreshTracker[key];
			if (!tracker.isComplete) {
				continue;
			}

			if (
				DateTime.utc() > tracker.lastRequestTimeUtc.plus({ seconds: 5 })
			) {
				delete this._RefreshTracker[key];
			}
		}

		// Check and see if we already have a request pending.
		const existing = this._RefreshTracker[id];
		if (existing) {
			return existing.rtr;
		}

		const did = DID.ForId(id);
		if (did) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(did);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = DID.RequestDIDs.Send({
			sessionId: BillingSessions.CurrentSessionId(),
			limitToIds: [id]
		});

		ret.outboundRequestPromise = rtrNew.outboundRequestPromise;

		ret.completeRequestPromise = new Promise((resolve, reject) => {
			ret._completeRequestPromiseResolve = resolve;
			ret._completeRequestPromiseReject = reject;
		});

		// Handlers once we get a response.
		if (rtrNew.completeRequestPromise) {
			rtrNew.completeRequestPromise.then(() => {
				if (ret._completeRequestPromiseResolve) {
					ret._completeRequestPromiseResolve(DID.ForId(id));
				}
			});

			rtrNew.completeRequestPromise.catch((e: Error) => {
				if (ret._completeRequestPromiseReject) {
					ret._completeRequestPromiseReject(e);
				}
			});

			rtrNew.completeRequestPromise.finally(() => {
				this._RefreshTracker[id].isComplete = true;
			});
		}

		this._RefreshTracker[id] = {
			lastRequestTimeUtc: DateTime.utc(),
			isComplete: false,
			rtr: rtrNew
		};

		return ret;
	}

	public static ValidateObject(o: IDID): IDID {
		return o;
	}

	public static GetMerged(mergeValues: Record<string, any>): IDID {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IDID {
		const id = GenerateID();
		const ret: IDID = {
			id,
			json: {
				DIDNumber: null,
				assignToType: null,
				assignToID: null,
				lastModifiedBillingId: null
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): IDID | null {
		if (!id) {
			return null;
		}

		const dids = store.state.Database.dids as Record<string, IDID>;
		if (!dids || Object.keys(dids).length === 0) {
			return null;
		}

		let did = dids[id];
		if (!did) {
			did = CaseInsensitivePropertyGet(dids, id);
		}
		if (!did) {
			return null;
		}

		return did;
	}

	public static UpdateIds(payload: Record<string, IDID>): void {
		store.commit("UpdateDIDs", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteDIDs", ids);
	}

	public static PermDIDsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.dids.request-any") !== -1 ||
			perms.indexOf("crm.dids.request-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermDIDsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.dids.push-any") !== -1 ||
			perms.indexOf("crm.dids.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermDIDsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.dids.delete-any") !== -1 ||
			perms.indexOf("crm.dids.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

(window as any).DEBUG_DID = DID;

export default {};
