<template>
	<v-card>
		<v-card-title>Backup This PC's Database</v-card-title>
		<v-card-text>
			<p>
				To download a backup of what is loaded on this PC click the below 
				button. 
			</p>
			<p>
				<span style="color: red;">Be aware, this will only save what is on this PC.</span> 
			</p>
			<p>
				For example:
				<ul>
					<li>
						If you limited the amount of data downloaded to just this past year, it 
						will only save this past year.
					</li>
					<li>
						If you only visited contacts when this was open, it will only save 
						contacts as the rest isn't on your computer.
					</li>
				</ul>
				
			</p>
			<p>
				The best time to use this is if you're stuck offline and are worried 
				about losing data that isn't sent to our servers.
			</p>
			<p>
				<v-btn @click="DoBackupThisPC" :disabled="disabled || !PermCRMBackupsRunLocal()">Backup This PC's Database</v-btn>
			</p>
		</v-card-text>
	</v-card>
</template>
<script lang="ts">
import { Component } from 'vue-property-decorator';
import CardBase from '@/Components/Cards/CardBase';
import PermissionsDeniedAlert from '@/Components/Alerts/PermissionsDeniedAlert.vue';
import GetDemoMode from '@/Utility/DataAccess/GetDemoMode';
import CRMBackups from '@/Permissions/CRMBackups';
import DownloadJSON from '@/Utility/DownloadJSON';

@Component({
	components: {
		PermissionsDeniedAlert,
	},
})
export default class BackupLocalDataCard extends CardBase {
	
	public $refs!: {
		
	};
	
	
	
	protected GetDemoMode = GetDemoMode;
		protected PermCRMBackupsRunLocal = CRMBackups.PermCRMBackupsRunLocal;
	
	protected loadingData = false;
	protected _LoadDataTimeout: ReturnType<typeof setTimeout> | null = null;
	
	public get IsLoadingData(): boolean {
		
		// if (this.$refs.assignmentsList && this.$refs.assignmentsList.IsLoadingData) {
		// 	return true;
		// }
		
		return this.loadingData;
	}
	
	public LoadData(): void {
		
		//console.log('@@@');
		
		this.loadingData = true;
		
		// In timeout to debounce
		if (this._LoadDataTimeout) {
			clearTimeout(this._LoadDataTimeout);
			this._LoadDataTimeout = null;
		}
		
		this._LoadDataTimeout = setTimeout(() => {
			
			this.loadingData = false;
			
		}, 250);
		
		// if (this.$refs.assignmentsList) {
		// 	this.$refs.assignmentsList.LoadData();
		// }
		
	}
	
	protected DoBackupThisPC(): void {
		console.debug('DoBackupThisPC()');
		
		const database = this.$store.state.Database;
		
		
		
		const obj: any = {
			backupVersion: 1,
			thisPCBackup: true,
			database,
		};
		
		const json = JSON.stringify(obj, null, '\t');
		//console.debug(json);
		
		DownloadJSON(json, 'DispatchPulseThisPCBackup.json');
		
	}
	
}
</script>