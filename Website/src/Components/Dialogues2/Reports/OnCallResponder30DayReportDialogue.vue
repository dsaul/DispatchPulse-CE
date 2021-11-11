<template>
	<v-dialog
		v-model="isOpen"
		persistent
		scrollable
		:fullscreen="MobileDeviceWidth()"
		>
		<v-card>
			<v-card-title>On Call Responder, Last 30 Days</v-card-title>
			<v-divider></v-divider>
			<v-card-text >
				
				<v-card flat>
					<v-form ref="form">
						<v-container>
							
							<v-row v-if="_RenderingActive == false && _RenderingComplete == false">
								<v-col cols="12" sm="8" offset-sm="2">
									This report will, provide a print out of the last 30 days' voicemail messages.
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
				<v-btn color="red darken-1" text @click="Cancel()">Cancel</v-btn>
				<v-btn color="green darken-1" text @click="Run()" :disabled="_RenderingComplete">Run</v-btn>
			</v-card-actions>
		</v-card>
	</v-dialog>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import RecordingEditor from '@/Components/Editors/RecordingEditor.vue';
import DialogueBase2 from '@/Components/Dialogues2/DialogueBase2';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import DownloadURI from '@/Utility/DownloadURI';
import { Reports } from '@/Data/Reports/Reports';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { IRunReportOnCallResponder30DayCB } from '@/Data/Reports/RPCRunReportOnCallResponder30Day';
import { IOnCallResponder30DayReportModelState } from '@/Data/Models/OnCallResponder30DayReportModelState/OnCallResponder30DayReportModelState';
import { IGetPDFLaTeXTaskCB } from '@/Data/Reports/RPCGetPDFLaTeXTask';

@Component({
	components: {
		RecordingEditor,
	},
})
export default class OnCallResponder30DayReportDialogue extends DialogueBase2 {
	
	@Prop({ default: null }) declare public readonly value: IOnCallResponder30DayReportModelState | null;
	
	public $refs!: {
		form: HTMLFormElement,
	};
	
	
	protected MobileDeviceWidth = MobileDeviceWidth;
	
	public SwitchToTabFromRoute(): void {
		//
	}
	
	protected StartOver(): void {
		this.$emit('start-over', null);
	}
	
	protected Cancel(): void {
		this.$emit('cancel', null);
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
			
			
			const rtr = Reports.RunReportOnCallResponder30Day.Send({
				sessionId: BillingSessions.CurrentSessionId(),
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
				rtr.completeRequestPromise.then((payload: IRunReportOnCallResponder30DayCB) => {
					console.log('RunReportOnCallResponder30Day returned', payload);
					
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
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		// this.$emit('run', this.value);
		
		// if (this.$refs.editor.IsValidated()) {
			
		// 	this.$emit('run', this.value);
			
		// 	this.$refs.editor.ResetValidation();
		// 	this.$refs.editor.SelectFirstTab();
		// } else {
		// 	Notifications.AddNotification({
		// 		severity: 'error',
		// 		message: 'Some of the form fields didn\'t pass validation.',
		// 		autoClearInSeconds: 10,
		// 	});
		// }
		
	}
	
	public get _RenderingActive(): boolean {
		if (!this.value) {
			return false;
		}
		return this.value._renderingActive;
	}
	
	public set _RenderingActive(flag: boolean) {
		if (!this.value) {
			return;
		}
		Vue.set(this.value, '_renderingActive', flag);
		
		this.$emit('input', this.value);
	}
	
	public get _RenderingProgressMessage(): string {
		if (!this.value) {
			return '';
		}
		return this.value._renderingProgressMessage;
	}
	
	public set _RenderingProgressMessage(payload: string) {
		if (!this.value) {
			return;
		}
		Vue.set(this.value, '_renderingProgressMessage', payload);
		this.$emit('input', this.value);
	}
	
	public get _ErrorMessage(): string {
		if (!this.value) {
			return '';
		}
		return this.value._errorMessage;
	}
	
	public set _ErrorMessage(payload: string) {
		if (!this.value) {
			return;
		}
		Vue.set(this.value, '_errorMessage', payload);
		this.$emit('input', this.value);
	}
	
	
	public get _ShowProgress(): boolean {
		if (!this.value) {
			return false;
		}
		return this.value._showProgress;
	}
	
	public set _ShowProgress(flag: boolean) {
		if (!this.value) {
			return;
		}
		Vue.set(this.value, '_showProgress', flag);
		this.$emit('input', this.value);
	}
	
	public get _RenderingComplete(): boolean {
		if (!this.value) {
			return false;
		}
		return this.value._renderingComplete;
	}
	
	public set _RenderingComplete(flag: boolean) {
		if (!this.value) {
			return;
		}
		Vue.set(this.value, '_renderingComplete', flag);
		this.$emit('input', this.value);
	}
	
	public get _DownloadLink(): string | null {
		if (!this.value) {
			return null;
		}
		return this.value._downloadLink;
	}
	
	public set _DownloadLink(flag: string | null) {
		if (!this.value) {
			return;
		}
		Vue.set(this.value, '_downloadLink', flag);
		this.$emit('input', this.value);
	}
	
	protected DownloadAgain(): void {
		if (null != this._DownloadLink) {
			DownloadURI(this._DownloadLink);
		}
		
	}
	
}
</script>