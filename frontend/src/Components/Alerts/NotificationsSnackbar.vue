<template>
	<div style="z-index: 256;">
		<v-snackbar :value="currentSnackbarNotification != null" :timeout="-1" top :color="SnackbarColor()"
			class="notification-bell-snackbar" absolute style="position: fixed;">

			<span v-if="this.currentSnackbarNotification">{{ this.currentSnackbarNotification.message || `No
				message?`}}</span>

			<template v-slot:action="{ attrs }">
				<v-btn text v-bind="attrs" @click="ClearCurrent()">
					Clear
				</v-btn>
			</template>
		</v-snackbar>
	</div>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { INotification, Notifications } from '@/Data/Models/Notifications/Notifications';
import { DateTime } from 'luxon';

@Component({
	props: {

	},
})
export default class NotificationsSnackbar extends Vue {

	protected currentSnackbarNotification: INotification | null = null;
	protected lastSnackbarColor = 'black';
	protected visibleForSeconds = 10;
	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;

	public mounted(): void {

		this.Periodic();
		this._periodicInterval = setInterval(this.Periodic, 250);
	}

	public destroyed(): void {
		if (this._periodicInterval != null) {
			clearInterval(this._periodicInterval);
		}
	}

	public Periodic(): void {


		this.currentSnackbarNotification = this.SnackbarNotification();
		//console.debug('notification', this.currentSnackbarNotification);
	}


	protected SnackbarColor(): string {

		if (null == this.currentSnackbarNotification) {
			return this.lastSnackbarColor;
		}

		switch (this.currentSnackbarNotification.severity) {
			case 'error':
				this.lastSnackbarColor = '#ff5252';
				return this.lastSnackbarColor;
			case 'info':
				this.lastSnackbarColor = '#2196f3';
				return this.lastSnackbarColor;
			case 'warning':
				this.lastSnackbarColor = '#fb8c00';
				return this.lastSnackbarColor;
			default:
				return this.lastSnackbarColor;
		}


	}


	protected ClearCurrent(): void {
		if (!this.currentSnackbarNotification) {
			return;
		}

		Notifications.ClearId(this.currentSnackbarNotification.id);

	}



	protected SnackbarNotification(): INotification | null {

		const all = this.$store.state.Notifications.notifications as INotification[];
		//console.debug(all);
		if (!all || all.length === 0) {
			return null;
		}

		let notificationToDisplay: INotification | null = null;

		for (const notification of all) {

			if (!notification.timestamp) {
				continue;
			}

			const displayEndTime = notification.timestamp.plus({ seconds: this.visibleForSeconds });
			if (null == displayEndTime) {
				continue;
			}

			if (displayEndTime < DateTime.local()) {
				continue;
			}

			if (notificationToDisplay == null) {
				notificationToDisplay = notification;
				continue;
			}


			if (notificationToDisplay.timestamp != null &&
				(notification.timestamp > notificationToDisplay.timestamp)
			) {
				notificationToDisplay = notification;
			}
		}


		return notificationToDisplay;
	}

}
</script>
<style></style>