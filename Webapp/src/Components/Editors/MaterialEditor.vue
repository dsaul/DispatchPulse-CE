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
			
			<v-toolbar-title class="white--text">Material</v-toolbar-title>

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
						<div class="title">Project Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The project no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the project while you were opening it.</li>
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
										<ProjectSelectField 
											:isDialogue="isDialogue"
											v-model="ProjectId"
											hint="Select the project this entry applies to."
											:rules="[ ValidateRequiredField ]"
											class="e2e-material-editor-project-select-field"
											:disabled="connectionStatus != 'Connected'"
											/>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<ProductSelectField
											:isDialogue="isDialogue"
											@input="ProductIdChanged"
											v-model="ProductId"
											hint="Select the product this entry applies to."
											:rules="[ ValidateRequiredField ]"
											class="e2e-material-editor-product-select-field"
											:disabled="connectionStatus != 'Connected'"
											/>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="4" sm="2" offset-sm="2">
										<v-text-field
											ref="quantity"
											v-model="Quantity"
											autocomplete="newpassword"
											label="Quantity"
											hint="How much?"
											:rules="[ ValidateRequiredField ]"
											class="e2e-material-editor-quantity"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
									<v-col cols="8" sm="6">
										<v-text-field
											v-model="QuantityUnit"
											autocomplete="newpassword"
											label="Unit"
											hint="Feet, Meters, Units, etc..."
											:rules="[ ValidateRequiredField ]"
											class="e2e-material-editor-quantity-unit"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
								</v-row>
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="Location"
											autocomplete="newpassword"
											label="Location"
											hint="Where this material was used."
											class="e2e-material-editor-location"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
									</v-col>
								</v-row>
								
								
								
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Date Used</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-input
											v-model="DateUsedLocal"
											:rules="[ ValidateRequiredField ]"
											:disabled="connectionStatus != 'Connected'"
											>
											<v-date-picker
												v-model="DateUsedLocal"
												value="ISO 8601"
												:elevation="1"
												class="e2e-material-editor-date-used"
												:disabled="connectionStatus != 'Connected'"
												>
											</v-date-picker>
										</v-input>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title" style="font-weight: bold;">Other</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col
										cols="12"
										sm="8"
										offset-sm="2"
										style="padding-top: 0px; padding-bottom: 0px;"
										>
										<v-switch
											v-model="IsExtra"
											label="Extra (Not Included in Contract)"
											style="margin-top:0px;"
											class="e2e-material-editor-is-extra"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-switch>
										<v-switch
											v-model="IsBilled"
											label="Billed (To Client)"
											style="margin-top:0px;"
											class="e2e-material-editor-is-billed"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-switch>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-textarea
											v-model="Notes"
											label="Notes"
											hint="Other notes about this entry."
											class="e2e-material-editor-notes"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-textarea>
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
											hint="The id of this material."
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
					@click="DialoguesOpen({ name: 'DeleteMaterialDialogue', state: {
						redirectToIndex: true,
						id: value.id,
					}})">
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
import ProductSelectField from '@/Components/Fields/ProductSelectField.vue';
import { DateTime } from 'luxon';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import { Product } from '@/Data/CRM/Product/Product';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IMaterial } from '@/Data/CRM/Material/Material';

@Component({
	components: {
		ProjectList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ProductSelectField,
		ReloadButton,
		NotificationBellButton,
	},
	
})
export default class MaterialEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IMaterial | null;
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
	
	protected ProductIdChanged(payload: string | null): void {
		
		// If product was changed to nothing don't do anything.
		if (!payload || 
			IsNullOrEmpty(payload) 
			) {
			return;
		}
		
		// Don't set this if something is already set for Quantity unit.
		if (this.QuantityUnit && !IsNullOrEmpty(this.QuantityUnit)) {
			return;
		}
		
		const product = Product.ForId(payload);
		if (product && product.json && !IsNullOrEmpty(product.json.quantityUnit)) {
			this.QuantityUnit = product.json.quantityUnit;
		} else {
			this.QuantityUnit = '\u00D7';
		}
		
	}
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	protected get ProjectId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.projectId
			) {
			return null;
		}
		
		return this.value.json.projectId;
	}
	
	protected set ProjectId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.projectId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get ProductId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.productId
			) {
			return null;
		}
		
		return this.value.json.productId;
	}
	
	protected set ProductId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.productId = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get Quantity(): number | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.quantity
			) {
			return null;
		}
		
		return this.value.json.quantity;
	}
	
	protected set Quantity(val: number | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.quantity = val;
		this.SignalChanged();
	}
	
	protected get QuantityUnit(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.quantityUnit
			) {
			return null;
		}
		
		return this.value.json.quantityUnit;
	}
	
	protected set QuantityUnit(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.quantityUnit = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get Location(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.location
			) {
			return null;
		}
		
		return this.value.json.location;
	}
	
	protected set Location(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.location = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get IsExtra(): boolean | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isExtra
			) {
			return null;
		}
		
		return this.value.json.isExtra;
	}
	
	protected set IsExtra(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isExtra = val;
		this.SignalChanged();
	}
	
	protected get Notes(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.notes
			) {
			return null;
		}
		
		return this.value.json.notes;
	}
	
	protected set Notes(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.notes = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get IsBilled(): boolean | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.isBilled
			) {
			return null;
		}
		
		return this.value.json.isBilled;
	}
	
	protected set IsBilled(val: boolean | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.isBilled = val;
		this.SignalChanged();
	}
	
	protected get DateUsedLocal(): string | null {
		
		if (!this.value) {
			return null;
		}
		
		const json = this.value.json;
		if (!json) {
			return null;
		}
		
		const iso8601 = json.dateUsedISO8601;
		if (!iso8601 || IsNullOrEmpty(iso8601)) {
			return null;
		}
		
		const utc = DateTime.fromISO(iso8601);
		if (!utc) {
			return null;
		}
		
		const local = utc.toLocal();
		if (!local) {
			return null;
		}
		
		return local.toFormat('yyyy-MM-dd');
	}
	
	
	
	protected set DateUsedLocal(val: string | null) {
		
		
		
		if (!this.value) {
			return;
		}
		
		let validatedVar = null;
		
		do {
			if (!val ||
				!this.value) {
				validatedVar = null;
				break;
			}
			
			
			const dbISO8601 = this.value.json.dateUsedISO8601;
			
			let dbUtc = null;
			
			if (dbISO8601) {
				dbUtc = DateTime.fromISO(dbISO8601);
			} else {
				dbUtc = DateTime.utc();
			}
			
			const dbLocal = dbUtc.toLocal();
			
			const valDateLocal = DateTime.fromFormat(val, 'yyyy-MM-dd');
			const dbLocalMod = dbLocal.set({
				year: valDateLocal.year,
				month: valDateLocal.month,
				day: valDateLocal.day,
			});
			
			const dbUtcMod = dbLocalMod.toUTC();
			validatedVar = dbUtcMod.toISO();
			
		} while (false);
		
		//console.log('set StartDateLocal', val, validatedVar);
		
		
		this.value.json.dateUsedISO8601 = validatedVar;
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