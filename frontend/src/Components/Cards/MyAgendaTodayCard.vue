<template>
	<v-card class="" style="margin: 30px; margin-top: 15px;">
		<v-progress-linear v-if="IsLoadingData" indeterminate></v-progress-linear>
		<v-card-title>My Agenda Today</v-card-title>

		<v-card-text v-if="!agentId">
			<v-alert type="warning" colored-border elevation="2" border="bottom">
				You need an agent assigned in <router-link to="/settings">settings</router-link> to be able to see this.
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
				<v-card-text style="padding-bottom: 0px;">
					<div class="headline mb-2">Scheduled</div>
				</v-card-text>
				<AssignmentsList ref="scheduledList" :showTopPagination="false" :showFilter="false" :rowsPerPage="5"
					:showOnlyForBillingUsersAgent="true" :showOnlyOpenAssignments="true" :showOnlyTodayOrEarlier="true"
					:hideAssignmentsWithNoStartTime="true" :focusIsProject="true"
					emptyMessage="There are no assignments scheduled for today." :disabled="disabled" />

				<v-card-text style="padding-bottom: 0px;">
					<div class="headline mb-2">Unscheduled</div>
				</v-card-text>

				<AssignmentsList ref="unscheduledList" :showTopPagination="false" :showFilter="false" :rowsPerPage="5"
					:showOnlyForBillingUsersAgent="true" :showOnlyOpenAssignments="true"
					:showOnlyTasksWithNoStartTime="true" :focusIsProject="true"
					emptyMessage="There are no unscheduled assignments." :disabled="disabled" />

			</div>













			<AddAssignmentDialogue2 v-model="addAssignmentModel" :isOpen="addAssignmentOpen" @Save="SaveAddAssignment"
				@Cancel="CancelAddAssignment" ref="addAssignmentDialogue" />

			<v-card-actions style="flex-wrap: wrap;">
				<v-btn :to="`/section/agents/${agentId}?tab=Agenda`" text style="margin-left: 0px;"
					class="e2e-my-agenda-today-card-see-full-agenda" :disabled="!PermAgentDisplayOwn()">
					See Full Agenda
				</v-btn>
				<v-btn text @click="OpenAddAssignment()" style="margin-left: 0px;"
					class="e2e-my-agenda-today-card-add-assignment" :disabled="disabled || !PermAssignmentCanPush()">
					Add Assignment
				</v-btn>
				<v-spacer></v-spacer>
			</v-card-actions>
		</div>
	</v-card>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import AssignmentsList from '@/Components/Lists/AssignmentsList.vue';
import AssignmentListItem from '@/Components/ListItems/AssignmentListItem.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import CardBase from './CardBase';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import { Assignment, IAssignment } from '@/Data/CRM/Assignment/Assignment';
import Dialogues from '@/Utility/Dialogues';
import AddAssignmentDialogue2 from '@/Components/Dialogues2/Assignments/AddAssignmentDialogue2.vue';

@Component({
	components: {
		AssignmentListItem,
		AssignmentsList,
		AddAssignmentDialogue2,
	},
	props: {
		contactId: String,
	},
})
export default class MyAgendaToday extends CardBase {

	// definite assignments as props
	public contactId!: string;

	public $refs!: {
		scheduledList: AssignmentsList,
		unscheduledList: AssignmentsList,
		addAssignmentDialogue: AddAssignmentDialogue2,
	};

	protected DialoguesOpen = Dialogues.Open;
	protected PermAssignmentCanPush = Assignment.PermAssignmentCanPush;
	protected PermAgentDisplayOwn = Agent.PermAgentDisplayOwn;





	protected agentId: string | null = null;
	protected agent: IAgent | null = null;
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;

	protected addAssignmentModel: IAssignment | null = null;
	protected addAssignmentOpen = false;



	public Periodic(): void {

		//console.debug("Periodic() agentId", this.agentId, "agent", this.agent);

		Vue.set(this, 'agentId', Agent.LoggedInAgentId());
		Vue.set(this, 'agent', Agent.ForLoggedInAgent());

		if (this.agentId && !this.agent) {
			this.LoadData();
		}

	}

	public get IsLoadingData(): boolean {

		if (this.$refs.scheduledList && this.$refs.scheduledList.IsLoadingData) {
			return true;
		}

		if (this.$refs.unscheduledList && this.$refs.unscheduledList.IsLoadingData) {
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

		if (this.$refs.scheduledList) {
			this.$refs.scheduledList.LoadData();
		}

		if (this.$refs.unscheduledList) {
			this.$refs.unscheduledList.LoadData();
		}




	}


	protected OpenAddAssignment(): void {

		//console.debug('OpenAddAssignment');

		this.addAssignmentModel = Assignment.GetEmpty();
		this.addAssignmentOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.addAssignmentDialogue) {
				this.$refs.addAssignmentDialogue.SwitchToTabFromRoute();
			}
		});

	}

	protected CancelAddAssignment(): void {

		//console.debug('CancelAddAssignment');

		this.addAssignmentOpen = false;

	}

	protected SaveAddAssignment(): void {

		//console.debug('SaveAddAssignment');

		this.addAssignmentOpen = false;

		if (!this.addAssignmentModel ||
			!this.addAssignmentModel.id) {
			return;
		}

		const payload: Record<string, IAssignment> = {};
		payload[this.addAssignmentModel.id] = this.addAssignmentModel;
		Assignment.UpdateIds(payload);

		this.$router.push(`/section/assignments/${this.addAssignmentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line

	}




































}
</script>