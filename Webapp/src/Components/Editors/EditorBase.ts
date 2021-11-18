import { Component, Vue, Prop  } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import { IJSONTable } from '@/Data/Models/JSONTable/JSONTable';

export interface IBreadcrumb {
	text: string;
	disabled: boolean;
	to: string;
}
export interface VForm extends Vue {
	validate(): boolean;
	resetValidation(): void;
}
@Component({
	components: {
		
	},
})
export default class EditorBase extends Vue {
	
	
	//this.$emit('input', null);
	@Prop({ default: null }) public readonly value!: IJSONTable | null;
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: null }) public readonly preselectTabName!: string | null;
	
	
	
	public tab = 0;
	public connectionMonitorInterval: ReturnType<typeof setTimeout> | null = null;
	// We put connected in here so it doesn't show the error by default.
	protected connectionStatus: string | null = 'Connected'; 
	
	
	public GetValidatedForms(): VForm[] {
		throw new Error('not subclassed');
	}
	
	public IsValidated(): boolean {
		
		for (const v of this.GetValidatedForms()) {
			if (v && !v.validate()) {
				return false;
			}
		}
		
		return true;
	}
	
	public ResetValidation(): void {
		
		const forms = this.GetValidatedForms();
		//console.log('forms', forms);
		
		for (const v of forms) {
			if (v) {
				v.resetValidation();
			}
		}
	}
	
	public SelectFirstTab(): void {
		this.tab = 0;
	}
	
	public mounted(): void {
		
		this.connectionMonitorInterval = setInterval(() => {
			Vue.set(this, 'connectionStatus', SignalRConnection.Connection.state);
		}, 1000);
		
		
		this.MountedAfter();
	}
	
	public destroyed(): void {
		
		if (this.connectionMonitorInterval) {
			clearInterval(this.connectionMonitorInterval);
			this.connectionMonitorInterval = null;
		}
		
		this.DestroyedAfter();
	}
	
	public SwitchToTabFromRoute(): void {
		
		//console.debug('SwitchToTabFromRoute');
		
		if (IsNullOrEmpty(this.preselectTabName)) {
			this.tab = 0;
		} else {
			const index = this.GetTabNameToIndexMap()[this.preselectTabName as string];
			this.tab = index;
		}
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {};
	}
	
	protected MountedAfter(): void {
		//
	}
	
	protected DestroyedAfter(): void {
		//
	}
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
}
