<template>
	<v-card class="e2e-assignments-in-billable-review-card" style="margin: 30px; margin-top: 15px;">
		<v-progress-linear v-if="loadingData" indeterminate></v-progress-linear>
		<v-card-title>Assignments in Billable Review</v-card-title>
		<v-card-text>
			<AssignmentsList ref="assignmentsList" :showOnlyBilliableReview="true" :showAwaitingPayment="false"
				:showOpen="true" :focusIsProject="true" :showClosed="false"
				emptyMessage="There are no assignments to show, make sure to complete assignments or set them to billable review for them to show here."
				:showFilter="false" :showTopPagination="false" :rowsPerPage="5" :disabled="disabled" />
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
export default class AssignmentsInBillableReviewCard extends CardBase {

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

		//console.log('@@@');

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