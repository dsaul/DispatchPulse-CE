<template>
	
	<div>
		
		<v-app-bar color="#747389" dark fixed app clipped-right >
			
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Loading&#8230;</v-toolbar-title>

			
			
			
			
			
		</v-app-bar>
		
		<v-alert
			v-if="connectionStatus != 'Connected'"
			type="error"
			elevation="2"
			style="margin-top: 10px; margin-left: 30px; margin-right: 30px;"
			>
			Disconnected from server.
		</v-alert>
		
		
		
		
		<div style="height: 50px;"></div>
		
			
	</div>
</template>

<script lang="ts">

import { Component } from 'vue-property-decorator';
import ViewBase from '@/Components/Views/ViewBase';
import CRMNavigation from '@/Permissions/CRMNavigation';


@Component({
	components: {
		
	},
})
export default class Dashboard extends ViewBase {
	
	protected cacheLicensedProjectsSchedulingTime = false;
	protected cacheLicensedOnCall = false;

	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;
	
	protected MountedAfter(): void {

		this._periodicInterval = setInterval(this.Periodic, 500);

		this.SwitchToTabFromRoute();
		
		//console.log('mounted');
		
		
	}

	protected destroyed(): void {
		if (this._periodicInterval != null) {
			clearInterval(this._periodicInterval);
		}
	}
	
	protected Periodic(): void {
		this.cacheLicensedProjectsSchedulingTime = CRMNavigation.PermCRMNavigationLicensedProjectsSchedulingTime();
		this.cacheLicensedOnCall = CRMNavigation.PermCRMNavigationLicensedOnCall();

		console.log('this.cacheLicensedProjectsSchedulingTime', this.cacheLicensedProjectsSchedulingTime);

		if (true === this.cacheLicensedProjectsSchedulingTime) {
			this.$router.push(`/section/dashboard/agent`).catch(((e: Error) => { }));// eslint-disable-line
		} else if (true === this.cacheLicensedOnCall) {
			this.$router.push(`/section/dashboard/on-call`).catch(((e: Error) => { }));// eslint-disable-line
		}



		

	}
	
	
	
}
</script>
