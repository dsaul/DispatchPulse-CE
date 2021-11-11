<template>
	<v-card-text
		class="value-image"
		>
		<img :src="value.uri" style="max-width: 100%;" />
	</v-card-text>
</template>
<script lang="ts">
import { IProjectNoteImage } from '@/Data/CRM/ProjectNoteImage/ProjectNoteImage';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {
	},
})
export default class ContentImage extends Vue {
	
	@Prop({ default: null }) public readonly value!: IProjectNoteImage;
	
}
</script>
<style scoped>
.value-image {
	padding-top: 0px;
	padding-bottom: 0px;
	margin-top: 16px;
}
</style>