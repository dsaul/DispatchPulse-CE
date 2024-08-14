<template>
	<div class="e2e-assignment-list-item">
		<v-snackbar v-model="InfoSnackbarVisible" color="info" :timeout="6000" :top="true">
			{{ InfoSnackbarText }}
			<template v-slot:action="{ attrs }">
				<v-btn v-bind="attrs" dark text @click="InfoSnackbarVisible = false">
					Close
				</v-btn>
			</template>
		</v-snackbar>
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>local_shipping</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">


				<v-list-item-title style="white-space: normal;" v-if="focusIsProject">
					<span v-if="Project && Project.json && Project.json.addresses">
						<span v-for="(addr, index) of Project.json.addresses" :key="addr.id">
							<span
								v-if="(index == Project.json.addresses.length - 1) && Project.json.addresses.length > 1">and
							</span>
							{{ addr.value }}<!--
							--><span v-if="!addr.value.endsWith('.')">.</span>
							<span
								v-if="(index != Project.json.addresses.length - 1) && Project.json.addresses.length > 1">,
							</span>

						</span>

					</span>
					<span v-else-if="!Project && loadingData" style="width: 50px;">
						<content-placeholders>
							<content-placeholders-text :lines="1" />
						</content-placeholders>
					</span>
					<span v-else>No Address</span>

					<span v-if="Project && Project.json && Project.json.name">
						{{ Project.json.name }}<!--
						--><span v-if="!Project.json.name.endsWith('.')">.</span>
					</span>
					<span v-else-if="!Project && loadingData" style="width: 50px;">
						<content-placeholders>
							<content-placeholders-text :lines="1" />
						</content-placeholders>
					</span>

					<span> {{ value.json.workRequested }} </span>
				</v-list-item-title>
				<v-list-item-title v-else-if="focusIsAgent">
					<span v-if="Agents && Agents.length > 0">
						{{ AgentDescriptionForAgentList(Agents) }}
					</span>
					<span v-else-if="(!Agents || Agents.length == 0) && loadingData">
						<content-placeholders>
							<content-placeholders-text :lines="1" />
						</content-placeholders>
					</span>
					<span v-else>
						Unassigned.
					</span>
					<span> {{ value.json.workRequested }} </span>
				</v-list-item-title>
				<v-list-item-title v-else>
					Unknown assignment focus.
				</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">




					<!-- Start -->
					<v-tooltip v-if="HasStart" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on">
								<v-avatar left>
									<v-icon small>fa-hourglass-start</v-icon>
								</v-avatar>
								<span v-if="value.json.desiredStartHasTime">{{ StartLocaleWithTime }}</span>
								<span v-else>{{ StartLocaleWithoutTime }}</span>
							</v-chip>
						</template>
						<span>Start</span>
					</v-tooltip>

					<!-- End -->
					<v-tooltip v-if="HasEnd" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on">
								<v-avatar left>
									<v-icon small>fa-hourglass-end</v-icon>
								</v-avatar>
								<span v-if="value.json.desiredEndHasTime">{{ EndLocaleWithTime }}</span>
								<span v-else>{{ EndLocaleWithoutTime }}</span>
							</v-chip>
						</template>
						<span>End</span>
					</v-tooltip>


					<!-- Agent Assigned -->
					<span v-if="Agents">
						<v-tooltip v-for="(agent, index) of Agents" :key="index" top>
							<template v-slot:activator="{ on }" v-on="on">
								<v-chip label outlined small style="margin-right: 5px;" v-on="on">
									<v-avatar left>
										<v-icon small>person</v-icon>
									</v-avatar>
									<span v-if="AgentNameForId(agent.id)">{{ AgentNameForId(agent.id) }}</span>
									<span v-else>Unnamed Agent</span>
								</v-chip>
							</template>
							<span>Agent</span>
						</v-tooltip>
					</span>
					<span v-else-if="(!Agents || Agents.length == 0) && loadingData">
						<content-placeholders>
							<content-placeholders-text :lines="1" />
						</content-placeholders>
					</span>

					<!-- Unassigned -->
					<v-tooltip v-if="!Agents || Agents.length == 0" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on" color="red">
								<v-avatar left>
									<v-icon small>person</v-icon>
								</v-avatar>
								Unassigned
							</v-chip>
						</template>
						<span>This assignment needs to be assigned to an agent.</span>
					</v-tooltip>

					<v-tooltip v-if="rootProject && rootProject.id !== value.json.projectId" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on" color="primary"
								@click.stop.prevent.once=""
								:to="`/section/projects/${value.json.projectId}?tab=General`">
								From a Child Project
							</v-chip>
						</template>
						<span>{{ ProjectNameForId(value.id) || 'No Name' }}</span>
					</v-tooltip>

				</v-list-item-subtitle>
			</v-list-item-content>

			<v-list-item-action v-if="showMenuButton">
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn icon v-on="on" :disabled="disabled">
							<v-icon>more_vert</v-icon>
						</v-btn>
					</template>

					<v-list dense>

						<v-list-item @click="StartTravel()" :disabled="disabled || !PermLabourCanPushSelf()">
							<v-list-item-icon>
								<v-icon>commute</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Start Travel Labour</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="StartOnSite()" :disabled="disabled || !PermLabourCanPushSelf()">
							<v-list-item-icon>
								<v-icon>build</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Start On-Site Labour</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="StartRemote()" :disabled="disabled || !PermLabourCanPushSelf()">
							<v-list-item-icon>
								<v-icon>business</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Start Remote Labour</v-list-item-title>
							</v-list-item-content>
						</v-list-item>


						<v-divider />

						<v-list-item :disabled="isDialogue || disabled || !PermAssignmentCanRequest()"
							@click="$emit('OpenEntry', value.id)">
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="$emit('DeleteEntry', value.id)"
							:disabled="disabled || !PermAssignmentCanDelete()">
							<v-list-item-icon>
								<v-icon>delete</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Delete…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
					</v-list>
				</v-menu>
			</v-list-item-action>
		</v-list-item>

	</div>
</template>

<script lang="ts">

import { Component, Prop } from 'vue-property-decorator';
import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ListItemBase from './ListItemBase';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import SignalRConnection from '@/RPC/SignalRConnection';
import { ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({

})
export default class AssignmentListItem extends ListItemBase {

	@Prop({ default: false }) public readonly focusIsProject!: boolean;
	@Prop({ default: false }) public readonly focusIsAgent!: boolean;
	@Prop({ default: null }) public readonly rootProject!: IProject | null;


	@Prop({ default: null }) declare public readonly value: IAssignment | null;


	protected AgentDescriptionForAgentList = Agent.DescriptionForAgentList;
	protected AgentNameForId = Agent.NameForId;
	protected ProjectNameForId = Project.NameForId;
	protected PermLabourCanPushSelf = Labour.PermLabourCanPushSelf;
	protected PermAssignmentCanRequest = Assignment.PermAssignmentCanRequest;
	protected PermAssignmentCanDelete = Assignment.PermAssignmentCanDelete;

	protected InfoSnackbarVisible = false;
	protected InfoSnackbarText = '';

	protected loadingData = false;

	public LoadData(): void {

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				if (this.value == null) {
					return;
				}

				const promises: Array<Promise<any>> = [];

				if (null != this.value.json.projectId && !IsNullOrEmpty(this.value.json.projectId)) {

					const project = Project.ForId(this.value.json.projectId);
					if (null == project && Project.PermProjectsCanRequest()) {

						const rtr = Project.FetchForId(this.value.json.projectId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}

					}
				}

				if (null != this.value.json.statusId && !IsNullOrEmpty(this.value.json.statusId)) {

					const status = ProjectStatus.ForId(this.value.json.statusId);
					if (null == status && ProjectStatus.PermProjectStatusCanRequest()) {

						const rtr = ProjectStatus.FetchForId(this.value.json.statusId);
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
						}

					}

				}

				if (null != this.value.json.agentIds && this.value.json.agentIds.length > 0) {

					const idsToRequest = [];

					for (const agentId of this.value.json.agentIds) {

						if (null == agentId) {
							continue;
						}

						const agent = Agent.ForId(agentId);
						if (null == agent) {
							idsToRequest.push(agentId);
						}

					}

					if (idsToRequest.length > 0 && Agent.PermAgentsCanRequest()) {

						const agentsRTR = Agent.RequestAgents.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToIds: idsToRequest,
						});
						if (agentsRTR.completeRequestPromise) {
							promises.push(agentsRTR.completeRequestPromise);
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








	}


	protected get Project(): IProject | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.projectId) {
			return null;
		}


		return Project.ForId(this.value.json.projectId);
	}

	protected get Agents(): IAgent[] {
		if (!this.value ||
			!this.value.id) {
			return [];
		}

		return Agent.ForAssignmentId(this.value.id);
	}



	protected get HasStart(): boolean {
		if (!this.value ||
			!this.value.id) {
			return false;
		}

		const startISO8601 = Assignment.StartISO8601ForId(this.value.id);

		if (typeof startISO8601 !== 'string') {
			return false;
		}

		return !IsNullOrEmpty(startISO8601);
	}

	protected get StartLocaleWithTime(): string | null {

		if (!this.value ||
			!this.value.id) {
			return null;
		}

		const iso8601 = Assignment.StartISO8601ForId(this.value.id);

		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}

		const utc = DateTime.fromISO(iso8601);
		const local = utc.toLocal();
		return local.toLocaleString(DateTime.DATETIME_FULL);
	}

	protected get StartLocaleWithoutTime(): string | null {
		if (!this.value ||
			!this.value.id) {
			return null;
		}

		const iso8601 = Assignment.StartISO8601ForId(this.value.id);

		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}

		const utc = DateTime.fromISO(iso8601);
		const local = utc.toLocal();
		return local.toLocaleString(DateTime.DATE_FULL);
	}

	protected get HasEnd(): boolean {
		if (!this.value ||
			!this.value.id) {
			return false;
		}

		const endISO8601 = Assignment.EndISO8601ForId(this.value.id);

		if (typeof endISO8601 !== 'string') {
			return false;
		}

		return !IsNullOrEmpty(endISO8601);
	}

	protected get EndLocaleWithTime(): string | null {
		if (!this.value ||
			!this.value.id) {
			return null;
		}

		const endISO8601 = Assignment.EndISO8601ForId(this.value.id);

		if (!endISO8601 || IsNullOrEmpty(endISO8601)) {
			return null;
		}

		const utc = DateTime.fromISO(endISO8601);
		const local = utc.toLocal();
		return local.toLocaleString(DateTime.DATETIME_FULL);
	}

	protected get EndLocaleWithoutTime(): string | null {
		if (!this.value ||
			!this.value.id) {
			return null;
		}

		const endISO8601 = Assignment.EndISO8601ForId(this.value.id);

		if (!endISO8601 || IsNullOrEmpty(endISO8601)) {
			return null;
		}

		const utc = DateTime.fromISO(endISO8601);
		const local = utc.toLocal();
		return local.toLocaleString(DateTime.DATE_FULL);
	}

	protected ClickEntry(): void {
		if (this.value && this.value.json && !IsNullOrEmpty(this.value.id)) {
			this.$emit('ClickEntry', this.value.id);
		}
	}

	protected StartTravel(): void {

		if (!this.value ||
			!this.value.id) {
			return;
		}

		const agentId = Agent.LoggedInAgentId();
		if (null != agentId &&
			!IsNullOrEmpty(agentId) &&
			null == Agent.ForLoggedInAgent()) {

			const rtrAgents = Agent.FetchForId(agentId);
			if (null != rtrAgents.completeRequestPromise) {
				rtrAgents.completeRequestPromise.then(() => {
					if (!this.value ||
						!this.value.id) {
						return;
					}

					Assignment.StartTravelForId(this.value.id);
				});
			}

		} else {
			Assignment.StartTravelForId(this.value.id);
		}




		this.InfoSnackbarText = 'Started Travel Timer';
		this.InfoSnackbarVisible = true;
	}

	protected StartOnSite(): void {

		if (!this.value ||
			!this.value.id) {
			return;
		}

		const agentId = Agent.LoggedInAgentId();
		if (null != agentId &&
			!IsNullOrEmpty(agentId) &&
			null == Agent.ForLoggedInAgent()) {

			const rtrAgents = Agent.FetchForId(agentId);
			if (null != rtrAgents.completeRequestPromise) {
				rtrAgents.completeRequestPromise.then(() => {
					if (!this.value ||
						!this.value.id) {
						return;
					}

					Assignment.StartOnSiteForId(this.value.id);
				});
			}

		} else {
			Assignment.StartOnSiteForId(this.value.id);
		}


		this.InfoSnackbarText = 'Started On Site Timer';
		this.InfoSnackbarVisible = true;
	}

	protected StartRemote(): void {

		if (!this.value ||
			!this.value.id) {
			return;
		}

		const agentId = Agent.LoggedInAgentId();
		if (null != agentId &&
			!IsNullOrEmpty(agentId) &&
			null == Agent.ForLoggedInAgent()) {

			const rtrAgents = Agent.FetchForId(agentId);
			if (null != rtrAgents.completeRequestPromise) {
				rtrAgents.completeRequestPromise.then(() => {
					if (!this.value ||
						!this.value.id) {
						return;
					}

					Assignment.StartRemoteForId(this.value.id);
				});
			}

		} else {
			Assignment.StartRemoteForId(this.value.id);
		}


		this.InfoSnackbarText = 'Started Remote Timer';
		this.InfoSnackbarVisible = true;

	}

}

</script>