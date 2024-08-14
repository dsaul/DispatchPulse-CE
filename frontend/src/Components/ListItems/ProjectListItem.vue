<template>
	<div class="e2e-project-list-item">
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
				<v-icon>person</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry" style="overflow:hidden;">
				<v-list-item-title style="white-space: normal;">
					<span v-if="ProjectAddressForId(value.id)">
						{{ ProjectAddressForId(value.id) }}
					</span>
					<span v-else>No Address</span>
				</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">

					<v-tooltip v-if="value.json.name" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on">
								<!--<v-avatar left>
									<v-icon small>domain</v-icon>
								</v-avatar>-->
								{{ value.json.name }}
							</v-chip>
						</template>
						<span>Name</span>
					</v-tooltip>

					<v-tooltip v-if="FormatCompanyListDescription(value.json.companies)" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on">
								<v-avatar left>
									<v-icon small>domain</v-icon>
								</v-avatar>
								{{ FormatCompanyListDescription(value.json.companies) }}
							</v-chip>
						</template>
						<span>Company Name</span>
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

						<v-list-item :disabled="isDialogue || disabled" @click="$emit('OpenEntry', value.id)">
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="$emit('DeleteEntry', value.id)"
							:disabled="disabled || !PermProjectsCanDelete()">
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
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ListItemBase from './ListItemBase';
import { Company } from '@/Data/CRM/Company/Company';
import { IProject, Project } from '@/Data/CRM/Project/Project';
import SignalRConnection from '@/RPC/SignalRConnection';
import { ProjectStatus } from '@/Data/CRM/ProjectStatus/ProjectStatus';
import { Contact } from '@/Data/CRM/Contact/Contact';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { Labour } from '@/Data/CRM/Labour/Labour';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({

})
export default class ProjectListItem extends ListItemBase {

	@Prop({ default: null }) declare public readonly value: IProject;

	protected FormatCompanyListDescription = Company.CompanyListDescriptionForIds;
	protected ProjectAddressForId = Project.AddressForId;
	protected PermLabourCanPushSelf = Labour.PermLabourCanPushSelf;
	protected PermProjectsCanDelete = Project.PermProjectsCanDelete;

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

				if (null != this.value.json.parentId &&
					!IsNullOrEmpty(this.value.json.parentId) &&
					this.value.json.parentId.indexOf('NULL_SENTINEL') === -1
				) {

					const project = Project.ForId(this.value.json.parentId);
					if (null == project && Project.PermProjectsCanRequest()) {

						const rtr = Project.FetchForId(this.value.json.parentId);
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

				if (null != this.value.json.contacts && this.value.json.contacts.length > 0) {


					const idsToRequest: string[] = [];

					for (const row of this.value.json.contacts) {

						if (null == row || null == row.value || IsNullOrEmpty(row.value)) {
							continue;
						}

						const contact = Contact.ForId(row.value);
						if (null == contact) {
							idsToRequest.push(row.value);
						}

					}

					if (idsToRequest.length > 0 && Contact.PermContactsCanRequest()) {


						const contactRTR = Contact.RequestContacts.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToIds: idsToRequest,
						});
						if (contactRTR.completeRequestPromise) {
							promises.push(contactRTR.completeRequestPromise);
						}


					}


				}


				if (null != this.value.json.companies && this.value.json.companies.length > 0) {


					const idsToRequest: string[] = [];

					for (const row of this.value.json.companies) {

						if (null == row || null == row.value || IsNullOrEmpty(row.value)) {
							continue;
						}

						const company = Company.ForId(row.value);
						if (null == company) {
							idsToRequest.push(row.value);
						}

					}

					if (idsToRequest.length > 0 && Company.PermCompaniesCanRequest()) {

						const companyRTR = Company.RequestCompanies.Send({
							sessionId: BillingSessions.CurrentSessionId(),
							limitToIds: idsToRequest,
						});
						if (companyRTR.completeRequestPromise) {
							promises.push(companyRTR.completeRequestPromise);
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

					Project.StartTravelForId(this.value.id);
				});
			}

		} else {
			Project.StartTravelForId(this.value.id);
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

					Project.StartOnSiteForId(this.value.id);
				});
			}

		} else {
			Project.StartOnSiteForId(this.value.id);
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

					Project.StartRemoteForId(this.value.id);
				});
			}

		} else {
			Project.StartRemoteForId(this.value.id);
		}


		this.InfoSnackbarText = 'Started Remote Timer';
		this.InfoSnackbarVisible = true;

	}
}

</script>