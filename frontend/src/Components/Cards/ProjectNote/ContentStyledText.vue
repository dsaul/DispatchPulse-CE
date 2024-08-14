<template>
	<v-card-text :class="{
		'value-styled-text': true,
		'strikethrough': strikethrough,
	}" v-html="value.html">
	</v-card-text>
</template>
<script lang="ts">
import { IProjectNoteCheckbox } from '@/Data/CRM/ProjectNoteCheckbox/ProjectNoteCheckbox';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {
	},
})
export default class ContentStyledText extends Vue {

	@Prop({ default: null }) public readonly value!: IProjectNoteCheckbox;
	@Prop({ default: false }) public readonly strikethrough!: boolean;
}
</script>
<style scoped>
.value-styled-text {
	padding-top: 0px;
	padding-bottom: 0px;
	margin-top: 5px;
	white-space: pre-line;
}

.strikethrough {
	text-decoration: line-through;
}
</style>