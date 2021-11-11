
import { ILabelledValue, LabelledValue } from '@/Data/Models/LabelledValue/LabelledValue';


export type IPhoneNumber = ILabelledValue;


export class PhoneNumber extends LabelledValue {
	
	
	public static ValidateObject(o: IPhoneNumber): IPhoneNumber {
		
		
		
		return o;
	}
	
	
}
