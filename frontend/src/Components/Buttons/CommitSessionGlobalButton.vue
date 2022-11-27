<template>
	<v-btn icon @click="CommitSessionGlobal()">
		<v-icon>save</v-icon>
	</v-btn>
</template>
<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component({
	props: {
		
	},
})
export default class CommitSessionGlobalButton extends Vue {
	
	// definite assignments as props
	
	
	// data
	
	protected CommitSessionGlobal(): void {
		console.log('CommitSessionGlobal()');
	}
}
</script>