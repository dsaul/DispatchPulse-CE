<template>
	<v-btn dark icon @click="$emit('reload')">
		<v-icon>sync</v-icon>
	</v-btn>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component({
	props: {

	},
})
export default class HelpMenuButton extends Vue {




}
</script>