<template>
	
	<div>
	
		<v-app-bar color="#747389" dark fixed app clipped-right >
			<v-progress-linear
				v-if="IsLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Agents</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="LoadData()" />
			
			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn
					dark
					icon
					v-on="on"
					>
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
					<!--<v-list-item
						@click="DoPrint()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
					<!--<v-list-item
						@click="ExportToCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>-->
					<v-list-item disabled>
						<v-list-item-content>
							<v-list-item-title>No Items</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</v-list>
			</v-menu>
			
			
			<template v-slot:extension>
				<v-tabs
				v-model="tab"
				
				background-color="transparent"
				align-with-title
				show-arrows
				>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab @click="$router.replace({query: { ...$route.query, tab: 'Agents'}}).catch(((e) => {}));">
						Agents
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs :items="breadcrumbs" style="background: white; padding-bottom: 5px; padding-top: 15px;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>
		
		<v-alert
			v-if="connectionStatus != 'Connected'"
			type="error"
			elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;"
			>
			Disconnected from server.
		</v-alert>
			
		<v-tabs-items v-model="tab" style="background: transparent;">
			<v-tab-item style="flex: 1;">
				<AgentsList
					ref="agentsList"
					:disabled="connectionStatus != 'Connected'"
					/>
			</v-tab-item>
		</v-tabs-items>
		
		<div style="height: 50px;"></div>
		
		<AddAgentDialogue2 
			v-model="addAgentModel"
			:isOpen="addAgentOpen"
			@Save="SaveAddAgent"
			@Cancel="CancelAddAgent"
			ref="addAgentDialogue"
			/>
	
		<v-footer color="#747389" class="white--text" app inset>
			<v-row
				no-gutters
				>
				<v-spacer />
				
				<AddMenuButton
					:disabled="connectionStatus != 'Connected'"
					>
					<v-list-item
						@click="OpenAddAgent()"
						class="e2e-add-menu-add-agent"
						:disabled="connectionStatus != 'Connected' || !PermAgentsCanPush()"
						>
						<v-list-item-icon>
							<v-icon>person</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Agent</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>
			</v-row>
		</v-footer>
	</div>
</template>

<script lang="ts">

import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import AgentsList from '@/Components/Lists/AgentsList.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Agent, IAgent } from '@/Data/CRM/Agent/Agent';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import AddAgentDialogue2 from '@/Components/Dialogues2/Agents/AddAgentsDialogue2.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		AddMenuButton,
		AgentsList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		AddAgentDialogue2,
		NotificationBellButton,
	},
})
export default class AgentsIndex extends ViewBase {
	
	
	public $refs!: {
		agentsList: AgentsList,
		addAgentDialogue: AddAgentDialogue2,
	};
	
	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Agents: 0,
		agents: 0,
	};
	
	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'Agents Index',
			disabled: true,
			to: '/section/agents/index',
		},
	];
	
	protected PermAgentsCanPush = Agent.PermAgentsCanPush;
	
	
	
	protected addAgentModel: IAgent | null = null;
	protected addAgentOpen = false;
	
	
	protected loadingData = false;
	
	
	
	
	
	public get IsLoadingData(): boolean {
		
		if (this.$refs.agentsList && this.$refs.agentsList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}
	
	public ReLoadData(): void {
		
		this.LoadData();
		
		if (this.$refs.agentsList) {
			this.$refs.agentsList.LoadData();
		}
		
	}
	
	public LoadData(): void {
		
		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {
				
				this.loadingData = true;
				
				setTimeout(() => {
					this.loadingData = false;
				}, 250);
				
			});
		});
		
	}
	
	protected MountedAfter(): void {
		this.SwitchToTabFromRoute();
		this.LoadData();
	}
	
	protected SwitchToTabFromRoute(): void {
		// Select the tab in the query string.
		if (IsNullOrEmpty(this.$route.query.tab as string | null)) {
			this.tab = 0;
		} else {
			const index = this.tabNameToIndex[this.$route.query.tab as string];
			this.tab = index;
		}
	}
	
	protected OpenAddAgent(): void {
		
		//console.debug('OpenAddAgent');
		
		this.addAgentModel = Agent.GetEmpty();
		this.addAgentOpen = true;
		
		requestAnimationFrame(() => {
			if (this.$refs.addAgentDialogue) {
				this.$refs.addAgentDialogue.SwitchToTabFromRoute();
			}
		});
		
		
		
	}
	
	protected CancelAddAgent(): void {
		
		//console.debug('CancelAddAgent');
		
		this.addAgentOpen = false;
		
	}
	
	protected SaveAddAgent(): void {
		
		//console.debug('SaveAddAgent');
		
		this.addAgentOpen = false;
		
		if (!this.addAgentModel || !this.addAgentModel.id || IsNullOrEmpty(this.addAgentModel.id)) {
			console.error('!this.addAgentModel || !this.addAgentModel.id || IsNullOrEmpty(this.addAgentModel.id)');
			return;
		}
		
		const payload: Record<string, IAgent> = {};
		payload[this.addAgentModel.id] = this.addAgentModel;
		Agent.UpdateIds(payload);
		
		this.$router.push(`/section/agents/${this.addAgentModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
	}
	
	
	
	
}
</script>
