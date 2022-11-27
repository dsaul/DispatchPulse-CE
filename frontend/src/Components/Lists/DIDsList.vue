<template>
	<div>
		<v-list
			v-if="PermDIDsCanRequest()"
			>
			<v-text-field 
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4 e2e-dids-list-filter"
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
					<DIDsListItem 
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						v-model="PageRows[index]" 
						:showMenuButton="showMenuButton"
						:isDialogue="isDialogue"
						@ClickEntry="ClickEntry"
						@OpenEntry="OpenEntry"
						@delete-entry="OpenDeleteDID"
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
					There are no phone numbers to show.
				</v-alert>
			</div>
			
		</v-list>
		<PermissionsDeniedAlert v-else />
		
		<DeleteDIDDialogue 
			v-model="deleteDIDModel"
			:isOpen="deleteDIDOpen"
			@Delete="SaveDeleteDID"
			@Cancel="CancelDeleteDID"
			ref="deleteDIDDialogue"
			/>
	</div>
</template>

<script lang="ts">

import DIDsListItem from '@/Components/ListItems/DIDsListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import ListBase from './ListBase';
import { DID, IDID } from '@/Data/CRM/DID/DID';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import DeleteDIDDialogue from '@/Components/Dialogues2/DIDs/DeleteDIDDialogue.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';


@Component({
	components: {
		DIDsListItem,
		PermissionsDeniedAlert,
		DeleteDIDDialogue,
	},
})
export default class DIDsList extends ListBase {
	
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	@Prop({ default: 'There are no phone numbers to show.' }) declare public readonly emptyMessage: string;
	
	public $refs!: {
		filterField: Vue,
		deleteDIDDialogue: DeleteDIDDialogue,
	};
	
	
	protected PermDIDsCanRequest = DID.PermDIDsCanRequest;
	
	protected deleteDIDModel: IDID | null = null;
	protected deleteDIDOpen = false;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		// console.debug('DIDsList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					if (DID.PermDIDsCanRequest()) {
						const rtr = DID.RequestDIDs.Send({
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

	protected GetRawRows(): Record<string, IDID> {
		return this.$store.state.Database.dids;
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IDID, key: string): boolean {
		
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
			
			haystack += o.json.DIDNumber;
			
			
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
	
	protected RowSortBy(o: IDID): string {
		return (o.json.DIDNumber || '1').toLowerCase();
	}
	
	protected GetEntryRouteForId(id: string): string {
		return `/section/dids/${id}?tab=General`;
	}
	
	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
	
	protected OpenDeleteDID(val: IDID): void {
		
		this.deleteDIDModel = val;
		this.deleteDIDOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.deleteDIDDialogue) {
				this.$refs.deleteDIDDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelDeleteDID(): void {
		
		this.deleteDIDOpen = false;
		
	}
	
	protected SaveDeleteDID(): void {
		
		if (!this.deleteDIDModel || !this.deleteDIDModel.id || IsNullOrEmpty(this.deleteDIDModel.id)) {
			console.error('!this.deleteDIDModel || !this.deleteDIDModel.id || IsNullOrEmpty(this.deleteDIDModel.id)');
			return;
		}
		
		const payload: string[] = [this.deleteDIDModel.id];
		DID.DeleteIds(payload);
		
		this.deleteDIDOpen = false;
		this.$router.push(`/section/dids/index`).catch(((e: Error) => { }));// eslint-disable-line
		
		
		
	}
}

</script>