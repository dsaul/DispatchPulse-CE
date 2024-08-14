<template>
	<v-card-text
		
		style="padding-top: 0px;padding-bottom: 0px; margin-top: 16px;"
		>
		<div v-if="IsYoutube">
			<iframe
				style="width: 100%; height: 480px; border: none;"
				allowfullscreen
				:src="`https://www.youtube.com/embed/${YouTubeVideoID}?rel=0`"></iframe>
		</div>
		<div v-else-if="value.uri.endsWith('.mp4')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/mp4">
			</video>
		</div>
		<div v-else-if="value.uri.endsWith('.m4v')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/mp4">
			</video>
		</div>
		<div v-else-if="value.uri.endsWith('.mov')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/quicktime">
			</video>
		</div>
		<div v-else-if="value.uri.endsWith('.ogg')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/ogg">
			</video>
		</div>
		<div v-else-if="value.uri.endsWith('.avi')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/avi">
			</video>
		</div>
		<div v-else-if="value.uri.endsWith('.mkv')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/mkv">
			</video>
		</div>
		<div v-else-if="value.uri.endsWith('.webm')">
			<video controls preload="none" style="max-width: 100%; max-height: 480px;">
				<source :src="value.uri" type="video/webm">
			</video>
		</div>
	</v-card-text>
</template>
<script lang="ts">
import { IProjectNoteVideo } from '@/Data/CRM/ProjectNoteVideo/ProjectNoteVideo';
import { Component, Vue, Prop } from 'vue-property-decorator';


@Component({
	components: {
	},
})
export default class ContentVideo extends Vue {
	
	@Prop({ default: null }) public readonly value!: IProjectNoteVideo;
	
	get IsYoutube(): boolean {
		return -1 !== this.value.uri.indexOf('youtube');
	}
	
	get YouTubeVideoID(): string | null {
		const regExp = /^.*(youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=|\&v=)([^#\&\?]*).*/;
		const match = this.value.uri.match(regExp);
	
		if (match && match[2].length === 11) {
			return match[2];
		} else {
			return null;
		}
	}
}
</script>
<style scoped>

</style>