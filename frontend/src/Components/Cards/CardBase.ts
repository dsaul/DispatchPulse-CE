import { Component, Prop } from 'vue-property-decorator';

import ComponentBase from '@/Components/ComponentBase/ComponentBase';

@Component({
	components: {
		
	},
})
export default class CardBase extends ComponentBase {
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: false }) public readonly disabled!: boolean;
	
	private _periodicInterval: ReturnType<typeof setTimeout> | null = null;
	
	public mounted(): void {
		
		this.LoadData();
		this.MountedAfter();
		
		this.Periodic();
		this._periodicInterval = setInterval(this.Periodic, 1000);
	}
	
	public destroyed(): void {
		if (this._periodicInterval != null) {
			clearInterval(this._periodicInterval);
		}
	}
	
	
	
	public LoadData(): void {
		//
	}
	
	public MountedAfter(): void {
		//
	}
	
	public Periodic(): void {
		//
	}
	
	
	
}
