import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ISO8601ToLocalDateOnly from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDateOnly';
import ISO8601ToLocalDatetime from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDatetime';

@Component({
	components: {
		
	},
})
export default class FieldBase extends Vue {
	
	@Prop({ default: 'Label' }) public readonly label!: string | null;
	@Prop({ default: null }) public readonly value!: string | null;
	@Prop({ default: () => [] }) public readonly rules!: Array<(v: string) => boolean | string>;
	@Prop({ default: false }) public readonly required!: boolean;
	@Prop({ default: null }) public readonly hint!: string | null;
	@Prop({ default: true }) public readonly showDetails!: boolean | null;
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: false }) public readonly disabled!: boolean;
	@Prop({ default: false }) public readonly readonly!: boolean;
	
	// When part of array adapters
	@Prop({ default: null }) public readonly index!: number | null;
	@Prop({ default: null }) public readonly isFirstIndex!: boolean | null;
	@Prop({ default: null }) public readonly isLastIndex!: boolean | null;
	
	
	protected selectDialogOpen = false;
	
	protected ISO8601ToLocalDateOnly = ISO8601ToLocalDateOnly;
	protected ISO8601ToLocalDatetime = ISO8601ToLocalDatetime;
	
	protected get IsValueEmpty(): boolean {
		return IsNullOrEmpty(this.value);
	}
	
	protected get ShowDetails(): boolean | null {
		return this.showDetails;
	}
	
	
	
	
}
