import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { LabourType } from "@/Data/CRM/LabourType/LabourType";
import { Project } from "@/Data/CRM/Project/Project";
import { LabourSubtypeException } from "@/Data/CRM/LabourSubtypeException/LabourSubtypeException";
import { LabourSubtypeHoliday } from "@/Data/CRM/LabourSubtypeHoliday/LabourSubtypeHoliday";
import { LabourSubtypeNonBillable } from "@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable";
import { guid } from "@/Utility/GlobalTypes";
import ITracker from "@/Utility/ITracker";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestLabour } from "@/Data/CRM/Labour/RPCRequestLabour";
import { RPCDeleteLabour } from "@/Data/CRM/Labour/RPCDeleteLabour";
import { RPCPushLabour } from "@/Data/CRM/Labour/RPCPushLabour";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface ILabour extends ICRMTable {
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
		projectId: string | null;
		agentId: string | null;
		assignmentId: string | null;
		typeId: string | null;

		timeMode: "none" | "date-and-hours" | "start-stop-timestamp" | null;

		hours: number | null;
		startISO8601: string | null;
		endISO8601: string | null;
		isActive: boolean;

		locationType: "none" | "travel" | "on-site" | "remote" | null;

		isExtra: boolean | null;
		isBilled: boolean | null;
		isPaidOut: boolean | null;

		isEnteredThroughTelephoneCompanyAccess: boolean | null;

		exceptionTypeId: string | null;
		holidayTypeId: string | null;
		nonBillableTypeId: string | null;

		notes: string | null;

		bankedPayOutAmount: number | null;
	};
}

export class Labour {
	// RPC Methods
	public static RequestLabour = RPCMethod.Register<RPCRequestLabour>(
		new RPCRequestLabour()
	);
	public static DeleteLabour = RPCMethod.Register<RPCDeleteLabour>(
		new RPCDeleteLabour()
	);
	public static PushLabour = RPCMethod.Register<RPCPushLabour>(
		new RPCPushLabour()
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

		const labour = Labour.ForId(id);
		if (labour) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(labour);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = Labour.RequestLabour.Send({
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
					ret._completeRequestPromiseResolve(Labour.ForId(id));
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

	public static GetMerged(mergeValues: Record<string, any>): ILabour {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): ILabour {
		const id = GenerateID();
		const ret: ILabour = {
			id,
			json: {
				id,
				lastModifiedISO8601: "",
				lastModifiedBillingId: "",
				projectId: null,
				agentId: null,
				assignmentId: null,
				typeId: null,
				timeMode: null,
				hours: null,
				startISO8601: null,
				endISO8601: null,
				isActive: false,
				locationType: "none",
				isExtra: null,
				isBilled: null,
				isPaidOut: null,
				notes: null,
				bankedPayOutAmount: null,
				exceptionTypeId: null,
				holidayTypeId: null,
				nonBillableTypeId: null,
				isEnteredThroughTelephoneCompanyAccess: null
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): ILabour | null {
		if (!id) {
			return null;
		}

		const statuses = store.state.Database.labour as Record<string, ILabour>;
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

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteLabour", ids);
	}

	public static UpdateIds(payload: Record<string, ILabour>): void {
		store.commit("UpdateLabour", payload);
	}

	public static ForProjectIds(projectIds: string[]): ILabour[] {
		const all: Record<string, ILabour> = store.state.Database.labour;
		if (!all) {
			return [];
		}

		const filtered = _.filter(all, (o: ILabour) => {
			const projectId = o.json.projectId;

			return !!_.find(
				projectIds,
				(suppliedId: string) => suppliedId === projectId
			);
		});

		return filtered;
	}

	public static ForAssignmentIds(projectIds: string[]): ILabour[] {
		const all: Record<string, ILabour> = store.state.Database.labour;
		if (!all) {
			return [];
		}

		const filtered = _.filter(all, (o: ILabour) => {
			const assignmentId = o.json.assignmentId;

			return !!_.find(
				projectIds,
				(suppliedId: string) => suppliedId === assignmentId
			);
		});

		return filtered;
	}

	public static DescriptionForId(id: string | null): string | null {
		if (id == null) {
			return null;
		}

		const labour = Labour.ForId(id);
		if (!labour) {
			return null;
		}

		const typeId = labour.json.typeId;
		if (!typeId) {
			return null;
		}

		const type = LabourType.ForId(typeId);
		if (!type) {
			return null;
		}

		let ret = "";

		if (type.json.isBillable) {
			ret += "Billable: ";

			const projectId = labour.json.projectId;
			if (!projectId) {
				ret += "No project. ";
			} else {
				const project = Project.ForId(projectId);
				if (!project) {
					ret += "No project. ";
				} else {
					ret += Project.CombinedDescriptionForId(projectId);
					ret += " ";
				}
			}
		} else if (type.json.isException) {
			ret += "Exception:";

			const exceptionTypeId = labour.json.exceptionTypeId;
			if (!exceptionTypeId) {
				ret += "Unknown ";
			} else {
				const exceptionType = LabourSubtypeException.ForId(
					exceptionTypeId
				);
				if (!exceptionType) {
					ret += "Unknown ";
				} else {
					ret += `${exceptionType.json.name} `;
				}
			}
		} else if (type.json.isHoliday) {
			ret += "Holiday:";

			const holidayTypeId = labour.json.holidayTypeId;
			if (!holidayTypeId) {
				ret += "Unknown ";
			} else {
				const holidayType = LabourSubtypeHoliday.ForId(holidayTypeId);
				if (!holidayType) {
					ret += "Unknown ";
				} else {
					ret += `${holidayType.json.name} `;
				}
			}
		} else if (type.json.isNonBillable) {
			ret += "Non Billable:";

			const nonBillableTypeId = labour.json.nonBillableTypeId;
			if (!nonBillableTypeId) {
				ret += "Unknown ";
			} else {
				const nonBillableType = LabourSubtypeNonBillable.ForId(
					nonBillableTypeId
				);
				if (!nonBillableType) {
					ret += "Unknown ";
				} else {
					ret += `${nonBillableType.json.name} `;
				}
			}
		} else if (type.json.isPayOutBanked) {
			ret += "Pay Out Banked: ${labour.json.bankedPayOutAmount}";
		}

		switch (labour.json.timeMode) {
			case "none":
			default:
				ret += "No Time. ";
				break;
			case "date-and-hours":
				ret += `${labour.json.hours} hours.`;
				break;
			case "start-stop-timestamp":
				ret += `Start: ${labour.json.startISO8601} End: ${labour.json.endISO8601} `;
				break;
		}

		return ret;
	}

	public static IsExtraForId(id: string | null): boolean {
		if (id == null) {
			return false;
		}

		const labour = Labour.ForId(id);
		if (!labour) {
			return false;
		}

		const project = Project.ForId(labour.json.projectId);
		if (!project) {
			return false;
		}

		if (project.json.forceLabourAsExtra) {
			return true;
		}

		return labour.json.isExtra || false;
	}

	public static ValidateObject(o: ILabour): ILabour {
		return o;
	}

	public static PermCRMLabourManualEntries(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.labour.manual-entries") !== -1;
		return ret;
	}

	public static PermLabourCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour.push-any") !== -1 ||
			perms.indexOf("crm.labour.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermLabourCanPushSelf(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour.push-any") !== -1 ||
			perms.indexOf("crm.labour.push-company") !== -1 ||
			perms.indexOf("crm.labour.push-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermLabourCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour.request-any") !== -1 ||
			perms.indexOf("crm.labour.request-company") !== -1 ||
			perms.indexOf("crm.labour.request-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermLabourCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.labour.delete-any") !== -1 ||
			perms.indexOf("crm.labour.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMReportLabourPDF(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.report.labour-pdf") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportLabourCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.export.labour-csv") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

(window as any).DEBUG_Labour = Labour;

export default {};
