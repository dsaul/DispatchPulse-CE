import { Component, Vue, Prop } from 'vue-property-decorator';
import _ from 'lodash';
import ArrayMove from '@/Utility/ArrayMove';
import { ILabelledValue } from '@/Data/Models/LabelledValue/LabelledValue';

@Component({
	components: {
		
	},
})
export default class ArrayAdapterBase extends Vue {

	
	
	
	@Prop({ default: null }) public value!: any[] | null;
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: true}) public readonly showDetails!: boolean;
	@Prop({ default: false }) public readonly disabled!: boolean;
	@Prop({ default: false }) public readonly readonly!: boolean;
	
	//protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	protected GenerateEmptyRow(): ILabelledValue | null {
		console.error('GenerateEmptyRow not implemented');
		return null;
	}
	
	
	
	//public mounted(): void {
		//console.debug('PhoneNumberArrayEditRows mounted() value:', this.value);
	//}
	
	
	
	protected OnInsertNewRowAtIndex(newIndex: number): void {
		
		//console.log('OnInsertNewRowAtIndex ', newIndex, this.value);
		
		
		
		if (null !== this.value) {
			
			
			this.value.splice(newIndex, 0, this.GenerateEmptyRow());
			// this.SignalChanged();
			this.$emit('input', this.value);
		}
		
		
	}
	
	protected OnRemoveRowAtIndex(index: number): void {
		
		//console.debug('OnRemoveRowAtIndex', index);
		
		if (null !== this.value) {
			
			this.value.splice(index, 1);
			// this.SignalChanged();
			this.$emit('input', this.value);
		}
	}
	
	protected OnMoveUp(index: number): void {
		
		//console.debug('OnMoveUp', index, this.value);
		
		if (null !== this.value) {
			
			const clone = _.cloneDeep(this.value);
			ArrayMove(clone, index, index - 1);
			this.$emit('input', clone);
			
			
			
		}
		
	}
	
	protected OnMoveDown(index: number): void {
		
		//console.debug('OnMoveDown', index, this.value);
		
		if (null !== this.value) {
			
			const clone = _.cloneDeep(this.value);
			ArrayMove(clone, index, index + 1);
			this.$emit('input', clone);
			
		}
		
	}
	
	
	// @Watch('value')
	// protected OnValueChanged(newVal: IPhoneNumber[], oldVal: IPhoneNumber[]) {
	// 	console.debug('OnValueChanged OnValueChanged() newVal:', newVal);
	// }
	
	protected PostChanged(): void {
		//console.log('PostChanged()', this.value);
		// this.SignalChanged();
		this.$emit('input', this.value);
	}
	
	// protected SignalChanged(): void {
		
	// 	// Debounce
		
	// 	if (this.debounceId) {
	// 		clearTimeout(this.debounceId);
	// 		this.debounceId = null;
	// 	}
		
	// 	this.debounceId = setTimeout(() => {
	// 		this.$emit('input', this.value);
	// 	}, 250);
	// }
}
