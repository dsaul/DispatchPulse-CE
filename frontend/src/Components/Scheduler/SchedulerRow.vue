<template>
	<div style="min-height: 60px; display:flex;background: white;">
		
		
			<SchedulerCell
				v-for="(column, index) in columns"
				:key="index"
				
				:date="date"
				:agentId="agentId"
				:cellWidthPx="cellWidthPx"
				:cellWidthMinimizedPx="cellWidthMinimizedPx"
				:isUnscheduled="column.type == 'unscheduled'"
				:isFirstAM="column.type == 'am1'"
				:isSecondAM="column.type == 'am2'"
				:isFirstPM="column.type == 'pm1'"
				:isSecondPM="column.type == 'pm2'"
				:localTimeStart="column.localTimeStart"
				:localTimeEnd="column.localTimeEnd"
				:minimized="minimizedColumns[index]"
				/>
			<div style="min-width: 300px;height: 1px;"></div> <!-- Padding to ensure the two scroll areas keep being aligned. -->
		
		
	</div>
</template>
<script lang="ts">

import { Component, Vue, Prop } from 'vue-property-decorator';
import ResizeObserver from 'resize-observer-polyfill';
import SchedulerCell from '@/Components/Scheduler/SchedulerCell.vue';
import { SchedulerColumn } from './Scheduler.vue';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Agent } from '@/Data/CRM/Agent/Agent';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		SchedulerCell,
	},
})
export default class SchedulerRow extends Vue {
	
	@Prop({ default: 150 }) public readonly cellWidthPx!: number;
	@Prop({ default: 50 }) public readonly cellWidthMinimizedPx!: number;
	
	@Prop({ default: null }) public readonly pushHeightTargetSelector!: string;
	@Prop({ default: null }) public readonly agentId!: string;
	@Prop({ default: null }) public readonly date!: string;
	
	@Prop({ default: () => [] }) public readonly columns!: SchedulerColumn[];
	@Prop({ default: () => [] }) public readonly minimizedColumns!: boolean[];
	
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	
	protected resizeObserver: ResizeObserver | null = null;
	
	protected LoadData(): void {
		
		//console.debug('LabourList LoadData()');
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
		
			SignalRConnection.Ready(() => {
				BillingPermissionsBool.Ready(() => {
					
					const promises: Array<Promise<any>> = [];
					
					const agentId = this.agentId;
					if (null == Agent.ForId(agentId)) {
						const rtr = Agent.FetchForId(agentId);
						
						if (rtr.completeRequestPromise) {
							promises.push(rtr.completeRequestPromise);
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
		
		}, 250);
		
	}
	
	protected mounted(): void {
		//console.log('SchedulerRow', this);
		
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		this.resizeObserver = new ResizeObserver((entries, observer) => {
			this.PushSize();
		});
		this.resizeObserver.observe(this.$el);
		
		this.LoadData();
	}
	
	
	
	protected get Unscheduled(): never[] {
		
		console.log(this.$store.state.Database.assignments);
		
		return [];
	}
	
	
	
	
	protected beforeDestroy(): void {
		if (this.resizeObserver) {
			this.resizeObserver.unobserve(this.$el);
		}
	}
	
	
	
	
	protected PushSize(): void {
		//console.log(this.pushHeightTargetSelector);
		const target = document.querySelector<HTMLElement>(this.pushHeightTargetSelector);
		//console.log(target, this.pushHeightTargetSelector);
		if (target != null) {
			target.style.height = `${this.$el.getBoundingClientRect().height}px`;
		}
	}
	
}

</script>
<style scoped>





</style>