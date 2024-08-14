import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { guid } from "@/Utility/GlobalTypes";
import ITracker from "@/Utility/ITracker";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestMaterials } from "@/Data/CRM/Material/RPCRequestMaterials";
import { RPCDeleteMaterials } from "@/Data/CRM/Material/RPCDeleteMaterials";
import { RPCPushMaterials } from "@/Data/CRM/Material/RPCPushMaterials";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IMaterial extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
		dateUsedISO8601: string | null;
		projectId: string | null;
		quantity: number | null;
		quantityUnit: string | null;
		productId: string | null;
		isExtra: boolean | null;
		isBilled: boolean | null;
		location: string | null;
		notes: string | null;
	};
}

export class Material {
	// RPC Methods
	public static RequestMaterials = RPCMethod.Register<RPCRequestMaterials>(
		new RPCRequestMaterials()
	);
	public static DeleteMaterials = RPCMethod.Register<RPCDeleteMaterials>(
		new RPCDeleteMaterials()
	);
	public static PushMaterials = RPCMethod.Register<RPCPushMaterials>(
		new RPCPushMaterials()
	);

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

		const material = Material.ForId(id);
		if (material) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(material);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = Material.RequestMaterials.Send({
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
					ret._completeRequestPromiseResolve(Material.ForId(id));
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

	public static GetMerged(mergeValues: Record<string, any>): IMaterial {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IMaterial {
		const id = GenerateID();
		const ret: IMaterial = {
			id,
			json: {
				id,
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
				dateUsedISO8601: DateTime.local()
					.toUTC()
					.toISO(),
				projectId: null,
				quantity: null,
				quantityUnit: null,
				productId: null,
				isExtra: null,
				isBilled: null,
				location: null,
				notes: null
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteMaterials", ids);
	}

	public static UpdateIds(payload: Record<string, IMaterial>): void {
		store.commit("UpdateMaterials", payload);
	}

	public static ForId(id: string | null): IMaterial | null {
		if (!id) {
			return null;
		}

		const statuses = store.state.Database.materials as Record<
			string,
			IMaterial
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

	public static ForProjectIds(projectIds: string[]): IMaterial[] {
		const all: Record<string, IMaterial> = store.state.Database.materials;
		if (!all) {
			return [];
		}

		const filtered = _.filter(all, (o: IMaterial) => {
			const projectId = o.json.projectId;

			return !!_.find(
				projectIds,
				(suppliedId: string) => suppliedId === projectId
			);
		});

		return filtered;
	}

	public static ValidateObject(o: IMaterial): IMaterial {
		return o;
	}

	public static PermMaterialsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.materials.push-any") !== -1 ||
			perms.indexOf("crm.materials.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermMaterialsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.materials.request-any") !== -1 ||
			perms.indexOf("crm.materials.request-company") !== -1 ||
			perms.indexOf("crm.materials.request-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermMaterialsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.materials.delete-any") !== -1 ||
			perms.indexOf("crm.materials.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMReportMaterialsPDF(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.report.materials-pdf") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportMaterialsCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.export.materials-csv") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
