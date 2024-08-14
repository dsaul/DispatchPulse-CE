<template>

	<div>

		<v-app-bar color="#747389" dark fixed app clipped-right>
			<v-progress-linear v-if="IsLoadingData" :indeterminate="true" absolute top
				color="white"></v-progress-linear>
			<v-app-bar-nav-icon
				@click.stop="$store.state.drawers.showNavigation = !$store.state.drawers.showNavigation">
				<v-icon>menu</v-icon>
			</v-app-bar-nav-icon>

			<v-toolbar-title class="white--text">All Calendars</v-toolbar-title>

			<v-spacer></v-spacer>

			<!--<OpenGlobalSearchButton />-->

			<NotificationBellButton />
			<HelpMenuButton></HelpMenuButton>
			<ReloadButton @reload="ReLoadData()" />

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

					<v-tab @click="$router.replace({ query: { ...$route.query, tab: 'Calendars' } }).catch(((e) => { }));"
						class="e2e-calendars-index-tab-calendars">
						Calendars
					</v-tab>
				</v-tabs>
			</template>

		</v-app-bar>

		<v-breadcrumbs :items="breadcrumbs" style="background: white; padding-bottom: 5px; padding-top: 15px;">
			<template v-slot:divider>
				<v-icon>mdi-forward</v-icon>
			</template>
		</v-breadcrumbs>

		<v-alert v-if="connectionStatus != 'Connected'" type="error" elevation="2"
			style="margin-top: 10px; margin-left: 15px; margin-right: 15px;">
			Disconnected from server.
		</v-alert>

		<v-tabs-items v-model="tab" style="background: transparent;">
			<v-tab-item style="flex: 1;">
				<CalendarList ref="calendarList" :disabled="connectionStatus != 'Connected'" />
			</v-tab-item>
		</v-tabs-items>

		<div style="height: 50px;"></div>

		<AddCalendarDialogue v-model="addCalendarModel" :isOpen="addCalendarOpen" @Save="SaveAddCalendar"
			@Cancel="CancelAddCalendar" ref="addCalendarDialogue" />

		<v-footer color="#747389" class="white--text" app inset>
			<v-row no-gutters>
				<v-spacer />

				<AddMenuButton :disabled="connectionStatus != 'Connected'">
					<v-list-item @click="OpenAddCalendar()" class="e2e-add-menu-add-calendar"
						:disabled="connectionStatus != 'Connected' || !PermCalendarsCanPush()">
						<v-list-item-icon>
							<v-icon>calendar_today</v-icon>
						</v-list-item-icon>
						<v-list-item-content>
							<v-list-item-title>Calendar</v-list-item-title>
						</v-list-item-content>
					</v-list-item>
				</AddMenuButton>

			</v-row>
		</v-footer>

	</div>
</template>

<script lang="ts">

import AddMenuButton from '@/Components/Buttons/AddMenuButton.vue';
import OpenGlobalSearchButton from '@/Components/Buttons/OpenGlobalSearchButton.vue';
import HelpMenuButton from '@/Components/Buttons/HelpMenuButton.vue';
import CalendarList from '@/Components/Lists/CalendarList.vue';
import CommitSessionGlobalButton from '@/Components/Buttons/CommitSessionGlobalButton.vue';
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import SignalRConnection from '@/RPC/SignalRConnection';
import ViewBase from '@/Components/Views/ViewBase';
import ReloadButton from '@/Components/Buttons/ReloadButton.vue';
import { Calendar, ICalendar } from '@/Data/CRM/Calendar/Calendar';
import NotificationBellButton from '@/Components/Buttons/NotificationBellButton.vue';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';
import AddCalendarDialogue from '@/Components/Dialogues2/Calendars/AddCalendarDialogue.vue';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';

@Component({
	components: {
		AddMenuButton,
		CalendarList,
		OpenGlobalSearchButton,
		HelpMenuButton,
		CommitSessionGlobalButton,
		ReloadButton,
		NotificationBellButton,
		AddCalendarDialogue,
	},
})
export default class CalendarsIndex extends ViewBase {

	public $refs!: {
		calendarList: CalendarList,
		addCalendarDialogue: AddCalendarDialogue,
	};



	public tab = 0;
	public tabNameToIndex: Record<string, number> = {
		Calendars: 0,
		calendars: 0,
	};

	public breadcrumbs = [
		{
			text: 'Dashboard',
			disabled: false,
			to: '/',
		},
		{
			text: 'All Calendars',
			disabled: true,
			to: '/section/calendars/index',
		},
	];


	protected PermCalendarsCanPush = Calendar.PermCalendarsCanPush;

	protected addCalendarModel: ICalendar | null = null;
	protected addCalendarOpen = false;

	protected loadingData = false;


	public get IsLoadingData(): boolean {


		if (this.$refs.calendarList && this.$refs.calendarList.IsLoadingData) {
			return true;
		}
		return this.loadingData;
	}

	public ReLoadData(): void {

		this.LoadData();

		if (this.$refs.calendarList) {
			this.$refs.calendarList.LoadData();
		}

	}

	public LoadData(): void {

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				this.loadingData = true;

				setTimeout(() => {
					this.loadingData = false;
				}, 250);

			});
		});
	}

	protected MountedAfter(): void {
		this.SwitchToTabFromRoute();
		this.LoadData();
	}

	protected SwitchToTabFromRoute(): void {
		// Select the tab in the query string.
		if (IsNullOrEmpty(this.$route.query.tab as string | null)) {
			this.tab = 0;
		} else {
			const index = this.tabNameToIndex[this.$route.query.tab as string];
			this.tab = index;
		}
	}

	protected OpenAddCalendar(): void {

		//console.debug('OpenAddCalendars');

		this.addCalendarModel = Calendar.GetEmpty();
		this.addCalendarOpen = true;

		requestAnimationFrame(() => {
			if (this.$refs.addCalendarDialogue) {
				this.$refs.addCalendarDialogue.SwitchToTabFromRoute();
			}
		});



	}

	protected CancelAddCalendar(): void {

		//console.debug('CancelAddCalendar');

		this.addCalendarOpen = false;

	}

	protected SaveAddCalendar(): void {

		//console.debug('SaveAddCalendar');



		if (!this.addCalendarModel || !this.addCalendarModel.id || IsNullOrEmpty(this.addCalendarModel.id)) {
			console.error('!this.addCalendarModel || !this.addCalendarModel.id || IsNullOrEmpty(this.addCalendarModel.id)');
			return;
		}

		const payload: { [id: string]: ICalendar; } = {};
		payload[this.addCalendarModel.id] = this.addCalendarModel;
		Calendar.UpdateIds(payload);

		requestAnimationFrame(() => {

			if (!this.addCalendarModel) {
				return;
			}

			const contact = BillingContacts.ForCurrentSession();
			if (!contact) {
				console.error('!contact');
				return '';
			}

			const rtr = Calendar.PerformRetrieveCalendar.Send({
				calendarId: this.addCalendarModel.id || null,
				sessionId: BillingSessions.CurrentSessionId(),
			});

			if (rtr.completeRequestPromise) {
				rtr.completeRequestPromise.finally(() => {
					if (!this.addCalendarModel) {
						return;
					}

					this.addCalendarOpen = false;
					this.$router.push(`/section/calendars/${this.addCalendarModel.id}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
				});
			}

		});



	}

}
</script>
