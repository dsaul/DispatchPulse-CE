<template>
	<v-row>
		<v-col cols="4" sm="2" offset-sm="2">
			<v-combobox label="Label" auto-select-first hint="Home, Work, Jobsite, etc." v-model="Label"
				:items="['Unspecified', 'Home', 'Work', 'Jobsite']" class="e2e-address-edit-row-label"
				:disabled="disabled" :readonly="readonly">
			</v-combobox>
		</v-col>
		<v-col cols="8" sm="6">
			<v-textarea :rows="2" :auto-grow="true" label="Address" hint="Enter a physical street address."
				class="e2e-address-edit-row-value" v-model="Value" :disabled="disabled" :readonly="readonly">

				<template v-slot:append-outer>
					<v-menu bottom left>
						<template v-slot:activator="{ on }">
							<v-btn icon v-on="on" :disabled="disabled">
								<v-icon>more_vert</v-icon>
							</v-btn>
						</template>

						<v-list dense>
							<v-list-item @click="OpenInMaps()" :disabled="isDialogue || IsValueEmpty">
								<v-list-item-icon>
									<v-icon>directions</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Open in Maps…</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-divider />
							<v-list-item @click="$emit('MoveUp', index)"
								:disabled="isFirstIndex || disabled || readonly">
								<v-list-item-icon>
									<v-icon>arrow_upward</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Move Up</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item @click="$emit('MoveDown', index)"
								:disabled="isLastIndex || disabled || readonly">
								<v-list-item-icon>
									<v-icon>arrow_downward</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Move Down</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-divider />
							<v-list-item @click="$emit('InsertNewRowAtIndex', index)" :disabled="disabled || readonly">
								<v-list-item-icon>
									<v-icon>add</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Add Row Above</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item @click="$emit('InsertNewRowAtIndex', index + 1)"
								:disabled="disabled || readonly">
								<v-list-item-icon>
									<v-icon>add</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Add Row Below</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item @click="$emit('RemoveRowAtIndex', index)" :disabled="disabled || readonly">
								<v-list-item-icon>
									<v-icon>remove</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Remove This Row</v-list-item-title>
								</v-list-item-content>
							</v-list-item>

						</v-list>
					</v-menu>
				</template>
			</v-textarea>

			<iframe v-if="NormalizedAddress" style="border: 0px; width:100%"
				:src="`https://maps.google.com/maps?q=${NormalizedAddress}&t=&z=13&ie=UTF8&iwloc=&output=embed`"
				sandbox="allow-scripts allow-popups">
			</iframe>



		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import RowBase from '@/Components/Rows/RowBase';
import { IAddress } from '@/Data/Models/Address/Address';

@Component({
	components: {
	},
})

export default class AddressEditRow extends RowBase {

	@Prop({ default: null }) declare public readonly value: IAddress | null;

	protected get NormalizedAddress(): string | null {
		if (this.value !== null && this.value.value !== null) {
			return this.value.value.replace(/[\r\n\x0B\x0C\u0085\u2028\u2029/]+/g, ' ');
		}
		return null;
	}

	protected OpenInMaps(): void {
		if (this.value !== null && this.value.value !== null) {
			window.open(`https://www.google.com/maps/dir/current+location/${this.NormalizedAddress}`, '_blank');
		} else {
			console.error('Can\'t open in google maps as value is null');
		}

	}
}
</script>