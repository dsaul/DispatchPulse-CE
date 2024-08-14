<template>
	<div class="outerdiv" style="border-bottom: ">
		<v-dialog v-model="selectDialogOpen" persistent scrollable :fullscreen="MobileDeviceWidth()">
			<!--<template v-slot:activator="{ on }">
				<v-btn color="primary" dark v-on="on">Open Dialog</v-btn>
			</template>-->
			<v-card>
				<v-card-title>Select a Voicemail</v-card-title>
				<v-divider></v-divider>
				<v-card-text>

					<VoicemailList :openEntryOnClick="false" :showMenuButton="false" @ClickEntry="OnClickEntry"
						ref="list" />

					<v-dialog v-model="newDialogueVisible" persistent scrollable :fullscreen="MobileDeviceWidth()">
						<v-card>
							<v-card-title>Create and Select New Voicemail</v-card-title>
							<v-divider></v-divider>
							<v-card-text>
								<VoicemailEditor ref="editor" v-model="newDialogueObject" :showAppBar="false"
									:showFooter="false" preselectTabName="General" :isMakingNew="true" />
							</v-card-text>
							<v-divider></v-divider>
							<v-card-actions>
								<v-spacer />
								<v-btn color="red darken-1" text @click="NewDialogueCancel()">Close</v-btn>
								<v-btn color="green darken-1" text @click="NewDialogueSaveAndAdd()">Save and
									Select</v-btn>
							</v-card-actions>
						</v-card>
					</v-dialog>

				</v-card-text>
				<v-divider></v-divider>
				<v-card-actions>
					<v-btn color="green darken-1" text @click="OpenNewDialogue()">New</v-btn>
					<v-btn color="red darken-1" text @click="ClearValue()">Clear</v-btn>
					<v-spacer />
					<v-btn color="green darken-1" text @click="selectDialogOpen = false;">Close</v-btn>
				</v-card-actions>
			</v-card>
		</v-dialog>
		<v-input :messages="[]" ref="input" persistent-hint :hint="hint" :required="required" v-model="value"
			:rules="rules" :disabled="disabled" :readonly="readonly">
			<template v-slot:label>
				<span style="font-size: 12px;">{{ label }}</span>
			</template>
			<template v-slot:append>
				<v-menu bottom left>
					<template v-slot:activator="{ on }">
						<v-btn icon v-on="on" :disabled="disabled">
							<v-icon>more_vert</v-icon>
						</v-btn>
					</template>

					<v-list dense>
						<v-list-item @click="OpenVoicemail()" :disabled="isDialogue || IsValueEmpty || disabled">
							<v-list-item-icon>
								<v-icon>open_in_new</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Open…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>
						<v-divider />
						<v-list-item @click="SelectNewVoicemail()" :disabled="disabled || readonly">
							<v-list-item-icon>
								<v-icon>edit</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Select Different…</v-list-item-title>
							</v-list-item-content>
						</v-list-item>

						<v-list-item @click="ClearValue()" :disabled="IsValueEmpty || disabled || readonly">
							<v-list-item-icon>
								<v-icon>clear</v-icon>
							</v-list-item-icon>
							<v-list-item-content>
								<v-list-item-title>Clear</v-list-item-title>
							</v-list-item-content>
						</v-list-item>

						<slot name="custom-menu-options"></slot>

					</v-list>
				</v-menu>
			</template>




			<div
				style="margin-top: 5px; margin-bottom: 5px; width: 100%; cursor: text; display: flex; align-items: stretch;">
				<div v-if="!Voicemail && value">
					{{ value }} (can't find)
				</div>
				<div v-else-if="VoicemailForId(value)" @click="SelectNewVoicemail()" :style="{
			flex: '1',
			display: 'flex',
			'align-items': 'flex-end',
			color: disabled ? 'rgba(0, 0, 0, 0.38)' : 'inherit',
		}">
					{{ VoicemailDisplayNameForId(value) || 'Empty voicemail name.' }}
				</div>
				<div v-else @click="SelectNewVoicemail()" :disabled="disabled || readonly"
					style="flex: 1; text-align: center;">
					<v-btn color="primary" text>Select or Add Voicemail</v-btn>
				</div>
				<div v-if="VoicemailForId(value) && !isDialogue" class="d-none d-sm-flex">
					<v-tooltip top>
						<template v-slot:activator="{ on }">
							<v-chip :to="`/section/voicemails/${value}?tab=General`" color="primary" label outlined
								style="margin: 4px;" v-on="on">
								Open
								<v-icon right small>open_in_new</v-icon>
							</v-chip>
						</template>
						<span>Open this contact.</span>
					</v-tooltip>
				</div>
			</div>






		</v-input>


	</div>

</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import FieldBase from './FieldBase';
import VoicemailList from '@/Components/Lists/VoicemailList.vue';
import IsNullOrEmpty from '@/Utility/IsNullOrEmpty';
import VoicemailEditor from '@/Components/Editors/VoicemailEditor.vue';

import { DateTime } from 'luxon';
import { IVoicemail, Voicemail } from '@/Data/CRM/Voicemail/Voicemail';
import MobileDeviceWidth from '@/Utility/MobileDeviceWidth';
import { BillingContacts } from '@/Data/Billing/BillingContacts/BillingContacts';
import SignalRConnection from '@/RPC/SignalRConnection';
import { Notifications } from '@/Data/Models/Notifications/Notifications';
import { BillingPermissionsBool } from '@/Data/Billing/BillingPermissionsBool/BillingPermissionsBool';

@Component({
	components: {
		VoicemailList,
		VoicemailEditor,
	},

})

export default class VoicemailSelectField extends FieldBase {

	@Prop({ default: 'Voicemail' }) declare public readonly label: string | null;
	@Prop({ default: false }) public readonly showCompanyContactsBelow!: boolean | null;

	public $refs!: {
		input: Vue,
		editor: VoicemailEditor,
		list: VoicemailList,
	};

	protected VoicemailForId = Voicemail.ForId;
	protected VoicemailDisplayNameForId = Voicemail.DisplayNameForId;
	protected MobileDeviceWidth = MobileDeviceWidth;

	protected newDialogueVisible = false;
	protected newDialogueObject: IVoicemail | null = null;

	public mounted(): void {
		//console.log('mounted', this.$refs);

		if (null != this.value && !IsNullOrEmpty(this.value)) {

			const contact = Voicemail.ForId(this.value);
			if (null == contact) {
				const id = this.value;
				SignalRConnection.Ready(() => {
					BillingPermissionsBool.Ready(() => {
						Voicemail.FetchForId(id);
					});
				});

			}
		}


		// Set the flex direction for the input element.
		do {

			const defaultInputSlots = this.$refs.input.$slots.default;

			if (!defaultInputSlots) {
				break;
			}

			for (const node of defaultInputSlots) {
				if (!node) {
					continue;
				}

				const elm: HTMLElement = node.elm as HTMLElement;
				if (!elm) {
					continue;
				}



				const elmParent = elm.parentElement;
				if (!elmParent) {
					continue;
				}

				elmParent.style.flexDirection = 'column';
				elmParent.style.alignItems = 'start';
				elmParent.style.borderBottom = 'thin solid grey';

				//console.log(elm);
			}




		} while (false);

	}

	protected get Voicemail(): IVoicemail | null {
		const contact = Voicemail.ForId(this.value);
		if (!contact) {
			return null;
		}
		return contact;
	}

	protected SelectNewVoicemail(): void {

		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}

		console.log('SelectNewVoicemail()');
		this.selectDialogOpen = true;

		requestAnimationFrame(() => {
			this.$refs.list.SelectFilterField();
		});

	}

	protected ClearValue(): void {
		if (this.disabled) {
			console.debug('not opening select because field is disabled');
			return;
		}
		if (this.readonly) {
			console.debug('not opening because field is read only');
			return;
		}

		console.log('ClearValue()');
		this.$emit('input', null);

		this.selectDialogOpen = false;
	}

	protected OpenVoicemail(): void {
		console.log('OpenVoicemail()');

		if (!IsNullOrEmpty(this.value)) {
			this.$router.push(`/section/voicemails/${this.value}?tab=General`).catch(((e: Error) => { }));// eslint-disable-line
		} else {
			console.error('can\'t open voicemails as IsNullOrEmpty(this.value)');
		}

	}

	protected OnClickEntry(id: string): void {
		console.debug('OnClickEntry', id);

		this.$emit('input', id);
		this.selectDialogOpen = false;
	}

	protected OpenNewDialogue(): void {

		console.debug('OpenNewDialogue()');

		this.newDialogueObject = Voicemail.GetEmpty();
		this.newDialogueVisible = true;

	}

	protected NewDialogueCancel(): void {
		console.debug('NewDialogueCancel()');

		this.$refs.editor.ResetValidation();
		this.newDialogueObject = Voicemail.GetEmpty();
		this.newDialogueVisible = false;
		//this.$refs.editor.SelectFirstTab();
	}

	protected NewDialogueSaveAndAdd(): void {
		console.debug('NewDialogueSaveAndAdd()', this.newDialogueObject);

		if (this.newDialogueObject && this.$refs.editor.IsValidated()) {

			// First add the agent.
			const state = this.newDialogueObject as IVoicemail;
			if (state.id) {
				state.lastModifiedISO8601 = DateTime.utc().toISO();
				state.json.lastModifiedBillingId = BillingContacts.CurrentBillingContactId();

				const payload: { [id: string]: IVoicemail; } = {};
				payload[state.id] = state;
				Voicemail.UpdateIds(payload);


				// Then choose this new agent.
				this.$emit('input', state.id);

				// Close new dialogue.
				this.$refs.editor.ResetValidation();
				this.newDialogueObject = Voicemail.GetEmpty();
				this.newDialogueVisible = false;
				//this.$refs.editor.SelectFirstTab();

				// Close select dialogue.
				this.selectDialogOpen = false;
			}


		} else {
			Notifications.AddNotification({
				severity: 'error',
				message: 'Some of the form fields didn\'t pass validation.',
				autoClearInSeconds: 10,
			});
		}



	}

}

Vue.component('VoicemailSelectField', VoicemailSelectField);

</script>
<style scoped>
.outerdiv:after {
	border-color: currentColor;
	border-style: solid;
	border-width: thin 0 thin 0;
}

.text-align-center {
	text-align: center;
}
</style>