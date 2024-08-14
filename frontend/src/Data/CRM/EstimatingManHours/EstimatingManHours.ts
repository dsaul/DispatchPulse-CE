import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { guid } from "@/Utility/GlobalTypes";
import ITracker from "@/Utility/ITracker";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestEstimatingManHours } from "@/Data/CRM/EstimatingManHours/RPCRequestEstimatingManHours";
import { RPCDeleteEstimatingManHours } from "@/Data/CRM/EstimatingManHours/RPCDeleteEstimatingManHours";
import { RPCPushEstimatingManHours } from "@/Data/CRM/EstimatingManHours/RPCPushEstimatingManHours";
import { RPCMethod } from "@/RPC/RPCMethod";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IEstimatingManHours extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		item: string | null;
		manHours: number | null;
		measurement: string | null;
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class EstimatingManHours {
	// RPC Methods
	public static RequestEstimatingManHours = RPCMethod.Register<
		RPCRequestEstimatingManHours
	>(new RPCRequestEstimatingManHours());
	public static DeleteEstimatingManHours = RPCMethod.Register<
		RPCDeleteEstimatingManHours
	>(new RPCDeleteEstimatingManHours());
	public static PushEstimatingManHours = RPCMethod.Register<
		RPCPushEstimatingManHours
	>(new RPCPushEstimatingManHours());

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

		const estimatingManHours = EstimatingManHours.ForId(id);
		if (estimatingManHours) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(estimatingManHours);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = EstimatingManHours.RequestEstimatingManHours.Send({
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
						EstimatingManHours.ForId(id)
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
	): IEstimatingManHours {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IEstimatingManHours {
		const id = GenerateID();
		const ret: IEstimatingManHours = {
			id,
			json: {
				id,
				item: null,
				manHours: null,
				measurement: null,
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO()
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): IEstimatingManHours | null {
		if (!id) {
			return null;
		}

		const statuses = store.state.Database.estimatingManHours as Record<
			string,
			IEstimatingManHours
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

	public static UpdateIds(
		payload: Record<string, IEstimatingManHours>
	): void {
		store.commit("UpdateEstimatingManHours", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteEstimatingManHours", ids);
	}

	public static ValidateObject(o: IEstimatingManHours): IEstimatingManHours {
		return o;
	}

	public static PermEstimatingManHoursCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.estimating-man-hours.push-any") !== -1 ||
			perms.indexOf("crm.estimating-man-hours.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermEstimatingManHoursCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.estimating-man-hours.request-any") !== -1 ||
			perms.indexOf("crm.estimating-man-hours.request-company") !== -1 ||
			perms.indexOf("crm.estimating-man-hours.request-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermEstimatingManHoursCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.estimating-man-hours.delete-any") !== -1 ||
			perms.indexOf("crm.estimating-man-hours.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportManHoursCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.export.man-hours-definitions-csv") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
