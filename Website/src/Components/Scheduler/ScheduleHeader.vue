<template>
	<div class="base" :style="{ 
		minWidth: `${minimized ? cellWidthMinimizedPx : cellWidthPx}px`,
		display: 'flex',
		alignItems: 'flex-end',
		}">
		<div :class="{
			headerTextMinimized: minimized
		}">
			{{minimized ? column.columnHeaderMinimized : column.columnHeader}}
		</div>
	</div>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { SchedulerColumn } from './Scheduler.vue';

@Component({
	components: {
	},
})
export default class SchedulerHeader extends Vue {
	
	@Prop({ default: false }) public readonly minimized!: boolean;
	@Prop({ default: 150 }) public readonly cellWidthPx!: number;
	@Prop({ default: 50 }) public readonly cellWidthMinimizedPx!: number;
	
	@Prop({ default: null }) public readonly column!: SchedulerColumn;
	
	constructor() {
		super();
	}
	
	
	
	
	
}
</script>
<style scoped>
div.base {
	font-weight: bold;
	padding: 10px;
	border-right: 1px solid #111;
	/* writing-mode: vertical-lr;
	transform: rotate(-180deg); */
}

.headerTextMinimized {
	writing-mode: vertical-lr;
	transform: rotate(-180deg);
}

</style>