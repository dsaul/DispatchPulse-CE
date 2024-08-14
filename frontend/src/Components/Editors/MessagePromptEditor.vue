<template>
	<div>
		<v-radio-group v-model="Type">
			<v-radio label="Text to Speach" value="polly">
			</v-radio>
			<v-radio label="Recording" value="recording">
			</v-radio>
		</v-radio-group>

		<div v-if="Type == 'polly'">
			<v-textarea v-model="PollyText" autocomplete="newpassword" label="" :hint="pollyHint" :disabled="disabled"
				:rules="[
			ValidateRequiredField
		]" persistent-hint>
			</v-textarea>
		</div>
		<div v-if="Type == 'recording'">
			<RecordingSelectField v-model="RecordingId" :isDialogue="false" :disabled="disabled"
				:hint="recordingHint" />
		</div>
	</div>
</template>
<script lang="ts">

import { Component, Vue, Prop } from 'vue-property-decorator';
import '@/Data/CRM/MessagePrompt/MessagePrompt';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import ValidateRequiredField from '@/Utility/Validators/ValidateRequiredField';
import { guid } from '@/Utility/GlobalTypes';
import RecordingSelectField from '@/Components/Fields/RecordingSelectField.vue';
import { IMessagePrompt } from '@/Data/CRM/MessagePrompt/MessagePrompt';

@Component({
	components: {
		RecordingSelectField,
	},

})
export default class MessagePromptEditor extends Vue {

	@Prop({ default: null }) public readonly value!: IMessagePrompt | null;
	@Prop({ default: false }) public readonly disabled!: boolean;
	@Prop({ default: '' }) public readonly pollyHint!: string;
	@Prop({ default: '' }) public readonly recordingHint!: string;


	public $refs!: {

	};

	protected ValidateRequiredField = ValidateRequiredField;


	public set Type(payload: 'polly' | 'recording' | null) {
		if (!this.value) {
			return;
		}

		Vue.set(this.value, 'type', payload);

		this.$emit('input', this.value);

	}
	public get Type(): 'polly' | 'recording' | null {
		if (!this.value) {
			return null;
		}
		return this.value.type;
	}


	protected get PollyText(): string | null {

		if (!this.value ||
			!this.value.text
		) {
			return null;
		}

		return this.value.text;
	}

	protected set PollyText(val: string | null) {

		if (!this.value ||
			!this.value.text
		) {
			return;
		}

		Vue.set(this.value, 'text', IsNullOrEmpty(val) ? null : val);

		this.$emit('input', this.value);
	}


	protected get RecordingId(): guid | null {
		if (!this.value ||
			!this.value.recordingId) {
			return null;
		}

		return this.value.recordingId;
	}

	protected set RecordingId(val: guid | null) {
		if (!this.value) {
			return;
		}

		console.debug('set RecordingId', val);

		Vue.set(this.value, 'recordingId', IsNullOrEmpty(val) ? null : val);

		this.$emit('input', this.value);
	}
}

</script>
<style scoped></style>