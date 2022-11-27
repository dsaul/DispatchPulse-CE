<template>
	<v-alert
		border="top"
		colored-border
		elevation="2"
		type="error"
		style="margin: 15px; padding-bottom: 10px;"
		class="fadeIn404"
		>
		You don't have access to this. Please ask your system administrator for access if you require it.
	</v-alert>
</template>
<script lang="ts">

import { Component, Vue } from 'vue-property-decorator';

@Component({
	components: {
	},
})
export default class AgentsDisplay extends Vue {
	
}

</script>