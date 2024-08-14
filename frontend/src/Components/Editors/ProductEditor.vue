<template>
	<div>

		<v-app-bar v-if="showAppBar" color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="isLoadingData" :indeterminate="true" absolute top
				color="white"></v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">Product</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn dark icon v-on="on">
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
				<v-tabs v-model="tab" background-color="transparent" align-with-title show-arrows>
					<v-tabs-slider color="white"></v-tabs-slider>

					<v-tab :disabled="!value"
						@click="$router.replace({ query: { ...$route.query, tab: 'General' } }).catch(((e) => { }));">
						General
					</v-tab>
				</v-tabs>
			</template>

		</v-app-bar>

		<v-breadcrumbs v-if="breadcrumbs" :items="breadcrumbs"
			style="padding-bottom: 0px; padding-top: 15px; background: white;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>

		<v-alert v-if="connectionStatus != 'Connected'" type="error" elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;">
			Disconnected from server.
		</v-alert>

		<div v-if="!value" style="margin-top: 20px;" class="fadeIn404">
			<v-container>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						<div class="title">Product Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The product no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the product while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>
			<v-tabs v-if="!showAppBar" v-model="tab" background-color="transparent" grow show-arrows
				style="visibility: none; height:0px;">
				<v-tab>
					General
				</v-tab>
			</v-tabs>

			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>

						<v-form autocomplete="newpassword" ref="generalForm">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">General</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field ref="name" v-model="Name" autocomplete="newpassword" label="Name"
											hint="Name of the Product" :rules="[ValidateRequiredField]"
											class="e2e-product-editor-name" :disabled="connectionStatus != 'Connected'">
										</v-text-field>
									</v-col>

								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="QuantityUnit" autocomplete="newpassword"
											label="Standard Unit of Measurement" hint="Feet, Meters, Units, etc..."
											:rules="[ValidateRequiredField]" class="e2e-product-editor-unit"
											:disabled="connectionStatus != 'Connected'">
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
										<v-text-field v-model="Id" readonly="readonly" label="Unique ID"
											hint="The id of this product.">
										</v-text-field>
									</v-col>
								</v-row>

							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
			</v-tabs-items>
		</div>

		<v-footer v-if="showFooter" color="#747389" class="white--text" app inset>
			<v-row no-gutters>
				<v-btn :disabled="!value || connectionStatus != 'Connected'" color="white" text rounded @click="DialoguesOpen({
			name: 'DeleteProductDialogue', state: {
				redirectToIndex: true,
				id: value.id,
			}
		})">
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
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import { IProduct, Product } from '@/Data/CRM/Product/Product';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';

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
export default class ProductEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IProduct | null;
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

		this.value.json.name = IsNullOrEmpty(val) ? null : val;
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

	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
		) {
			return null;
		}

		return this.value.id;
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
<style scoped></style>