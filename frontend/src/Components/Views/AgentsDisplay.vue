<template>

	<AgentEditor v-model="Agent" ref="editor" :showAppBar="true" :showFooter="true" :breadcrumbs="Breadcrumbs"
		:preselectTabName="$route.query.tab" :isMakingNew="false" :isLoadingData="loadingData" @reload="LoadData()" />

</template>

<script lang="ts">

import { Component } from 'vue-property-decorator';
import AgentEditor from '@/Components/Editors/AgentEditor.vue';
import _ from 'lodash';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { DateTime } from 'luxon';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AgentEditor,
	},
})
export default class AgentsDisplay extends ViewBase {

	public $refs!: {
		editor: AgentEditor,
	};

	protected loadingData = false;



	protected get Breadcrumbs(): Array<{
		text: string;
		disabled: boolean;
		to: string;
	}> {

		if (!this.Agent) {
			return [];
		}

		return [
			{
				text: 'Dashboard',
				disabled: false,
				to: '/',
			},
			{
				text: 'Agents Index',
				disabled: false,
				to: '/section/agents/index',
			},
			{
				text: this.AgentName,
				disabled: true,
				to: '',
			},
		];
	}

	public LoadData(): void {

		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				const rtr = Agent.FetchForId(this.$route.params.id);
				if (rtr.completeRequestPromise) {

					this.loadingData = true;

					rtr.completeRequestPromise.finally(() => {
						this.loadingData = false;
					});
				}

			});
		});
	}

	protected SwitchToTabFromRoute(): void {
		this.$refs.editor.SwitchToTabFromRoute();
	}

	protected MountedAfter(): void {

		if (!this.$route.params.id
			|| IsNullOrEmpty(this.$route.params.id)
		) {
			this.$router.push(`/section/agents/`).catch(((e: Error) => { }));// eslint-disable-line
		}

		this.SwitchToTabFromRoute();
		this.LoadData();
	}



	get AgentName(): string {

		const agent = this.Agent;

		//return 'asdasd';

		if (!agent) {
			return 'Agent';
		}

		if (IsNullOrEmpty(agent.json.name)) {
			return 'Agent';
		}

		return `Agent: ${(this.Agent as IAgent).json.name}`;
	}

	get Agent(): IAgent | null {
		return _.cloneDeep(Agent.ForId(this.$route.params.id)) as IAgent;
	}


	set Agent(val: IAgent | null) {
		if (!val) {
			return;
		}

		val.lastModifiedISO8601 = DateTime.utc().toISO();
		val.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

		const id = this.$route.params.id;
		if (!id || IsNullOrEmpty(id)) {
			console.error('!id || IsNullOrEmpty(id)');
			return;
		}

		const payload: Record<string, IAgent> = {};
		payload[id] = val;
		Agent.UpdateIds(payload);
	}

}
</script>
