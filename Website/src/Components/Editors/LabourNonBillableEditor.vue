<template>
	<div>
	
		<v-app-bar
			v-if="showAppBar"
			color="#747389"
			dark
			fixed
			app
			clipped-right
			>
			<v-progress-linear
				v-if="isLoadingData"
				:indeterminate="true"
				absolute
				top
				color="white"
			></v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Labour Non Billable</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

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

					<v-tab
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'General'}}).catch(((e) => {}));"
						>
						General
					</v-tab>
				</v-tabs>
			</template>
			
		</v-app-bar>
		
		<v-breadcrumbs
			v-if="breadcrumbs"
			:items="breadcrumbs"
			style="padding-bottom: 0px; padding-top: 15px; background: white;"
			>
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
		
		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Labour Non Billable Type Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The labour non billable type no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the labour non billable type while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
			<v-tabs
				v-if="!showAppBar"
				v-model="tab"
				background-color="transparent"
				grow
				show-arrows
				style="visibility: none; height:0px;"
			>
				<v-tab>
					General
				</v-tab>
			</v-tabs>
			
			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>
						
						<v-form
							autocomplete="newpassword"
							ref="generalForm"
							>
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">General</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											ref="name"
											v-model="Name"
											autocomplete="newpassword"
											label="Name"
											hint="Non Billable Name"
											:rules="[ ValidateRequiredField ]"
											class="e2e-labour-non-billable-editor-name"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
									
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											ref="description"
											v-model="Description"
											autocomplete="newpassword"
											label="Description"
											hint="Non Billable Description"
											:rules="[ ValidateRequiredField ]"
											class="e2e-labour-non-billable-editor-description"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
									
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											ref="icon"
											v-model="Icon"
											autocomplete="newpassword"
											label="Icon"
											hint="Non Billable Icon"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
									
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Advanced</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Id"
											readonly="readonly"
											label="Unique ID"
											hint="The id of this labour non billable type."
											>
										</v-text-field>
									</v-col>
								</v-row>

								
							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
			</v-tabs-items>
		</div>
		
		<v-footer
			v-if="showFooter"
			color="#747389"
			class="white--text"
			app
			inset>
			<v-row
				no-gutters
				>
				<v-btn
					:disabled="!value || connectionStatus != 'Connected'"
					color="white"
					text
					rounded
					@click="DialoguesOpen({ name: 'DeleteNonBillableDialogue', state: value})"
					>
					<v-icon left>delete</v-icon>
					Delete
				</v-btn>
			</v-row>
		</v-footer>
	</div>
</template>
<script lang="ts">
import Dialogues from '@/Utility/Dialogues';
import EditorBase, { IBreadcrumb, VForm } from './EditorBase';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import ContactList from '@/Components/Lists/ContactList.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { ILabourSubtypeNonBillable } from '@/Data/CRM/LabourSubtypeNonBillable/LabourSubtypeNonBillable';

@Component({
	components: {
		ProjectList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
	},
	
})
export default class LabourNonBillableEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: ILabourSubtypeNonBillable | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
		quantity: Vue,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected DialoguesOpen = Dialogues.Open;
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	
	public GetValidatedForms(): VForm[] {
		return [
			this.$refs.generalForm as VForm,
		];
	}
	
	protected GetTabNameToIndexMap(): Record<string, number> {
		return {
			General: 0,
			general: 0,
			Projects: 1,
			projects: 1,
		};
	}
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	protected get Name(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.name
			) {
			return null;
		}
		
		return this.value.json.name;
	}
	
	protected set Name(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.name = IsNullOrEmpty(val) ? '' : val as string;
		this.SignalChanged();
	}
	
	protected get Description(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.description
			) {
			return null;
		}
		
		return this.value.json.description;
	}
	
	protected set Description(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.description = IsNullOrEmpty(val) ? '' : val as string;
		this.SignalChanged();
	}
	
	protected get Icon(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.icon
			) {
			return null;
		}
		
		return this.value.json.icon;
	}
	
	protected set Icon(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.icon = IsNullOrEmpty(val) ? '' : val as string;
		this.SignalChanged();
	}
	
	
	
	
	
	
	
	
	
	protected SignalChanged(): void {
		
		// Debounce
		
		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}
		
		this.debounceId = setTimeout(() => {
			this.$emit('input', this.value);
		}, 250);
	}
}

</script>
<style scoped>

</style>