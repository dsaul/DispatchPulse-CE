<template>
	<v-row>
		<v-col cols="4" sm="2" offset-sm="2">
			<v-combobox
				label="Label"
				auto-select-first
				hint="Home, Mobile, Work, etc."
				:items="[ 'Unspecified', 'Home', 'Mobile', 'Work']"
				v-model="Label"
				class="e2e-email-edit-row-label"
				:disabled="disabled"
				:readonly="readonly"
				>
			</v-combobox>
		</v-col>
		<v-col cols="8" sm="6">
			<v-text-field
				autocomplete="newpassword"
				label="E-Mail Address"
				hint="Enter an e-mail address."
				v-model="Value"
				class="e2e-email-edit-row-value"
				:disabled="disabled"
				:readonly="readonly"
				>
				
				<template v-slot:append-outer>
					<v-menu bottom left>
						<template v-slot:activator="{ on }">
							<v-btn
							icon
							v-on="on"
							:disabled="disabled"
							>
								<v-icon>more_vert</v-icon>
							</v-btn>
						</template>

						<v-list dense>
							<v-list-item
								@click="ComposeEMail()"
								>
								<v-list-item-icon>
									<v-icon>email</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Compose E-Mail…</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-divider />
							<v-list-item
								@click="$emit('MoveUp', index)"
								:disabled="isFirstIndex || disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>arrow_upward</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Move Up</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="$emit('MoveUp', index)"
								:disabled="isLastIndex || disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>arrow_downward</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Move Down</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-divider />
							<v-list-item
								@click="$emit('InsertNewRowAtIndex', index)"
								:disabled="disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>add</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Add Row Above</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="$emit('InsertNewRowAtIndex', index + 1)"
								:disabled="disabled || readonly"
								>
								<v-list-item-icon>
									<v-icon>add</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Add Row Below</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="$emit('RemoveRowAtIndex', index)"
								:disabled="disabled || readonly"
								>
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
			</v-text-field>
		</v-col>
	</v-row>
</template>

<script lang="ts">
import { Component, Prop } from 'vue-property-decorator';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import RowBase from './RowBase';
import { IEMail } from '@/Data/Models/EMail/EMail';

@Component({
	components: {
	},
	
})

export default class EMailEditRow extends RowBase {
	
	@Prop({ default: null }) declare public readonly value: IEMail | null;
	
	protected ComposeEMail(): void {
		if (this.value === null) {
			console.error('can\'t make email as value is null');
			return;
		}
		
		if (IsNullOrEmpty(this.value.value)) {
			console.error('can\'t make email as IsNullOrEmpty(this.value.value)');
			return;
		}
		
		if (this.value.value !== null) {
			window.location.href = `mailto:${this.value.value}`;
		}
	}
}
</script>
