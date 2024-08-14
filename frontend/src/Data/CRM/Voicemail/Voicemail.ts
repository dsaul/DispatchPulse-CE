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
import { RPCRequestVoicemails } from "@/Data/CRM/Voicemail/RPCRequestVoicemails";
import { RPCDeleteVoicemails } from "@/Data/CRM/Voicemail/RPCDeleteVoicemails";
import { RPCPushVoicemails } from "@/Data/CRM/Voicemail/RPCPushVoicemails";
import { RPCPerformGetVoicemailRecordingLink } from "@/Data/CRM/Voicemail/RPCPerformGetVoicemailRecordingLink";
import { RPCPerformVoicemailMarkAsHandled } from "@/Data/CRM/Voicemail/RPCPerformVoicemailMarkAsHandled";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { IRoundTripRequest } from "@/RPC/SignalRConnection";

export interface IVoicemailTimelineItem {
	type: null | "text";
	timestampISO8601: null | string;
	description: null | string;
	colour: null | string;
}

export interface IVoicemail extends ICRMTable {
	json: {
		type: null | "OnCall";
		onCallAutoAttendantId: guid | null;
		lastModifiedBillingId: guid | null;
		messageLeftAtISO8601: string | null;
		callerIdNumber: string | null;
		callerIdName: string | null;
		callbackNumber: string | null;
		timeline: IVoicemailTimelineItem[];
		recordingsS3Bucket: string | null;
		recordingsS3Host: string | null;
		recordingsS3Key: string | null;
		recordingS3HttpsURI: string | null;
		recordingS3CmdURI: string | null;
		onCallAttemptsProgress: Array<{
			calendarId: guid | null;
			callAttempts: number | null;
			callAttemptsMax: number | null;
			callFiles: Array<{
				fileName: string | null;
				isPBXDone: boolean | null;
				isPBXError: boolean | null;
				originalCallFile: string | null;
				archivedCallFile: string | null;
			}>;
			sentMMS: boolean | null;
			sentEMail: boolean | null;
			givenUp: boolean | null;
		}>;
		noAgentResponseNotificationNumber: string | null;
		noAgentResponseNotificationEMail: string | null;
		markedHandledNotificationEMail: string | null;
		onCallAttemptsFinished: boolean | null;
		isMarkedHandled: boolean | null;
		markedHandledBy: string | null;
		nextAttemptAfterISO8601: string | null;
		minutesBetweenCallAttempts: number | null;
	};
}

export class Voicemail {
	// RPC Methods
	public static RequestVoicemails = RPCMethod.Register<RPCRequestVoicemails>(
		new RPCRequestVoicemails()
	);
	public static DeleteVoicemails = RPCMethod.Register<RPCDeleteVoicemails>(
		new RPCDeleteVoicemails()
	);
	public static PushVoicemails = RPCMethod.Register<RPCPushVoicemails>(
		new RPCPushVoicemails()
	);
	public static PerformGetVoicemailRecordingLink = RPCMethod.Register<
		RPCPerformGetVoicemailRecordingLink
	>(new RPCPerformGetVoicemailRecordingLink());
	public static PerformVoicemailMarkAsHandled = RPCMethod.Register<
		RPCPerformVoicemailMarkAsHandled
	>(new RPCPerformVoicemailMarkAsHandled());

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

		const aa = Voicemail.ForId(id);
		if (aa) {
			ret.outboundRequestPromise = Promise.resolve();
			ret.completeRequestPromise = Promise.resolve(aa);
			return ret;
		}

		// We'll need to request this.
		const rtrNew = Voicemail.RequestVoicemails.Send({
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
					ret._completeRequestPromiseResolve(Voicemail.ForId(id));
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

	public static ValidateObject(o: IVoicemail): IVoicemail {
		return o;
	}

	public static GetMerged(mergeValues: Record<string, any>): IVoicemail {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}

	public static GetEmpty(): IVoicemail {
		const id = GenerateID();
		const ret: IVoicemail = {
			id,
			json: {
				type: null,
				onCallAutoAttendantId: null,
				lastModifiedBillingId: null,
				messageLeftAtISO8601: null,
				callerIdNumber: null,
				callerIdName: null,
				callbackNumber: null,
				timeline: [],
				recordingsS3Bucket: null,
				recordingsS3Host: null,
				recordingsS3Key: null,
				recordingS3HttpsURI: null,
				recordingS3CmdURI: null,
				onCallAttemptsProgress: [],
				onCallAttemptsFinished: false,
				isMarkedHandled: false,
				noAgentResponseNotificationNumber: null,
				noAgentResponseNotificationEMail: null,
				markedHandledNotificationEMail: null,
				nextAttemptAfterISO8601: null,
				minutesBetweenCallAttempts: null,
				markedHandledBy: null
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO()
		};

		return ret;
	}

	public static ForId(id: string | null): IVoicemail | null {
		if (!id) {
			return null;
		}

		const autoAttendants = store.state.Database.voicemails as {
			[id: string]: IVoicemail;
		};
		if (!autoAttendants || Object.keys(autoAttendants).length === 0) {
			return null;
		}

		let aa = autoAttendants[id];
		if (!aa) {
			aa = CaseInsensitivePropertyGet(autoAttendants, id);
		}
		if (!aa) {
			return null;
		}

		return aa;
	}

	public static UpdateIds(payload: { [id: string]: IVoicemail }): void {
		store.commit("UpdateVoicemails", payload);
	}

	public static DeleteIds(ids: string[]): void {
		store.commit("DeleteVoicemails", ids);
	}

	public static PermVoicemailsCanRequest(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.voicemails.request-any") !== -1 ||
			perms.indexOf("crm.voicemails.request-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermVoicemailsCanPush(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.voicemails.push-any") !== -1 ||
			perms.indexOf("crm.voicemails.push-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static PermVoicemailsCanDelete(): boolean {
		if (GetDemoMode()) {
			return true;
		}
		const perms = BillingPermissionsBool.AllForBillingContactId();
		const ret =
			perms.indexOf("crm.voicemails.delete-any") !== -1 ||
			perms.indexOf("crm.voicemails.delete-company") !== -1;
		//console.log(perms, ret);
		return ret;
	}

	public static DisplayNameForId(id: string | null): string | null {
		const aa = Voicemail.ForId(id);
		if (!aa || !aa.json || !aa.json.messageLeftAtISO8601) {
			return null;
		}

		const dt = DateTime.fromISO(aa.json.messageLeftAtISO8601);

		return `${aa.json.callerIdNumber} at ${dt.toLocaleString(
			DateTime.DATETIME_MED
		)}`;
	}

	public static DisplayTypeForId(id: string | null): string | null {
		const aa = Voicemail.ForId(id);
		if (!aa || !aa.json || !aa.json.type) {
			return null;
		}

		switch (aa.json.type) {
			case "OnCall":
				return "On-Call";
			case null:
				return "Unknown";
		}
		return null;
	}

	public static DisplayCallerIdNameForId(id: string | null): string | null {
		const aa = Voicemail.ForId(id);
		if (
			!aa ||
			!aa.json ||
			!aa.json.callerIdName ||
			IsNullOrEmpty(aa.json.callerIdName)
		) {
			return null;
		}

		return aa.json.callerIdName;
	}

	public static DisplayCallerIdNumberForId(id: string | null): string | null {
		const aa = Voicemail.ForId(id);
		if (
			!aa ||
			!aa.json ||
			!aa.json.callerIdNumber ||
			IsNullOrEmpty(aa.json.callerIdNumber)
		) {
			return null;
		}

		return aa.json.callerIdNumber;
	}

	public static DisplayCallbackNumberForId(id: string | null): string | null {
		const aa = Voicemail.ForId(id);
		if (
			!aa ||
			!aa.json ||
			!aa.json.callbackNumber ||
			IsNullOrEmpty(aa.json.callbackNumber)
		) {
			return null;
		}

		return aa.json.callbackNumber;
	}
}

(window as any).DEBUG_Voicemail = Voicemail;

export default {};
