<template>
	<div>
		<v-list
			v-if="PermContactsCanRequest()"
			>
			<v-text-field 
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4 e2e-contact-list-filter"
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
					<ContactListItem 
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						v-model="PageRows[index]" 
						:showMenuButton="showMenuButton"
						:isDialogue="isDialogue"
						@ClickEntry="ClickEntry"
						@OpenEntry="OpenEntry"
						@DeleteEntry="DeleteEntry"
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
					{{emptyMessage}}
				</v-alert>
			</div>
			
		</v-list>
		<PermissionsDeniedAlert v-else />
	</div>
	
</template>

<script lang="ts">
import ContactListItem from '@/Components/ListItems/ContactListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import ListBase from './ListBase';
import { Contact, IContact } from '@/Data/CRM/Contact/Contact';
import { Company } from '@/Data/CRM/Company/Company';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		ContactListItem,
		PermissionsDeniedAlert,
	},
})
export default class ContactList extends ListBase {
	
	@Prop({ default: null }) public readonly showOnlyCompanyId!: string | null;
	
	@Prop({ default: 'There are no people to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	
	public $refs!: {
		filterField: Vue,
	};
	
	protected PermContactsCanRequest = Contact.PermContactsCanRequest;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.debug("ContactList LoadData()");
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					if (Contact.PermContactsCanRequest()) {
						const rtr = Contact.RequestContacts.Send({
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

	protected GetRawRows(): Record<string, IContact> {
		return this.$store.state.Database.contacts;
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IContact, key: string): boolean {
		let result = true;
		
		do {
			if (!o || !o.id || !o.json) {
				result = false;
				break;
			}
			
			if (null !== this.showOnlyCompanyId) {
				if (o.json.companyId !== this.showOnlyCompanyId) {
					result = false;
					break;
				}
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
			
			if (this.showFilter) {
				let haystack = '';
				haystack += o.json.name;
				haystack += o.json.title;
				
				const companyName = Company.NameForId(o.json.companyId);
				if (companyName) {
					haystack += companyName;
				}
				
				haystack += o.json.notes;
				
				for (const number of o.json.phoneNumbers) {
					haystack += number.value;
				}
				
				for (const email of o.json.emails) {
					haystack += email.value;
				}
				
				for (const address of o.json.addresses) {
					haystack += address.value;
				}
				
				
				
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
	
	protected RowSortBy(o: IContact): string {
		return (o.json.name || '1').toLowerCase();
	}
	
	protected GetEntryRouteForId(id: string): string {
		return `/section/contacts/${id}?tab=General`;
	}
	
	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteContactDialogue';
	}
	
	protected GetDeleteDialogueModelState(id: string): {
		redirectToIndex: boolean;
		id: guid;
	} | null {
		
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