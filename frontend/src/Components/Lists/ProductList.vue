<template>
	<div>
		<v-list v-if="PermProductsCanRequest()">
			<v-text-field v-if="showFilter" autocomplete="newpassword" class="mx-4" v-model="filter" hide-details
				label="Filter" prepend-inner-icon="search" solo style="margin-bottom: 10px;" ref="filterField">
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
					<ProductsListItem v-for="(row, index) in PageRows" :key="row.id" v-model="PageRows[index]"
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
				<v-alert outlined type="info" elevation="0"
					style="margin-left: 15px; margin-right: 15px; margin-bottom: 0px;">
					{{ emptyMessage }}
				</v-alert>
			</div>

		</v-list>
		<PermissionsDeniedAlert v-else />
	</div>
</template>

<script lang="ts">

import ProductsListItem from '@/Components/ListItems/ProductsListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import ListBase from './ListBase';
import { IProduct, Product } from '@/Data/CRM/Product/Product';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		ProductsListItem,
		PermissionsDeniedAlert,
	},
})
export default class ProductsList extends ListBase {

	@Prop({ default: 'There are no products to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;

	public $refs!: {
		filterField: Vue,
	};

	protected PermProductsCanRequest = Product.PermProductsCanRequest;

	protected filter = '';

	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;

	public get IsLoadingData(): boolean {

		return this.loadingData;
	}

	public LoadData(): void {

		//console.debug('ProductsList LoadData()');

		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}

		this._LoadDataTimeout = setTimeout(() => {

			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {

					const promises: Array<Promise<any>> = [];

					if (Product.PermProductsCanRequest()) {
						const rtr = Product.RequestProducts.Send({
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

	protected GetOpenAsDialogue(): boolean {
		return true;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected GetEntryRouteForId(id: string): string {
		throw Error('not implemented');
		//return `/section/projects/${id}?tab=General`;
	}

	protected GetOpenDialogueName(): string {
		return 'EditProductDialogue';
	}

	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteProductDialogue';
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

	protected GetRawRows(): Record<string, IProduct> {
		return this.$store.state.Database.products;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IProduct, key: string): boolean {
		let result = true;

		do {

			if (!o || !o.id || !o.json) {
				result = false;
				break;
			}

			//if (this.showOnlyProjectId) {
			//	if (o.json.projectId !== this.showOnlyProjectId) {
			//		result = false;
			//		break;
			//	}
			//}

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

			if (this.showFilter) {
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
			}

		} while (false);

		return result;
	}

	protected RowSortBy(o: IProduct): string {

		return o.json.name || '1';

	}

	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
}

</script>