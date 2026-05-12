<template>
  <div class="ocr-editor-page">
    <div class="toolbar">
      <h1>OCR Demo</h1>
      <div class="toolbar-actions">
        <span class="version-badge">版本: {{ version }}</span>
        <button class="save-btn" :disabled="saving" @click="manualSave">
          {{ saving ? '保存中...' : '手动保存' }}
        </button>
      </div>
    </div>

    <div v-if="error" class="error-message">{{ error }}</div>

    <div v-if="loading" class="loading">加载中...</div>

    <div v-else-if="data" class="editor-container">
      <DynamicForm :model-value="data" :schema="FORM_SCHEMA" @update:model-value="updateData" />

      <div class="json-preview">
        <h3>当前 JSON 数据</h3>
        <pre>{{ JSON.stringify(data, null, 2) }}</pre>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import DynamicForm from '../components/DynamicForm.vue'
import { useOcrEditor } from '../composables/useOcrEditor'
import { FORM_SCHEMA } from '../types/ocr'

const taskId = 1

const { data, version, loading, saving, error, load, updateData, manualSave } = useOcrEditor(taskId)

onMounted(() => {
  load()
})
</script>

<style scoped>
.ocr-editor-page {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #eee;
}

.toolbar h1 {
  margin: 0;
  font-size: 20px;
  color: #333;
}

.toolbar-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.version-badge {
  padding: 4px 12px;
  background: #e6f7ff;
  color: #1890ff;
  border-radius: 12px;
  font-size: 13px;
}

.save-btn {
  padding: 8px 20px;
  background: #1890ff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.2s;
}

.save-btn:hover:not(:disabled) {
  background: #1677cc;
}

.save-btn:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.error-message {
  padding: 12px;
  background: #fff2f0;
  border: 1px solid #ffccc7;
  border-radius: 4px;
  color: #ff4d4f;
  margin-bottom: 16px;
  font-size: 14px;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #666;
}

.editor-container {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  align-items: start;
}

.json-preview {
  background: #1e1e1e;
  border-radius: 8px;
  padding: 16px;
}

.json-preview h3 {
  margin: 0 0 12px 0;
  color: #ccc;
  font-size: 14px;
}

.json-preview pre {
  margin: 0;
  color: #ce9178;
  font-family: 'Cascadia Code', 'Fira Code', 'JetBrains Mono', monospace;
  font-size: 13px;
  line-height: 1.6;
  overflow-x: auto;
  white-space: pre;
}
</style>