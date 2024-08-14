<template>
	<v-card style="margin: 4px; margin-top: 10px; margin-bottom: 10px;">
		<v-card-text>

			<v-alert type="info" colored-border elevation="2" border="bottom">
				This account will be made with employee permissions, if they need more permissions you can add them in
				settings.
			</v-alert>

			<v-container>
				<v-row no-gutters>
					<v-col cols="12">
						<v-text-field ref="item" v-model="FullName" autocomplete="newpassword" label="Full Name"
							hint="The full name for this contact, for example, John Doe."
							:rules="[ValidateRequiredField]">
						</v-text-field>
					</v-col>
				</v-row>
				<v-row no-gutters>
					<v-col cols="12">
						<v-text-field ref="item" v-model="EMail" autocomplete="newpassword" label="E-Mail"
							hint="They will always log in with their email address" :rules="[ValidateRequiredField]"
							type="email">
						</v-text-field>
					</v-col>
				</v-row>
				<v-row no-gutters>
					<v-col cols="12">
						<v-text-field ref="item" v-model="PhoneNumber" autocomplete="newpassword" label="Phone #"
							hint="What is the phone number for this account, will be used for text notifications."
							type="phone" :rules="[ValidateRequiredField]">
						</v-text-field>
					</v-col>
				</v-row>
				<v-row no-gutters>
					<v-col cols="12">
						<v-text-field ref="item" v-model="Password1" autocomplete="newpassword" label="Password"
							hint="The password you'll use to login." :rules="[ValidateRequiredField]" type="password">
						</v-text-field>
					</v-col>
				</v-row>
				<v-row no-gutters>
					<v-col cols="12">
						<v-text-field ref="item" v-model="Password2" autocomplete="newpassword" label="Password, Again"
							hint="Type the password again, so we're sure we got it right."
							:rules="[ValidateRequiredField]" type="password">
						</v-text-field>
					</v-col>
				</v-row>
				<v-row v-if="Password1 != Password2">
					<v-col cols="12">
						<v-alert border="top" colored-border type="error" elevation="2">
							The passwords must be the same!
						</v-alert>
					</v-col>
				</v-row>
			</v-container>
		</v-card-text>
		<v-card-actions>
			<v-btn text color="red" @click="OnClickRemove()">
				Remove
			</v-btn>
		</v-card-actions>
	</v-card>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import bcrypt from 'bcryptjs';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import { IRegisterAdditionalUser } from '@/Data/Models/RegisterDialogueModelState/RegisterDialogueModelState';

@Component({
	components: {

	},
})
export default class RegisterAdditionalAccount extends Vue {
	@Prop({ default: null }) public readonly value!: IRegisterAdditionalUser | null;

	protected ValidateRequiredField = ValidateRequiredField;

	protected debounceId: ReturnType<typeof setTimeout> | null = null;

	protected OnClickRemove(): void {
		this.$emit('remove', this.value);
	}


	protected get FullName(): string | null {

		if (!this.value) {
			return null;
		}

		return this.value.fullName;
	}

	protected set FullName(payload: string | null) {
		if (!this.value) {
			return;
		}

		this.value.fullName = payload;

		this.SignalChanged();
	}

	protected get EMail(): string | null {

		if (!this.value) {
			return null;
		}

		return this.value.email;
	}

	protected set EMail(payload: string | null) {
		if (!this.value) {
			return;
		}

		this.value.email = payload;

		this.SignalChanged();
	}

	protected get PhoneNumber(): string | null {

		if (!this.value) {
			return null;
		}

		return this.value.phoneNumber;
	}

	protected set PhoneNumber(payload: string | null) {
		if (!this.value) {
			return;
		}

		this.value.phoneNumber = payload;

		this.SignalChanged();
	}

	protected get Password1(): string | null {

		if (!this.value) {
			return null;
		}

		return this.value.password1;
	}

	protected set Password1(payload: string | null) {
		if (!this.value) {
			return;
		}

		this.value.password1 = payload;

		const salt = bcrypt.genSaltSync(11);
		this.value.passwordHash = payload == null || IsNullOrEmpty(payload) ? null : bcrypt.hashSync(payload, salt);

		this.SignalChanged();
	}

	protected get Password2(): string | null {

		if (!this.value) {
			return null;
		}

		return this.value.password2;
	}

	protected set Password2(payload: string | null) {
		if (!this.value) {
			return;
		}

		this.value.password2 = payload;

		this.SignalChanged();
	}

	protected SignalChanged(): void {

		// Debounce

		if (this.debounceId) {
			clearTimeout(this.debounceId);
			this.debounceId = null;
		}

		this.debounceId = setTimeout(() => {
			this.$emit('input', this.value);
		}, 250);
	}
}
</script>
<style scoped></style>