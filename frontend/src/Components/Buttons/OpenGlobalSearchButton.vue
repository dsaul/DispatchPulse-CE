<template>
	<v-btn icon @click="DialoguesOpen({ name: 'GlobalSearchDialogue', state: null })">
		<v-icon>search</v-icon>
	</v-btn>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import Dialogues from '@/Utility/Dialogues';

@Component({
	components: {

	},
})
export default class OpenGlobalSearchButton extends Vue {

	protected DialoguesOpen = Dialogues.Open;


}
</script>