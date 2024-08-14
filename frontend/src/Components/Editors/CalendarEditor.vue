<template>
	<div>

		<v-app-bar v-if="showAppBar" color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="isLoadingData" :indeterminate="true" absolute top color="white">
			</v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">Calendar: {{ Name }}</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->
			<NotificationBellButton />
			<HelpMenuButton>
				<v-list-item @click="OnlineHelpFiles()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>Calendar Tutorial Pages</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item @click="OpenGoogleTutorial()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>How to Get the Web Link from Google Calendar</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item @click="OpenMicrosoft365Tutorial()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>How to Get the Web Link from Microsoft 365</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				<v-list-item @click="OpenAppleICloudCalendarTutorial()">
					<v-list-item-icon>
						<v-icon>book</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title>How to Get the Web Link from Apple iCloud Calendar</v-list-item-title>
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
						<div class="title">Calendar Not Found</div>
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
										<div class="title">Basic</div>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<!-- https://duckduckgo.com/ac/?callback=autocompleteCallback&q=a&kl=wt-wt&_=1577800653510 -->
										<v-text-field v-model="Name" autocomplete="newpassword" label="Name" id="Name"
											hint="The name of this calendar." persistent-hint :rules="[
			ValidateRequiredField
		]" class="e2e-calendar-editor-name" :disabled="connectionStatus != 'Connected' || !PermCalendarsCanPush()">
										</v-text-field>
									</v-col>
								</v-row>
								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<!-- https://duckduckgo.com/ac/?callback=autocompleteCallback&q=a&kl=wt-wt&_=1577800653510 -->
										<v-text-field v-model="ICalFileURI" autocomplete="newpassword"
											label="Web link to the calendar's &quot;ical/ics&quot; file."
											id="ICalFileURI" hint="The name of this calendar." :rules="[
			ValidateRequiredField
		]" class="e2e-calendar-editor-name" :disabled="connectionStatus != 'Connected' || !PermCalendarsCanPush()">
										</v-text-field>
										<v-alert border="left" colored-border type="info" elevation="2">
											Click on the below for instructions on how to get the web link from some
											popular
											calendar services.
											<ul>
												<li><a href="https://www.dispatchpulse.com/Support/Calendar/HowToAddAGoogleCalendarToOnCallResponder"
														target="_blank">Google Calendar</a></li>
												<li><a href="https://www.dispatchpulse.com/Support/Calendar/HowToAddAOffice365CalendarToOnCallResponder"
														target="_blank">Microsoft Office 365</a></li>
												<li><a href="https://www.dispatchpulse.com/Support/Calendar/HowToAddAAppleICloudCalendarToOnCallResponder"
														target="_blank">Apple iCloud Calendar</a></li>
											</ul>
										</v-alert>
									</v-col>
								</v-row>
								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Status</div>
									</v-col>
								</v-row>
								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
										<p>
											This calendar was last retrieved at
											<CalendarLastRetrievedChip :calendar="value" />
											.
										</p>
										<p>
											<SyncCalendarButton
												:disabled="connectionStatus != 'Connected' || !PermCalendarsCanPush()"
												@reload-status="OnReloadStatus" :calendar="value" />
										</p>
									</v-col>
								</v-row>

								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Events at a Glance</div>
									</v-col>
								</v-row>
								<v-row v-if="!isMakingNew">
									<v-col cols="12" sm="8" offset-sm="2">
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


								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<div class="title">Advanced</div>
									</v-col>
								</v-row>

								<v-row>
									<v-col cols="12" sm="8" offset-sm="2">
										<v-text-field v-model="Id" readonly="readonly" label="Unique ID"
											hint="The id of this calendar.">
										</v-text-field>
									</v-col>
								</v-row>



							</v-container>
						</v-form>
					</v-card>
				</v-tab-item>

			</v-tabs-items>
		</div>

		<DeleteCalendarDialogue v-model="deleteCalendarModel" :isOpen="deleteCalendarOpen" @Delete="SaveDeleteCalendar"
			@Cancel="CancelDeleteCalendar" ref="deleteCalendarDialogue" />

		<v-footer v-if="showFooter" color="#747389" class="white--text" app inset>
			<v-row no-gutters>
				<v-btn :disabled="!value || connectionStatus != 'Connected' || !PermCalendarsCanDelete()" color="white"
					text rounded @click="OpenDeleteCalendar()">
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
import { Calendar, ICalendar } from '@/Data/CRM/Calendar/Calendar';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import SyncCalendarButton from '@/Components/Buttons/SyncCalendarButton.vue';
import CalendarLastRetrievedChip from '@/Components/Chips/CalendarLastRetrievedChip.vue';
import { DateTime } from 'luxon';
import DeleteCalendarDialogue from '@/Components/Dialogues2/Calendars/DeleteCalendarDialogue.vue';

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
		SyncCalendarButton,
		CalendarLastRetrievedChip,
		DeleteCalendarDialogue,
	},

})
export default class CalendarEditor extends EditorBase {

	@Prop({ default: null }) declare public readonly value: ICalendar | null;
	@Prop({ default: false }) public readonly isLoadingData!: boolean;
	@Prop({ default: false }) public readonly showAppBar!: boolean;
	@Prop({ default: false }) public readonly showFooter!: boolean;
	@Prop({ default: null }) public readonly breadcrumbs!: IBreadcrumb[] | null;
	@Prop({ default: null }) declare public readonly preselectTabName: string | null;
	@Prop({ default: false }) public readonly isMakingNew!: boolean;

	public $refs!: {
		generalForm: Vue,
		calendar: Vue,
		deleteCalendarDialogue: DeleteCalendarDialogue,
	};

	protected ValidateRequiredField = ValidateRequiredField;
	protected PermCalendarsCanDelete = Calendar.PermCalendarsCanDelete;
	protected PermCalendarsCanPush = Calendar.PermCalendarsCanPush;


	protected deleteCalendarModel: ICalendar | null = null;
	protected deleteCalendarOpen = false;

	protected calendarFocus = '';
	protected events: VCalendarEvent[] = [];

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

	protected OnReloadStatus(): void {
		// if (this.$refs.regStatusChip) {
		// 	this.$refs.regStatusChip.LoadData();
		// }

		this.LoadData();
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

	protected get ICalFileURI(): string | null {

		if (!this.value ||
			!this.value.json ||
			!this.value.json.iCalFileURI
		) {
			return null;
		}

		return this.value.json.iCalFileURI;
	}

	protected set ICalFileURI(val: string | null) {

		if (!this.value ||
			!this.value.json
		) {
			return;
		}

		if (val && !IsNullOrEmpty(val)) {
			val = val.replace(/^webcal:\/\//, 'https://');
		}



		this.value.json.iCalFileURI = IsNullOrEmpty(val) ? null : val;
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

	protected MountedAfter(): void {

		console.log('MountedAfter', this.value);

		this.LoadData();


		const dtNow = DateTime.local();
		this.calendarFocus = dtNow.toFormat('yyyy-LL-dd');

	}

	@Watch('value')
	protected valueChanged(val: string, oldVal: string): void { // eslint-disable-line @typescript-eslint/no-unused-vars

		console.log('valueChanged', val);

		this.LoadData();
	}

	protected LoadData(): void {
		if (!this.value || !this.value.id) {
			return;
		}

		const gatheredEvents: VCalendarEvent[] = [];

		//console.log('getEvents', this.value?.json.occurancesRoughlyAroundThisMonth)


		do {
			if (!this.value ||
				!this.value.json ||
				!this.value.json.occurancesRoughlyAroundThisMonth) {
				break;
			}

			for (const obj of this.value.json.occurancesRoughlyAroundThisMonth) {

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
					color: 'blue',
					timed: true,
				});
			}






		} while (false);

		this.events = gatheredEvents;


		//console.log(Calendar.CalendarDataForId(this.value.id));
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



	protected OpenDeleteCalendar(): void {

		//console.debug('OpenDeleteCalendars');

		this.deleteCalendarModel = this.value;
		this.deleteCalendarOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.deleteCalendarDialogue) {
				this.$refs.deleteCalendarDialogue.SwitchToTabFromRoute();
			}
		});



	}

	protected CancelDeleteCalendar(): void {

		//console.debug('CancelCalendarCalendar');

		this.deleteCalendarOpen = false;

	}

	protected SaveDeleteCalendar(): void {

		//console.debug('SaveDeleteCalendar');



		if (!this.deleteCalendarModel || !this.deleteCalendarModel.id || IsNullOrEmpty(this.deleteCalendarModel.id)) {
			console.error('!this.deleteCalendarModel || !this.deleteCalendarModel.id || IsNullOrEmpty(this.deleteCalendarModel.id)');
			return;
		}

		const payload: string[] = [this.deleteCalendarModel.id];
		Calendar.DeleteIds(payload);

		this.deleteCalendarOpen = false;
		this.$router.push(`/section/calendars/index`).catch(((e: Error) => { }));// eslint-disable-line



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
		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}

	protected OpenGoogleTutorial(): void {
		window.open('https://www.dispatchpulse.com/Support/Calendar/HowToAddAGoogleCalendarToOnCallResponder', '_blank');
	}

	protected OpenMicrosoft365Tutorial(): void {
		window.open('https://www.dispatchpulse.com/Support/Calendar/HowToAddAOffice365CalendarToOnCallResponder', '_blank');
	}

	protected OpenAppleICloudCalendarTutorial(): void {
		window.open('https://www.dispatchpulse.com/Support/Calendar/HowToAddAAppleICloudCalendarToOnCallResponder', '_blank');
	}

}

</script>
<style scoped></style>