<template>
	<v-chip label outlined style="margin: 4px;" small>
		{{ RetrievedDate }}
	</v-chip>
</template>


<script lang="ts">
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { Component, Vue, Prop } from 'vue-property-decorator';
import { DateTime } from 'luxon';
import { ICalendar } from '@/Data/CRM/Calendar/Calendar';

@Component({
	components: {

	},
})
export default class CalendarLastRetrievedChip extends Vue {

	@Prop({ default: null }) public readonly calendar!: ICalendar | null;

	protected get RetrievedDate(): string {
		if (!this.calendar ||
			!this.calendar.json ||
			!this.calendar.json.iCalFileLastRetrievedISO8601 ||
			IsNullOrEmpty(this.calendar.json.iCalFileLastRetrievedISO8601)) {
			return 'Never';
		}

		const dt = DateTime.fromISO(this.calendar.json.iCalFileLastRetrievedISO8601);
		return dt.toLocaleString(DateTime.DATETIME_FULL);
	}


}
</script>