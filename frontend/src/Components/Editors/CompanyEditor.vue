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
				>
			</v-progress-linear>
			<v-app-bar-nav-icon @click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>
			
			<v-toolbar-title class="white--text">Company: {{Name}}</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item
					@click="OnlineHelpFiles()"
					>
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Company Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</HelpMenuButton>
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
					<v-list-item
						@click="DoPrint()"
						:disabled="connectionStatus != 'Connected' || !PermCRMReportCompaniesPDF()"
						>
						<v-list-item-icon>
							<v-icon>print</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Print/Report&hellip;</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
					<v-list-item
						@click="CSVDownloadCompany(value)"
						:disabled="!PermCRMExportCompaniesCSV()"
						>
						<v-list-item-icon>
							<v-icon>import_export</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Export to CSV&hellip;</v-list-item-title>
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
					<v-tab
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'Projects'}}).catch(((e) => {}));"
						>
						Projects
					</v-tab>
					<v-tab
						:disabled="!value"
						@click="$router.replace({query: { ...$route.query, tab: 'Contacts'}}).catch(((e) => {}));"
						>
						Contacts
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
						<div class="title">Company Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The company no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the company while you were opening it.</li>
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
			>
				<v-tab>
					General
				</v-tab>
				<v-tab :disabled="isMakingNew">
					Projects
				</v-tab>
				<v-tab :disabled="isMakingNew">
					Contacts
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
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<!-- https://duckduckgo.com/ac/?callback=autocompleteCallback&q=a&kl=wt-wt&_=1577800653510 -->
										<v-text-field
											v-model="Name"
											autocomplete="newpassword"
											label="Name"
											id="Name"
											hint="The name of this company."
											:rules="[
												ValidateRequiredField
											]"
											class="e2e-company-editor-name"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermCompaniesCanPush()"
											>
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										
										<img v-if="LogoURI" style="max-width: 100%; max-height: 600px;" :src="LogoURI">
										
										<v-text-field
											v-if="!(LogoURI || '').startsWith('data:')"
											v-model="LogoURI"
											autocomplete="newpassword"
											label="Company Logo (Link)"
											hint="A link to the logo of this company."
											:disabled="(LogoURI || '').startsWith('data:') || connectionStatus != 'Connected'"
											:rules="[
											]"
											class="e2e-company-logo-uri"
											:readonly="!PermCompaniesCanPush()"
											>
										</v-text-field>
										<v-file-input
											prepend-icon="mdi-camera"
											accept="image/*"
											label="Company Logo (File)"
											@change="LocalFileChange"
											:rules="[
											]"
											:disabled="connectionStatus != 'Connected' || !PermCompaniesCanPush()"
											>
										</v-file-input>
										
										
										
										
										
										
										
										
										
										
										
										
										
										<!--<v-text-field v-model="LogoURI" autocomplete="newpassword" label="Logo URI" id="LogoURI" hint="A link to the logo of this company."></v-text-field>-->
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="WebsiteURI"
											autocomplete="newpassword"
											label="Website URI"
											id="WebsiteURI"
											hint="A link to the website of this company."
											class="e2e-company-editor-website-uri"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermCompaniesCanPush()"
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
											hint="The id of this company."
											>
										</v-text-field>
									</v-col>
								</v-row>



							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-alert
							border="top"
							colored-border
							type="info"
							elevation="2"
							style="margin: 15px; padding-bottom: 10px;"
							>
							Projects where {{Name}} is listed as a company.
						</v-alert>
						<ProjectList
							:showOnlyCompanyId="$route.params.id"
							:isDialogue="isDialogue"
							/>
					</v-card>
				</v-tab-item>
				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-alert
							border="top"
							colored-border
							type="info"
							elevation="2"
							style="margin: 15px; padding-bottom: 10px;"
							>
							Contacts where {{Name}} is listed as their company.
						</v-alert>
						<ContactList 
							:showOnlyCompanyId="$route.params.id"
							:isDialogue="isDialogue"
							/>
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
					:disabled="!value || connectionStatus != 'Connected' || !PermCompaniesCanDelete()"
					color="white"
					text
					rounded
					@click="DialoguesOpen({ name: 'DeleteCompanyDialogue', state: {
						redirectToIndex: true,
						id: value.id,
					}})"
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
import GenerateID from '@/Utility/GenerateID';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import CSVDownloadCompany from '@/Data/CRM/Company/CSVDownloadCompany';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Company, ICompany } from '@/Data/CRM/Company/Company';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';

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
export default class CompanyEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: ICompany | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
	};
	
	protected CSVDownloadCompany = CSVDownloadCompany;
	protected ValidateRequiredField = ValidateRequiredField;
	protected DialoguesOpen = Dialogues.Open;
	protected PermCRMReportCompaniesPDF = Company.PermCRMReportCompaniesPDF;
	protected PermCRMExportCompaniesCSV = Company.PermCRMExportCompaniesCSV;
	protected PermCompaniesCanDelete = Company.PermCompaniesCanDelete;
	protected PermCompaniesCanPush = Company.PermCompaniesCanPush;
	
	protected debounceId: ReturnType<typeof setTimeout> | null = null;
	
	constructor() {
		super();
		
	}
	
	
	
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
	
	protected get LogoURI(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.logoURI
			) {
			return null;
		}
		
		return this.value.json.logoURI;
	}
	
	protected set LogoURI(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.logoURI = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected get WebsiteURI(): string | null {
		if (!this.value ||
			!this.value.json ||
			!this.value.json.websiteURI
			) {
			return null;
		}
		
		return this.value.json.websiteURI;
	}
	
	protected set WebsiteURI(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.websiteURI = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}
	
	protected LocalFileChange(file: File): void {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		//console.log('file', file);
		
		if (!file) {
			this.LogoURI = '';
			return;
		}
		
		const reader = new FileReader();
		reader.onloadend = () => {
			//console.log('fileread', reader);
			
			const type = file.type;
			
			if (reader.result) {
				const dataUri = '' + reader.result;
				const img = document.createElement('img');
				
				//console.log(dataUri);
				
				img.addEventListener('load', () => {
					
					if (!this.value ||
						!this.value.json
						) {
						return;
					}
					
					const maxHeight = 800;
					const maxWidth = 1000;
					
					const origHeight = img.height;
					const origWidth = img.width;
					
					const maxRatioHeight = maxHeight / origHeight;
					const maxRatioWidth = maxWidth / origWidth;
					
					const smallestRatio = Math.min(maxRatioHeight, maxRatioWidth);
					
					const targetHeight = origHeight * smallestRatio;
					const targetWidth = origWidth * smallestRatio;
					
					//console.log('img height', targetHeight, 'width', targetWidth);
					
					// https://stackoverflow.com/questions/20958078/resize-a-base-64-image-in-javascript-without-using-canvas
					// create an off-screen canvas
					const canvas = document.createElement('canvas');
					const ctx = canvas.getContext('2d');
					
					if (ctx) {
						// set its dimension to target size
						canvas.width = targetWidth;
						canvas.height = targetHeight;
						
						// draw source image into the off-screen canvas:
						ctx.drawImage(img, 0, 0, targetWidth, targetHeight);
						
						// encode image to data-uri with base64 version of compressed image
						const newData = canvas.toDataURL(type, 0.92);
						
						//console.log(newData);
						this.LogoURI = newData;
					}
					
					
					
				});
				
				img.src = dataUri;
			}
			
			
			
			
		};
		reader.readAsDataURL(file);
		
		//console.log('files', file);
	}
	
	protected get Id(): string | null {
		if (!this.value ||
			!this.value.id
			) {
			return null;
		}
		
		return this.value.id;
	}
	
	
	protected DoPrint(): void {
		
		Dialogues.Open({ 
			name: 'CompanyReportDialogue', 
			state: {
				allLoadedCompanies: false,
				specificCompanies: [ {
				id: GenerateID(),
				value: this.value?.id,
				label: '',
			} ],
			},
		});
		
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
	
	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');
		
		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}
}

</script>
<style scoped>

</style>