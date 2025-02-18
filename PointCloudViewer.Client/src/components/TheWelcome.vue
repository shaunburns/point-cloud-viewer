<template>
  <div class="flex flex-col items-center justify-center h-full w-full overflow-hidden">
    <h1 class="text-2xl font-bold mb-4 flex-none">Point Cloud Viewer</h1>
    <div class="flex-auto w-full h-full relative">
      <canvas ref="pointCloudViewerNativeCanvas" oncontextmenu="event.preventDefault()" tabindex="-1" class="absolute top-0 left-0"></canvas>
    </div>
  </div>
</template>

<script lang="ts" setup>
  import { onMounted, onUnmounted, ref } from 'vue'

  const pointCloudViewerNativeCanvas = ref<HTMLCanvasElement | null>(null);
  const pointCloudViewerInstance = ref(null);

  const resizeCanvas = () => {
    const canvas = pointCloudViewerNativeCanvas.value;
    if (!canvas)
      return;

    const container = canvas.parentElement;
    if (!container)
      return;

    const aspectRatio = 16 / 9;
    const containerWidth = container.clientWidth;
    const containerHeight = container.clientHeight;

    if (containerWidth / containerHeight > aspectRatio) {
      canvas.style.width = `${containerHeight * aspectRatio}px`;
      canvas.style.height = `${containerHeight}px`;
    } else {
      canvas.style.width = `${containerWidth}px`;
      canvas.style.height = `${containerWidth / aspectRatio}px`;
    }

    canvas.style.left = `${(containerWidth - canvas.clientWidth) / 2}px`;
    canvas.style.top = `${(containerHeight - canvas.clientHeight) / 2}px`;
  };

  onMounted(async () => {
    try {
      const wasmModule = await import('@/assets/wasm/PointCloudViewer.js');

      const moduleOptions = {
        canvas: pointCloudViewerNativeCanvas.value,
      };
      pointCloudViewerInstance.value = await wasmModule.default(moduleOptions);

      pointCloudViewerInstance.value._StartRendering();
    } catch (error) {
      console.error('Error loading WASM:', error);
    }

    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);
  });

  onUnmounted(() => {
    pointCloudViewerInstance.value._StopRendering();
    pointCloudViewerInstance.value = null;

    window.removeEventListener('resize', resizeCanvas);
  });

</script>
