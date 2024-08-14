<template>
	<div class="e2e-contact-list-item">
		<v-list-item>
			<v-list-item-avatar>
				<v-icon>person</v-icon>
			</v-list-item-avatar>

			<v-list-item-content @click="ClickEntry">
				<v-list-item-title style="white-space: normal;">{{ value.fullName }}</v-list-item-title>
				<v-list-item-subtitle style="width: 1px; /*to force flex to allow this to get smaller*/">

					<v-tooltip v-if="IsLicensedForProjectsSchedulingAndTime" top>

						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on">
								<v-icon x-small left>mdi-license</v-icon> Projects, Scheduling, and Time
							</v-chip>
						</template>
						<span>This account is licensed for "Projects, Scheduling, and Time".</span>
					</v-tooltip>

					<v-tooltip v-if="IsLicensedForOnCall" top>
						<template v-slot:activator="{ on }" v-on="on">
							<v-chip label outlined small style="margin-right: 5px;" v-on="on">
								<v-icon x-small left>mdi-license</v-icon> On Call Responder
							</v-chip>
						</template>
						<span>This account is licensed for "On Call Responder".</span>
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
						<v-list-item :disabled="isDialogue || disabled || IsOwnBillingContact"
							@click="$emit('edit-entry', value.uuid)">
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Edit…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item :disabled="isDialogue || disabled || IsOwnBillingContact"
							@click="$emit('edit-entry-permissions', value.uuid)">
							<v-list-item-icon>
								<v-icon>mdi-shield-lock</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Permissions…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item :disabled="isDialogue || disabled"
							@click="$emit('edit-entry-licenses', value.uuid)">
							<v-list-item-icon>
								<v-icon>mdi-license</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Licenses…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-list-item @click="$emit('delete-entry', value)" :disabled="disabled || IsOwnBillingContact">
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
import ListItemBase from './ListItemBase';
import { BillingContacts, IBillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({

})
export default class BilingContactListItem extends ListItemBase {

	@Prop({ default: null }) declare public readonly value: IBillingContacts | null;

	protected loadingData = false;


	public LoadData(): void {

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				if (this.value == null) {
					return;
				}

				const promises: Array<Promise<any>> = [];



				if (promises.length > 0) {
					Promise.all(promises).finally(() => {
						this.loadingData = false;
					});
				}

			});
		});


	}

	protected get IsLicensedForProjectsSchedulingAndTime(): boolean {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.licenseAssignedProjectsSchedulingTime) {
			return false;
		}

		return this.value.json.licenseAssignedProjectsSchedulingTime;
	}

	protected get IsLicensedForOnCall(): boolean {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.licenseAssignedOnCall) {
			return false;
		}

		return this.value.json.licenseAssignedOnCall;
	}

	protected get IsOwnBillingContact(): boolean {

		if (this.value === null) {
			return false;
		}

		return this.value.uuid === BillingContacts.CurrentBillingContactId();

	}


	protected ClickEntry(): void {
		this.$emit('ClickEntry', this.value);
	}

}

</script>