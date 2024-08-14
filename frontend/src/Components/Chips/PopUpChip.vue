<template>
	<div style="display: inline-block;">
		<v-menu v-model="menuActive" :close-on-content-click="false" offset-y>
			<template v-slot:activator="{ on: onMenu, attrs }">
				<v-tooltip top>
					<template v-slot:activator="{ on: onTooltip }">
						<v-chip color="primary" label outlined style="margin: 4px;" v-bind="attrs"
							v-on="{ ...onMenu, ...onTooltip }">
							Notes
						</v-chip>
					</template>
					<span>{{ tooltipText }}</span>
				</v-tooltip>

			</template>

			<v-card>
				<slot></slot>

				<v-card-actions>
					<slot name="actions">
						<v-spacer></v-spacer>

						<v-btn small text @click="menuActive = false">Close</v-btn>
					</slot>
				</v-card-actions>
			</v-card>
		</v-menu>
	</div>
</template>


<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {

	},
})
export default class PopUpChip extends Vue {

	@Prop({ default: null }) public readonly tooltipText!: string;

	protected menuActive = false;



}
</script>