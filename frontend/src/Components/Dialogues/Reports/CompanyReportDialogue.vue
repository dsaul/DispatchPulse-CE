<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Company Report</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<v-card flat>
					<v-form ref="form">
						<v-container>
							
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col cols="12" sm="8" offset-sm="2">
									<div class="title">Companies</div>
								</v-col>
							</v-row>
							
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<v-switch
										v-model="AllLoadedCompanies"
										label="All Loaded Companies"
										style="margin-top:0px;"
										>
									</v-switch>
								</v-col>
							</v-row>
							<v-row v-if="!AllLoadedCompanies && _RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<LabeledCompanyEditRowArrayAdapter 
										:hideLabel="true"
										v-model="SpecificCompanies"
										/>
								</v-col>
							</v-row>
							
							
							
							<!--
							
							
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col cols="12" sm="8" offset-sm="2">
									<div class="title">Include</div>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<v-switch
										v-model="IncludeCompanies"
										label="Companies"
										style="margin-top:0px;"
										>
									</v-switch>
									<v-switch
										v-model="IncludeContacts"
										label="Contacts"
										style="margin-top:0px;"
										>
									</v-switch>
									<v-switch
										v-model="IncludeSchedule"
										label="Schedule"
										style="margin-top:0px;"
										>
									</v-switch>
									<v-switch
										v-model="IncludeNotes"
										label="Notes"
										style="margin-top:0px;"
										>
									</v-switch>
									<v-switch
										v-model="IncludeLabour"
										label="Labour"
										style="margin-top:0px;"
										>
									</v-switch>
									<v-switch
										v-model="IncludeMaterials"
										label="Materials"
										style="margin-top:0px;"
										>
									</v-switch>
								</v-col>
							</v-row>-->
							
							<v-row v-if="_ErrorMessage">
								<v-col cols="12" sm="8" offset-sm="2">
									<v-alert
										type="error"
										colored-border
										border="bottom"
										elevation="2"
										>
										{{_ErrorMessage}}
									</v-alert>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive">
								<v-col cols="12" sm="8" offset-sm="2">
									<div class="title">Processing&hellip;</div>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<v-progress-linear
										indeterminate
										v-model="_RenderingProgressMessage"
										height="25"
										>
										<template v-slot:default="{ value }">
											<strong>{{value}}</strong>
										</template>
									</v-progress-linear>
								</v-col>
							</v-row>
							
							<v-row v-if="_RenderingComplete">
								<v-col cols="12" sm="8" offset-sm="2">
									<div class="title">Complete</div>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingComplete">
								<v-col cols="12" sm="8" offset-sm="2">
									<v-btn large @click="DownloadAgain()" color="primary">Download Again</v-btn>
								</v-col>
							</v-row>
							
							
						</v-container>
					</v-form>
				</v-card>
				

			</v-card-text>
			<v-divider></v-divider>
			<v-card-actions>
				<v-btn color="red darken-1" text @click="StartOver()">Start Over</v-btn>
				<v-spacer/>
				<v-btn color="red darken-1" text @click="Close()">Close</v-btn>
				<v-btn color="green darken-1" text @click="Run()" :disabled="_RenderingComplete">Run</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import DialogueBase from '@/Components/Dialogues/DialogueBase';
import GenerateID from '@/Utility/GenerateID';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import DownloadURI from '@/Utility/DownloadURI';
import LabeledCompanyEditRowArrayAdapter from '@/Components/Rows/LabeledCompanyEditRowArrayAdapter.vue';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { Reports } from '@/Data/Reports/Reports';
import { IRunReportCompaniesCB } from '@/Data/Reports/RPCRunReportCompanies';
import { ILabeledCompanyId } from '@/Data/Models/LabeledCompanyId/LabeledCompanyId';
import { IGetPDFLaTeXTaskCB } from '@/Data/Reports/RPCGetPDFLaTeXTask';

interface CompanyReportState {
	allLoadedCompanies: boolean;
	specificCompanies: ILabeledCompanyId[];
	
	_renderingActive: boolean;
	_showProgress: boolean;
	_renderingProgressMessage: string;
	_renderingComplete: boolean;
	_downloadLink: string | null;
	_errorMessage: string;
}

@Component({
	components: {
		LabeledCompanyEditRowArrayAdapter,
	},
})
export default class CompanyReportDialogue extends DialogueBase {
	
	public static GenerateEmpty(): CompanyReportState {
		
		
		return {
			allLoadedCompanies: true,
			specificCompanies: [{
				id: GenerateID(),
				value: '',
				label: '',
			}],
			
			_renderingActive: false,
			_showProgress: false,
			_renderingComplete: false,
			_downloadLink: null,
			_renderingProgressMessage: '',
			_errorMessage: '',
		};
	}
	
	public $refs!: {
		form: HTMLFormElement,
	};
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	constructor() {
		super();
		this.ModelState = CompanyReportDialogue.GenerateEmpty();
	}
	
	public get AllLoadedCompanies(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		return (this.ModelState as CompanyReportState).allLoadedCompanies;
	}
	
	public set AllLoadedCompanies(flag: boolean) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state.allLoadedCompanies = flag;
		this.ModelState = state;
	}
	
	get SpecificCompanies(): ILabeledCompanyId[] {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return [];
		}
		const state = this.ModelState;
		return state.specificCompanies;
		
	}
	
	set SpecificCompanies(val: ILabeledCompanyId[]) {
		const state = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state.specificCompanies = val;
		this.ModelState = state;
	}
	
	public get _RenderingActive(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		return (this.ModelState as CompanyReportState)._renderingActive;
	}
	
	public set _RenderingActive(flag: boolean) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._renderingActive = flag;
		this.ModelState = state;
	}
	
	public get _RenderingProgressMessage(): string {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return '';
		}
		return (this.ModelState as CompanyReportState)._renderingProgressMessage;
	}
	
	public set _RenderingProgressMessage(payload: string) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._renderingProgressMessage = payload;
		this.ModelState = state;
	}
	
	public get _ErrorMessage(): string {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return '';
		}
		return (this.ModelState as CompanyReportState)._errorMessage;
	}
	
	public set _ErrorMessage(payload: string) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._errorMessage = payload;
		this.ModelState = state;
	}
	
	public get _ShowProgress(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		return (this.ModelState as CompanyReportState)._showProgress;
	}
	
	public set _ShowProgress(flag: boolean) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._showProgress = flag;
		this.ModelState = state;
	}
	
	public get _RenderingComplete(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		return (this.ModelState as CompanyReportState)._renderingComplete;
	}
	
	public set _RenderingComplete(flag: boolean) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._renderingComplete = flag;
		this.ModelState = state;
	}
	
	public get _DownloadLink(): string | null {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return null;
		}
		return (this.ModelState as CompanyReportState)._downloadLink;
	}
	
	public set _DownloadLink(flag: string | null) {
		const state = this.ModelState as CompanyReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._downloadLink = flag;
		this.ModelState = state;
	}
	
	
	get DialogueName(): string {
		return 'CompanyReportDialogue';
	}
	
	protected Close(): void {
		console.log('Close');
		
		
		//this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = CompanyReportDialogue.GenerateEmpty();
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected DownloadAgain(): void {
		if (null != this._DownloadLink) {
			DownloadURI(this._DownloadLink);
		}
		
	}
	
	protected StartOver(): void {
		this.ModelState = CompanyReportDialogue.GenerateEmpty();
	}
	
	protected Run(): void {
		
		do {
			
			this._ErrorMessage = '';
			
			if (!this.$refs.form.validate()) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'Some of the form fields didn\'t pass validation.',
					autoClearInSeconds: 10,
				});
				break;
			}
			
			const specificIds = this.SpecificCompanies;
			const filtered: string[] = [];
			if (false === this.AllLoadedCompanies) {
				for (const specificId of specificIds) {
					if (!specificId || !specificId.value || IsNullOrEmpty(specificId.value)) {
						continue;
					}
					filtered.push(specificId.value);
				}
			}
			
			if (false === this.AllLoadedCompanies && filtered.length === 0) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'No companies selected.',
					autoClearInSeconds: 10,
				});
				break;
			}
			
			
			
			const rtr = Reports.RunReportCompanies.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				runOnAllCompanies: this.AllLoadedCompanies,
				companyIds: filtered,
			});
			
			if (rtr.completeRequestPromise) {
			
				this._RenderingActive = true;
				this._ShowProgress = true;
				this._RenderingProgressMessage = 'Sending Request…';
				
				rtr.completeRequestPromise.catch((e: Error) => {
					this._RenderingActive = false;
					this._ShowProgress = false;
					this._RenderingProgressMessage = '';
					this._ErrorMessage = e.message;
					throw e;
				});
				rtr.completeRequestPromise.then((payload: IRunReportCompaniesCB) => {
					console.log('RunReportCompanies returned', payload);
					
					if (payload.isError) {
						this._RenderingActive = false;
						this._ShowProgress = false;
						this._ErrorMessage = payload.errorMessage;
						return;
					}
					
					const taskId = payload.taskId;
					if (!taskId) {
						this._RenderingActive = false;
						this._ShowProgress = false;
						this._ErrorMessage = 'Did not get task ID from server.';
						return;
					}
					
					this._RenderingProgressMessage = 'Waiting…';
					
					
					const fn = () => {
						
						const rtrComplete = Reports.GetPDFLaTeXTask.Send({
							sessionId: BillingSessions.CurrentSessionId(), 
							taskId,
						});
						if (rtrComplete.completeRequestPromise) {
							rtrComplete.completeRequestPromise.catch((e: Error) => {// eslint-disable-line @typescript-eslint/no-unused-vars
								this._RenderingActive = false;
								this._ShowProgress = false;
								this._ErrorMessage = 'Error during processing.';
							});
							rtrComplete.completeRequestPromise.then((ltxPld: IGetPDFLaTeXTaskCB) => {
								
								if (false === ltxPld.isCompleted) {
									this._RenderingProgressMessage = `Processing (${ltxPld.status})…`;
									
									if (ltxPld.status === 'Error') {
										this._ErrorMessage = ltxPld.errorMessage;
									}
									
									if (ltxPld.status !== 'Error') {
										setTimeout(fn, 250);
									}
									return;
								}
								
								if (null === ltxPld.tempLink || IsNullOrEmpty(ltxPld.tempLink)) {
									
									this._RenderingActive = false;
									this._ShowProgress = false;
									this._ErrorMessage = 'Completed, but didn\'t get a link to download.';
									return;
								}
								
								this._RenderingActive = false;
								this._ShowProgress = false;
								this._RenderingComplete = true;
								this._DownloadLink = ltxPld.tempLink;
								DownloadURI(this._DownloadLink);
								
							});
						}
						
						
					};
					
					setTimeout(fn, 250);
					
				});
				
				//
				
			}
			
		} while (false);
		
		
		
		
		
	}
	
	
}
</script>