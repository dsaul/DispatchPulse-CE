<template>
	<v-menu :disabled="disabled" top offset-y>
		<template v-slot:activator="{ on }">
			<v-btn :disabled="disabled" color="white" text rounded v-on="on">
				<v-icon left dark>add</v-icon>
				Add
			</v-btn>
		</template>
		<v-list dense>
			<slot></slot>
		</v-list>
	</v-menu>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component({
	props: {

	},
})
export default class AddMenuButton extends Vue {

	@Prop({ default: false }) public readonly disabled!: boolean;

	// definite assignments as props


	// data


}
</script>