<template>
	<div>
		<v-list
			v-if="PermOnCallAutoAttendantsCanRequest()"
			>
			<v-text-field 
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4 e2e-on-call-auto-attendants-list-filter"
				v-model="filter"
				hide-details
				label="Filter"
				prepend-inner-icon="search"
				solo
				style="margin-bottom: 10px;"
				ref="filterField"
				>
			</v-text-field>
			
			<div v-if="PageRows.length != 0">
				<template>
					<div class="text-center" v-if="showTopPagination === true">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							>
						</v-pagination>
					</div>
				</template>
				
				<v-list-item-group color="primary">
					<OnCallAutoAttendantsListItem 
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						v-model="PageRows[index]" 
						:showMenuButton="showMenuButton"
						:isDialogue="isDialogue"
						@ClickEntry="ClickEntry"
						@OpenEntry="OpenEntry"
						@delete-entry="OpenDeleteOnCallAutoAttendant"
						:disabled="disabled"
						/>
				</v-list-item-group>
				
				<template>
					<div class="text-center">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							>
						</v-pagination>
					</div>
				</template>
			</div>
			<div v-else>
				<div v-if="loadingData" style="margin-left: 20px; margin-right: 20px;">
					<content-placeholders>
						<content-placeholders-heading :img="true" />
						<content-placeholders-heading :img="true" />
						<content-placeholders-heading :img="true" />
						<content-placeholders-heading :img="true" />
						<content-placeholders-heading :img="true" />
						<content-placeholders-heading :img="true" />
						<content-placeholders-heading :img="true" />
						<!-- <content-placeholders-text :lines="3" /> -->
					</content-placeholders>
				</div>
				<v-alert
					v-else
					outlined
					type="info"
					elevation="0"
					style="margin-left: 15px; margin-right: 15px; margin-bottom: 0px;"
					>
					There are no On-Call Responders to show.
				</v-alert>
			</div>
			
		</v-list>
		<PermissionsDeniedAlert v-else />
		
		<DeleteOnCallAutoAttendantDialogue2
			v-model="deleteOnCallAutoAttendantModel"
			:isOpen="deleteOnCallAutoAttendantOpen"
			@Delete="SaveDeleteOnCallAutoAttendant"
			@Cancel="CancelDeleteOnCallAutoAttendant"
			ref="deleteOnCallAutoAttendantDialogue"
			/>
	</div>
</template>

<script lang="ts">

import OnCallAutoAttendantsListItem from '@/Components/ListItems/OnCallAutoAttendantsListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import ListBase from './ListBase';
import { IOnCallAutoAttendant, OnCallAutoAttendant } from '@/Data/CRM/OnCallAutoAttendant/OnCallAutoAttendant';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import DeleteOnCallAutoAttendantDialogue2 from '@/Components/Dialogues2/OnCallAutoAttendants/DeleteOnCallAutoAttendantDialogue2.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';


@Component({
	components: {
		OnCallAutoAttendantsListItem,
		PermissionsDeniedAlert,
		DeleteOnCallAutoAttendantDialogue2,
	},
})
export default class OnCallAutoAttendantsList extends ListBase {
	
	@Prop({ default: 'There are no auto attendants to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	
	public $refs!: {
		filterField: Vue,
		deleteOnCallAutoAttendantDialogue: DeleteOnCallAutoAttendantDialogue2,
	};
	
	
	protected PermOnCallAutoAttendantsCanRequest = OnCallAutoAttendant.PermOnCallAutoAttendantsCanRequest;
	
	protected deleteOnCallAutoAttendantModel: IOnCallAutoAttendant | null = null;
	protected deleteOnCallAutoAttendantOpen = false;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		// console.debug('OnCallAutoAttendantsList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					if (OnCallAutoAttendant.PermOnCallAutoAttendantsCanRequest()) {
						const rtr = OnCallAutoAttendant.RequestOnCallAutoAttendants.Send({
							sessionId: BillingSessions.CurrentSessionId(),
						});
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
					}
					
					
					if (promises.length > 0) {
						
						this.loadingData = true;
						
						Promise.all(promises).finally(() => {
							this.loadingData = false;
						});
					}
					
					
				});
			});
		
		}, 250);
		
	}
	
	
	public SelectFilterField(): void {
		//console.log('SelectFilterField()', this.$refs.filterField);
		if (this.$refs.filterField) {
			const input = this.$refs.filterField.$el.querySelector('input');
			if (input) {
				input.focus();
			}
		}
	}

	protected GetRawRows(): Record<string, IOnCallAutoAttendant> {
		return this.$store.state.Database.onCallAutoAttendants;
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IOnCallAutoAttendant, key: string): boolean {
		
		let result = true;
		
		do {
			if (!o || !o.id || !o.json) {
				result = false;
				break;
			}
			
			if (this.excludeIds && this.excludeIds.length > 0) {
				
				let isInExcludeList = false;
				
				for (const id of this.excludeIds) {
					if (null === id) {
						continue;
					}
					
					if (o.id === id) {
						isInExcludeList = true;
						break;
					}
				}
				
				if (isInExcludeList) {
					result = false;
					break;
				}
				
			}
			
			let haystack = '';
			
			haystack += o.json.name;
			
			
			haystack = haystack.replace(/\W/g, '');
			haystack = haystack.toLowerCase();
			
			
			let needle = this.filter.toLowerCase();
			needle = needle.replace(/\W/g, '');
			
			//console.log('haystack:',haystack,'needle:',needle);
			
			if (haystack.indexOf(needle) === -1) {
				result = false;
				break;
			}
			
		} while (false);
		
		return result;
	}
	
	protected RowSortBy(o: IOnCallAutoAttendant): string {
		return (o.json.name || '1').toLowerCase();
	}
	
	protected GetEntryRouteForId(id: string): string {
		return `/section/on-call/${id}?tab=General`;
	}
	

	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
	
	protected OpenDeleteOnCallAutoAttendant(val: IOnCallAutoAttendant): void {
		
		this.deleteOnCallAutoAttendantModel = val;
		this.deleteOnCallAutoAttendantOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.deleteOnCallAutoAttendantDialogue) {
				this.$refs.deleteOnCallAutoAttendantDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelDeleteOnCallAutoAttendant(): void {
		
		this.deleteOnCallAutoAttendantOpen = false;
		
	}
	
	protected SaveDeleteOnCallAutoAttendant(): void {
		
		if (!this.deleteOnCallAutoAttendantModel ||
			!this.deleteOnCallAutoAttendantModel.id ||
			IsNullOrEmpty(this.deleteOnCallAutoAttendantModel.id)) {
			console.error('SaveDeleteOnCallAutoAttendant error 1');
			return;
		}
		
		const payload: string[] = [this.deleteOnCallAutoAttendantModel.id];
		OnCallAutoAttendant.DeleteIds(payload);
		
		this.deleteOnCallAutoAttendantOpen = false;
		this.$router.push(`/section/on-call/index`).catch(((e: Error) => { }));// eslint-disable-line
		
		
		
	}
}

</script>