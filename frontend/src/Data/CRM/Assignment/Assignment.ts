import GenerateID from "@/Utility/GenerateID";
import { DateTime } from "luxon";
import _ from "lodash";
import store from "@/plugins/store/store";
import CaseInsensitivePropertyGet from "@/Utility/CaseInsensitivePropertyGet";
import { Project } from "@/Data/CRM/Project/Project";
import IsNullOrEmpty from "@/Utility/IsNullOrEmpty";
import TruncateStringToWord from "@/Utility/TruncateStringToWord";
import { ILabour, Labour } from "@/Data/CRM/Labour/Labour";
import { BillingContacts } from "@/Data/Billing/BillingContacts/BillingContacts";
import { Agent } from "../Agent/Agent";
import { guid } from "@/Utility/GlobalTypes";
import ITracker from "@/Utility/ITracker";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import { BillingPermissionsBool } from "@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool";
import { RPCRequestAssignments } from "@/Data/CRM/Assignment/RPCRequestAssignments";
import { RPCDeleteAssignments } from "@/Data/CRM/Assignment/RPCDeleteAssignments";
import { RPCPushAssignments } from "@/Data/CRM/Assignment/RPCPushAssignments";
import { RPCMethod } from "@/RPC/RPCMethod";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";
import { ISchedulerLocalTime } from "@/Data/Models/SchedulerLocalTime/SchedulerLocalTime";
import { ISchedulerCellDragData } from "@/Data/Models/SchedulerCellDragData/SchedulerCellDragData";
import { ILabourType } from "../LabourType/LabourType";

export interface IAssignment extends ICRMTable {
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

		/**
		 * @deprecated use agentIds instead
		 */
		agentId?: string | null;
		agentIds: Array<string | null>;

		workRequested: string | null;

		/**
		 * @deprecated
		 */
		workPerformed: string | null;

		/**
		 * @deprecated
		 */
		internalComments: string | null;

		hasStartISO8601: boolean;
		startTimeMode:
			| "none"
			| "morning-first-thing"
			| "morning-second-thing"
			| "afternoon-first-thing"
			| "afternoon-second-thing"
			| "time";
		startISO8601: string | null;

		hasEndISO8601: boolean;
		endTimeMode: "none" | "time";
		endISO8601: string | null;

		statusId: string | null;
	};
}

export class Assignment {
	// RPC Methods
	public static RequestAssignments = RPCMethod.Register<
		RPCRequestAssignments
	>(new RPCRequestAssignments());
	public static DeleteAssignments = RPCMethod.Register<RPCDeleteAssignments>(
		new RPCDeleteAssignments()
	);
	public static PushAssignments = RPCMethod.Register<RPCPushAssignments>(
		new RPCPushAssignments()
	);

	public static _RefreshTracker: { [id: string]: ITracker } = {};

	public static ValidateObject(o: IAssignment): IAssignment {
		// Add missing variables.

		if (!o.json.agentIds) {
			o.json.agentIds = [];
		}

		// Migrate deprecated values.
		if (o.json.agentId && !IsNullOrEmpty(o.json.agentId)) {
			// tslint:disable-line: deprecation

			const obj = _.find(o.json.agentIds, value => {
				return value === o.json.agentId; // tslint:disable-line: deprecation
			});

			if (!obj) {
				o.json.agentIds.push(o.json.agentId); // tslint:disable-line: deprecation
			}

			delete o.json.agentId; // tslint:disable-line: deprecation
		}

		return o;
	}

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

		const assignment = Assignment.ForId(id);
		if (assignment) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(assignment);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = Assignment.RequestAssignments.Send({
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
					ret._completeRequestPromiseResolve(Assignment.ForId(id));
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

	public static GetMerged(mergeValues: Record<string, any>): IAssignment {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IAssignment {
		const id = GenerateID();
		const ret: IAssignment = {
			id,
			json: {
				id,
				lastModifiedISO8601: DateTime.utc().toISO(),
				lastModifiedBillingId: null,
				projectId: null,
				agentIds: [],

				workRequested: null,
				workPerformed: null,
				internalComments: null,

				hasStartISO8601: false,
				startTimeMode: "none",
				startISO8601: DateTime.utc().toISO(),

				hasEndISO8601: false,
				endTimeMode: "none",
				endISO8601: null,

				statusId: null
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static UpdateIds(payload: Record<string, IAssignment>): void {
		store.commit("UpdateAssignments", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteAssignments", ids);
	}

	public static ForId(id: string | null): IAssignment | null {
		if (!id) {
			return null;
		}

		const assignments = store.state.Database.assignments as Record<
			string,
			IAssignment
		>;
		if (!assignments || Object.keys(assignments).length === 0) {
			return null;
		}

		let assignment = assignments[id];
		if (!assignment) {
			assignment = CaseInsensitivePropertyGet(assignments, id);
		}
		if (!assignment) {
			return null;
		}

		return assignment;
	}

	public static ForProjectIds(projectIds: string[]): IAssignment[] {
		const all: Record<string, IAssignment> =
			store.state.Database.assignments;
		if (!all) {
			return [];
		}

		const filtered = _.filter(all, (o: IAssignment) => {
			const projectId = o.json.projectId;

			return !!_.find(
				projectIds,
				(suppliedId: string) => suppliedId === projectId
			);
		});

		return filtered;
	}

	public static ForSchedulerMinimization(params: {
		date: string;
		isUnscheduled: boolean;
		isFirstAM: boolean;
		isSecondAM: boolean;
		isFirstPM: boolean;
		isSecondPM: boolean;
		localTimeStart: ISchedulerLocalTime | null;
		localTimeEnd: ISchedulerLocalTime | null;
	}): boolean {
		const assignments = store.state.Database.assignments as Record<
			string,
			IAssignment
		>;

		const filtered = _.filter(assignments, (o: IAssignment) => {
			if (!o) {
				return false;
			}
			if (!o.id) {
				return false;
			}
			if (!o.json) {
				return false;
			}

			const startTimeMode = Assignment.StartTimeModeForId(o.id);

			// Check one direction
			if (!params.isFirstAM && startTimeMode === "morning-first-thing") {
				return false;
			}
			if (
				!params.isSecondAM &&
				startTimeMode === "morning-second-thing"
			) {
				return false;
			}
			if (
				!params.isFirstPM &&
				startTimeMode === "afternoon-first-thing"
			) {
				return false;
			}
			if (
				!params.isSecondPM &&
				startTimeMode === "afternoon-second-thing"
			) {
				return false;
			}
			if (!params.isUnscheduled && startTimeMode === "none") {
				return false;
			}

			// Check the other
			if (params.isFirstAM && startTimeMode !== "morning-first-thing") {
				return false;
			}
			if (params.isSecondAM && startTimeMode !== "morning-second-thing") {
				return false;
			}
			if (params.isFirstPM && startTimeMode !== "afternoon-first-thing") {
				return false;
			}
			if (
				params.isSecondPM &&
				startTimeMode !== "afternoon-second-thing"
			) {
				return false;
			}
			if (params.isUnscheduled && startTimeMode !== "none") {
				return false;
			}

			// Check date for today, unless this is unscheduled.
			if (!params.isUnscheduled) {
				// Cells that are not unscheduled have to have a start time.
				if (!o.json.hasStartISO8601) {
					return false;
				}

				// startTimeMode has been checked for above

				// We must have a date specified.
				const startISO8601 = Assignment.StartISO8601ForId(o.id);
				if (
					startTimeMode !== "none" &&
					(!startISO8601 || IsNullOrEmpty(startISO8601))
				) {
					return false;
				}

				if (!startISO8601) {
					return false;
				}

				const dateObj = DateTime.fromISO(startISO8601);
				const dateLocalObj = dateObj.toLocal();
				if (dateLocalObj.toFormat("yyyy-MM-dd") !== params.date) {
					return false;
				}
			}

			// Check time
			if (
				!params.isUnscheduled &&
				!params.isFirstAM &&
				!params.isSecondAM &&
				!params.isFirstPM &&
				!params.isSecondPM &&
				params.localTimeStart != null &&
				params.localTimeEnd != null
			) {
				//console.log('this.localTimeStart', this.localTimeStart);
				//console.log('this.localTimeEnd', this.localTimeEnd);

				const startISO8601 = Assignment.StartISO8601ForId(o.id);
				if (
					startTimeMode !== "time" ||
					!startISO8601 ||
					IsNullOrEmpty(startISO8601)
				) {
					return false;
				}

				const dateParsed = DateTime.fromFormat(
					params.date,
					"yyyy-MM-dd",
					{
						zone: "local",
						setZone: true
					}
				);

				const assginmentStartDB = DateTime.fromISO(startISO8601);
				const assginmentStartLocal = assginmentStartDB.toLocal();

				let startRange = DateTime.fromObject(
					{
						year: dateParsed.year,
						month: dateParsed.month,
						day: dateParsed.day,
						hour: params.localTimeStart.hour || 0,
						minute: params.localTimeStart.minute || 0,
						second: params.localTimeStart.second || 0,
						millisecond: 0
					},
					{
						zone: "local"
					}
				);
				startRange = startRange.minus({ seconds: 1 });

				let endRange = DateTime.fromObject(
					{
						year: dateParsed.year,
						month: dateParsed.month,
						day: dateParsed.day,
						hour: params.localTimeEnd.hour || 23,
						minute: params.localTimeEnd.minute || 59,
						second: params.localTimeEnd.second || 59,
						millisecond: 0
					},
					{
						zone: "local"
					}
				);
				endRange = endRange.plus({ seconds: 1 });

				/*console.debug('assginmentStartLocal', +assginmentStartLocal, 'startRange', +startRange);
					console.debug('assginmentStartLocal', +assginmentStartLocal, 'endRange', +endRange);
					
					console.debug(
						'startRange', startRange.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS),
						'assginmentStartLocal', assginmentStartLocal.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS),
						'endRange', endRange.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS)
					);*/

				if (
					+assginmentStartLocal <= +startRange ||
					+assginmentStartLocal >= +endRange
				) {
					//console.error('nope!');
					return false;
				}
			}

			return true;
		});

		return filtered.length > 0;
	}

	public static ForSchedulerCell(
		agentId: string | null,
		date: string,
		isUnscheduled: boolean,
		isFirstAM: boolean,
		isSecondAM: boolean,
		isFirstPM: boolean,
		isSecondPM: boolean,
		localTimeStart: ISchedulerLocalTime | null,
		localTimeEnd: ISchedulerLocalTime | null
	): ISchedulerCellDragData[] {
		const assignments = store.state.Database.assignments as Record<
			string,
			IAssignment
		>;

		const filtered = _.filter(assignments, (o: IAssignment) => {
			if (!o) {
				return false;
			}
			if (!o.id) {
				return false;
			}
			if (!o.json) {
				return false;
			}
			//
			// Check Agent
			//
			// For assigned

			if (agentId && !IsNullOrEmpty(agentId)) {
				// cell has agent

				const foundAgentId = _.find(o.json.agentIds, value => {
					return value?.trim() === agentId?.trim();
				});

				// If we don't find our agent id in the list we don't want to show it, return false.
				if (!foundAgentId) {
					return false;
				}
			}

			const startTimeMode = Assignment.StartTimeModeForId(o.id);

			// Check one direction
			if (!isFirstAM && startTimeMode === "morning-first-thing") {
				return false;
			}
			if (!isSecondAM && startTimeMode === "morning-second-thing") {
				return false;
			}
			if (!isFirstPM && startTimeMode === "afternoon-first-thing") {
				return false;
			}
			if (!isSecondPM && startTimeMode === "afternoon-second-thing") {
				return false;
			}
			if (!isUnscheduled && startTimeMode === "none") {
				return false;
			}

			// Check the other
			if (isFirstAM && startTimeMode !== "morning-first-thing") {
				return false;
			}
			if (isSecondAM && startTimeMode !== "morning-second-thing") {
				return false;
			}
			if (isFirstPM && startTimeMode !== "afternoon-first-thing") {
				return false;
			}
			if (isSecondPM && startTimeMode !== "afternoon-second-thing") {
				return false;
			}
			if (isUnscheduled && startTimeMode !== "none") {
				return false;
			}

			// Check date for today, unless this is unscheduled.
			if (!isUnscheduled) {
				// Cells that are not unscheduled have to have a start time.
				if (!o.json.hasStartISO8601) {
					return false;
				}

				// startTimeMode has been checked for above

				// We must have a date specified.
				const startISO8601 = Assignment.StartISO8601ForId(o.id);
				if (
					startTimeMode !== "none" &&
					(!startISO8601 || IsNullOrEmpty(startISO8601))
				) {
					return false;
				}

				if (!startISO8601) {
					return false;
				}

				const dateObj = DateTime.fromISO(startISO8601);
				const dateLocalObj = dateObj.toLocal();
				if (dateLocalObj.toFormat("yyyy-MM-dd") !== date) {
					return false;
				}
			}

			// Check time
			if (
				!isUnscheduled &&
				!isFirstAM &&
				!isSecondAM &&
				!isFirstPM &&
				!isSecondPM &&
				localTimeStart != null &&
				localTimeEnd != null
			) {
				//console.log('this.localTimeStart', this.localTimeStart);
				//console.log('this.localTimeEnd', this.localTimeEnd);

				const startISO8601 = Assignment.StartISO8601ForId(o.id);
				if (
					startTimeMode !== "time" ||
					!startISO8601 ||
					IsNullOrEmpty(startISO8601)
				) {
					return false;
				}

				const dateParsed = DateTime.fromFormat(date, "yyyy-MM-dd", {
					zone: "local",
					setZone: true
				});

				const assginmentStartDB = DateTime.fromISO(startISO8601);
				const assginmentStartLocal = assginmentStartDB.toLocal();

				let startRange = DateTime.fromObject(
					{
						year: dateParsed.year,
						month: dateParsed.month,
						day: dateParsed.day,
						hour: localTimeStart.hour || 0,
						minute: localTimeStart.minute || 0,
						second: localTimeStart.second || 0,
						millisecond: 0
					},
					{
						zone: "local"
					}
				);
				startRange = startRange.minus({ seconds: 1 });

				let endRange = DateTime.fromObject(
					{
						year: dateParsed.year,
						month: dateParsed.month,
						day: dateParsed.day,
						hour: localTimeEnd.hour || 23,
						minute: localTimeEnd.minute || 59,
						second: localTimeEnd.second || 59,
						millisecond: 0
					},
					{
						zone: "local"
					}
				);
				endRange = endRange.plus({ seconds: 1 });

				/*console.debug('assginmentStartLocal', +assginmentStartLocal, 'startRange', +startRange);
					console.debug('assginmentStartLocal', +assginmentStartLocal, 'endRange', +endRange);
					
					console.debug(
						'startRange', startRange.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS),
						'assginmentStartLocal', assginmentStartLocal.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS),
						'endRange', endRange.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS)
					);*/

				if (
					+assginmentStartLocal <= +startRange ||
					+assginmentStartLocal >= +endRange
				) {
					//console.error('nope!');
					return false;
				}
			}

			return true;
		});

		const res: ISchedulerCellDragData[] = [];

		// We need to go through and create an entry for each agent this is assigned to.

		for (const assignment of filtered) {
			const validAgentIds = _.filter(assignment.json.agentIds, value => {
				if (value === undefined) {
					return false;
				}
				return true;
			});

			const invalidAgentIds = _.filter(
				assignment.json.agentIds,
				value => {
					if (!value || IsNullOrEmpty(value)) {
						return true;
					}
					if (!Agent.ForId(value)) {
						return true;
					}
					return false;
				}
			);

			// console.log('validAgentIds', validAgentIds);

			if (!agentId || IsNullOrEmpty(agentId)) {
				// this cell is unassigned

				if (validAgentIds.length === 0) {
					res.push({
						assignment,
						forAgentId: null
					});
				} else {
					for (const discard of invalidAgentIds) {
						// eslint-disable-line @typescript-eslint/no-unused-vars
						res.push({
							assignment,
							forAgentId: null
						});
					}
				}
			} else {
				// this cell is assigned

				for (const thisAgentId of validAgentIds) {
					if (thisAgentId !== agentId) {
						continue;
					}

					res.push({
						assignment,
						forAgentId: thisAgentId
					});
				}
			}
		}

		//console.log(res);

		return res;
	}

	public static NameForId(id: string | null): string | null {
		const assignment = Assignment.ForId(id);
		if (
			!assignment ||
			!assignment.json ||
			IsNullOrEmpty(assignment.json.projectId)
		) {
			return null;
		}

		const project = Project.ForId(assignment.json.projectId);
		if (!project || !project.json) {
			return "Assignment without project.";
		}

		let ret = "";

		if (project.json && project.json.addresses) {
			for (const addr of project.json.addresses) {
				if (!IsNullOrEmpty(addr.value)) {
					ret += `${addr.value} `;
				}
			}
		}

		if (
			project.json &&
			project.json.name &&
			!IsNullOrEmpty(project.json.name)
		) {
			ret += ` (${project.json.name}) `;
		}

		return ret.trim();
	}

	public static WorkDescriptionForId(id: string | null): string | null {
		if (id == null) {
			return null;
		}

		const assignment = Assignment.ForId(id);
		if (assignment == null) {
			return null;
		}

		if (
			null == assignment.json.workRequested ||
			IsNullOrEmpty(assignment.json.workRequested)
		) {
			return null;
		}

		return TruncateStringToWord(assignment.json.workRequested, 30, true);
	}

	public static LocationDescriptionForId(id: string | null): string | null {
		if (!id) {
			return null;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			return null;
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return (
				"No Address. Work: " +
				(Assignment.WorkDescriptionForId(id) || "No Work Specified")
			);
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return (
				"No Address. Work: " +
				(Assignment.WorkDescriptionForId(id) || "No Work Specified")
			);
		}

		return Project.CombinedDescriptionForId(projectId);
	}

	public static StartTravelForId(id: string | null): void {
		if (!id) {
			console.error("Assignment.StartTravelOnId !id", id);
			return;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error("Assignment.StartTravelOnId !assignment", assignment);
			return;
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			console.error("Assignment.StartTravelOnId !projectId", projectId);
			return;
		}

		const agent = Agent.ForLoggedInAgent();
		if (!agent) {
			console.error("Assignment.StartTravelOnId !agent", agent);
			return;
		}
		if (!agent.id) {
			console.error("Assignment.StartTravelOnId !agent.id", agent);
			return;
		}

		const labour = Labour.GetEmpty();
		if (!labour.id) {
			console.error("!labour.id");
			return;
		}

		labour.lastModifiedISO8601 = DateTime.utc().toISO();
		labour.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		labour.json.projectId = projectId;
		labour.json.agentId = agent.id;
		labour.json.assignmentId = id;
		labour.json.isEnteredThroughTelephoneCompanyAccess = false;

		// Find the default type id for billable.
		const allTypes = store.state.Database.labourTypes as Record<
			string,
			ILabourType
		>;
		let billableType = null;
		for (const type of Object.values(allTypes) as ILabourType[]) {
			if (type.json.isBillable === true && type.json.default === true) {
				billableType = type;
			}
		}

		if (!billableType) {
			console.error("Assignment.StartTravelOnId !billableType");
			return;
		}
		if (!billableType.id) {
			console.error("Assignment.StartTravelOnId !billableType.id");
			return;
		}

		labour.json.typeId = billableType.id;
		labour.json.timeMode = "start-stop-timestamp";
		labour.json.hours = null;
		labour.json.startISO8601 = DateTime.utc().toISO();
		labour.json.endISO8601 = null;
		labour.json.isActive = true;
		labour.json.locationType = "travel";
		labour.json.isExtra = null;
		labour.json.isBilled = null;
		labour.json.isPaidOut = null;

		labour.json.exceptionTypeId = null;
		labour.json.holidayTypeId = null;
		labour.json.nonBillableTypeId = null;

		labour.json.notes = null;
		labour.json.bankedPayOutAmount = null;

		//console.log('start travel', labour);

		const payload: Record<string, ILabour> = {};
		payload[labour.id] = labour;
		store.commit("UpdateLabour", payload);
	}

	public static StartOnSiteForId(id: string | null): void {
		if (!id) {
			console.error("Assignment.StartOnSiteForId !id", id);
			return;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error(
				"Assignment.StartOnSiteForId !assignment",
				assignment
			);
			return;
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			console.error("Assignment.StartOnSiteForId !projectId", projectId);
			return;
		}

		const agent = Agent.ForLoggedInAgent();
		if (!agent) {
			console.error("Assignment.StartOnSiteForId !agent", agent);
			return;
		}
		if (!agent.id) {
			console.error("Assignment.StartOnSiteForId !agent.id", agent);
			return;
		}

		const labour = Labour.GetEmpty();
		if (!labour.id) {
			console.error("!labour.id");
			return;
		}

		labour.lastModifiedISO8601 = DateTime.utc().toISO();
		labour.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		labour.json.projectId = projectId;
		labour.json.agentId = agent.id;
		labour.json.assignmentId = id;
		labour.json.isEnteredThroughTelephoneCompanyAccess = false;

		// Find the default type id for billable.
		const allTypes = store.state.Database.labourTypes as Record<
			string,
			ILabourType
		>;
		let billableType = null;
		for (const type of Object.values(allTypes) as ILabourType[]) {
			if (type.json.isBillable === true && type.json.default === true) {
				billableType = type;
			}
		}

		if (!billableType) {
			console.error("Assignment.StartOnSiteOnId !billableType");
			return;
		}
		if (!billableType.id) {
			console.error("Assignment.StartOnSiteOnId !billableType.id");
			return;
		}

		labour.json.typeId = billableType.id;
		labour.json.timeMode = "start-stop-timestamp";
		labour.json.hours = null;
		labour.json.startISO8601 = DateTime.utc().toISO();
		labour.json.endISO8601 = null;
		labour.json.isActive = true;
		labour.json.locationType = "on-site";
		labour.json.isExtra = null;
		labour.json.isBilled = null;
		labour.json.isPaidOut = null;

		labour.json.exceptionTypeId = null;
		labour.json.holidayTypeId = null;
		labour.json.nonBillableTypeId = null;

		labour.json.notes = null;
		labour.json.bankedPayOutAmount = null;

		console.log("start on-site", labour);

		const payload: Record<string, ILabour> = {};
		payload[labour.id] = labour;
		store.commit("UpdateLabour", payload);
	}

	public static StartRemoteForId(id: string | null): void {
		if (!id) {
			console.error("Assignment.StartRemoteForId !id", id);
			return;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error(
				"Assignment.StartRemoteForId !assignment",
				assignment
			);
			return;
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			console.error("Assignment.StartRemoteForId !projectId", projectId);
			return;
		}

		const agent = Agent.ForLoggedInAgent();
		if (!agent) {
			console.error("Assignment.StartRemoteForId !agent", agent);
			return;
		}
		if (!agent.id) {
			console.error("Assignment.StartRemoteForId !agent.id", agent);
			return;
		}

		const labour = Labour.GetEmpty();
		if (!labour.id) {
			console.error("!labour.id");
			return;
		}

		labour.lastModifiedISO8601 = DateTime.utc().toISO();
		labour.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();
		labour.json.projectId = projectId;
		labour.json.agentId = agent.id;
		labour.json.assignmentId = id;
		labour.json.isEnteredThroughTelephoneCompanyAccess = false;

		// Find the default type id for billable.
		const allTypes = store.state.Database.labourTypes as Record<
			string,
			ILabourType
		>;
		let billableType = null;
		for (const type of Object.values(allTypes) as ILabourType[]) {
			if (type.json.isBillable === true && type.json.default === true) {
				billableType = type;
			}
		}

		if (!billableType) {
			console.error("Assignment.StartRemoteForId !billableType");
			return;
		}
		if (!billableType.id) {
			console.error("Assignment.StartRemoteForId !billableType.id");
			return;
		}

		labour.json.typeId = billableType.id;
		labour.json.timeMode = "start-stop-timestamp";
		labour.json.hours = null;
		labour.json.startISO8601 = DateTime.utc().toISO();
		labour.json.endISO8601 = null;
		labour.json.isActive = true;
		labour.json.locationType = "remote";
		labour.json.isExtra = null;
		labour.json.isBilled = null;
		labour.json.isPaidOut = null;

		labour.json.exceptionTypeId = null;
		labour.json.holidayTypeId = null;
		labour.json.nonBillableTypeId = null;

		labour.json.notes = null;
		labour.json.bankedPayOutAmount = null;

		console.log("start remote", labour);

		const payload: Record<string, ILabour> = {};
		payload[labour.id] = labour;
		store.commit("UpdateLabour", payload);
	}

	public static HasStartISO8601ForId(id: string | null): boolean {
		if (!id) {
			//console.error('Assignment.HasStartISO8601ForId !id', id);
			return false;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			//console.error('Assignment.HasStartISO8601ForId !assignment', assignment);
			return false;
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return assignment.json.hasStartISO8601;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return assignment.json.hasStartISO8601;
		}

		if (project.json.forceAssignmentsToUseProjectSchedule) {
			return project.json.hasStartISO8601;
		}

		return assignment.json.hasStartISO8601;
	}

	public static StartTimeModeForId(
		id: string | null
	):
		| "none"
		| "morning-first-thing"
		| "morning-second-thing"
		| "afternoon-first-thing"
		| "afternoon-second-thing"
		| "time" {
		if (!id) {
			console.error("Assignment.StartTimeModeForId !id", id);
			return "none";
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error(
				"Assignment.StartTimeModeForId !assignment",
				assignment
			);
			return "none";
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return assignment.json.startTimeMode;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return assignment.json.startTimeMode;
		}

		if (project.json.forceAssignmentsToUseProjectSchedule) {
			return project.json.startTimeMode;
		}

		return assignment.json.startTimeMode;
	}

	public static StartISO8601ForId(id: string | null): string | null {
		if (!id) {
			console.error("Assignment.StartISO8601ForId !id", id);
			return "none";
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error(
				"Assignment.StartISO8601ForId !assignment",
				assignment
			);
			return "none";
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return assignment.json.startISO8601;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return assignment.json.startISO8601;
		}

		if (project.json.forceAssignmentsToUseProjectSchedule) {
			if (project.json.hasStartISO8601 === false) {
				return null;
			}

			return project.json.startISO8601;
		}

		if (assignment.json.hasStartISO8601 === false) {
			return null;
		}

		return assignment.json.startISO8601;
	}

	public static HasEndISO8601ForId(id: string | null): boolean {
		if (!id) {
			console.error("Assignment.HasEndISO8601ForId !id", id);
			return false;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			//console.error('Assignment.HasEndISO8601ForId !assignment', assignment);
			return false;
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return assignment.json.hasEndISO8601;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return assignment.json.hasEndISO8601;
		}

		if (project.json.forceAssignmentsToUseProjectSchedule) {
			return project.json.hasEndISO8601;
		}

		return assignment.json.hasEndISO8601;
	}

	public static EndTimeModeForId(id: string | null): "none" | "time" {
		if (!id) {
			console.error("Assignment.EndTimeModeForId !id", id);
			return "none";
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error(
				"Assignment.EndTimeModeForId !assignment",
				assignment
			);
			return "none";
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return assignment.json.endTimeMode;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return assignment.json.endTimeMode;
		}

		if (project.json.forceAssignmentsToUseProjectSchedule) {
			return project.json.endTimeMode;
		}

		return assignment.json.endTimeMode;
	}

	public static EndISO8601ForId(id: string | null): string | null {
		if (!id) {
			console.error("Assignment.EndISO8601ForId !id", id);
			return "none";
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error("Assignment.EndISO8601ForId !assignment", assignment);
			return "none";
		}

		const projectId = assignment.json.projectId;
		if (!projectId) {
			return assignment.json.endISO8601;
		}

		const project = Project.ForId(projectId);
		if (!project) {
			return assignment.json.endISO8601;
		}

		if (project.json.forceAssignmentsToUseProjectSchedule) {
			return project.json.endISO8601;
		}

		return assignment.json.endISO8601;
	}

	public static RemoveAgentIdFromId(
		id: string | null,
		agentId: string | null
	): void {
		if (!id) {
			console.error("Assignment.AddAgentIdsToId !id");
			return;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error("Assignment.AddAgentIdsToId !assignment");
			return;
		}

		const agentIds = assignment.json.agentIds;

		const removeIdx = _.findIndex(agentIds, value => {
			return value === agentId;
		});

		console.log("removeIdx", removeIdx);

		if (removeIdx !== -1) {
			agentIds.splice(removeIdx, 1);

			const clone = _.cloneDeep(assignment) as IAssignment;
			clone.lastModifiedISO8601 = DateTime.utc().toISO();
			clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

			clone.json.agentIds = agentIds;

			// Save changes.
			const payload: Record<string, IAssignment> = {};
			payload[id] = clone;
			store.commit("UpdateAssignments", payload);
		}
	}

	public static AddAgentIdsToId(
		id: string | null,
		agentId: string | null
	): void {
		if (!id) {
			console.error("Assignment.AddAgentIdsToId !id");
			return;
		}

		const assignment = Assignment.ForId(id);
		if (!assignment) {
			console.error("Assignment.AddAgentIdsToId !assignment");
			return;
		}

		const agentIds = assignment.json.agentIds;

		// we allow multiple null values as that is considered "unassigned", and we don't want to loose those.
		if (agentId != null) {
			const existingElement = _.find(agentIds, value => {
				return value === agentId;
			});

			if (existingElement) {
				console.debug(
					"Agent already exists in list, not adding a second time."
				);
				return;
			}
		}

		agentIds.push(agentId);

		const clone = _.cloneDeep(assignment) as IAssignment;
		clone.lastModifiedISO8601 = DateTime.utc().toISO();
		clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

		clone.json.agentIds = agentIds;

		// Save changes.
		const payload: Record<string, IAssignment> = {};
		payload[id] = clone;
		store.commit("UpdateAssignments", payload);
	}

	public static UpdateStartTimeForId(
		id: string | null,
		params: {
			startTimeMode:
				| "none"
				| "morning-first-thing"
				| "morning-second-thing"
				| "afternoon-first-thing"
				| "afternoon-second-thing"
				| "time";
			hasStartISO8601: boolean;
			startISO8601: string | null;
		}
	): void {
		console.debug("Assignment.UpdateStartTimeForId", id, params);

		if (!id) {
			console.error("Assignment.UpdateAgentForId !id");
			return;
		}

		//console.debug('#');

		const assignment = Assignment.ForId(id);

		const clone = _.cloneDeep(assignment) as IAssignment;
		clone.lastModifiedISO8601 = DateTime.utc().toISO();
		clone.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

		clone.json.startTimeMode = params.startTimeMode;
		clone.json.hasStartISO8601 = params.hasStartISO8601;
		clone.json.startISO8601 = params.startISO8601;

		//console.debug('##');

		// Save changes.
		const payload: Record<string, IAssignment> = {};
		payload[id] = clone;
		store.commit("UpdateAssignments", payload);
	}

	public static PermAssignmentCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.assignments.push-any") !== -1 ||
			perms.indexOf("crm.assignments.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermAssignmentCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.assignments.request-any") !== -1 ||
			perms.indexOf("crm.assignments.request-company") !== -1 ||
			perms.indexOf("crm.assignments.request-self") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermAssignmentCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.assignments.delete-any") !== -1 ||
			perms.indexOf("crm.assignments.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMReportAssignmentPDF(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.report.assignments-pdf") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermCRMExportAssignmentCSV(): boolean {
		if (GetDemoMode()) {
			return true;
		}

		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret = perms.indexOf("crm.export.assignments-csv") !== -1;
		//console.log(perms, ret);
		return ret;
	}
}

export default {};
