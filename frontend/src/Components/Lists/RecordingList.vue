<template>
	<div>
		<v-list v-if="PermRecordingsCanRequest()">
			<v-text-field v-if="showFilter" autocomplete="newpassword" v-model="filter" hide-details label="Filter"
				prepend-inner-icon="search" solo style="margin-bottom: 10px;" ref="filterField">
			</v-text-field>

			<div v-if="PageRows.length != 0">
				<template>
					<div class="text-center" v-if="showTopPagination === true">
						<v-pagination v-model="CurrentPage" :length="PageCount"
							:total-visible="breadcrumbsVisibleCount">
						</v-pagination>
					</div>
				</template>

				<v-list-item-group color="primary">
					<RecordingListItem v-for="(row, index) in PageRows" :key="row.id" v-model="PageRows[index]"
						:showMenuButton="showMenuButton" :isDialogue="isDialogue" @ClickEntry="ClickEntry"
						@OpenEntry="OpenEntry" @DeleteEntry="DeleteEntry" :disabled="disabled" />
				</v-list-item-group>

				<template>
					<div class="text-center">
						<v-pagination v-model="CurrentPage" :length="PageCount"
							:total-visible="breadcrumbsVisibleCount">
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
				<v-alert v-else outlined type="info" elevation="0"
					style="margin-left: 15px; margin-right: 15px; margin-bottom: 0px;">
					There are no recordings to show.
				</v-alert>
			</div>

		</v-list>
		<PermissionsDeniedAlert v-else />


	</div>
</template>

<script lang="ts">

import RecordingListItem from '@/Components/ListItems/RecordingListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import ListBase from './ListBase';
import { IRecording, Recording } from '@/Data/CRM/Recording/Recording';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		RecordingListItem,
		PermissionsDeniedAlert,
	},
})
export default class RecordingList extends ListBase {

	@Prop({ default: 'There are no recordings to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;

	public $refs!: {
		filterField: Vue,
	};


	protected PermRecordingsCanRequest = Recording.PermRecordingsCanRequest;

	protected filter = '';

	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;

	public get IsLoadingData(): boolean {

		return this.loadingData;
	}

	public LoadData(): void {

		// console.debug('RecordingList LoadData()');

		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}

		this._LoadDataTimeout = setTimeout(() => {

			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {

					const promises: Array<Promise<any>> = [];

					if (Recording.PermRecordingsCanRequest()) {
						const rtr = Recording.RequestRecordings.Send({
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

	protected GetRawRows(): Record<string, IRecording> {
		return this.$store.state.Database.recordings;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IRecording, key: string): boolean {

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

	protected RowSortBy(o: IRecording): string {
		return (o.json.name || '1').toLowerCase();
	}

	protected GetEntryRouteForId(id: string): string {
		return `/section/recordings/${id}?tab=General`;
	}

	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteRecordingDialogue';
	}

	protected GetDeleteDialogueModelState(id: string): {
		redirectToIndex: boolean;
		id: guid;
	} {

		return {
			redirectToIndex: false,
			id,
		};
	}

	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
}

</script>