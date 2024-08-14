import "@/Data/Models/LabelledValue/LabelledValue";
import {
	ILabelledValue,
	LabelledValue
} from "@/Data/Models/LabelledValue/LabelledValue";

export type IAddress = ILabelledValue;

export class Address extends LabelledValue {
	public static ValidateObject(o: IAddress): IAddress {
		return o;
	}
}
