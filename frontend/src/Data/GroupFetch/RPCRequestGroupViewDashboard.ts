import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IAgent } from '../CRM/Agent/Agent';
import { IAssignment } from '../CRM/Assignment/Assignment';
import { ILabour } from '../CRM/Labour/Labour';
import { IProject } from '../CRM/Project/Project';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestGroupViewDashboardPayload = IIdempotencyRequest
	
	export interface IRequestGroupViewDashboardCB extends IIdempotencyResponse {
		agents: Record<string, IAgent>;
		assignments: Record<string, IAssignment>;
		labour: Record<string, ILabour>;
		projects: Record<string, IProject>;
	}

export class RPCRequestGroupViewDashboard extends RPCMethod {
	public Send(payload: IRequestGroupViewDashboardPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestGroupViewDashboard';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestGroupViewDashboardCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestGroupViewDashboardCB): boolean {
		
		
		
		if (!payload.agents) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "dashboard", didn't recieve agents.`));
			}
			return false;
		}
	
		if (!payload.assignments) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "dashboard", didn't recieve assignments.`));
			}
			return false;
		}
	
		if (!payload.labour) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "dashboard", didn't recieve labour.`));
			}
			return false;
		}
	
		if (!payload.projects) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "dashboard", didn't recieve projects.`));
			}
			return false;
		}
	
		
		// Default action
		store.commit('UpdateAgentsRemote', payload.agents);
		store.commit('UpdateAssignmentsRemote', payload.assignments);
		store.commit('UpdateLabourRemote', payload.labour);
		store.commit('UpdateProjectsRemote', payload.projects);
		
		return true;
	}
}

export default {};
