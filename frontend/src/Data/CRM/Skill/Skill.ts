import GenerateID from '@/Utility/GenerateID';
import { DateTime } from 'luxon';
import _ from 'lodash';
import store from '@/plugins/store/store';
import { RPCRequestSkills } from '@/Data/CRM/Skill/RPCRequestSkills';
import { RPCDeleteSkills } from '@/Data/CRM/Skill/RPCDeleteSkills';
import { RPCPushSkills } from '@/Data/CRM/Skill/RPCPushSkills';
import { RPCMethod } from '@/RPC/RPCMethod';
import { ICRMTable } from '@/Data/Models/JSONTable/CRMTable';

export interface ISkill extends ICRMTable {
	json: {
		lastModifiedBillingId: string | null;
	};
}

export class Skill {
	// RPC Methods
	public static RequestSkills = RPCMethod.Register<RPCRequestSkills>(new RPCRequestSkills());
	public static DeleteSkills = RPCMethod.Register<RPCDeleteSkills>(new RPCDeleteSkills());
	public static PushSkills = RPCMethod.Register<RPCPushSkills>(new RPCPushSkills());
	
	public static GetMerged(mergeValues: Record<string, any>): ISkill {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): ISkill {
		const id = GenerateID();
		const ret: ISkill = {
			id,
			json: {
				lastModifiedBillingId: null,
			},
			searchString: null,
			lastModifiedISO8601: DateTime.utc().toISO(),
		};
		
		return ret;
	}
	
	public static UpdateIds(payload: Record<string, ISkill>): void {
		store.commit('UpdateSkills', payload);
	}
	
	
	public static DeleteIds(ids: string[]): void {
		
		store.commit('DeleteSkills', ids);
		
	}
	
	public static ValidateObject(o: ISkill): ISkill {
		
		
		
		return o;
	}
	
}





export default {};

