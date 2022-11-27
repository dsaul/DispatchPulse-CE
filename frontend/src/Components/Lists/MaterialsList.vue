<template>
	<div>
		<v-list
			v-if="PermMaterialsCanRequest()"
			>
			<v-text-field
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4 e2e-materials-list-filter"
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
					<div
						v-for="(row, index) in PageRows" 
						:key="row.id" 
						>
						<v-toolbar
							v-if="row.isHeader"
							dense
							style="margin-left: 20px; margin-right: 20px;margin-top: 10px;margin-bottom: 10px;"
							elevation="1"
							color="rgb(116, 115, 137)"
							short
							dark
							>
							<v-toolbar-title dense v-text="row.title"></v-toolbar-title>
							<v-spacer></v-spacer>
							<v-btn
								v-if="row.trailingButton"
								icon
								@click="row.trailingButtonCallback"
								:disabled="disabled"
								>
								<v-icon v-text="row.trailingButtonIcon"></v-icon>
							</v-btn>
						</v-toolbar>
						<MaterialsListItem 
							v-else
							v-model="PageRows[index]" 
							:showMenuButton="showMenuButton"
							:isDialogue="isDialogue"
							@ClickEntry="ClickEntry"
							@OpenEntry="OpenEntry"
							@DeleteEntry="DeleteEntry"
							:rootProject="rootProject"
							:disabled="disabled"
							/>
					</div>
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
				<v-alert
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
import Dialogues from '@/Utility/Dialogues';
import MaterialsListItem from '@/Components/ListItems/MaterialsListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import _ from 'lodash';
import ListBase, { IListHeader } from './ListBase';
import { DateTime } from 'luxon';
import { IMaterial, Material } from '@/Data/CRM/Material/Material';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { Product } from '@/Data/CRM/Product/Product';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		MaterialsListItem,
		PermissionsDeniedAlert,
	},
})
export default class MaterialsList extends ListBase {
	
	@Prop({ default: null }) public readonly showOnlyProjectId!: string;
	@Prop({ default: 'There are no materials to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: null }) public readonly prefillProjectId!: string | null;
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	@Prop({ default: false }) public readonly showChildrenOfProjectIdAsWell!: boolean;
	@Prop({ default: null }) public readonly rootProject!: IProject;
	
	public $refs!: {
		filterField: Vue,
	};
	
	protected PermMaterialsCanRequest = Material.PermMaterialsCanRequest;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.debug('MaterialsList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					if (Material.PermMaterialsCanRequest()) {
						const rtr = Material.RequestMaterials.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToProjectId: this.showOnlyProjectId,
							showChildrenOfProjectIdAsWell: this.showChildrenOfProjectIdAsWell,
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
	
	public InsertRowHeaders(rows: IMaterial[] | IListHeader[]): void {
		
		let lastDate: string | null = null;
		
		for (let i = 0; i < rows.length; i++) {
			
			const row = rows[i] as IMaterial;
			
			if (!row.json.dateUsedISO8601) {
				continue;
			}
			
			const startIsoStr = row.json.dateUsedISO8601;
			const startIso = DateTime.fromISO(startIsoStr);
			const startLocal = startIso.toLocal();
			const date = startLocal.toLocaleString(DateTime.DATE_MED);
			
			//console.log('##', lastDate , startIso);
			
			if (lastDate !== date) {
				
				// Insert
				
				rows.splice(i, 0, { 
					isHeader: true,
					title: date,
					trailingButton: true,
					trailingButtonIcon: 'add',
					trailingButtonCallback: () => {
						
						Dialogues.Open({ name: 'ModifyMaterialDialogue', state: {
							json: {
								agentId: Agent.LoggedInAgentId(),
								projectId: this.prefillProjectId,
								dateUsedISO8601: row.json.dateUsedISO8601,
							},
						}});
						
					},
				} as IListHeader);
				
				lastDate = date;
				i++;
			}
			
			//console.debug('obj', rows[i]);
			
		}
		
		
		
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
		return 'ModifyMaterialDialogue';
	}
	
	protected GetOpenDialogueModelState(id: string): IMaterial | null {
		
		const material = Material.ForId(id);
		
		const clone = _.cloneDeep(material);
		return clone;
	}
	
	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteMaterialDialogue';
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
	
	protected GetRawRows(): Record<string, IMaterial> {
		return this.$store.state.Database.materials;
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IMaterial, key: string): boolean {
		let result = true;
		
		do {
			if (!o || !o.id || !o.json) {
				result = false;
				break;
			}
			
			if (this.showOnlyProjectId) {
				
				let projects = [];
				
				const materialEntryProject = Project.ForId(o.json.projectId);
				const showOnlyProject = Project.ForId(this.showOnlyProjectId);
				
				
				if (this.showChildrenOfProjectIdAsWell) {
					
					projects = Project.RecursiveChildProjectsOfId(this.showOnlyProjectId);
				} else {
					
					
					if (showOnlyProject) {
						projects.push(showOnlyProject);
					}
					
				}
				
				const found = !!_.find(projects, (value) => {
					return materialEntryProject?.id === value.id;
				});
				
				if (!found) {
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
				
				
				
				// Billing Full Name 
				if (o.json.lastModifiedBillingId) {
					
					const name = BillingContacts.NameForCurrentSession();
					haystack += name;
				}
				
				if (o.json.dateUsedISO8601) {
					haystack += DateTime.fromISO(o.json.dateUsedISO8601).toLocaleString(DateTime.DATETIME_FULL);
				}
				
				const projectName = Project.NameForId(o.json.projectId);
				if (projectName) {
					haystack += ` ${projectName} `;
				}
				
				const projectDescription = Project.CombinedDescriptionForId(o.json.projectId);
				if (projectDescription) {
					haystack += ` ${projectDescription} `;
				}
				
				
				
				haystack += ` ${o.json.quantity} ${o.json.quantityUnit} `;
				
				const productName = Product.NameForId(o.json.productId);
				if (productName) {
					haystack += ` ${productName} `;
				}
				
				haystack += o.json.isExtra ? 'Extra ' : '';
				haystack += o.json.isBilled ? 'Billed ' : '';
				
				haystack += o.json.location;
				
				haystack += o.json.notes;
				
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
	
	protected RowSortBy(o: IMaterial): string {
		
		return o.json.dateUsedISO8601 || '1';
		
	}
	
	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
}

</script>