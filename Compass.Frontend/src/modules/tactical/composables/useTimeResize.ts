import { ref } from 'vue';



export function useTimeResize(pixelsPerMinute: number, onResizeEnd: (id: string, newDuration: number) => void) {
  const resizingId = ref<string | null>(null);
  const previewDuration = ref<number | null>(null);
  let startY = 0;
  let initialDuration = 0;

  const startResize = (e: MouseEvent, id: string, currentDuration: number) => {
    e.stopPropagation(); // Evita ativar o Drag & Drop do card pai
    resizingId.value = id;
    initialDuration = currentDuration;
    previewDuration.value = currentDuration;
    startY = e.clientY;

    const onMouseMove = (moveEvent: MouseEvent) => {
      const deltaY = moveEvent.clientY - startY;
      
      // Calcula a diferença e faz um "Snap" de 15 em 15 minutos
      const deltaMinutes = Math.round((deltaY / pixelsPerMinute) / 15) * 15;
      let newDur = initialDuration + deltaMinutes;
      
      if (newDur < 15) newDur = 15; // Trava mínima de 15 minutos
      previewDuration.value = newDur;
    };

    const onMouseUp = () => {
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
      
      // Se houve mudança real, dispara a API
      if (resizingId.value && previewDuration.value !== null && previewDuration.value !== initialDuration) {
        onResizeEnd(resizingId.value, previewDuration.value);
      }
      
      resizingId.value = null;
      previewDuration.value = null;
    };

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
  };

  return { resizingId, previewDuration, startResize };
}