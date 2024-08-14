<template>
	<CalendarEditor v-model="Calendar" ref="editor" :showAppBar="true" :showFooter="true" :breadcrumbs="Breadcrumbs"
		:preselectTabName="$route.query.tab" :isMakingNew="false" :isLoadingData="loadingData" @reload="LoadData()" />

</template>

<script lang="ts">
import { Component } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import _ from 'lodash';
import '@/plugins/store/Database';
import CalendarEditor from '@/Components/Editors/CalendarEditor.vue';
import { DateTime } from 'luxon';
import { Calendar, ICalendar } from '@/Data/CRM/Calendar/Calendar';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import ViewBase from '@/Components/Views/ViewBase';
import SignalRConnection from '@/RPC/SignalRConnection';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		CalendarEditor,
	},
})
export default class CalendarDisplay extends ViewBase {

	public $refs!: {
		editor: CalendarEditor,
	};

	protected loadingData = false;


	public LoadData(): void {

		if (!this.$route.params.id || IsNullOrEmpty(this.$route.params.id)) {
			return;
		}

		SignalRConnection.Ready(() => {
			BillingPermissionsBool.Ready(() => {

				const rtr = Calendar.FetchForId(this.$route.params.id);
				if (rtr.completeRequestPromise) {

					this.loadingData = true;

					rtr.completeRequestPromise.finally(() => {
						this.loadingData = false;
					});
				}

			});
		});
	}

	protected MountedAfter(): void {

		if (!this.$route.params.id
			|| IsNullOrEmpty(this.$route.params.id)
		) {
			this.$router.push(`/section/calendars/`).catch(((e: Error) => { }));// eslint-disable-line
		}

		this.SwitchToTabFromRoute();
		this.LoadData();
	}

	protected SwitchToTabFromRoute(): void {
		this.$refs.editor.SwitchToTabFromRoute();
	}

	protected get Breadcrumbs(): Array<{
		text: string;
		disabled: boolean;
		to: string;
	}> {
		return [
			{
				text: 'Dashboard',
				disabled: false,
				to: '/',
			},
			{
				text: 'All Calendars',
				disabled: false,
				to: '/section/calendars/index',
			},
			{
				text: 'Calendar',
				disabled: true,
				to: '',
			},
		];
	}

	get Calendar(): ICalendar | null {

		//console.debug('this.$route.params.id', this.$route.params.id);

		const cal = Calendar.ForId(this.$route.params.id);

		if (!cal) {
			return null;
		}
		return _.cloneDeep(cal) as ICalendar;
	}


	set Calendar(val: ICalendar | null) {
		if (!val) {
			return;
		}

		val.lastModifiedISO8601 = DateTime.utc().toISO();
		val.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

		const id = this.$route.params.id;
		if (!id || IsNullOrEmpty(id)) {
			console.error('!id || IsNullOrEmpty(id)');
			return;
		}

		const payload: { [id: string]: ICalendar; } = {};
		payload[id] = val;
		Calendar.UpdateIds(payload);
	}








}
</script>
