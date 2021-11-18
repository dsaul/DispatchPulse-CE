<template>
	<div>
		<v-list
			v-if="PermProjectsCanRequest()"
			>
			<v-text-field
				v-if="showFilter"
				autocomplete="newpassword"
				class="mx-4 e2e-project-list-filter-field"
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
				<div class="text-center" v-if="showTopPagination === true">
					<v-pagination
						v-model="CurrentPage"
						:length="PageCount"
						:total-visible="breadcrumbsVisibleCount"
						>
					</v-pagination>
				</div>
				
				<v-list-item-group color="primary">
					<ProjectListItem 
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
				
					<div class="text-center">
						<v-pagination
							v-model="CurrentPage"
							:length="PageCount"
							:total-visible="breadcrumbsVisibleCount"
							>
						</v-pagination>
					</div>
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
						<!-- <content-placeholders-text :lines="3" /> -->
					</content-placeholders>
				</div>
				<v-alert
					v-else
					border="top"
					colored-border
					type="info"
					elevation="2"
					style="margin: 15px; padding-bottom: 10px;"
					>
					{{emptyMessage}}
				</v-alert>
			</div>
			
		</v-list>
		<PermissionsDeniedAlert v-else />
	</div>
</template>

<script lang="ts">

import { Component, Vue, Prop } from 'vue-property-decorator';
import ProjectListItem from '@/Components/ListItems/ProjectListItem.vue';
import _ from 'lodash';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ListBase from './ListBase';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { Company } from '@/Data/CRM/Company/Company';
import { IProjectStatus, ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { DateTime } from 'luxon';
import { Agent } from '@/Data/CRM/Agent/Agent';
import SignalRConnection from '@/RPC/SignalRConnection';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { guid } from '@/Utility/GlobalTypes';

@Component({
	components: {
		ProjectListItem,
		PermissionsDeniedAlert,
	},
})
export default class ProjectList extends ListBase {
	
	@Prop({ default: true }) public readonly showOpen!: boolean;
	@Prop({ default: true }) public readonly showAwaitingPayment!: boolean;
	@Prop({ default: true }) public readonly showClosed!: boolean;
	
	@Prop({ default: false }) public readonly isReverseSort!: boolean;
	
	@Prop({ default: null }) public readonly showOnlyContactId!: string | null;
	@Prop({ default: null }) public readonly showOnlyCompanyId!: string | null;
	@Prop({ default: null }) public readonly showOnlyChildrenOfProjectId!: string | null;
	
	@Prop({ default: null }) public readonly showOnlyAgentId!: string | null;
	@Prop({ default: false }) public readonly showOnlyUpcoming!: boolean;
	@Prop({ default: false }) public readonly showOnlyPastDue!: boolean;
	@Prop({ default: false }) public readonly showOnlyDueWithNoLabour!: boolean;
	
	@Prop({ default: () => [] }) public readonly excludeIds!: string[];
	
	@Prop({ default: 'There are no projects to show.' }) declare public readonly emptyMessage: string;
	
	public $refs!: {
		filterField: Vue,
	};
	
	protected PermProjectsCanRequest = Project.PermProjectsCanRequest;
	
	
	protected filter = '';
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.debug('ProjectList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					
					const promises: Array<Promise<any>> = [];
				
					const agentId = Agent.LoggedInAgentId();
					
					// Make sure we have the logged in agent loaded.
					if (null != agentId &&
						!IsNullOrEmpty(agentId) &&
						null == Agent.ForLoggedInAgent()) {
							
						const rtrAgents = Agent.FetchForId(agentId);
						if (rtrAgents.completeRequestPromise) {
							promises.push(rtrAgents.completeRequestPromise);
						}
						
					}
					
					if (Project.PermProjectsCanRequest()) {
						const rtrProjects = Project.RequestProjects.Send({
							sessionId: BillingSessions.CurrentSessionId(),
						});
						if (rtrProjects.completeRequestPromise) {
							promises.push(rtrProjects.completeRequestPromise);
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

	protected GetEntryRouteForId(id: string): string {
		return `/section/projects/${id}?tab=General`;
	}
	
	protected GetDeleteEntryDialogueName(): string {
		return 'DeleteProjectDialogue';
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
	
	
	protected GetRawRows(): Record<string, IProject> {
		return this.$store.state.Database.projects;
	}
	
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected RowFilter(o: IProject, key: string): boolean {
				
		// Get project status.
		
		//console.log(o);
		
		//console.log(this.$store.state.Database.projectStatus);
		if (!o || !o.id || !o.json) {
			return false;
		}
		
		
		
		let status: IProjectStatus | null = null;
		
		if (null != o.json.statusId) {
			
			status = ProjectStatus.ForId(o.json.statusId);
		}
		if (!status) {
			console.error('!status', o.json.statusId);
			return false;
		}
		
		
		
		
		let result = true;
		
		do {
			
			
			const isOpen = status.json.isOpen;
			const isClosed = status.json.isClosed;
			const isAwaitingPayment = status.json.isAwaitingPayment;
			
			if (status === null) {
				console.error('project has null status', o);
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
			
			if (this.showOnlyPastDue) {
				
				const hasEndISO8601 = o.json.hasEndISO8601;
				
				const endISO8601 = o.json.endISO8601;
				if (!hasEndISO8601 || !endISO8601 || IsNullOrEmpty(endISO8601)) {
					result = false;
					break;
				}
				
				const time = DateTime.fromISO(endISO8601).toLocal();
				const now = DateTime.local();
				
				//console.log('time, now', time, now);
				
				if (time > now) {
					result = false;
					break;
				}
				
			}
			
			if (this.showOnlyDueWithNoLabour) {
				
				const startISO8601 = o.json.startISO8601;
				if (!startISO8601 || IsNullOrEmpty(startISO8601)) {
					result = false;
					break;
				}
				
				const time = DateTime.fromISO(startISO8601).toLocal();
				const now = DateTime.local();
				
				
				if (time > now) { // not due
					result = false;
					break;
				}
				
				const labourEntries = Labour.ForProjectIds([o.id]);
				if (labourEntries.length > 0) {
					result = false;
					break;
				}
				
				
				
			}
			
			
			
			if (this.showOnlyUpcoming) {
				
				//Project.ForId()
				
				if (!o.json.startISO8601 || IsNullOrEmpty(o.json.startISO8601)) {
					result = false;
					break;
				}
				
				const dateStart = DateTime.fromISO(o.json.startISO8601);
				if (!dateStart) {
					result = false;
					break;
				}
				
				
				const dateStartLocal = dateStart.toLocal();
				
				const dateNow = DateTime.local();
				const dateNow1WeekLater = dateNow.plus({ weeks: 1 });
				
				
				
				if (dateStartLocal < dateNow) {
					result = false;
					break;
				}
				
				if (dateStartLocal > dateNow1WeekLater) {
					result = false;
					break;
				}
				
				//console.log('#!#', o, dateStartLocal, dateNow, dateNow1WeekLater);
			}
			
			
			if (null !== this.showOnlyContactId) {
				
				let contactExists = false;
				
				// Contact exists 
				for (const labelledContactId of o.json.contacts) {
					if (labelledContactId.value === this.showOnlyContactId) {
						contactExists = true;
						break;
					}
				}
				
				
				if (!contactExists) {
					result = false;
					break;
				}
			}
			
			if (null !== this.showOnlyCompanyId) {
				
				let companyExists = false;
				
				for (const labelledCompanyId of o.json.companies) {
					if (labelledCompanyId.value === this.showOnlyCompanyId) {
						companyExists = true;
						break;
					}
				}
				
				
				if (!companyExists) {
					result = false;
					break;
				}
			}
			
			if (null !== this.showOnlyChildrenOfProjectId) {

				if (o.json.parentId !== this.showOnlyChildrenOfProjectId) {
					result = false;
					break;
				}
			}
			
			if (null !== this.showOnlyAgentId) {
				
				do {
					let hasLabourEntry = false;
					const labourEntries = Labour.ForProjectIds([o.id]);
					
					for (const entry of labourEntries) {
						if (entry.json.agentId === this.showOnlyAgentId) {
							hasLabourEntry = true;
							break;
						}
					}
					if (hasLabourEntry) {
						break;
					}
					
					let hasAssignment = false;
					const assignmentEntries = Assignment.ForProjectIds([o.id]);
					for (const entry of assignmentEntries) {
						
						const located = _.find(entry.json.agentIds, (value) => {
							return value === this.showOnlyAgentId;
						});
						
						if (located) {
							hasAssignment = true;
							break;
						}
						
					}
					if (hasAssignment) {
						break;
					}
					
					result = false;
					
					
				} while (false);
				
				if (!result) {
					break;
				}
				
			}
			
			
			if (this.showOpen === false && isOpen === true) {
				result = false;
				break;
			}
			if (this.showClosed === false && isClosed === true) {
				result = false;
				break;
			}
			if (this.showAwaitingPayment === false && isAwaitingPayment === true) {
				result = false;
				break;
			}
			
			
			if (this.showFilter) {
				let haystack = '';
				
				for (const addr of o.json.addresses) {
					haystack += addr.value;
				}
				
				haystack += o.json.name;
				
				for (const row of o.json.contacts) {
					const id = row.value;
					const contact = Contact.ForId(id);
					if (contact) {
						haystack += contact.json.name;
					}
				}
				
				
				for (const labelledCompany of o.json.companies) {
					
					const companyId: string| null = labelledCompany.value;
					if (IsNullOrEmpty(companyId)) {
						continue;
					}
					
					if (null != companyId) {
						
						const company = Company.ForId(companyId);
						
						if (company) {
							haystack += ' ';
							haystack += company.json.name;
							haystack += ' ';
						}
						
					}
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
	
	
	
	protected RowSortBy(o: IProject): string {
		
		
		
		
		let haystack = '';
		haystack += o.lastModifiedISO8601 || '';
		for (const addr of o.json.addresses) {
			haystack += addr.value;
			haystack += ' ';
		}
		
		
		
		
		return haystack;
	}
	
	protected IsReverseSort(): boolean {
		return this.isReverseSort;
	}
	
	
}

</script>