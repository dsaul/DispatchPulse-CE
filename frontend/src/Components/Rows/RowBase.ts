import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { ILabelledValue } from '@/Data/Models/LabelledValue/LabelledValue';

@Component({
	components: {
		
	},
})
export default class RowBase extends Vue {
	
	@Prop({ default: null }) public readonly value!: ILabelledValue | null;
	@Prop({ default: null }) public readonly index!: number | null;
	@Prop({ default: null }) public readonly isFirstIndex!: boolean | null;
	@Prop({ default: null }) public readonly isLastIndex!: boolean | null;
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: false }) public readonly disabled!: boolean;
	@Prop({ default: false }) public readonly readonly!: boolean;
	
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	//@Watch('value')
	//protected OnValueChanged(newVal: IPhoneNumber, oldVal: IPhoneNumber) {
	//	console.debug('PhoneNumberEditRow OnValueChanged() newVal:', newVal);
	//}
	
	protected get IsValueEmpty(): boolean {
		return IsNullOrEmpty(this.Value);
	}
	
	protected get Label(): string | null {
		if (this.value != null) {
			return this.value.label;
		}
		return null;
	}
	
	protected set Label(val: string | null) {
		if (this.value != null) {
			this.value.label = val;
			this.SignalChanged();
			return;
		}
		
	}
	
	protected get Value(): string | null {
		
		if (this.value != null) {
			return this.value.value;
		}
		return null;
	}
	
	protected set Value(val: string | null) {
		if (this.value != null) {
			this.value.value = val;
			this.SignalChanged();
			//console.log('setValue');
			return;
		}
		
	}
	
	
	
	protected SignalChanged(): void {
		
		// Debounce
		
		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}
		
		this.debounceId = setTimeout(() => {
			this.$emit('input', this.value);
		}, 250);
	}
}
