<template>
	<v-menu bottom left offset-y>
		<template v-slot:activator="{ on }">
			<v-btn dark icon v-on="on">
				<v-icon>help</v-icon>
			</v-btn>
		</template>

		<v-list dense>
			<div v-if="DefaultSlotHasContent">
				<v-subheader style="height: 20px; padding-top: 10px;">This Section…</v-subheader>

				<slot></slot>

				<v-divider />
			</div>

			<v-subheader style="height: 20px; padding-top: 10px;">General…</v-subheader>
			<v-list-item @click="OpenOnlineHelp()">
				<v-list-item-icon>
					<v-icon>book</v-icon>
				</v-list-item-icon>
				<v-list-item-content>
					<v-list-item-title>Online Help</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
			<v-list-item @click="OpenYouTubeTutorials()">
				<v-list-item-icon>
					<v-icon>ondemand_video</v-icon>
				</v-list-item-icon>
				<v-list-item-content>
					<v-list-item-title>YouTube Tutorials</v-list-item-title>
				</v-list-item-content>
			</v-list-item>
		</v-list>
	</v-menu>
</template>
<script lang="ts">
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import { Component, Vue } from 'vue-property-decorator';

@Component({
	props: {

	},
})
export default class HelpMenuButton extends Vue {

	protected static LiveChatVisible = false;

	// definite assignments as props


	// data

	protected OpenOnlineHelp(): void {
		//console.log('OpenOnlineHelp()');

		window.open('https://www.dispatchpulse.com/Support', '_blank');
	}
	protected OpenYouTubeTutorials(): void {
		//console.log('OpenYouTubeTutorials()');

		window.open('https://www.youtube.com/channel/UCvqvWEU47Mz2iNeelUrrZVA/videos', '_blank');

	}


	public get DefaultSlotHasContent(): boolean {

		//console.log(this.$slots);


		if (this.$slots === null || this.$slots === undefined) {
			return false;
		}

		//console.log("#1");

		const slots = this.$slots;
		if (!slots) {
			return false;
		}

		//console.log("#2");

		const defaultSlots = slots.default;
		if (!defaultSlots) {
			return false;
		}
		//console.log("#3");
		const defaultSlot = defaultSlots[0];
		return !!defaultSlot;
	}


}
</script>