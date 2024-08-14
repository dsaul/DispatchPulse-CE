import "@/Data/Models/LabelledValue/LabelledValue";
import {
	ILabelledValue,
	LabelledValue
} from "@/Data/Models/LabelledValue/LabelledValue";

export type IEMail = ILabelledValue;

export class EMail extends LabelledValue {
	public static ValidateObject(o: IEMail): IEMail {
		return o;
	}
}
