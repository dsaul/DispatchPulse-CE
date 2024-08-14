import { Component, Vue, Prop } from "vue-property-decorator";
import { IJSONTable } from "@/Data/Models/JSONTable/JSONTable";

@Component({
	components: {}
})
export default class ListItemBase extends Vue {
	//@ClickEntry
	//@OpenEntry
	//@DeleteEntry

	@Prop({ default: null }) public readonly value!: IJSONTable | null;

	@Prop({ default: false }) public readonly isPlaceholder!: boolean;
	@Prop({
		default: () => {
			"";
		}
	})
	public readonly placeholderValues!: { name: string }; // tslint:disable-line

	@Prop({ default: true }) public readonly showMenuButton!: boolean;
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: false }) public readonly disabled!: boolean;

	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;

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

		this.DestroyedAfter();
	}

	public LoadData(): void {
		//
	}

	public MountedAfter(): void {
		//
	}

	public DestroyedAfter(): void {
		//
	}

	public Periodic(): void {
		//
	}
}
