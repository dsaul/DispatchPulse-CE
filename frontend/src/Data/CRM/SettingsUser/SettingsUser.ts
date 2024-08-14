import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { RPCRequestSettingsUser } from "@/Data/CRM/SettingsUser/RPCRequestSettingsUser";
import { RPCDeleteSettingsUser } from "@/Data/CRM/SettingsUser/RPCDeleteSettingsUser";
import { RPCPushSettingsUser } from "@/Data/CRM/SettingsUser/RPCPushSettingsUser";
import { RPCMethod } from "@/RPC/RPCMethod";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";

export interface ISettingsUser extends ICRMTable {
	json: {
		key: string;
		value: string;

		lastModifiedBillingId: string | null;
	};
}

export class SettingsUser {
	// RPC Methods
	public static RequestSettingsUser = RPCMethod.Register<
		RPCRequestSettingsUser
	>(new RPCRequestSettingsUser());
	public static DeleteSettingsUser = RPCMethod.Register<
		RPCDeleteSettingsUser
	>(new RPCDeleteSettingsUser());
	public static PushSettingsUser = RPCMethod.Register<RPCPushSettingsUser>(
		new RPCPushSettingsUser()
	);

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

	// 	const su = SettingsUser.ForId(id);
	// 	if (su) {
	// 		ret.outboundRequestPromise = Promise.resolve();
	// 		ret.completeRequestPromise = Promise.resolve(su);
	// 		return ret;
	// 	}

	// 	// We'll need to request this.
	// 	const rtrNew = SettingsUser.RequestSettingsUser.Send({
	// sessionId: BillingSessions.CurrentSessionId(),
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
	// 				ret._completeRequestPromiseResolve(SettingsUser.ForId(id));
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

	public static GetMerged(mergeValues: Record<string, any>): ISettingsUser {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ISettingsUser {
		const id = GenerateID();
		const ret: ISettingsUser = {
			id,
			json: {
				key: "",
				value: "",
				lastModifiedBillingId: null
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ValidateObject(o: ISettingsUser): ISettingsUser {
		return o;
	}

	public static ForId(id: string | null): ISettingsUser | null {
		if (!id) {
			return null;
		}

		const statuses = store.state.Database.settingsUser as Record<
			string,
			ISettingsUser
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

	public static UpdateIds(payload: Record<string, ISettingsUser>): void {
		store.commit("UpdateSettingsUser", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteSettingsUser", ids);
	}
}

export default {};
