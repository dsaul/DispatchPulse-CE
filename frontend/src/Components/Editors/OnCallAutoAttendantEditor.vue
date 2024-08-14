<template>
	<div>

		<v-app-bar v-if="showAppBar" color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="isLoadingData" :indeterminate="true" absolute top color="white">
			</v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">On-Call Responder: {{ Name }}</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item @click="OnlineHelpFiles()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>On-Call Responder Tutorial Pages</v-list-item-title>
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
					<v-tab :disabled="!value || isMakingNew"
						@click="$router.replace({ query: { ...$route.query, tab: 'Calendar' } }).catch(((e) => { }));">
						Calendar
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
						<div class="title">On-Call Responder Not Found</div>
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
				<v-tab :disabled="isMakingNew">
					Calendar
				</v-tab>
			</v-tabs>

			<v-tabs-items v-model="tab" style="background: transparent;">
				<v-tab-item style="flex: 1;">
					<v-card flat>

						<v-form autocomplete="newpassword" ref="generalForm">
							<v-container>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<!-- https://duckduckgo.com/ac/?callback=autocompleteCallback&q=a&kl=wt-wt&_=1577800653510 -->
										<v-text-field v-model="Name" autocomplete="newpassword" label="Name" id="Name"
											hint="The name of this responder." persistent-hint :rules="[
			ValidateRequiredField
		]" class="e2e-responder-editor-name"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="FailoverNumber" autocomplete="newpassword"
											label="Failover Number" id="FailoverNumber"
											hint="The number to direct calls to if for whatever reason the system isn't able to answer. Enter a number like 12045554444. Don't put letters, spaces, or any punctuation. Only North American numbers are supported."
											persistent-hint :rules="[
			ValidateRequiredField
		]" class="e2e-responder-editor-failover-number"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="NoAgentResponseNotificationNumber"
											autocomplete="newpassword" label="No Agent Response Notification Number"
											id="NoAgentResponseNotificationNumber"
											hint="If no agent responds, notify this phone number. Enter a number like 12045554444. Don't put letters, spaces, or any punctuation. Only North American numbers are supported."
											persistent-hint :rules="[
			ValidateRequiredField
		]" class="e2e-responder-editor-no-agent-response-notification-number"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="NoAgentResponseNotificationEMail"
											autocomplete="newpassword" label="No Agent Response Notification E-Mail"
											id="NoAgentResponseNotificationEMail"
											hint="If no agent responds, notify this email address. If you need this to go to multiple people, create a distribution list at your mail provider."
											persistent-hint :rules="[

		]" class="e2e-responder-editor-no-agent-response-notification-email"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="MarkedHandledNotificationEMail"
											autocomplete="newpassword" label="Voicemail Handled Notification E-Mail"
											id="MarkedHandledNotificationEMail"
											hint="Once an voice message is marked as handled, notify this email address. A good use for this is sending it to your service coordinators so that they can handle this appropriately. If you need this to go to multiple people, create a distribution list at your mail provider."
											persistent-hint :rules="[

		]" class="e2e-responder-editor-marked-handled-notification-email"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Calendars</div>
										<div class="subtitle" style="font-weight: bold; margin-top: 15px;">Who gets
											called, and
											in what order?</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<CalendarSelectFieldArrayAdapter :isDialogue="isDialogue"
											:disabled="connectionStatus != 'Connected'"
											:readonly="!PermOnCallAutoAttendantsCanPush()"
											v-model="AgentOnCallPriorityCalendars" />
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle" style="font-weight: bold; margin-bottom: 15px;">Call
											attempts to
											each calendar before giving up.</div>
										<v-slider v-model="CallAttemptsToEachCalendarBeforeGivingUp" label="Minutes"
											max="5" min="1" thumb-label
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
											<template v-slot:append>
												<v-text-field v-model="CallAttemptsToEachCalendarBeforeGivingUp"
													class="mt-0 pt-0" hide-details single-line type="number"
													style="width: 60px"
													:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
												</v-text-field>
											</template>
										</v-slider>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle" style="font-weight: bold; margin-bottom: 15px;">Minutes
											between
											each call attempt.</div>
										<v-slider v-model="MinutesBetweenCallAttempts" label="Minutes" max="60" min="1"
											thumb-label
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
											<template v-slot:append>
												<v-text-field v-model="MinutesBetweenCallAttempts" class="mt-0 pt-0"
													hide-details single-line type="number" style="width: 60px"
													:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()">
												</v-text-field>
											</template>
										</v-slider>
									</v-col>
								</v-row>



								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Recordings</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle" style="font-weight: bold;">Intro Message</div>
										<MessagePromptEditor v-model="MessagePromptIntro"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()"
											pollyHint="Type a script that will be read as soon as the caller is connected. Make sure to direct them to press 1 to leave a message for the on call representative. If the system doesn't read it out exactly how you want, try adding spaces or punctuation at those spots."
											recordingHint="Upload a recording that will be played as soon as the caller is connected. Make sure to direct them to press 1 to leave a message for the on call representative." />

									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle" style="font-weight: bold;">Callback Number Message</div>
										<MessagePromptEditor v-model="MessagePromptAskForCallbackNumber"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()"
											pollyHint="Type a script that directs the caller to provide a callback number."
											recordingHint="Upload a recording that prompts the caller to leave a callback number." />
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle" style="font-weight: bold;">Details Message</div>
										<MessagePromptEditor v-model="MessagePromptAskForMessage"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()"
											pollyHint="Type a script that directs the caller to provide a recording with any other additional details."
											recordingHint="Upload a recording that prompts the caller to provide a recording with any other additional details." />
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="subtitle" style="font-weight: bold;">Thank You Afterwards</div>
										<MessagePromptEditor v-model="MessagePromptThankYouAfter"
											:disabled="connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanPush()"
											pollyHint="Type a script thanks the caller for their message, and gives any other additional information. Keep in mind that the caller might hang up when they're done the message so this might not be heard."
											recordingHint="Upload a recording that thanks their caller for their message, and gives any other additional information. Keep in mind that the caller might hang up when they're done the message so this might not be heard." />
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
											hint="The id of this responder.">
										</v-text-field>
									</v-col>
								</v-row>



							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>

				<v-tab-item style="flex: 1;">
					<v-card flat>
						<v-form autocomplete="newpassword" ref="calendarForm">
							<v-container>
								<v-row>
									<v-col cols="12">
										<v-toolbar flat>
											<v-btn outlined class="mr-4" color="grey darken-2"
												@click="SetCalendarFocusToToday">
												Today
											</v-btn>
											<v-btn icon class="ma-2" @click="$refs.calendar.prev()">
												<v-icon>mdi-chevron-left</v-icon>
											</v-btn>
											<v-spacer />
											<v-toolbar-title>
												{{ CalendarTitle }}
											</v-toolbar-title>
											<v-spacer />
											<v-btn icon class="ma-2" @click="$refs.calendar.next()">
												<v-icon>mdi-chevron-right</v-icon>
											</v-btn>
										</v-toolbar>
										<v-sheet tile height="600" width="100%">
											<v-calendar ref="calendar" v-model="calendarFocus" type="week"
												event-overlap-mode="stack" :event-overlap-threshold="30"
												:weekdays="[0, 1, 2, 3, 4, 5, 6]" :events="events" @change="LoadData"
												:event-color="GetEventColor" />
										</v-sheet>
									</v-col>
								</v-row>



							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>



			</v-tabs-items>
		</div>

		<DeleteOnCallAutoAttendantDialogue2 v-model="deleteOnCallAutoAttendantModel"
			:isOpen="deleteOnCallAutoAttendantOpen" @Delete="SaveDeleteOnCallAutoAttendant"
			@Cancel="CancelDeleteOnCallAutoAttendant" ref="deleteOnCallAutoAttendantDialogue" />

		<v-footer v-if="showFooter" color="#747389" class="white--text" app inset>
			<v-row no-gutters>
				<v-btn :disabled="!value || connectionStatus != 'Connected' || !PermOnCallAutoAttendantsCanDelete()"
					color="white" text rounded @click="OpenDeleteOnCallAutoAttendant()">
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
import _ from 'lodash';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { IOnCallAutoAttendant, OnCallAutoAttendant } from '@/Data/CRM/OnCallAutoAttendant/OnCallAutoAttendant';
import CalendarSelectFieldArrayAdapter from '@/Components/Fields/CalendarSelectFieldArrayAdapter.vue';
import { DateTime } from 'luxon';
import { Calendar, ICalendar } from '@/Data/CRM/Calendar/Calendar';
import DeleteOnCallAutoAttendantDialogue2 from '@/Components/Dialogues2/OnCallAutoAttendants/DeleteOnCallAutoAttendantDialogue2.vue';
import MessagePromptEditor from '@/Components/Editors/MessagePromptEditor.vue';
import { IMessagePrompt } from '@/Data/CRM/MessagePrompt/MessagePrompt';

// interface VCalendarDate {
// 	date: string;
// 	time: string;
// 	year: number;
// 	month: number;
// 	day: number;
// 	hour: number;
// 	minute: number;
// 	weekday: number;
// 	hasDay: boolean;
// 	hasTime: boolean;
// 	past: boolean;
// 	present: boolean;
// 	future: boolean;
// }

interface VCalendarEvent {
	name: string;
	start: Date;
	end: Date;
	color: string;
	timed: boolean;
}

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
		DeleteOnCallAutoAttendantDialogue2,
		MessagePromptEditor,
	},

})
export default class OnCallAutoAttendantEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: IOnCallAutoAttendant | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;

	public $refs!: {
		generalForm: Vue,
		deleteOnCallAutoAttendantDialogue: DeleteOnCallAutoAttendantDialogue2,
	};

	protected ValidateRequiredField = ValidateRequiredField;
	protected PermOnCallAutoAttendantsCanDelete = OnCallAutoAttendant.PermOnCallAutoAttendantsCanDelete;
	protected PermOnCallAutoAttendantsCanPush = OnCallAutoAttendant.PermOnCallAutoAttendantsCanPush;

	protected deleteOnCallAutoAttendantModel: IOnCallAutoAttendant | null = null;
	protected deleteOnCallAutoAttendantOpen = false;

	protected calendarFocus = '';
	protected events: VCalendarEvent[] = [];
	protected calendarColours = ['blue', 'indigo', 'deep-purple', 'cyan', 'green', 'orange', 'grey darken-1',
		'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black',
		'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black',
		'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black', 'black'];

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
			Calendar: 1,
			calendar: 1,
		};
	}

	protected MountedAfter(): void {

		//console.log('MountedAfter', this.value);

		this.LoadData();


		const dtNow = DateTime.local();
		this.calendarFocus = dtNow.toFormat('yyyy-LL-dd');

	}

	@Watch('value')
	protected valueChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars

		//console.log('valueChanged', val);

		this.LoadData();
	}

	protected LoadData(): void {
		if (!this.value ||
			!this.value.id ||
			!this.value.json ||
			!this.value.json.agentOnCallPriorityCalendars
		) {
			return;
		}

		//console.debug('this.value', this.value);

		const gatheredEvents: VCalendarEvent[] = [];

		const completePromises: Array<Promise<any>> = [];

		let i = -1;

		for (const calId of this.value.json.agentOnCallPriorityCalendars) {
			if (!calId || IsNullOrEmpty(calId)) {
				continue;
			}

			i++;
			const localI = i;

			const rtr = Calendar.FetchForId(calId);

			if (rtr.completeRequestPromise) {
				completePromises.push(rtr.completeRequestPromise);

				rtr.completeRequestPromise.then((cal: ICalendar) => {

					//console.log('index', localI, i);

					if (!cal ||
						!cal.json ||
						!cal.json.occurancesRoughlyAroundThisMonth) {
						return;
					}

					for (const obj of cal.json.occurancesRoughlyAroundThisMonth) {

						const dtStartDB = DateTime.fromISO(obj.startISO8601);
						const dtStartLocal = dtStartDB.toLocal();
						const dtEndDB = DateTime.fromISO(obj.endISO8601);
						let dtEndLocal = dtEndDB.toLocal();

						// If the end time is on the day break, remove one second to make it show up.
						if (dtEndLocal.hour === 0 && dtEndLocal.minute === 0 && dtEndLocal.second === 0) {
							dtEndLocal = dtEndLocal.minus({ seconds: 1 });
						}


						gatheredEvents.push({
							name: `${obj.description}\n${obj.phoneNumber}`,
							start: dtStartLocal.toJSDate(),
							end: dtEndLocal.toJSDate(),
							color: this.calendarColours[localI],
							timed: true,
						});
					}
				});

			}
		}

		Promise.all(completePromises).then(() => {
			this.events = gatheredEvents;
		});


	}



	protected GetEventColor(event: VCalendarEvent): string {
		return event.color;
	}

	protected SetCalendarFocusToToday(): void {
		const dtNow = DateTime.local();
		this.calendarFocus = dtNow.toFormat('yyyy-LL-dd');
	}

	protected get CalendarTitle(): string {
		const dt = DateTime.fromFormat(this.calendarFocus, 'yyyy-LL-dd');
		return dt.toFormat('MMMM, yyyy');
	}

	protected get CallAttemptsToEachCalendarBeforeGivingUp(): number | null {

		if (!this.value ||
			!this.value.json ||
			null === this.value.json.callAttemptsToEachCalendarBeforeGivingUp
		) {
			return null;
		}

		return this.value.json.callAttemptsToEachCalendarBeforeGivingUp;
	}

	protected set CallAttemptsToEachCalendarBeforeGivingUp(val: number | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.callAttemptsToEachCalendarBeforeGivingUp = null == val ? null : val;
		this.SignalChanged();
	}

	protected get MinutesBetweenCallAttempts(): number | null {

		if (!this.value ||
			!this.value.json ||
			null === this.value.json.minutesBetweenCallAttempts
		) {
			return null;
		}

		return this.value.json.minutesBetweenCallAttempts;
	}

	protected set MinutesBetweenCallAttempts(val: number | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.minutesBetweenCallAttempts = null == val ? null : val;
		this.SignalChanged();
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

	protected get FailoverNumber(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.failoverNumber
		) {
			return null;
		}

		return this.value.json.failoverNumber;
	}

	protected set FailoverNumber(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.failoverNumber = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get NoAgentResponseNotificationNumber(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.noAgentResponseNotificationNumber
		) {
			return null;
		}

		return this.value.json.noAgentResponseNotificationNumber;
	}

	protected set NoAgentResponseNotificationNumber(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.noAgentResponseNotificationNumber = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}






	protected get NoAgentResponseNotificationEMail(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.noAgentResponseNotificationEMail
		) {
			return null;
		}

		return this.value.json.noAgentResponseNotificationEMail;
	}

	protected set NoAgentResponseNotificationEMail(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.noAgentResponseNotificationEMail = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}



	protected get MarkedHandledNotificationEMail(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.markedHandledNotificationEMail
		) {
			return null;
		}

		return this.value.json.markedHandledNotificationEMail;
	}

	protected set MarkedHandledNotificationEMail(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		this.value.json.markedHandledNotificationEMail = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}











	protected get AgentOnCallPriorityCalendars(): Array<string | null> {

		if (!this.value) {
			return [null];
		}

		//console.log('get Calendars ', JSON.stringify(this.value.json.agentOnCallPriorityCalendars));

		const json = this.value.json;
		if (!json) {
			return [null];
		}

		const agentOnCallPriorityCalendars = json.agentOnCallPriorityCalendars;
		if (!agentOnCallPriorityCalendars || json.agentOnCallPriorityCalendars.length === 0) {
			return [null];
		}




		return agentOnCallPriorityCalendars;
	}

	protected set AgentOnCallPriorityCalendars(val: Array<string | null>) {

		//console.debug('set AgentOnCallPriorityCalendars', val, this.value);

		if (!this.value) {
			return;
		}

		const clone = _.cloneDeep(this.value) as IOnCallAutoAttendant;
		clone.json.agentOnCallPriorityCalendars = val;
		this.$emit('input', clone);

		//console.log('set AgentOnCallPriorityCalendars #2');
		//console.debug('this.value', this.value);

		// this.SignalChanged();
	}





	protected get RecordingsAskForCallbackNumberPollyText(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.recordings ||
			!this.value.json.recordings.intro ||
			!this.value.json.recordings.intro.text
		) {
			return null;
		}

		return this.value.json.recordings.askForCallbackNumber.text;
	}

	protected set RecordingsAskForCallbackNumberPollyText(val: string | null) {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.recordings ||
			!this.value.json.recordings.intro
		) {
			return;
		}

		this.value.json.recordings.askForCallbackNumber.text = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}

	protected get RecordingsAskForMessagePollyText(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.recordings ||
			!this.value.json.recordings.askForMessage ||
			!this.value.json.recordings.askForMessage.text
		) {
			return null;
		}

		return this.value.json.recordings.askForMessage.text;
	}

	protected set RecordingsAskForMessagePollyText(val: string | null) {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.recordings ||
			!this.value.json.recordings.askForMessage
		) {
			return;
		}

		this.value.json.recordings.askForMessage.text = IsNullOrEmpty(val) ? null : val;
		this.SignalChanged();
	}


	protected get RecordingsThankYouAfterPollyText(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.recordings ||
			!this.value.json.recordings.thankYouAfter ||
			!this.value.json.recordings.thankYouAfter.text
		) {
			return null;
		}

		return this.value.json.recordings.thankYouAfter.text;
	}

	protected set RecordingsThankYouAfterPollyText(val: string | null) {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.recordings ||
			!this.value.json.recordings.thankYouAfter
		) {
			return;
		}

		this.value.json.recordings.thankYouAfter.text = IsNullOrEmpty(val) ? null : val;
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

	protected OpenDeleteOnCallAutoAttendant(): void {

		this.deleteOnCallAutoAttendantModel = this.value;
		this.deleteOnCallAutoAttendantOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.deleteOnCallAutoAttendantDialogue) {
				this.$refs.deleteOnCallAutoAttendantDialogue.SwitchToTabFromRoute();
			}
		});



	}

	protected CancelDeleteOnCallAutoAttendant(): void {

		this.deleteOnCallAutoAttendantOpen = false;

	}

	protected SaveDeleteOnCallAutoAttendant(): void {

		if (!this.deleteOnCallAutoAttendantModel ||
			!this.deleteOnCallAutoAttendantModel.id ||
			IsNullOrEmpty(this.deleteOnCallAutoAttendantModel.id)) {
			console.error('SaveDeleteOnCallAutoAttendant error 1');
			return;
		}

		const payload: string[] = [this.deleteOnCallAutoAttendantModel.id];

		console.log(payload);

		OnCallAutoAttendant.DeleteIds(payload);

		this.deleteOnCallAutoAttendantOpen = false;
		this.$router.push(`/section/on-call/index`).catch(((e: Error) => { }));// eslint-disable-line



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

	protected get MessagePromptIntro(): IMessagePrompt | null {

		//console.log('MessagePromptIntro');

		if (!this.value) {
			return null;
		}

		return this.value.json.recordings.intro;
	}

	protected set MessagePromptIntro(payload: IMessagePrompt | null) {
		if (!this.value) {
			return;
		}
		if (!payload) {
			return;
		}

		this.value.json.recordings.intro = payload;
		this.SignalChanged();
	}

	protected get MessagePromptAskForCallbackNumber(): IMessagePrompt | null {

		//console.log('MessagePromptIntro');

		if (!this.value) {
			return null;
		}

		return this.value.json.recordings.askForCallbackNumber;
	}

	protected set MessagePromptAskForCallbackNumber(payload: IMessagePrompt | null) {
		if (!this.value) {
			return;
		}
		if (!payload) {
			return;
		}

		this.value.json.recordings.askForCallbackNumber = payload;
		this.SignalChanged();
	}


	protected get MessagePromptAskForMessage(): IMessagePrompt | null {

		//console.log('MessagePromptIntro');

		if (!this.value) {
			return null;
		}

		return this.value.json.recordings.askForMessage;
	}

	protected set MessagePromptAskForMessage(payload: IMessagePrompt | null) {
		if (!this.value) {
			return;
		}
		if (!payload) {
			return;
		}

		this.value.json.recordings.askForMessage = payload;
		this.SignalChanged();
	}


	protected get MessagePromptThankYouAfter(): IMessagePrompt | null {

		//console.log('MessagePromptIntro');

		if (!this.value) {
			return null;
		}

		return this.value.json.recordings.thankYouAfter;
	}

	protected set MessagePromptThankYouAfter(payload: IMessagePrompt | null) {
		if (!this.value) {
			return;
		}
		if (!payload) {
			return;
		}

		this.value.json.recordings.thankYouAfter = payload;
		this.SignalChanged();
	}

}

</script>
<style scoped></style>