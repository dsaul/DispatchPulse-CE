import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { guid } from "@/Utility/GlobalTypes";
import ITracker from "@/Utility/ITracker";

import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestAgentsEmploymentStatus } from "@/Data/CRM/EmploymentStatus/RPCRequestEmploymentStatus";
import { RPCPushAgentsEmploymentStatus } from "@/Data/CRM/EmploymentStatus/RPCPushEmploymentStatus";
import { RPCDeleteAgentsEmploymentStatus } from "@/Data/CRM/EmploymentStatus/RPCDeleteEmploymentStatus";
import { RPCMethod } from "@/RPC/RPCMethod";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IEmploymentStatus extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string | null;
		shouldBeListedInScheduler: boolean;
		isDefault: boolean;
		isContractor: boolean;
		isEmployee: boolean;
		isActive: boolean;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class EmploymentStatus {
	// RPC Methods
	public static RequestAgentsEmploymentStatus = RPCMethod.Register<
		RPCRequestAgentsEmploymentStatus
	>(new RPCRequestAgentsEmploymentStatus());
	public static PushAgentsEmploymentStatus = RPCMethod.Register<
		RPCPushAgentsEmploymentStatus
	>(new RPCPushAgentsEmploymentStatus());
	public static DeleteAgentsEmploymentStatus = RPCMethod.Register<
		RPCDeleteAgentsEmploymentStatus
	>(new RPCDeleteAgentsEmploymentStatus());

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

		const employmentStatus = EmploymentStatus.ForId(id);
		if (employmentStatus) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(employmentStatus);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = EmploymentStatus.RequestAgentsEmploymentStatus.Send({
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
					ret._completeRequestPromiseResolve(
						EmploymentStatus.ForId(id)
					);
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

	public static GetMerged(
		mergeValues: Record<string, any>
	): IEmploymentStatus {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IEmploymentStatus {
		const id = GenerateID();
		const ret: IEmploymentStatus = {
			id,
			json: {
				id,
				name: "",
				shouldBeListedInScheduler: false,
				lastModifiedISO8601: null,
				lastModifiedBillingId: null,
				isDefault: false,
				isContractor: false,
				isEmployee: false,
				isActive: false
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): IEmploymentStatus | null {
		if (!id) {
			return null;
		}

		const statuses = store.state.Database.agentsEmploymentStatus as Record<
			string,
			IEmploymentStatus
		>;
		if (!statuses || Object.keys(statuses).length === 0) {
			return null;
		}

		let status = statuses[id];
		if (!status || !status.json) {
			status = CaseInsensitivePropertyGet(statuses, id);
		}
		if (!status || !status.json) {
			return null;
		}

		return status;
	}

	public static UpdateIds(payload: Record<string, IEmploymentStatus>): void {
		store.commit("UpdateAgentsEmploymentStatus", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteAgentsEmploymentStatus", ids);
	}

	public static All(): IEmploymentStatus[] {
		const dblist = store.state.Database.agentsEmploymentStatus as Record<
			string,
			IEmploymentStatus
		>;

		const filtered = _.filter(dblist, (o: IEmploymentStatus) => {
			// eslint-disable-line @typescript-eslint/no-unused-vars
			return true;
		});

		const sorted = _.sortBy(filtered, (o: IEmploymentStatus) => {
			return o.json.name;
		});

		return sorted;
	}

	public static ValidateObject(o: IEmploymentStatus): IEmploymentStatus {
		if (!o.json.hasOwnProperty("isDefault")) {
			o.json.isDefault = false;
		}
		if (!o.json.hasOwnProperty("isContractor")) {
			o.json.isContractor = false;
		}
		if (!o.json.hasOwnProperty("isEmployee")) {
			o.json.isEmployee = false;
		}
		if (!o.json.hasOwnProperty("isActive")) {
			o.json.isActive = false;
		}

		return o;
	}

	public static PermEmploymentStatusCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.employment-status.push-any") !== -1 ||
			perms.indexOf("crm.employment-status.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermEmploymentStatusCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.employment-status.request-any") !== -1 ||
			perms.indexOf("crm.employment-status.request-company") !== -1 ||
			perms.indexOf("crm.employment-status.request-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermEmploymentStatusCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.employment-status.delete-any") !== -1 ||
			perms.indexOf("crm.employment-status.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportEmploymentStatusCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.export.employment-status-definitions-csv") !==
			-1;
		//console.log(perms, ret);
		return ret;
	}
}

(window as any).DEBUG_EmploymentStatus = EmploymentStatus;

export default {};
