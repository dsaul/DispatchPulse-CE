import { Component, Vue, Prop } from "vue-property-decorator";
import _ from "lodash";
import ResizeObserver from "resize-observer-polyfill";
import Dialogues from "@/Utility/Dialogues";
import { ICRMTable } from "@/Data/Models/JSONTable/CRMTable";
import { guid } from "@/Utility/GlobalTypes";

export interface IListHeader {
	isHeader: boolean;
	title: string;
	trailingButton: boolean;
	trailingButtonIcon: string;
	trailingButtonCallback: () => void;
}

@Component({
	components: {}
})
export default class ListBase extends Vue {
	@Prop({ default: 10 }) public readonly rowsPerPage!: number;
	@Prop({ default: true }) public readonly showFilter!: boolean;
	@Prop({ default: true }) public readonly showTopPagination!: boolean;
	@Prop({ default: true }) public readonly showMenuButton!: boolean;
	@Prop({ default: true }) public readonly openEntryOnClick!: boolean;
	@Prop({ default: "There are no entries to show." })
	public readonly emptyMessage!: string;
	@Prop({ default: false }) public readonly isDialogue!: boolean;
	@Prop({ default: false }) public readonly disabled!: boolean;

	public CurrentPage = 1;

	protected breadcrumbsVisibleCount = 5;

	protected resizeObserver: ResizeObserver | null = null;
	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;

	protected get PageCount(): number {
		return Math.ceil(this.Rows.length / this.rowsPerPage);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	public InsertRowHeaders(rows: ICRMTable[] | IListHeader[]): void {
		//
	}

	public mounted(): void {
		this.LoadData();

		Vue.nextTick(() => {
			// eslint-disable-next-line @typescript-eslint/no-unused-vars
			this.resizeObserver = new ResizeObserver((entries, observer) => {
				if (window.matchMedia("(min-width: 1600px)").matches) {
					this.breadcrumbsVisibleCount = 26;
				} else if (window.matchMedia("(min-width: 1400px)").matches) {
					this.breadcrumbsVisibleCount = 22;
				} else if (window.matchMedia("(min-width: 1200px)").matches) {
					this.breadcrumbsVisibleCount = 18;
				} else if (window.matchMedia("(min-width: 1000px)").matches) {
					this.breadcrumbsVisibleCount = 14;
				} else if (window.matchMedia("(min-width: 800px)").matches) {
					this.breadcrumbsVisibleCount = 8;
				} else {
					this.breadcrumbsVisibleCount = 5;
				}
			});
			this.resizeObserver.observe(this.$el);
		});

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

	public beforeDestroy(): void {
		if (this.resizeObserver) {
			this.resizeObserver.unobserve(this.$el);
		}
	}

	public get PageRows(): ICRMTable[] {
		const start = this.rowsPerPage * (this.CurrentPage - 1);
		const end =
			this.rowsPerPage * (this.CurrentPage - 1) + this.rowsPerPage;

		const ret = this.Rows.slice(start, end) as ICRMTable[];

		//console.log('slice',start,end);

		this.InsertRowHeaders(ret);

		return ret;
	}

	public get Rows(): ICRMTable[] {
		//console.log('rows',this);

		const filtered = _.filter(
			this.GetRawRows(),
			(o: ICRMTable, key: string) => this.RowFilter(o, key)
		);

		//console.log(filtered);

		const sorted = _.sortBy(filtered, (o: any) => this.RowSortBy(o));

		if (this.IsReverseSort()) {
			sorted.reverse();
		}

		return sorted;
	}

	protected IsReverseSort(): boolean {
		return false;
	}

	// Methods that must be subclassed.
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: ICRMTable, key: string): boolean {
		console.error(
			"RowFilter is not subclassed! returning true for everything!"
		);
		return true;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowSortBy(
		o: string | number | boolean | symbol | ICRMTable
	): string | number | boolean | symbol | ICRMTable {
		console.error("RowSortBy is not subclassed! returning 1!");
		return 1;
	}

	protected GetEntryRouteForId(o: string): string {
		console.error("GetEntryRouteForId is not subclassed! returning /!");
		return `/?o=${o}`;
	}

	protected GetDeleteEntryDialogueName(): string {
		console.error(
			'GetDeleteEntryDialogueName is not subclassed! returning ""!'
		);
		return "";
	}

	protected GetRawRows(): { [id: string]: ICRMTable } {
		console.error("GetRawRows is not subclassed! returning empty array!");
		return {};
	}

	protected GetOpenAsDialogue(): boolean {
		return false;
	}

	protected GetOpenDialogueName(): string {
		console.error('GetOpenDialogueName is not subclassed! returning ""!');
		return "";
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected GetOpenDialogueModelState(id: string): ICRMTable | null {
		return null;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected GetDeleteDialogueModelState(
		id: string
	): {
		redirectToIndex: boolean;
		id: guid;
	} | null {
		return null;
	}

	protected ClickEntry(id: string): void {
		//console.debug('ClickEntry', id, this.openEntryOnClick);

		this.$emit("ClickEntry", id);

		if (this.openEntryOnClick) {
			this.OpenEntry(id);
		}
	}

	protected OpenEntry(id: string): void {
		//console.log('OpenEntry', id);

		if (this.GetOpenAsDialogue()) {
			Dialogues.Open({
				name: this.GetOpenDialogueName(),
				state: this.GetOpenDialogueModelState(id)
			});
		} else {
			this.$router.push(this.GetEntryRouteForId(id));
		}
	}

	protected DeleteEntry(id: string): void {
		const state = this.GetDeleteDialogueModelState(id);

		console.log(state);

		Dialogues.Open({
			name: this.GetDeleteEntryDialogueName(),
			state
		});
	}
}
