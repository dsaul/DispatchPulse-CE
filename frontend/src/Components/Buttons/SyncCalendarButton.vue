<template>
	<v-btn class="ma-2" :loading="loading" :disabled="disabled || loading" color="green" style="color: white;"
		@click="OnClick">
		Sync {{ calendar.json.name }}
	</v-btn>
</template>


<script lang="ts">
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { BillingSessions } from '@/Data/Billing/BillingSessions/BillingSessions';
import { Calendar, ICalendar } from '@/Data/CRM/Calendar/Calendar';
import { IPerformRetrieveCalendarCB } from '@/Data/CRM/Calendar/RPCPerformRetrieveCalendar';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {

	},
})
export default class SyncCalendarButton extends Vue {

	@Prop({ default: null }) public readonly calendar!: ICalendar | null;
	@Prop({ default: false }) public readonly disabled!: boolean;

	protected loading = false;

	protected mounted(): void {

		//

	}

	protected OnClick(): void {

		if (!this.calendar) {
			console.error('!this.calendar');
			return;
		}

		if (!this.calendar.id) {
			console.error('!this.calendar.id');
			return;
		}

		console.log('OnClick');

		const contact = BillingContacts.ForCurrentSession();
		if (!contact) {
			console.error('!contact');
			return;
		}

		this.loading = true;

		const rtr = Calendar.PerformRetrieveCalendar.Send({
			calendarId: this.calendar.id,
			sessionId: BillingSessions.CurrentSessionId(),
		});
		if (rtr.completeRequestPromise !== null) {
			rtr.completeRequestPromise.then((payload: IPerformRetrieveCalendarCB) => {
				if (payload.complete !== true) {
					Notifications.AddNotification({
						severity: 'error',
						message: 'There was an error retrieving this calendar..',
						autoClearInSeconds: 10,
					});
				}



				console.debug('IPerformRetrieveCalendarCB', payload);
			});
			rtr.completeRequestPromise.finally(() => {
				this.loading = false;
				this.$emit('reload-status');
			});
		}
	}


}
</script>