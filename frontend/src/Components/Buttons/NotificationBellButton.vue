<template>
	<div class="notification-bell-button">
		
		<v-menu
			bottom left offset-y>
			<template v-slot:activator="{ on }">
				<v-badge
					:content="currentNotificationCount"
					:value="currentNotificationCount"
					color="red"
					overlap
					offset-x="25"
					offset-y="25"
					>
					<v-btn
						dark
						icon
						v-on="on"
						>
						<v-icon>notifications</v-icon>
					</v-btn>
				</v-badge>
			</template>

			<v-list dense>
				
				
				<v-list-item
					v-if="currentNotificationCount != 0"
					>
					<v-list-item-content>
						<v-list-item-title>Notifications</v-list-item-title>
					</v-list-item-content>
					<v-list-item-action>
						<v-btn
							text
							small
							@click="DoClearAll()"
							:disabled="currentNotificationCount == 0"
							>
							Clear All
						</v-btn>
						
					</v-list-item-action>
				</v-list-item>
				
				<!-- <v-divider /> -->
				<v-list-item
					v-if="currentNotificationCount == 0"
					disabled
					>
					<v-list-item-content>
						<v-list-item-title>No Notifications</v-list-item-title>
					</v-list-item-content>
				</v-list-item>
				
				<v-list-item
					two-line
					v-for="(notification, index) in this.currentNotifications"
					:key="index"
					
					>
					<v-list-item-icon
						style="margin: 0px; margin-top: 20px; margin-right: 10px;"
						>
						<v-icon v-if="notification.severity == 'error'" color="#ff5252">mdi-alert-circle</v-icon>
						<v-icon v-else-if="notification.severity == 'warning'" color="#fb8c00">mdi-alert</v-icon>
						<v-icon v-else-if="notification.severity == 'info'" color="#2196f3">mdi-information</v-icon>
						<v-icon v-else-if="notification.severity == 'success'" color="#2196f3">mdi-check</v-icon>
					</v-list-item-icon>
					<v-list-item-content>
						<v-list-item-title v-if="notification.severity == 'error'">Error</v-list-item-title>
						<v-list-item-title v-else-if="notification.severity == 'warning'">Warning</v-list-item-title>
						<v-list-item-title v-else-if="notification.severity == 'info'">Info</v-list-item-title>
						<v-list-item-title v-else-if="notification.severity == 'success'">Success</v-list-item-title>
						<v-list-item-subtitle>{{notification.message}}</v-list-item-subtitle>
					</v-list-item-content>
					<v-list-item-action
						@click="DoClearIndex(index)"
						>
						<v-btn icon>
							<v-icon color="grey lighten-1">mdi-close</v-icon>
						</v-btn>
					</v-list-item-action>
				</v-list-item>
				<!-- 
				
				-->
			</v-list>
		</v-menu>
	</div>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { INotification, Notifications } from '@/Data/Models/Notifications/Notifications';

@Component({
	props: {
		
	},
})
export default class NotificationBellButton extends Vue {
	
	protected menuActive = false;
	
	protected _periodicInterval: ReturnType<typeof setTimeout> | null = null;
	
	protected currentNotificationCount = 0;
	protected currentNotifications: INotification[] = [];
	
	
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
		
		this.currentNotificationCount = this.NotificationCount();
		this.currentNotifications = this.$store.state.Notifications.notifications;
		//console.debug('notification', this.currentSnackbarNotification);
	}
	
	
	
	
	
	
	
	
	
	
	protected NotificationCount(): number {
		
		if (!this.$store ||
			!this.$store.state || 
			!this.$store.state.Notifications) {
			return 0;
		}
		
		//console.log('this.$store.state.Notifications.notifications', this.$store.state.Notifications.notifications);
		
		return this.$store.state.Notifications.notifications.length;
	}
	
	protected DoClearAll(): void {
		Notifications.ClearAllNotifications();
	}
	
	protected DoClearIndex(index: number): void {
		Notifications.ClearIndex(index);
	}
}
</script>
<style>

.notification-bell-button .v-badge__badge {
	pointer-events: none;
}

/* .notification-bell-snackbar .v-snack__wrapper .v-sheet {
	z-index: 10000;
} */

</style>