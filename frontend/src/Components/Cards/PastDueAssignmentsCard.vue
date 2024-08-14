<template>
	<v-card class="e2e-past-due-assignments-card" style="margin: 30px; margin-top: 15px;">
		<v-progress-linear v-if="loadingData" indeterminate></v-progress-linear>
		<v-card-title>Past Due Assignments</v-card-title>
		<v-card-text>
			<AssignmentsList ref="assignmentsList" :showOnlyPastDue="true" :focusIsProject="true"
				:showOnlyOpenAssignments="true" :showFilter="false" :showTopPagination="false" :rowsPerPage="5"
				emptyMessage="There are no assignments to show, make sure to add a scheduled end to assignments or their projects for them to show here."
				:disabled="disabled" />
		</v-card-text>
	</v-card>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import CardBase from '@/Components/Cards/CardBase';
import AssignmentsList from '@/Components/Lists/AssignmentsList.vue';


@Component({
	components: {
		AssignmentsList,
	},
})
export default class PastDueAssignmentsCard extends CardBase {

	public $refs!: {
		assignmentsList: AssignmentsList,
	};




	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;

	public get IsLoadingData(): boolean {

		if (this.$refs.assignmentsList && this.$refs.assignmentsList.IsLoadingData) {
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

			this.loadingData = false;

		}, 250);

		if (this.$refs.assignmentsList) {
			this.$refs.assignmentsList.LoadData();
		}

	}


}
</script>