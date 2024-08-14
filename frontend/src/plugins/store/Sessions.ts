import SignalRConnection from "@/RPC/SignalRConnection";
import GetDemoMode from "@/Utility/DataAccess/GetDemoMode";
import { GroupFetch } from "@/Data/GroupFetch/GroupFetch";
import { BillingContacts } from "@/Data/Billing/BillingContacts/BillingContacts";
import { BillingSessions } from "@/Data/Billing/BillingSessions/BillingSessions";
import PerformLocalLogout from "@/Utility/PerformLocalLogout";

export interface IGetNewSessionPayload {
	companyAbbreviation: string;
	contactEMail: string;
	contactPassword: string;
}
export interface IGetNewSessionQuery {
	companyAbbreviation: string;
	contactEMail: string;
	contactPassword: string;
	agentDescription: string;
	tzIANA: string;
	nocache: number;
}

export interface IGetContactForSession {
	session: string;
	tzIANA: string;
	nocache: number;
}

export default {
	state: {
		sessionId: ""
	},
	getters: {
		// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
		SessionId(state: any) {
			return state.sessionId;
		}
	},
	mutations: {
		// eslint-disable-next-line @typescript-eslint/explicit-module-boundary-types
		SetSession(state: any, payload: string) {
			state.sessionId = payload;

			SignalRConnection.Ready(() => {
				//console.log('demo mode', GetDemoMode());

				if (!GetDemoMode()) {
					//console.log('set session', payload);

					const billingContactsRTR = BillingContacts.RequestBillingContactsForCurrentSession.Send(
						{
							sessionId: BillingSessions.CurrentSessionId()
						}
					);
					if (billingContactsRTR.completeRequestPromise) {
						billingContactsRTR.completeRequestPromise.catch(
							(e: Error) => {
								console.error(
									"SetSession>RequestBillingContactsForCurrentSession error",
									e
								);

								PerformLocalLogout();
							}
						);
					}

					//console.log(billingContactsRTR);

					const billingSessionsRTR = BillingSessions.RequestBillingSessionsForCurrentSession.Send(
						{
							sessionId: BillingSessions.CurrentSessionId()
						}
					);
					if (billingSessionsRTR.completeRequestPromise) {
						billingSessionsRTR.completeRequestPromise.catch(
							(e: Error) => {
								console.error(
									"SetSession>RequestBillingSessionsForCurrentSession error",
									e
								);

								PerformLocalLogout();
							}
						);
					}

					//SignalRConnection.RequestDatabase();
					GroupFetch.RequestGroupGeneral.Send({
						sessionId: BillingSessions.CurrentSessionId()
					});
				}
			});
		}
	},
	actions: {
		DestroySession(): void {
			BillingSessions.PerformLogOutSession.Send({
				sessionId: BillingSessions.CurrentSessionId()
			});
		}
	}
};
