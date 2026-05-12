import { ref, onUnmounted } from 'vue'
import type { OcrData } from '../types/ocr'
import { fetchOcr, updateOcr } from '../api/ocr'

export function useOcrEditor(taskId: number) {
  const data = ref<OcrData | null>(null)
  const version = ref(0)
  const loading = ref(false)
  const saving = ref(false)
  const error = ref<string | null>(null)

  let saveTimer: ReturnType<typeof setTimeout> | null = null

  async function load() {
    loading.value = true
    error.value = null

    try {
      const result = await fetchOcr(taskId)
      data.value = result.data
      version.value = result.version
    } catch (e) {
      error.value = e instanceof Error ? e.message : '加载失败'
    } finally {
      loading.value = false
    }
  }

  async function save(): Promise<boolean> {
    if (!data.value) return false

    saving.value = true
    error.value = null

    try {
      const result = await updateOcr(taskId, {
        data: data.value,
        version: version.value
      })
      version.value = result.version
      return true
    } catch (e) {
      error.value = e instanceof Error ? e.message : '保存失败'
      return false
    } finally {
      saving.value = false
    }
  }

  function debouncedSave(delay = 500) {
    if (saveTimer) clearTimeout(saveTimer)
    saveTimer = setTimeout(() => save(), delay)
  }

  function updateData(newData: OcrData) {
    data.value = newData
    debouncedSave()
  }

  async function manualSave() {
    if (saveTimer) clearTimeout(saveTimer)
    return await save()
  }

  onUnmounted(() => {
    if (saveTimer) clearTimeout(saveTimer)
  })

  return {
    data,
    version,
    loading,
    saving,
    error,
    load,
    updateData,
    manualSave
  }
}