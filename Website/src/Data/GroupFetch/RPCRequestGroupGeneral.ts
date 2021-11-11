import { RPCMethod } from '@/RPC/RPCMethod';
import IIdempotencyResponse from '@/RPC/IIdempotencyResponse';
import IIdempotencyRequest from '@/RPC/IIdempotencyRequest';

import store from '@/plugins/store/store';
import { IAssignmentStatus } from '../CRM/AssignmentStatus/AssignmentStatus';
import { IEmploymentStatus } from '../CRM/EmploymentStatus/EmploymentStatus';
import { IEstimatingManHours } from '../CRM/EstimatingManHours/EstimatingManHours';
import { ILabourSubtypeHoliday } from '../CRM/LabourSubtypeHoliday/LabourSubtypeHoliday';
import { ILabourSubtypeException } from '../CRM/LabourSubtypeException/LabourSubtypeException';
import { ILabourSubtypeNonBillable } from '../CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';
import { ILabourType } from '../CRM/LabourType/LabourType';
import { IProjectStatus } from '../CRM/ProjectStatus/ProjectStatus';
import { ISettingsDefault } from '../CRM/SettingsDefault/SettingsDefault';
import { ISettingsProvisioning } from '../CRM/SettingsProvisioning/SettingsProvisioning';
import { ISettingsUser } from '../CRM/SettingsUser/SettingsUser';
import { IRoundTripRequest } from '@/RPC/SignalRConnection';

export type IRequestGroupGeneralPayload = IIdempotencyRequest
	
	export interface IRequestGroupGeneralCB extends IIdempotencyResponse {
		assignmentStatus: Record<string, IAssignmentStatus>;
		agentsEmploymentStatus: Record<string, IEmploymentStatus>;
		estimatingManHours: Record<string, IEstimatingManHours>;
		labourSubtypeHolidays: Record<string, ILabourSubtypeHoliday>;
		labourSubtypeException: Record<string, ILabourSubtypeException>;
		labourSubtypeNonBillable: Record<string, ILabourSubtypeNonBillable>;
		labourTypes: Record<string, ILabourType>;
		projectStatus: Record<string, IProjectStatus>;
		settingsDefault: Record<string, ISettingsDefault>;
		settingsProvisioning: Record<string, ISettingsProvisioning>;
		settingsUser: Record<string, ISettingsUser>;
	}

export class RPCRequestGroupGeneral extends RPCMethod {
	public Send(payload: IRequestGroupGeneralPayload): IRoundTripRequest {
		return super.Send(payload);
	}
	public GetServerMethodName(): string | null {
		return 'RequestGroupGeneral';
	}
	public GetClientCallbackMethodName(): string | null {
		return 'RequestGroupGeneralCB';
	}
	public RecieveDefaultAction(rtr: IRoundTripRequest, payload: IRequestGroupGeneralCB): boolean {
		
		if (!payload.assignmentStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve assignment status.`));
			}
			return false;
		}
	
		if (!payload.agentsEmploymentStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve employment status.`));
			}
			return false;
		}
	
		if (!payload.estimatingManHours) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve man hours.`));
			}
			return false;
		}
	
		if (!payload.labourSubtypeHolidays) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve labour subtype holidays.`));
			}
			return false;
		}
	
		if (!payload.labourSubtypeException) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve labour subtype exception.`));
			}
			return false;
		}
	
		if (!payload.labourSubtypeNonBillable) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve labour subtype non-billable.`));
			}
			return false;
		}
	
		if (!payload.labourTypes) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve labour types.`));
			}
			return false;
		}
	
		if (!payload.projectStatus) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve project status.`));
			}
			return false;
		}
	
		if (!payload.settingsDefault) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve settings default.`));
			}
			return false;
		}
	
	
		if (!payload.settingsProvisioning) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve settings provisioning.`));
			}
			return false;
		}
	
		if (!payload.settingsUser) {
			if (rtr && rtr._completeRequestPromiseReject) {
				rtr._completeRequestPromiseReject(
					new Error(`Error requesting group "general", didn't recieve settings user.`));
			}
			return false;
		}
	
	
	
	
	
	
	
		// Default action
		store.commit('UpdateAssignmentStatusRemote', payload.assignmentStatus);
		store.commit('UpdateAgentsEmploymentStatusRemote', payload.agentsEmploymentStatus);
		store.commit('UpdateEstimatingManHoursRemote', payload.estimatingManHours);
		store.commit('UpdateLabourSubtypeHolidaysRemote', payload.labourSubtypeHolidays);
		store.commit('UpdateLabourSubtypeExceptionRemote', payload.labourSubtypeException);
		store.commit('UpdateLabourSubtypeNonBillableRemote', payload.labourSubtypeNonBillable);
		store.commit('UpdateLabourTypesRemote', payload.labourTypes);
		store.commit('UpdateProjectStatusRemote', payload.projectStatus);
		store.commit('UpdateSettingsDefaultRemote', payload.settingsDefault);
		store.commit('UpdateSettingsProvisioningRemote', payload.settingsProvisioning);
		store.commit('UpdateSettingsUserRemote', payload.settingsUser);
		
		return true;
	}
}

export default {};
