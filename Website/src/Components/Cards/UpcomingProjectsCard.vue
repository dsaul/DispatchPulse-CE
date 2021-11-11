<template>
	<v-card
		class="e2e-upcoming-projects-card"
		style="margin: 30px; margin-top: 15px;"
		>
		<v-progress-linear v-if="loadingData" indeterminate></v-progress-linear>
		<v-card-title>Upcoming Projects</v-card-title>
		<v-card-text>
			Scheduled to start in the next week:
			<ProjectList
				ref="projectList"
				:showOnlyUpcoming="true"
				:showFilter="false"
				emptyMessage="There are no projects to show, make sure to add a scheduled start to projects for them to show here."
				:showTopPagination="false"
				:rowsPerPage="5"
				:disabled="disabled"
				/>
		</v-card-text>
	</v-card>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import CardBase from '@/Components/Cards/CardBase';
import ProjectList from '@/Components/Lists/ProjectList.vue';

@Component({
	components: {
		ProjectList,
	},
})
export default class UpcomingProjectsCard extends CardBase {
	
	public $refs!: {
		projectList: ProjectList,
	};
	
	
	
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.projectList && this.$refs.projectList.IsLoadingData) {
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
		
		if (this.$refs.projectList) {
			this.$refs.projectList.LoadData();
		}
		
	}
	
}
</script>