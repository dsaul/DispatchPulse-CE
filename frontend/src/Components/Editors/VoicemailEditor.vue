<template>
	<div>

		<v-app-bar v-if="showAppBar" color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="isLoadingData" :indeterminate="true" absolute top color="white">
			</v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">Voicemail</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item @click="OnlineHelpFiles()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Voicemail Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
			</HelpMenuButton>
			<ReloadButton @reload="$emit('reload')" />

			<!--<CommitSessionGlobalButton />-->

			<v-menu bottom left offset-y>
				<template v-slot:activator="{ on }">
					<v-btn dark icon v-on="on">
						<v-icon>more_vert</v-icon>
					</v-btn>
				</template>

				<v-list dense>
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
						<div class="title">Voicemail Not Found</div>
					</v-col>
				</v-row>
				<v-row>
					<v-col cols="12" sm="8" offset-sm="2">
						This could be for several reasons:
						<ul>
							<li>The page hasn't finished loading.</li>
							<li>The calendar no longer exists and this is an old bookmark.</li>
							<li>Someone deleted the calendar while you were opening it.</li>
							<li>There is trouble connecting to the internet.</li>
							<li>Your mobile phone is in a place with a poor connection.</li>
							<li>Other reasons the app can't connect to Dispatch Pulse.</li>
						</ul>
					</v-col>
				</v-row>
			</v-container>
		</div>
		<div v-else>

			<v-tabs v-if="!showAppBar" v-model="tab" background-color="transparent" grow show-arrows>
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
										<div class="title">Caller Information</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">




										<v-text-field v-model="MessageLeftAt" label="Message Left At" :readonly="true">
										</v-text-field>
										<v-text-field v-model="CallerIdName" label="Caller ID Name" :readonly="true">
										</v-text-field>
										<v-text-field v-model="CallerIdNumber" label="Caller ID Number"
											:readonly="true">
										</v-text-field>
										<v-text-field v-model="CallbackNumber" label="Callback Number" :readonly="true">
										</v-text-field>






									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Recording</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<S3VoicemailPlayer :voicemail="value" />
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Actions</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-btn class="ma-2" color="" :disabled="!value || IsHandled"
											@click="MarkAsHandled" :loading="MarkAsHandledLoading">
											Mark as Handled
										</v-btn>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Additional Details</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-radio-group v-model="Type" :readonly="true" dense>
											<template v-slot:label>
												<div><strong>Voicemail Type</strong></div>
											</template>
											<v-radio label="Unknown" :value="null">
											</v-radio>
											<v-radio label="On-Call Responder" value="OnCall">
											</v-radio>
										</v-radio-group>

										<OnCallAutoAttendantSelectField v-if="Type == 'OnCall'"
											v-model="OnCallAutoAttendantId" :readonly="true" />
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Audit Timeline</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">

										<v-timeline align-top dense>

											<v-timeline-item v-for="(entry, index) in Timeline" :key="index"
												:color="entry.colour" small>
												<div v-if="entry.type === 'text'">
													<v-card-title
														style="margin: 0px; padding: 0px; word-break: normal; font-size: 1rem">{{ entry.description }}</v-card-title>
													<v-card-subtitle
														style="margin: 0px; padding: 0px; word-break: normal;">{{ ISO8601ToLocalDatetimeSuperSpecific(entry.timestampISO8601) }}</v-card-subtitle>

												</div>
											</v-timeline-item>

										</v-timeline>

									</v-col>
								</v-row>

							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>





			</v-tabs-items>
		</div>

		<DeleteVoicemailDialogue v-model="deleteVoicemailModel" :isOpen="deleteVoicemailOpen"
			@Delete="SaveDeleteVoicemail" @Cancel="CancelDeleteVoicemail" ref="deleteVoicemailDialogue" />

		<v-footer v-if="showFooter" color="#747389" class="white--text" app inset>
			<v-row no-gutters>
				<v-btn :disabled="!value || connectionStatus != 'Connected' || !PermVoicemailsCanDelete()" color="white"
					text rounded @click="OpenDeleteVoicemail()">
					<v-icon left>delete</v-icon>
					Delete
				</v-btn>
			</v-row>
		</v-footer>
	</div>
</template>
<script lang="ts">
import EditorBase, { IBreadcrumb, VForm } from './EditorBase';
import ProjectList from '@/Components/Lists/ProjectList.vue';
import ContactList from '@/Components/Lists/ContactList.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component, Vue, Prop, Watch } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IVoicemail, IVoicemailTimelineItem, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import CalendarSelectFieldArrayAdapter from '@/Components/Fields/CalendarSelectFieldArrayAdapter.vue';
import DeleteVoicemailDialogue from '@/Components/Dialogues2/Voicemails/DeleteVoicemailDialogue.vue';
import OnCallAutoAttendantSelectField from '@/Components/Fields/OnCallAutoAttendantSelectField.vue';
import { guid } from '@/Utility/GlobalTypes';
import ISO8601ToLocalDatetimeSuperSpecific from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDatetimeSuperSpecific';
import ISO8601ToLocalDatetime from '@/Utility/Formatters/ISO8601/ISO8601ToLocalDatetime';
import S3VoicemailPlayer from '@/Components/Media/S3VoicemailPlayer.vue';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { IPerformGetVoicemailRecordingLinkCB } from '@/Data/CRM/Voicemail/RPCPerformGetVoicemailRecordingLink';

@Component({
	components: {
		ProjectList,
		ContactList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		CalendarSelectFieldArrayAdapter,
		DeleteVoicemailDialogue,
		OnCallAutoAttendantSelectField,
		S3VoicemailPlayer,
	},

})
export default class VoicemailEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IVoicemail | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;

	public $refs!: {
		generalForm: Vue,
		deleteVoicemailDialogue: DeleteVoicemailDialogue,
	};

	protected ValidateRequiredField = ValidateRequiredField;
	protected PermVoicemailsCanDelete = Voicemail.PermVoicemailsCanDelete;
	protected PermVoicemailsCanPush = Voicemail.PermVoicemailsCanPush;
	protected ISO8601ToLocalDatetimeSuperSpecific = ISO8601ToLocalDatetimeSuperSpecific;
	protected ISO8601ToLocalDatetime = ISO8601ToLocalDatetime;

	protected deleteVoicemailModel: IVoicemail | null = null;
	protected deleteVoicemailOpen = false;

	protected MarkAsHandledLoading = false;

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
		};
	}

	protected MountedAfter(): void {

		//console.log('MountedAfter', this.value);

		this.LoadData();
		//console.log('value, ', this.value);
	}

	@Watch('value')
	protected valueChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars

		//console.log('valueChanged', val);

		this.LoadData();
	}

	protected LoadData(): void {
		if (!this.value ||
			!this.value.id ||
			!this.value.json
		) {
			return;
		}



	}




	protected get Type(): null | 'OnCall' {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.type
		) {
			return null;
		}

		return this.value.json.type;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set Type(val: null | 'OnCall') {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		//this.value.json.type = IsNullOrEmpty(val) ? null : val;
		//this.SignalChanged();
	}







	protected get MessageLeftAt(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.messageLeftAtISO8601
		) {
			return null;
		}

		console.log('this.value.json.messageLeftAtISO8601', this.value.json.messageLeftAtISO8601);

		return ISO8601ToLocalDatetime(this.value.json.messageLeftAtISO8601);
		//return this.value.json.messageLeftAtISO8601;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set MessageLeftAt(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		// this.value.json.messageLeftAtISO8601 = IsNullOrEmpty(val) ? null : val;
		// this.SignalChanged();
	}





	protected get CallerIdName(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.callerIdName ||
			IsNullOrEmpty(this.value.json.callerIdName)
		) {
			return 'None';
		}

		return this.value.json.callerIdName;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set CallerIdName(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		// this.value.json.callerIdName = IsNullOrEmpty(val) ? null : val;
		// this.SignalChanged();
	}

	protected get CallerIdNumber(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.callerIdNumber ||
			IsNullOrEmpty(this.value.json.callerIdNumber)
		) {
			return 'None';
		}

		return this.value.json.callerIdNumber;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set CallerIdNumber(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		// this.value.json.callerIdNumber = IsNullOrEmpty(val) ? null : val;
		// this.SignalChanged();
	}

	protected get CallbackNumber(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.callbackNumber ||
			IsNullOrEmpty(this.value.json.callbackNumber)
		) {
			return 'None';
		}

		return this.value.json.callbackNumber;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set CallbackNumber(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		// this.value.json.callbackNumber = IsNullOrEmpty(val) ? null : val;
		// this.SignalChanged();
	}


	protected get OnCallAutoAttendantId(): guid | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.onCallAutoAttendantId
		) {
			return null;
		}

		//console.log('OnCallAutoAttendantId', this.value.json.onCallAutoAttendantId);

		return this.value.json.onCallAutoAttendantId;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set OnCallAutoAttendantId(val: guid | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		// this.value.json.onCallAutoAttendantId = IsNullOrEmpty(val) ? null : val;
		// this.SignalChanged();
	}

	protected get Timeline(): IVoicemailTimelineItem[] | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.timeline
		) {
			return null;
		}

		//console.log('OnCallAutoAttendantId', this.value.json.onCallAutoAttendantId);

		return this.value.json.timeline;
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected set Timeline(val: IVoicemailTimelineItem[] | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		console.error('You can\'t set this value.');

		// this.value.json.timeline = IsNullOrEmpty(val) ? null : val;
		// this.SignalChanged();
	}

	protected get IsHandled(): boolean {
		if (!this.value ||
			!this.value.json) {
			return false;
		}

		return this.value.json.isMarkedHandled || false;
	}



	protected MarkAsHandled(): void {

		const sessionId = BillingSessions.CurrentSessionId();
		if (!sessionId) {
			console.error('!sessionId');
			return;
		}

		if (!this.value) {
			console.error('!this.value');
			return;
		}

		if (!this.value.id || IsNullOrEmpty(this.value.id)) {
			console.error('!this.value.id || IsNullOrEmpty(this.value.id)');
			return;
		}

		this.MarkAsHandledLoading = true;

		const rtr = Voicemail.PerformVoicemailMarkAsHandled.Send({
			sessionId,
			voicemailId: this.value.id,
		});
		if (rtr.completeRequestPromise) {
			// eslint-disable-next-line @typescript-eslint/no-unused-vars
			rtr.completeRequestPromise.then((payload: IPerformGetVoicemailRecordingLinkCB) => {
				console.debug('marked as handled');
			});
			rtr.completeRequestPromise.finally(() => {
				this.MarkAsHandledLoading = false;
			});
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

	protected OpenDeleteVoicemail(): void {

		this.deleteVoicemailModel = this.value;
		this.deleteVoicemailOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.deleteVoicemailDialogue) {
				this.$refs.deleteVoicemailDialogue.SwitchToTabFromRoute();
			}
		});



	}

	protected CancelDeleteVoicemail(): void {

		this.deleteVoicemailOpen = false;

	}

	protected SaveDeleteVoicemail(): void {

		if (!this.deleteVoicemailModel ||
			!this.deleteVoicemailModel.id ||
			IsNullOrEmpty(this.deleteVoicemailModel.id)) {
			console.error('SaveDeleteVoicemail error 1');
			return;
		}

		const payload: string[] = [this.deleteVoicemailModel.id];

		console.log(payload);

		Voicemail.DeleteIds(payload);

		this.deleteVoicemailOpen = false;
		this.$router.push(`/section/voicemails/index`).catch(((e: Error) => { }));// eslint-disable-line



	}


	protected SignalChanged(): void {

		// Debounce

		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}

		this.debounceId = setTimeout(() => {
			console.debug('save changes');
			this.$emit('input', this.value);
		}, 1000);
	}

	protected OnlineHelpFiles(): void {
		//console.log('OpenOnlineHelp()');

		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}
}

</script>
<style scoped></style>