<template>
	<div class="event-outer-padding" :style="{
		paddingTop: `${EventPaddingYPx}px`,
		paddingBottom: `${EventPaddingYPx}px`,
		paddingLeft: `${EventPaddingXPx}px`,
		paddingRight: `${EventPaddingXPx}px`,
		cursor: PermAssignmentCanPush() ? 'move' : 'default',
		}"
		@dblclick="OnDoubleClick"
		>
		<div class="event-base" :style="{
			width: `${EventWidthPx}px`,
			
			}">
			<div v-if="Headline" style="min-height:10px; background: #FFAA00; color: white; font-weight: bold; font-size: 10px; padding-left: 5px; padding-right: 5px;">
				{{Headline}}
			</div>
			<div v-if="ShowStartTimeInEvent" style="font-weight: bold; font-size: 12px; padding-left: 5px; padding-right: 5px;">
				{{StartTimeExactSmall}}
			</div>
			<div v-if="Addresses1Line && Addresses1Line.length > 0" style="font-weight: bold; font-size: 12px; padding-left: 5px; padding-right: 5px;">
				<div v-for="(address, index) of Addresses1Line" :key="index">
					{{address}}
				</div>
			</div>
			<div v-if="SharedAssignment" style=" font-size: 12px; padding-left: 5px; padding-right: 5px; font-style: italic;">
				{{SharedAssignment}}
			</div>
			<div v-if="ProjectName" style=" font-size: 12px; padding-left: 5px; padding-right: 5px;">
				{{ProjectName || 'No project name.'}}
			</div>
			<div v-if="AssignmentWorkDescriptionForId(value.assignment.id)" style=" font-size: 12px; padding-left: 5px; padding-right: 5px;">
				{{AssignmentWorkDescriptionForId(value.assignment.id)}}
			</div>
		</div>
	</div>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { DateTime } from 'luxon';
import { Project } from '@/Data/CRM/Project/Project';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Company } from '@/Data/CRM/Company/Company';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import { Agent } from '@/Data/CRM/Agent/Agent';
import _ from 'lodash';
import SignalRConnection from '@/RPC/SignalRConnection';
import { AssignmentStatus } from '@/Data/CRM/AssignmentStatus/AssignmentStatus';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { ISchedulerCellDragData } from '@/Data/Models/SchedulerCellDragData/SchedulerCellDragData';
import { IAddress } from '@/Data/Models/Address/Address';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';
import { ILabeledContactId } from '@/Data/Models/LabeledContactId/LabeledContactId';

@Component({
	components: {
	},
})
export default class SchedulerEvent extends Vue {
	
	@Prop({ default: null }) public readonly value!: ISchedulerCellDragData | null;
	@Prop({ default: null }) public readonly agentId!: string | null;
	
	@Prop({ default: 1 }) public readonly eventPaddingYPx!: number;
	@Prop({ default: 2 }) public readonly eventPaddingXPx!: number;
	
	@Prop({ default: false }) public readonly minimized!: boolean;
	@Prop({ default: 150 }) public readonly cellWidthPx!: number;
	@Prop({ default: 50 }) public readonly cellWidthMinimizedPx!: number;
	
	protected AssignmentWorkDescriptionForId = Assignment.WorkDescriptionForId;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	protected mounted(): void {
		this.LoadData();
	}
	
	
	protected LoadData(): void {
		
		//console.debug('LabourList LoadData()');
		
		
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					// Agents
					
					const agentsToRequest: string[] = [];
					if (null != this.agentId && !IsNullOrEmpty(this.agentId) && null == Agent.ForId(this.agentId)) {
						agentsToRequest.push(this.agentId);
					}
					
					if (this.value && this.value.assignment) {
						const assignment = this.value.assignment;
						for (const id of assignment.json.agentIds) {
							if (null != id && !IsNullOrEmpty(id) && null == Agent.ForId(id)) {
								agentsToRequest.push(id);
							}
						}
					}
					
					if (agentsToRequest.length > 0 && Agent.PermAgentsCanRequest()) {
						const rtr = Agent.RequestAgents.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToIds: agentsToRequest,
						});
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}
						
					}
					
					// Assignment Status
					if (this.value && this.value.assignment) {
						
						const assignment = this.value.assignment;
						
						const statusId = assignment.json.statusId;
						if (null != statusId &&
							!IsNullOrEmpty(statusId) &&
							null == AssignmentStatus.ForId(statusId) &&
							AssignmentStatus.PermAssignmentStatusCanRequest()
							) {
							const rtr = AssignmentStatus.FetchForId(statusId);
							if (rtr.completeRequestPromise) {
								promises.push(rtr.completeRequestPromise);
							}
							
						}
					}
					
					
					
					// Projects
					
					if (this.value && this.value.assignment) {
						
						const assignment = this.value.assignment;
						
						const projectId = assignment.json.projectId;
						if (null != projectId &&
							!IsNullOrEmpty(projectId) &&
							null == Project.ForId(projectId) &&
							Project.PermProjectsCanRequest()
							) {
							
							const projectsRtr = Project.FetchForId(projectId);
							if (projectsRtr.completeRequestPromise) {
								promises.push(projectsRtr.completeRequestPromise);
							}
							
							
							if (projectsRtr.completeRequestPromise) {
								projectsRtr.completeRequestPromise.then(() => {
									
									
									const project = Project.ForId(projectId);
									if (project) {
										
										//project companies
										const companies = project.json.companies;
										
										const companiesToFetch: string[] = [];
										
										for (const company of companies) {
											const companyId = company.value;
											
											if (null != companyId && !IsNullOrEmpty(companyId) && null == Company.ForId(companyId)) {
												companiesToFetch.push(companyId);
											}
										}
										
										if (Company.PermCompaniesCanRequest()) {
											const companyRtr = Company.RequestCompanies.Send({
												sessionId: BillingSessions.CurrentSessionId(),
												limitToIds: companiesToFetch,
											});
											if (companyRtr.completeRequestPromise) {
												promises.push(companyRtr.completeRequestPromise);
											}
										}
										
										//project contacts
										const contacts = project.json.contacts;
										
										const contactsToFetch: string[] = [];
										
										for (const contact of contacts) {
											const contactId = contact.value;
											
											if (null != contactId && !IsNullOrEmpty(contactId) && null == Company.ForId(contactId)) {
												contactsToFetch.push(contactId);
											}
										}
										
										if (Contact.PermContactsCanRequest()) {
											const contactsRtr = Contact.RequestContacts.Send({
												sessionId: BillingSessions.CurrentSessionId(),
												limitToIds: contactsToFetch,
											});
											if (contactsRtr.completeRequestPromise) {
												promises.push(contactsRtr.completeRequestPromise);
											}
										}
										
									}
									
									
									
								});
							}
							
							
							
							
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
	
	public get AssignmentId(): string | null {
		if (!this.value ||
			!this.value.assignment ||
			!this.value.assignment.id) {
			return null;
		}
		return this.value.assignment.id;
	}
	
	public get ProjectName(): string | null {
		
		
		
		if (!this.value ||
			!this.value.assignment || 
			!this.value.assignment.json) {
			return null;
		}
		
		
		const projectId = this.value.assignment.json.projectId;
		if (!projectId || IsNullOrEmpty(projectId)) {
			return null;
		}
		
		const project = Project.ForId(projectId);
		
		//const all = this.$store.state.Database.projects as Record<string, IProject>;
		
		if (!project ||
			!project.json ||
			!project.json.name ||
			IsNullOrEmpty(project.json.name)) {
			return null;
		}
		
		return project.json.name;
	}
	
	public get Addresses1Line(): string[] {
		
		const ret: string[] = [];
		if (!this.value?.assignment) {
			return ret;
		}
		
		const projectId = this.value.assignment.json.projectId;
		if (!projectId || IsNullOrEmpty(projectId)) {
			return ret;
		}
		
		const project = Project.ForId(projectId);
		if (!project) {
			return ret;
		}
		
		const addresses: IAddress[] = project.json.addresses;
		for (const addr of addresses) {
			
			let addrStr = addr.value;
			if (!addrStr) {
				continue;
			}
			
			addrStr = addrStr.trim();
			if (IsNullOrEmpty(addrStr)) {
				continue;
			}
			
			const firstLine = addrStr.split('\n', 1)[0];
			ret.push(firstLine);
		}
		
		
		return ret;
		
	}
	
	public get Headline(): string | null {
		
		if (!this.value ||
			!this.value.assignment) {
			return null;
		}
		
		// If there is a project, we show that as the headline.
		const projectId = this.value.assignment.json.projectId;
		const project = Project.ForId(projectId);
		
		
		if (projectId && !IsNullOrEmpty(projectId) && project) {
			
			// General Contractor first
			const companies: ILabeledCompanyId[] = project.json.companies;
						
			for (const company of companies) {
				
				const label = (company.label || '').trim();
				if (IsNullOrEmpty(label)) {
					continue;
				}
				
				if (0 === 'General Contractor'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true}) ||
					0 === 'Managing Company'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true}) || 
					0 === 'General'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true})
					) {
					
					const companyId = company.value;
					if (!companyId || IsNullOrEmpty(companyId)) {
						continue;
					}
					
					const companyName = Company.NameForId(companyId);
					if (IsNullOrEmpty(companyName)) {
						continue;
					}
					
					return companyName;
				}
				
				//if (company.label === '')
			}
			
			// Contacts second.
			const contacts: ILabeledContactId[] = project.json.contacts;
			
			for (const contact of contacts) {
				
				const label = (contact.label || '').trim();
				if (IsNullOrEmpty(label)) {
					continue;
				}
				
				if (0 === 'Homeowner'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true}) ||
					0 === 'Home Owner'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true}) || 
					0 === 'Contact'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true}) || 
					0 === 'Manager'.localeCompare(label, 'en', { sensitivity: 'base', ignorePunctuation: true}) 
					) {
					
					const contactId = contact.value;
					if (!contactId || IsNullOrEmpty(contactId)) {
						continue;
					}
					
					
					const contactName = Contact.NameForId(contactId);
					if (IsNullOrEmpty(contactName)) {
						continue;
					}
					
					return contactName;
				}
				
			}
			
			
		} else { // If there is no project...
			return 'No Project';
		}
		
		
		
		
		
		
		
		
		
		
		
		
		
		return null;
	}
	
	public get ShowStartTimeInEvent(): boolean {
		if (!this.value ||
			!this.value.assignment ||
			!this.value.assignment.id ||
			!this.value.assignment.json) {
			return false;
		}
		return Assignment.StartTimeModeForId(this.value.assignment.id) === 'time';
	}
	
	public get StartTimeExactSmall(): string | null {
		if (!this.value || 
			!this.value.assignment || 
			!this.value.assignment.id ||
			!this.value.assignment.json || 
			!this.value.assignment.json.startISO8601) {
			return null;
		}
		
		const iso8601 = Assignment.StartISO8601ForId(this.value.assignment.id) as string;
		
		return DateTime.fromISO(iso8601).toLocaleString(DateTime.TIME_SIMPLE);
	}
	
	public get SharedAssignment(): string | null {
		
		if (!this.value || 
			!this.value.assignment || 
			!this.value.assignment.json) {
			return null;
		}
		
		const agentIds = this.value.assignment.json.agentIds;
		
		
		// console.log('##', this.agentId, agentIds);
		
		const filtered = _.filter(agentIds, (value) => {
			
			return this.agentId !== value;
			
		});
		
		// console.log('###', filtered);
		
		if (!filtered || filtered.length === 0) {
			return null;
		}
		
		
		
		
		
		if (filtered.length > 0) { // must be undefined
			
			let ret = 'Shared with ';
			
			for (let i = 0; i < filtered.length; i++) {
				
				if (i !== 0) {
					ret += ', ';
				}
				if (i !== 0 && i === filtered.length - 1) {
					ret += 'and ';
				}
				
				const agent = Agent.ForId(filtered[i]);
				if (!agent) {
					ret += '"Unassigned"';
				} else {
					ret += Agent.NameForId(filtered[i]);
				}
				
				
				
			}
			
			ret += '.';
			
			
			
			
			return ret;
			
		}
		
		return null;
	}
	
	protected get EventPaddingYPx(): number {
		return this.eventPaddingYPx;
	}
	
	protected get EventPaddingXPx(): number {
		return this.eventPaddingXPx;
	}
	
	protected get EventWidthPx(): number {
		return (this.minimized ? this.cellWidthMinimizedPx : this.cellWidthPx) - (this.eventPaddingXPx * 2) - 1;
	}
	
	protected OnDoubleClick(): void {
		if (!this.value ||
			!this.value.assignment) {
			return;
		}
		this.$router.push(`/section/assignments/${this.value.assignment.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
	}
	
	
	
	
	
	
	
}
</script>
<style scoped>
div.event-base {
	background: #FFD300;
	border: 1px solid #FFAA00;
	border-top: 3px solid #FFAA00;
	border-radius: 2px;
	z-index: 10;
	
}

</style>