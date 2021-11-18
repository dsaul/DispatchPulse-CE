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
			
			<v-toolbar-title class="white--text">Project Note</v-toolbar-title>

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
						<div class="title">Project Note Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The project note no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the project note while you were opening it.</li>
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
									<v-col
										cols="12" sm="8" offset-sm="2"
										>
										<div class="title">General</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col
										cols="12" sm="8" offset-sm="2"
										>
										<ProjectSelectField
											:isDialogue="isDialogue"
											v-model="ProjectId"
											:showDetails="false"
											:disabled="connectionStatus != 'Connected'"
											/>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col
										cols="12" sm="8" offset-sm="2"
										>
										<AssignmentSelectField
											:isDialogue="isDialogue"
											v-model="AssignmentId"
											:showOnlyProjectId="ProjectId"
											:showDetails="false"
											:disabled="connectionStatus != 'Connected'"
											/>
									</v-col>
								</v-row>
								
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Type</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2" style="padding-top: 0px; padding-bottom: 0px;">
										<v-radio-group
											v-model="ContentType"
											style="margin-top: 0px;"
											:rules="[ ValidateRequiredField ]"
											:disabled="connectionStatus != 'Connected'"
											>
											<v-radio
												label="Text"
												value="styled-text"
												color="primary"
												>
											</v-radio>
											<v-radio
												label="Checkbox"
												value="checkbox"
												color="primary"
												>
											</v-radio>
											<v-radio
												label="Image"
												value="image"
												color="primary"
												>
											</v-radio>
											<v-radio
												label="Video"
												value="video"
												color="primary"
												>
											</v-radio>
										</v-radio-group>
									</v-col>
								</v-row>
								
								
								
								<v-row>
									<v-col
										v-if="ContentType"
										cols="12" sm="8" offset-sm="2"
										>
										<div class="title">Content</div>
									</v-col>
								</v-row>
								
								
								<v-row v-if="ContentType == 'styled-text'">
									<v-col cols="12" sm="8" offset-sm="2">
										<vue-editor
											v-model="value.json.content.html"
											:editorToolbar="customToolbar"
											>
										</vue-editor>
									</v-col>
								</v-row>
								
								<v-row v-if="ContentType == 'checkbox'">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="value.json.content.text"
											autocomplete="newpassword"
											label="Checkbox Text"
											hint="Text for the checkbox."
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
										<v-checkbox
											v-model="value.json.content.checkboxState"
											label="Checked"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-checkbox>
									</v-col>
								</v-row>
								
								<v-row v-if="ContentType == 'image'">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-if="!value.json.content.uri.startsWith('data:')"
											v-model="value.json.content.uri"
											autocomplete="newpassword"
											label="Image Link/URI"
											hint="Enter the link/uri to the image."
											:disabled="value.json.content.uri.startsWith('data:') || connectionStatus != 'Connected'"
											:rules="[
												ValidateRequiredField
											]"
											>
										</v-text-field>
										<v-file-input
											prepend-icon="mdi-camera"
											accept="image/*"
											label="Local File"
											@change="LocalFileChange"
											:rules="[
												ValidateRequiredField
											]"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-file-input>
										
										<v-img v-if="value.json.content.uri" :src="value.json.content.uri" contain></v-img>
									</v-col>
								</v-row>
								
								<v-row v-if="ContentType == 'video'">
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field
											v-model="value.json.content.uri"
											autocomplete="newpassword"
											label="Video Link/URI"
											hint="Enter the link/uri to the video."
											:disabled="connectionStatus != 'Connected'"
											>
										</v-text-field>
										
										
										<div v-if="IsYoutube">
											<iframe
												style="width: 100%; height: 480px; border: none;"
												allowfullscreen
												:src="`https://www.youtube.com/embed/${YouTubeVideoID}?rel=0`"></iframe>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.mp4')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/mp4">
											</video>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.m4v')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/mp4">
											</video>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.mov')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/quicktime">
											</video>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.ogg')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/ogg">
											</video>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.avi')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/avi">
											</video>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.mkv')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/mkv">
											</video>
										</div>
										<div v-else-if="value.json.content.uri.endsWith('.webm')">
											<video controls preload="none" style="max-width: 100%; max-height: 480px;">
												<source :src="value.json.content.uri" type="video/webm">
											</video>
										</div>
										
										
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Privacy</div>
									</v-col>
								</v-row>
								
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-switch
											v-model="InternalOnly"
											label="Internal Only"
											:disabled="connectionStatus != 'Connected'"
											>
										</v-switch>
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
											hint="The id of this project note."
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
					@click="DialoguesOpen({ name: 'DeleteProjectNoteDialogue', state: {
						redirectToIndex: false,
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
//https://www.npmjs.com/package/vue2-editor
import Dialogues from '@/Utility/Dialogues';
import EditorBase, { IBreadcrumb, VForm } from './EditorBase';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component, Vue, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { VueEditor } from 'vue2-editor';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import { Assignment } from '@/Data/CRM/Assignment/Assignment';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IProjectNote } from '@/Data/CRM/ProjectNote/ProjectNote';
import { IProjectNoteStyledText } from '@/Data/CRM/ProjectNoteStyledText/ProjectNoteStyledText';
import { IProjectNoteCheckbox } from '@/Data/CRM/ProjectNoteCheckbox/ProjectNoteCheckbox';
import { IProjectNoteImage } from '@/Data/CRM/ProjectNoteImage/ProjectNoteImage';
import { IProjectNoteVideo } from '@/Data/CRM/ProjectNoteVideo/ProjectNoteVideo';

@Component({
	components: {
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		VueEditor,
		ReloadButton,
		NotificationBellButton,
	},
	
})
export default class ProjectNoteEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IProjectNote | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;
	
	public $refs!: {
		generalForm: Vue,
	};
	
	protected ValidateRequiredField = ValidateRequiredField;
	protected DialoguesOpen = Dialogues.Open;
	
	protected editortest = 'asd';
	protected customToolbar = [
		[{ header: [1, 2, 3, 4, 5, 6, false] }],
		[{ font: [] }],
		['bold', 'italic', 'underline', 'strike'], 
		[{ script: 'sub'}, { script: 'super' }], 
		[{ align: [] }],
		[{ list: 'ordered' }, { list: 'bullet' }, { list: 'check' }], ['code-block', 'blockquote'],
		[{ color: [] }, { background: [] }],
		['link'],
		['clean'],
	];
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
		};
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
		
		// Make sure assignment is also on project, otherwise remove the assignment.
		if (this.AssignmentId && !IsNullOrEmpty(this.AssignmentId)) {
			
			const assignment = Assignment.ForId(this.AssignmentId);
			if (assignment) {
				if (assignment.json.projectId !== val) {
					this.AssignmentId = null;
				}
			}
		}
		
		
		
		
		
		this.SignalChanged();
	}
	
	protected get AssignmentId(): string | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.assignmentId
			) {
			return null;
		}
		
		return this.value.json.assignmentId;
		
	}
	
	protected set AssignmentId(val: string | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.assignmentId = IsNullOrEmpty(val) ? null : val;
		
		this.SignalChanged();
	}
	
	
	protected get InternalOnly(): boolean {
		
		if (!this.value ||
			!this.value.json
			) {
			return false;
		}
		
		return !!this.value.json.internalOnly;
		
	}
	
	
	protected set InternalOnly(flag: boolean) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.internalOnly = !!flag;
		
		this.SignalChanged();
	}
	
	
	
	
	
	
	protected get ContentType(): 'styled-text' | 'checkbox' | 'image' | 'video' | null {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.contentType
			) {
			return null;
		}
		
		return this.value.json.contentType;
	}
	
	protected set ContentType(val: 'styled-text' | 'checkbox' | 'image' | 'video' | null) {
		
		if (!this.value ||
			!this.value.json
			) {
			return;
		}
		
		this.value.json.contentType = IsNullOrEmpty(val) ? null : val;
		
		switch (this.value.json.contentType) {
			case 'styled-text':
				
				this.value.json.content = {
					html: '',
				} as IProjectNoteStyledText;
				
				break;
			case 'checkbox':
				this.value.json.content = {
					text: '',
					checkboxState: false,
				} as IProjectNoteCheckbox;
				break;
			case 'image':
				this.value.json.content = {
					uri: '',
				} as IProjectNoteImage;
				break;
			case 'video':
				this.value.json.content = {
					uri: '',
				} as IProjectNoteVideo;
				break;
		}
		
		
		
		this.SignalChanged();
	}
	
	protected LocalFileChange(file: File): void {
		
		if (!this.value ||
			!this.value.json ||
			!this.value.json.content ||
			this.value.json.contentType !== 'image'
			) {
			return;
		}
		
		//console.log('file', file);
		
		if (!file) {
			(this.value.json.content as IProjectNoteImage).uri = '';
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
						!this.value.json ||
						!this.value.json.content ||
						this.value.json.contentType !== 'image'
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
						(this.value.json.content as IProjectNoteImage).uri = newData;
					}
					
					
					
				});
				
				img.src = dataUri;
			}
			
			
			
			
		};
		reader.readAsDataURL(file);
		
		//console.log('files', file);
	}
	
	
	protected get IsYoutube(): boolean {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.content ||
			this.value.json.contentType !== 'video' || 
			!(this.value.json.content as IProjectNoteVideo).uri || 
			IsNullOrEmpty((this.value.json.content as IProjectNoteVideo).uri)
			) {
			return false;
		}
		
		return -1 !== (this.value.json.content as IProjectNoteVideo).uri.indexOf('youtube');
	}
	
	protected get YouTubeVideoID(): string | null {
		
		if (!this.value ||
			!this.value.json || 
			!this.value.json.content ||
			this.value.json.contentType !== 'video' || 
			!(this.value.json.content as IProjectNoteVideo).uri || 
			IsNullOrEmpty((this.value.json.content as IProjectNoteVideo).uri)
			) {
			return null;
		}
		
		const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
		const match = (this.value.json.content as IProjectNoteVideo).uri.match(regExp);
	
		if (match && match[2].length === 11) {
			return match[2];
		} else {
			return null;
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