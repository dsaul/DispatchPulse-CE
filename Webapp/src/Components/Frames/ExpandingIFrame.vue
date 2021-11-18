<template>
	<iframe
		:src="src"
		style="width: 1px; min-width: 100%; border: 0px;"
		ref="iframe"
		>
	</iframe>
</template>
<script lang="ts">
import { Component, Vue, Prop } from 'vue-property-decorator';
import { iframeResizer } from 'iframe-resizer';

@Component({
	components: {
	},
	
})
export default class ExpandingIFrame extends Vue {
	
	@Prop({ default: 'about:blank' }) public readonly src!: string | null;
	
	
	public $refs!: {
		iframe: HTMLIFrameElement,
	};
	
	public mounted(): void {
		iframeResizer({ log: false }, this.$refs.iframe);
	}
	
	
}

Vue.component('ExpandingIFrame', ExpandingIFrame);

</script>
<style scoped>

</style>