import _ from 'lodash';

export interface IRegisterAdditionalUser {
	id: string;
	fullName: string | null;
	email: string | null;
	phoneNumber: string | null;
	password1: string | null;
	password2: string | null;
	passwordHash: string | null;
}

export interface IRegisterDialogueModelState {
	currentDialogueStep: number;
	
	ip: string | null;
	currency: string | null;
	perUserCost: number | null;
	
	companyName: string | null;
	companyAbbreviation: string | null;
	companyAbbreviationInUse: boolean;
	companyIndustry: string | null;
	companyMarketingCampaign: string | null;
	companyAddressLine1: string | null;
	companyAddressLine2: string | null;
	companyAddressCity: string | null;
	companyAddressProvince: string | null;
	companyAddressPostalCode: string | null;
	companyAddressCountry: string | null;
	
	mainContactFullName: string | null;
	mainContactEMail: string | null;
	mainContactPhoneNumber: string | null;
	mainContactPassword1: string | null;
	mainContactPassword2: string | null;
	mainContactPasswordHash: string | null;
	mainContactEMailMarketing: boolean;
	mainContactEMailTutorials: boolean;
	
	otherAccountsToAdd: IRegisterAdditionalUser[];
	
	registeredBillingCompanyId: string | null;
	registeredBillingContactId: string | null;
	registeredBillingSessionId: string | null;
	
	stripeIntentClientSecret: string | null;
	
	billingFrequency: 'Monthly' | 'Quarterly' | 'Annually' | null;
	summaryRunningTotal: number;
}

export class RegisterDialogueModelState {
	
	public static GetMerged(mergeValues: Record<string, any>): IRegisterDialogueModelState {
		const ret = this.GetEmpty();
		_.merge(ret, mergeValues);
		return ret;
	}
	
	public static GetEmpty(): IRegisterDialogueModelState {
		const ret: IRegisterDialogueModelState = {
			currentDialogueStep: 1,
			
			ip: null,
			currency: null,
			perUserCost: null,
			
			companyName: null,
			companyAbbreviation: null,
			companyAbbreviationInUse: false,
			companyIndustry: null,
			companyMarketingCampaign: null,
			companyAddressLine1: null,
			companyAddressLine2: null,
			companyAddressCity: null,
			companyAddressProvince: null,
			companyAddressPostalCode: null,
			companyAddressCountry: null,
			
			mainContactFullName: null,
			mainContactEMail: null,
			mainContactPhoneNumber: null,
			mainContactPassword1: null,
			mainContactPassword2: null,
			mainContactPasswordHash: null,
			mainContactEMailMarketing: false,
			mainContactEMailTutorials: false,
			
			otherAccountsToAdd: [],
			
			registeredBillingCompanyId: null,
			registeredBillingContactId: null,
			registeredBillingSessionId: null,
			
			stripeIntentClientSecret: null,
			
			billingFrequency: 'Annually',
			
			summaryRunningTotal: 0,
		};
		
		return ret;
	}
	
}




 

export default {};

