<template>
	<v-dialog
		v-model="IsOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>Labour Report</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<v-card flat>
					<v-form ref="form">
						<v-container>
							
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col cols="12" sm="8" offset-sm="2">
									<div class="title">Labour</div>
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
										v-model="AllLoadedLabour"
										label="All Loaded Labour"
										style="margin-top:0px;"
										disabled
										>
									</v-switch>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col cols="12" sm="8" offset-sm="2">
									<div class="title">Filter By</div>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<AgentSelectField 
										v-model="FilterByAgentId"
										class="e2e-labour-report-dialogue-agent"
										/>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<ProjectSelectField 
										v-model="FilterByProjectId"
										/>
								</v-col>
							</v-row>
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="3"
									offset-sm="2"
									>
									<v-menu
										v-model="filterDateStartMenu"
										:close-on-content-click="false"
										:nudge-right="40"
										transition="scale-transition"
										offset-y
										min-width="290px"
									>
										<template v-slot:activator="{ on }">
										<v-text-field
											v-model="FilterDateStart"
											label="From"
											prepend-inner-icon="event"
											readonly
											hide-details
											
											v-on="on"
										></v-text-field>
										</template>
										<v-date-picker
											v-model="FilterDateStart"
											@input="filterDateStartMenu = false"
											:allowed-dates="FilterDateStartAllowedDates"
											:elevation="1"
											>
											<div style="text-align: center; width: 100%;">From Date</div>
										</v-date-picker>
									</v-menu>
								</v-col>
								<v-col
									cols="12"
									sm="3"
									>
									<v-menu
										v-model="filterDateEndMenu"
										:close-on-content-click="false"
										:nudge-right="40"
										transition="scale-transition"
										offset-y
										min-width="290px"
									>
										<template v-slot:activator="{ on }">
										<v-text-field
											v-model="FilterDateEnd"
											label="To"
											prepend-inner-icon="event"
											readonly
											hide-details
											
											v-on="on"
										></v-text-field>
										</template>
										<v-date-picker
											v-model="FilterDateEnd"
											@input="filterDateEndMenu = false"
											:allowed-dates="FilterDateEndAllowedDates"
											:elevation="1"
											>
											<div style="text-align: center; width: 100%;">To Date</div>
										</v-date-picker>
									</v-menu>
								</v-col>
								<v-col
									cols="12"
									sm="2"
									>
									<v-menu offset-y>
										<template v-slot:activator="{ on }">
											<v-btn
												color="primary"
												dark
												v-on="on"
												style="width: 100%; height: 100%;"
												>
												<v-icon>date_range</v-icon>
											</v-btn>
										</template>
										<v-list>
											<v-list-item @click="SetFilterDateRange(payPeriods.lastPeriodStart, payPeriods.lastPeriodEnd)">
												<v-list-item-title>Last Pay Period</v-list-item-title>
											</v-list-item>
											<v-list-item @click="SetFilterDateRange(payPeriods.currentPeriodStart, payPeriods.currentPeriodEnd)">
												<v-list-item-title>Current Pay Period</v-list-item-title>
											</v-list-item>
											<v-list-item @click="SetFilterDateRange(payPeriods.nextPeriodStart, payPeriods.nextPeriodEnd)">
												<v-list-item-title>Next Pay Period</v-list-item-title>
											</v-list-item>
											
											<div v-if="payPeriods">
												<v-divider />
												<v-list-item
													v-for="(item, index) in payPeriods.historicPeriods"
													:key="index"
													@click="SetFilterDateRange(item.start, item.end)"
													>
													<v-list-item-title>{{DateTimeToDateShortPrefixed(item.start)}} to {{DateTimeToDateShortPrefixed(item.end)}}</v-list-item-title>
												</v-list-item>
											</div>
										</v-list>
									</v-menu>
								</v-col>
							</v-row>
							<!--<v-row v-if="!AllLoadedLabour && _RenderingActive == false && _RenderingComplete == false">
								<v-col
									cols="12"
									sm="8"
									offset-sm="2"
									style="padding-top: 0px; padding-bottom: 0px;"
									>
									<ProjectSelectFieldArrayAdapter 
										v-model="SpecificProjects"
										/>
								</v-col>
							</v-row>-->
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
										v-model="IncludeLabourForOtherProjectsThatMatchAddresses"
										label="Labour for Other Projects with Matching Addresses"
										style="margin-top:0px;"
										:disabled="!FilterByProjectId"
										>
									</v-switch>
								</v-col>
							</v-row>
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
import { DateTime } from 'luxon';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import DownloadURI from '@/Utility/DownloadURI';
import AgentSelectField from '@/Components/Fields/AgentSelectField.vue';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { IGetPayPeriodsNextToDate } from '@/Utility/GetPayPeriodsNextToDate';
import GetPayPeriodsNextToDate from '@/Utility/GetPayPeriodsNextToDate';
import DateTimeToDateShortPrefixed from '@/Utility/Formatters/DateTime/DateTimeToDateShortPrefixed';
import Dialogues from '@/Utility/Dialogues';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { Reports } from '@/Data/Reports/Reports';
import { IRunReportLabourCB } from '@/Data/Reports/RPCRunReportLabour';
import { IGetPDFLaTeXTaskCB } from '@/Data/Reports/RPCGetPDFLaTeXTask';

interface LabourReportState {
	
	allLoadedLabour: boolean;
	specificLabour: Array<string | null>;
	
	filterByProjectId: string | null;
	filterByAgentId: string | null;
	
	filterDateStart: string | null;
	filterDateEnd: string | null;
	
	
	includeLabourForOtherProjectsThatMatchAddresses: boolean;
	
	_renderingActive: boolean;
	_showProgress: boolean;
	_renderingProgressMessage: string;
	_renderingComplete: boolean;
	_downloadLink: string | null;
	_errorMessage: string;
	
	
	
	
	
}

@Component({
	components: {
		AgentSelectField,
	},
})
export default class LabourReportDialogue extends DialogueBase {
	
	public static GenerateEmpty(): LabourReportState {
		
		
		return {
			allLoadedLabour: true,
			specificLabour: [null],
			
			filterByProjectId: null,
			filterByAgentId: null,
			filterDateStart: DateTime.local().minus({ years: 50 }).toFormat('yyyy-LL-dd'),
			filterDateEnd: DateTime.local().plus({ years: 50 }).toFormat('yyyy-LL-dd'),
			
			includeLabourForOtherProjectsThatMatchAddresses: true,
			
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
	protected DateTimeToDateShortPrefixed = DateTimeToDateShortPrefixed;
	
	protected payPeriods: IGetPayPeriodsNextToDate | null = null;
	protected filterDateStartMenu = false;
	protected filterDateEndMenu = false;
	
	constructor() {
		super();
		this.ModelState = LabourReportDialogue.GenerateEmpty();
	}
	
	public mounted(): void {
		this.payPeriods = GetPayPeriodsNextToDate();
	}
	
	public get AllLoadedLabour(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		return (this.ModelState as LabourReportState).allLoadedLabour;
	}
	
	public set AllLoadedLabour(flag: boolean) {
		const state = this.ModelState as LabourReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state.allLoadedLabour = flag;
		this.ModelState = state;
	}
	
	
	protected SetFilterDateRange(from: DateTime, to: DateTime): void {
		
		//console.debug('SetFilterDateRange()', from, to);
		
		this.FilterDateStart = from.toFormat('yyyy-LL-dd');
		this.FilterDateEnd = to.toFormat('yyyy-LL-dd');
	}
	
	
	protected FilterDateStartAllowedDates(val: string): boolean { // 1970-05-31
		//console.log('FilterDateStartAllowedDates', val);
		
		const validateValue = DateTime.fromFormat(val, 'yyyy-LL-dd', {
			zone: 'local',
		});
		
		if (!this.FilterDateEnd) {
			return true;
		}
		
		
		const filterEndLocal = DateTime.fromFormat(this.FilterDateEnd, 'yyyy-LL-dd', {
			zone: 'local',
		});
		
		
		return validateValue <= filterEndLocal;
	}
	protected FilterDateEndAllowedDates(val: string): boolean { // 1970-05-31
		
		const validateValue = DateTime.fromFormat(val, 'yyyy-LL-dd', {
			zone: 'local',
		});
		
		if (!this.FilterDateStart) {
			return true;
		}
		
		const filterStartLocal = DateTime.fromFormat(this.FilterDateStart, 'yyyy-LL-dd', {
			zone: 'local',
		});
		
		//console.log('FilterDateEndAllowedDates', val);
		
		return validateValue >= filterStartLocal;
		
	}
	
	
	
	
	
	
	
	get FilterByProjectId(): string | null {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return null;
		}
		const state: LabourReportState = this.ModelState;
		return state.filterByProjectId;
		
	}
	
	set FilterByProjectId(val: string | null) {
		const state: LabourReportState = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}

		state.filterByProjectId = val;
		this.ModelState = state;
	}
	
	get FilterByAgentId(): string | null {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return null;
		}
		const state: LabourReportState = this.ModelState;
		return state.filterByAgentId;
		
	}
	
	set FilterByAgentId(val: string | null) {
		const state: LabourReportState = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}

		state.filterByAgentId = val;
		this.ModelState = state;
	}
	
	get FilterDateStart(): string | null {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return null;
		}
		
		const state: LabourReportState = this.ModelState;
		return state.filterDateStart;
	}
	
	set FilterDateStart(val: string | null) {
		const state: LabourReportState = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		
		state.filterDateStart = val;
		this.ModelState = state;
	}
	
	get FilterDateEnd(): string | null {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return null;
		}
		
		const state: LabourReportState = this.ModelState;
		return state.filterDateEnd;
	}
	
	set FilterDateEnd(val: string | null) {
		const state: LabourReportState = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		
		state.filterDateEnd = val;
		this.ModelState = state;
	}
	
	
	
	
	get IncludeLabourForOtherProjectsThatMatchAddresses(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		if (!this.FilterByProjectId) {
			return false;
		}
		
		const state: LabourReportState = this.ModelState;
		return state.includeLabourForOtherProjectsThatMatchAddresses;
		
	}
	
	set IncludeLabourForOtherProjectsThatMatchAddresses(val: boolean) {
		const state: LabourReportState = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}

		state.includeLabourForOtherProjectsThatMatchAddresses = val;
		this.ModelState = state;
	}
	
	get SpecificLabour(): Array<string | null> {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return [];
		}
		const state: LabourReportState = this.ModelState;
		return state.specificLabour;
		
	}
	
	set SpecificLabour(val: Array<string | null>) {
		const state: LabourReportState = this.ModelState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}

		state.specificLabour = val;
		this.ModelState = state;
	}
	
	public get _RenderingActive(): boolean {
		if (!this.ModelState) {
			console.warn('Attempted get on null ModelState');
			return false;
		}
		return (this.ModelState as LabourReportState)._renderingActive;
	}
	
	public set _RenderingActive(flag: boolean) {
		const state = this.ModelState as LabourReportState;
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
		return (this.ModelState as LabourReportState)._renderingProgressMessage;
	}
	
	public set _RenderingProgressMessage(payload: string) {
		const state = this.ModelState as LabourReportState;
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
		return (this.ModelState as LabourReportState)._errorMessage;
	}
	
	public set _ErrorMessage(payload: string) {
		const state = this.ModelState as LabourReportState;
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
		return (this.ModelState as LabourReportState)._showProgress;
	}
	
	public set _ShowProgress(flag: boolean) {
		const state = this.ModelState as LabourReportState;
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
		return (this.ModelState as LabourReportState)._renderingComplete;
	}
	
	public set _RenderingComplete(flag: boolean) {
		const state = this.ModelState as LabourReportState;
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
		return (this.ModelState as LabourReportState)._downloadLink;
	}
	
	public set _DownloadLink(flag: string | null) {
		const state = this.ModelState as LabourReportState;
		if (!state) {
			console.warn('Attempted set on null ModelState');
			return;
		}
		state._downloadLink = flag;
		this.ModelState = state;
	}
	
	get DialogueName(): string {
		return 'LabourReportDialogue';
	}
	
	protected Close(): void {
		console.log('Close');
		
		
		//this.$refs.editor.ResetValidation();
		Dialogues.Close(this.DialogueName);
		this.ModelState = LabourReportDialogue.GenerateEmpty();
		//this.$refs.editor.SelectFirstTab();
	}
	
	protected DownloadAgain(): void {
		if (null != this._DownloadLink) {
			DownloadURI(this._DownloadLink);
		}
		
	}
	
	protected StartOver(): void {
		this.ModelState = LabourReportDialogue.GenerateEmpty();
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
			
			const specificIds = this.SpecificLabour;
			const filtered = [];
			if (false === this.AllLoadedLabour) {
				for (const specificId of specificIds) {
					if (!specificId || IsNullOrEmpty(specificId)) {
						continue;
					}
					filtered.push(specificId);
				}
			}
			
			if (false === this.AllLoadedLabour && filtered.length === 0) {
				Notifications.AddNotification({
					severity: 'error',
					message: 'No labour selected.',
					autoClearInSeconds: 10,
				});
				break;
			}
			
			
			let filterStartLocal = null;
			let filterStartStr = null;
			let filterEndLocal = null;
			let filterEndStr = null;
			
			if (null != this.FilterDateStart) {
				filterStartLocal = DateTime.fromFormat(this.FilterDateStart, 'yyyy-LL-dd', {
					zone: 'local',
				});
				filterStartStr = filterStartLocal.toISO();
			}
			if (null != this.FilterDateEnd) {
				filterEndLocal = DateTime.fromFormat(this.FilterDateEnd, 'yyyy-LL-dd', {
					zone: 'local',
				});
				filterEndLocal = filterEndLocal.set({
					hour: 23,
					minute: 59,
					second: 59,
				});
				filterEndStr = filterEndLocal.toISO();
			}
			
			const rtr = Reports.RunReportLabour.Send({
				sessionId: BillingSessions.CurrentSessionId(),
				runOnAllLabour: this.AllLoadedLabour,
				agentId: this.FilterByAgentId,
				projectId: this.FilterByProjectId,
				startISO8601: filterStartStr,
				endISO8601: filterEndStr,
				includeLabourForOtherProjectsWithMatchingAddresses: this.IncludeLabourForOtherProjectsThatMatchAddresses,
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
				rtr.completeRequestPromise.then((payload: IRunReportLabourCB) => {
					console.log('RunReportLabour returned', payload);
					
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