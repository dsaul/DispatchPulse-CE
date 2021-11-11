import { Component, Vue, Prop } from 'vue-property-decorator';

@Component({
	components: {
		
	},
})
export default class DialogueBase2 extends Vue {
	
	@Prop({ default: null }) public readonly value!: any | null;
	@Prop({ default: false }) public readonly isOpen!: boolean | null;
	@Prop({ default: 'Unnamed Dialogue' }) public readonly dialogueName!: string | null;
	
	public mounted(): void {
		this.SwitchToTabFromRoute();
	}
	
	public SwitchToTabFromRoute(): void {
		//
	}
}
