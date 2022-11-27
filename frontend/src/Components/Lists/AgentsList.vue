<template>
	<div>
		<v-list
			v-if="PermAgentsCanRequest()"
			>
			<v-text-field
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4 e2e-agent-list-filter"
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
					<div class="text-center">
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
								:disabled="disabled || !PermCRMLabourManualEntries()"
								>
								<v-icon v-text="row.trailingButtonIcon"></v-icon>
							</v-btn>
						</v-toolbar>
						<AgentsListItem
							v-else
							v-model="PageRows[index]" 
							:showMenuButton="showMenuButton"
							:isDialogue="isDialogue"
							@ClickEntry="ClickEntry"
							@OpenEntry="OpenEntry"
							@DeleteEntry="DeleteEntry"
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
import ListBase, { IListHeader } from './ListBase';
import AgentsListItem from '@/Components/ListItems/AgentsListItem.vue';
import ContactListItem from '@/Components/ListItems/ContactListItem.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { EmploymentStatus } from '@/Data/CRM/EmploymentStatus/EmploymentStatus';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		AgentsListItem,
		ContactListItem,
		PermissionsDeniedAlert,
	},
	
})
export default class AgentsList extends ListBase {
	
	@Prop({ default: 'There are no agents to show.' }) declare public readonly emptyMessage: string;
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	
	public $refs!: {
		filterField: Vue,
	};
	
	protected PermAgentsCanRequest = Agent.PermAgentsCanRequest;
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		// console.debug('AgentsList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
				
					if (Agent.PermAgentsCanRequest()) {
						const rtr = Agent.RequestAgents.Send({
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
	
	public InsertRowHeaders(rows: IAgent[] | IListHeader[]): void {
		
		let lastStatusId: guid | null = null;
		
		for (let i = 0; i < rows.length; i++) {
			
			const row = rows[i] as IAgent;
			
			if (!row.json.employmentStatusId) {
				continue;
			}
			
			
			
			//console.log('##', lastDate , startIso);
			
			if (lastStatusId !== row.json.employmentStatusId) {
				
				// Insert
				
				const status = EmploymentStatus.ForId(row.json.employmentStatusId);
				
				rows.splice(i, 0, { 
					isHeader: true,
					title: status?.json.name || '?',
					trailingButton: false,
					trailingButtonIcon: 'add',
					trailingButtonCallback: () => {
						//
					},
				} as IListHeader);
				
				lastStatusId = row.json.employmentStatusId;
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
	
	protected GetEntryRouteForId(id: string): string {
		return `/section/agents/${id}?tab=General`;
	}
	
	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteAgentDialogue';
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
	
	protected GetRawRows(): Record<string, IAgent> {
		return this.$store.state.Database.agents;
	}
	
	
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IAgent, key: string): boolean {
		
		
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
			haystack += o.json.title;
			
			
			const status = EmploymentStatus.ForId(o.json.employmentStatusId);
			if (status) {
				haystack += o.json.name;
			}
			
			haystack += ` $${o.json.hourlyWage}/h `;
			haystack += o.json.notificationSMSNumber;
			
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
	
	protected RowSortBy(o: IAgent): string {
		
		const agentName = (o.json.name || '1').toLowerCase();
		
		const stautsId = o.json.employmentStatusId;
		
		const status = EmploymentStatus.ForId(stautsId);
		
		
		
		return `${status?.json.name} ${agentName}`;
	}
	
	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
}

</script>