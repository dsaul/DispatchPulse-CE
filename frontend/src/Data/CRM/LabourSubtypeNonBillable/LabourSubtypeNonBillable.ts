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
import { RPCRequestLabourSubtypeNonBillable } from "@/Data/CRM/LabourSubtypeNonBillable/RPCRequestLabourSubtypeNonBillable";
import { RPCDeleteLabourSubtypeNonBillable } from "@/Data/CRM/LabourSubtypeNonBillable/RPCDeleteLabourSubtypeNonBillable";
import { RPCPushLabourSubtypeNonBillable } from "@/Data/CRM/LabourSubtypeNonBillable/RPCPushLabourSubtypeNonBillable";
import { RPCMethod } from "@/RPC/RPCMethod";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface ILabourSubtypeNonBillable extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string;
		description: string | null;
		icon: string | null;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class LabourSubtypeNonBillable {
	// RPC Methods
	public static RequestLabourSubtypeNonBillable = RPCMethod.Register<
		RPCRequestLabourSubtypeNonBillable
	>(new RPCRequestLabourSubtypeNonBillable());
	public static DeleteLabourSubtypeNonBillable = RPCMethod.Register<
		RPCDeleteLabourSubtypeNonBillable
	>(new RPCDeleteLabourSubtypeNonBillable());
	public static PushLabourSubtypeNonBillable = RPCMethod.Register<
		RPCPushLabourSubtypeNonBillable
	>(new RPCPushLabourSubtypeNonBillable());

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

		const lsnb = LabourSubtypeNonBillable.ForId(id);
		if (lsnb) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(lsnb);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = LabourSubtypeNonBillable.RequestLabourSubtypeNonBillable.Send(
			{
				sessionId: BillingSessions.CurrentSessionId(),
				limitToIds: [id]
			}
		);

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
						LabourSubtypeNonBillable.ForId(id)
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
	): ILabourSubtypeNonBillable {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ILabourSubtypeNonBillable {
		const id = GenerateID();
		const ret: ILabourSubtypeNonBillable = {
			id,
			json: {
				id,
				name: "",
				description: null,
				icon: null,
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO()
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): ILabourSubtypeNonBillable | null {
		if (!id) {
			return null;
		}

		const types = store.state.Database.labourSubtypeNonBillable as Record<
			string,
			ILabourSubtypeNonBillable
		>;
		if (!types || Object.keys(types).length === 0) {
			return null;
		}

		let type = types[id];
		if (!type || !type.json) {
			type = CaseInsensitivePropertyGet(types, id);
		}
		if (!type || !type.json) {
			return null;
		}

		return type;
	}

	public static UpdateIds(
		payload: Record<string, ILabourSubtypeNonBillable>
	): void {
		store.commit("UpdateLabourSubtypeNonBillable", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteLabourSubtypeNonBillable", ids);
	}

	public static All(): ILabourSubtypeNonBillable[] {
		const dblist = store.state.Database.labourSubtypeNonBillable as Record<
			string,
			ILabourSubtypeNonBillable
		>;

		const filtered = _.filter(dblist, (o: ILabourSubtypeNonBillable) => {
			// eslint-disable-line @typescript-eslint/no-unused-vars
			return true;
		});

		const sorted = _.sortBy(filtered, (o: ILabourSubtypeNonBillable) => {
			return o.json.name;
		});

		return sorted;
	}

	public static NameForId(id: string | null): string | null {
		try {
			if (id == null) {
				return null;
			}

			const all = store.state.Database.labourSubtypeNonBillable as Record<
				string,
				ILabourSubtypeNonBillable
			>;

			let dbObj: ILabourSubtypeNonBillable = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}

			return dbObj.json.name;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
	}

	public static IconForId(id: string | null): string | null {
		try {
			if (id == null) {
				return null;
			}

			const all = store.state.Database.labourSubtypeNonBillable as Record<
				string,
				ILabourSubtypeNonBillable
			>;

			let dbObj: ILabourSubtypeNonBillable = all[id];
			if (!dbObj) {
				dbObj = CaseInsensitivePropertyGet(all, id);
			}

			const icon = dbObj.json.icon;
			//console.log('icon', icon);
			return icon;
		} catch (e) {
			//console.error('ProductNameForId', e);
			return null;
		}
	}

	public static ValidateObject(
		o: ILabourSubtypeNonBillable
	): ILabourSubtypeNonBillable {
		return o;
	}

	public static PermLabourSubtypeNonBillableCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour-subtype-non-billable.push-any") !== -1 ||
			perms.indexOf("crm.labour-subtype-non-billable.push-company") !==
				-1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermLabourSubtypeNonBillableCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour-subtype-non-billable.request-any") !==
				-1 ||
			perms.indexOf("crm.labour-subtype-non-billable.request-company") !==
				-1 ||
			perms.indexOf("crm.labour-subtype-non-billable.request-self") !==
				-1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermLabourSubtypeNonBillableCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour-subtype-non-billable.delete-any") !==
				-1 ||
			perms.indexOf("crm.labour-subtype-non-billable.delete-company") !==
				-1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportLabourSubtypeNonBillableCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.export.labour-non-billable-definitions-csv") !==
			-1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
