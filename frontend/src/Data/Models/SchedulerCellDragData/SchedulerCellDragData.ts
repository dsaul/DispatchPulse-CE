import { IAssignment } from '@/Data/CRM/Assignment/Assignment';
import _ from 'lodash';

export interface ISchedulerCellDragData {
	assignment: IAssignment | null;
	forAgentId: string | null;
}

export class SchedulerCellDragData {
	
	public static GetMerged(mergeValues: Record<string, any>): ISchedulerCellDragData {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ISchedulerCellDragData {
		const ret: ISchedulerCellDragData = {
			assignment: null,
			forAgentId: null,
		};
		
		return ret;
	}
	
}




 

export default {};

