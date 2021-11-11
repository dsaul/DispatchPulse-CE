<template>
	<v-row>
		<v-col cols="4" sm="2" offset-sm="2">
			<v-combobox
				label="Label"
				auto-select-first
				hint="Home, Mobile, Work, Fax, etc."
				:items="[ 'Unspecified', 'Home', 'Mobile', 'Work', 'Fax']"
				v-model="Label"
				class="e2e-phone-number-edit-row-label"
				:disabled="disabled"
				:readonly="readonly"
				>
			</v-combobox>
		</v-col>
		<v-col cols="8" sm="6">
			<v-text-field
				autocomplete="newpassword"
				label="Number"
				hint="Enter a telephone number."
				v-model="Value"
				type="tel"
				class="e2e-phone-number-edit-row-value"
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
								@click="CallPhoneNo()"
								:disabled="IsValueEmpty"
								>
								<v-list-item-icon>
									<v-icon>phone</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Call Phone…</v-list-item-title>
								</v-list-item-content>
							</v-list-item>
							<v-list-item
								@click="SendSMS()"
								:disabled="IsValueEmpty"
								>
								<v-list-item-icon>
									<v-icon>sms</v-icon>
								</v-list-item-icon>
								<v-list-item-content>
									<v-list-item-title>Send Text Message…</v-list-item-title>
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
								@click="$emit('MoveDown', index)"
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
import { IPhoneNumber } from '@/Data/Models/PhoneNumber/PhoneNumber';

@Component({
	components: {
	},
	
})

export default class PhoneNumberEditRow extends RowBase {
	
	@Prop({ default: null }) declare public readonly value: IPhoneNumber | null;
	
	protected CallPhoneNo(): void {
		
		if (this.value === null) {
			console.error('can\'t place call as entry is null');
			return;
		}
		
		if (IsNullOrEmpty(this.value.value)) {
			console.error('can\'t place call as IsNullOrEmpty(this.value.value)');
			return;
		}
		
		if (this.value.value !== null) {
			const num = this.value.value.replace(/\D/g, '');
			window.location.href = `tel:${num}`;
		}
		
	}
	
	protected SendSMS(): void {
		
		if (this.value === null) {
			console.error('can\'t place sms as entry is null');
			return;
		}
		
		if (IsNullOrEmpty(this.value.value)) {
			console.error('can\'t place sms as IsNullOrEmpty(this.value.value)');
			return;
		}
		
		if (this.value.value !== null) {
			const num = this.value.value.replace(/\D/g, '');
			window.location.href = `sms:${num}`;
		}
		
	}
	
	
	
	
	
}
</script>