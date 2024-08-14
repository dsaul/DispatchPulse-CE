<template>
	<v-card class="" style="margin: 30px;">
		<v-progress-linear v-if="loadingData" indeterminate></v-progress-linear>
		<v-card-title>My Labour Entries, Active and Today</v-card-title>
		<v-card-text v-if="!agentId">
			<v-alert type="warning" colored-border elevation="2" border="bottom">
				You need an agent assigned in settings to be able to see this.
			</v-alert>
		</v-card-text>
		<div v-else>
			<div v-if="null == agent && loadingData" style="margin-left: 20px; margin-right: 20px;">
				<content-placeholders>
					<content-placeholders-heading :img="true" />
					<!-- <content-placeholders-text :lines="3" /> -->
				</content-placeholders>
			</div>
			<div v-else>
				<LabourList ref="labourList" :showTopPagination="false" :showFilter="false" :openEntryOnClick="false"
					:rowsPerPage="5" :showOnlyForBillingUsersAgent="true" :showOnlyActiveAndToday="true"
					:focusIsProject="true" :showDateFilters="false" emptyMessage="There are no labour entries today."
					:disabled="disabled" />
			</div>
			<v-card-actions style="flex-wrap: wrap;">
				<v-btn text :to="`/section/agents/${agentId}?tab=Labour`" style="margin-left: 0px;"
					class="e2e-my-labour-entries-active-and-today-card-see-all-my-labour">
					See All My Labour
				</v-btn>
				<v-btn text @click="DialoguesOpen({
			name: 'ModifyLabourDialogue',
			state: {
				json: {
					agentId: agentId,
				}
			}
		})" style="margin-left: 0px;" class="e2e-my-labour-entries-active-and-today-card-add-manual-entry"
					:disabled="disabled || !PermCRMLabourManualEntries() || !PermLabourCanPush()">
					Add Manual Entry
				</v-btn>
				<v-spacer></v-spacer>
			</v-card-actions>
		</div>
	</v-card>
</template>
<script lang="ts">

import { Component, Vue } from 'vue-property-decorator';
import LabourList from '@/Components/Lists/LabourList.vue';
import LabourListItem from '@/Components/ListItems/LabourListItem.vue';
import _ from 'lodash';
import { DateTime } from 'luxon';
import CardBase from './CardBase';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { ILabour, Labour } from '@/Data/CRM/Labour/Labour';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {
		LabourList,
		LabourListItem,
	},
})
export default class MyLabourEntriesActiveAndToday extends CardBase {

	public $refs!: {
		labourList: LabourList,
	};

	protected DialoguesOpen = Dialogues.Open;
	protected AgentLoggedInAgentId = Agent.LoggedInAgentId;
	protected AgentForLoggedInAgent = Agent.ForLoggedInAgent;
	protected PermLabourCanPush = Labour.PermLabourCanPush;
	protected PermCRMLabourManualEntries = Labour.PermCRMLabourManualEntries;

	protected agentId: string | null = null;
	protected agent: IAgent | null = null;
	protected loadingData = false;

	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;

	public Periodic(): void {

		//console.debug("Periodic() agentId", this.agentId, "agent", this.agent);

		Vue.set(this, 'agentId', Agent.LoggedInAgentId());
		Vue.set(this, 'agent', Agent.ForLoggedInAgent());

		if (this.agentId && !this.agent) {
			this.LoadData();
		}

	}

	public get IsLoadingData(): boolean {

		if (this.$refs.labourList && this.$refs.labourList.IsLoadingData) {
			return true;
		}

		return this.loadingData;
	}

	public LoadData(): void {

		this.loadingData = true;

		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}

		this._LoadDataTimeout = setTimeout(() => {

			if (null != this.agentId && !IsNullOrEmpty(this.agentId) && null == this.agent) {

				const rtr = Agent.FetchForId(this.agentId);

				if (rtr.completeRequestPromise != null) {
					rtr.completeRequestPromise.finally(() => {
						Vue.set(this, 'agentId', Agent.LoggedInAgentId());
						Vue.set(this, 'agent', Agent.ForLoggedInAgent());
						this.loadingData = false;
					});
				}

			} else {
				this.loadingData = false;
			}

		}, 250);

		if (this.$refs.labourList) {
			this.$refs.labourList.LoadData();
		}


	}







	public get Entries(): ILabour[] {

		// This moved out of filter to optimize.
		const loggedInAgentId = Agent.LoggedInAgentId();
		const localNow = DateTime.local();
		const localEndOfDay = localNow.endOf('day');
		const localStartOfDay = localNow.startOf('day');

		const filtered: ILabour[] = _.filter(
			this.$store.state.Database.labour,
			(o: ILabour) => {

				//console.log('Entries',o);

				// Only show assignments for this billing user's agent.
				if (o.json.agentId !== loggedInAgentId) {
					return false;
				}

				if (!o.json.startISO8601) {
					return false;
				}

				// Only show open assignments, and assignments in which today's date is a part.
				const startISO = DateTime.fromISO(o.json.startISO8601);
				const localStart = startISO.toLocal();

				const endISO = DateTime.fromISO(o.json.endISO8601 as string); // TODO: this may be incorrect
				const localEnd = endISO.toLocal();

				const localIsActive = o.json.isActive === true;
				const localStartIsToday = (localStart >= localStartOfDay && localStart <= localEndOfDay);
				const localEndIsToday = (localEnd >= localStartOfDay && localEnd <= localEndOfDay);

				//console.log("localStartOfDay", localStartOfDay);
				//console.log("localEndOfDay", localEndOfDay);
				//console.log("localStartIsToday", localStartIsToday);
				//console.log("localEndIsToday", localEndIsToday);
				//console.log(o.json.isActive, localStart, localEnd);

				if (localIsActive || localStartIsToday || localEndIsToday) {
					return true;
				}

				return false;
			},
		);

		const sorted: ILabour[] = _.sortBy(filtered, (o: ILabour) => {
			return o.json.startISO8601;
		});

		// we may have to change this to last.
		//return _.first(sorted, 5);




		return sorted;
	}


}
</script>
