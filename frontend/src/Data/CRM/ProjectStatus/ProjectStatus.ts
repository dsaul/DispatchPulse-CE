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
import { RPCRequestProjectStatus } from "@/Data/CRM/ProjectStatus/RPCRequestProjectStatus";
import { RPCDeleteProjectStatus } from "@/Data/CRM/ProjectStatus/RPCDeleteProjectStatus";
import { RPCPushProjectStatus } from "@/Data/CRM/ProjectStatus/RPCPushProjectStatus";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IProjectStatus extends ICRMTable {
	json: {
		/**
		 * @deprecated
		 */
		id: string;
		name: string | null;
		isOpen: boolean | null;
		isAwaitingPayment: boolean | null;
		isClosed: boolean | null;
		isNewProjectStatus: boolean | null;
		/**
		 * @deprecated
		 */
		lastModifiedISO8601: string | null;
		lastModifiedBillingId: string | null;
	};
}

export class ProjectStatus {
	// RPC Methods
	public static RequestProjectStatus = RPCMethod.Register<
		RPCRequestProjectStatus
	>(new RPCRequestProjectStatus());
	public static DeleteProjectStatus = RPCMethod.Register<
		RPCDeleteProjectStatus
	>(new RPCDeleteProjectStatus());
	public static PushProjectStatus = RPCMethod.Register<RPCPushProjectStatus>(
		new RPCPushProjectStatus()
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

		const projectStatus = ProjectStatus.ForId(id);
		if (projectStatus) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(projectStatus);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = ProjectStatus.RequestProjectStatus.Send({
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
					ret._completeRequestPromiseResolve(ProjectStatus.ForId(id));
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

	public static GetMerged(mergeValues: Record<string, any>): IProjectStatus {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IProjectStatus {
		const id = GenerateID();
		const ret: IProjectStatus = {
			id,
			json: {
				id,
				name: "",
				isOpen: false,
				isAwaitingPayment: false,
				isClosed: false,
				isNewProjectStatus: false,
				lastModifiedBillingId: null,
				lastModifiedISO8601: DateTime.utc().toISO()
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): IProjectStatus | null {
		if (!id) {
			return null;
		}

		const statuses = store.state.Database.projectStatus as Record<
			string,
			IProjectStatus
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

	public static UpdateIds(payload: Record<string, IProjectStatus>): void {
		store.commit("UpdateProjectStatus", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteProjectStatus", ids);
	}

	public static ValidateObject(o: IProjectStatus): IProjectStatus {
		return o;
	}

	public static PermProjectStatusCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.project-status.push-any") !== -1 ||
			perms.indexOf("crm.project-status.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermProjectStatusCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.project-status.request-any") !== -1 ||
			perms.indexOf("crm.project-status.request-company") !== -1 ||
			perms.indexOf("crm.project-status.request-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermProjectStatusCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.project-status.delete-any") !== -1 ||
			perms.indexOf("crm.project-status.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportProjectStatusCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.export.project-status-definitions-csv") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
