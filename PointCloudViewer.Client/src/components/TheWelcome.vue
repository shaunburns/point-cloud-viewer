<template>
  <div>
    <h1>Point Cloud Viewer</h1>
    <canvas ref="pointCloudViewerNativeCanvas" oncontextmenu="event.preventDefault()" tabindex=-1 width="512" heght="512"></canvas>
  </div>
</template>

<script lang="ts" setup>
  import { onMounted, onUnmounted, ref } from 'vue'

  const pointCloudViewerNativeCanvas = ref(null);
  const pointCloudViewerInstance = ref(null);

  onMounted(async () => {
    try {
      const wasmModule = await import('@/assets/wasm/PointCloudViewer.js');

      const moduleOptions = {
        canvas: pointCloudViewerNativeCanvas.value,
      };

      pointCloudViewerInstance.value = await wasmModule.default(moduleOptions);
      console.log('WASM Module Loaded:', pointCloudViewerInstance.value);

      pointCloudViewerInstance.value._StartRendering();
    } catch (error) {
      console.error('Error loading WASM:', error);
    }
  });

  onUnmounted(() => {
    pointCloudViewerInstance.value._StopRendering();
    pointCloudViewerInstance.value = null;
  });

</script>
