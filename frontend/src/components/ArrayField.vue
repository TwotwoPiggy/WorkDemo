<template>
  <div class="field array-field">
    <label v-if="label">{{ label }}</label>

    <div class="items">
      <div v-for="(item, index) in items" :key="index" class="array-item">
        <div class="item-header">
          <span>项 {{ index + 1 }}</span>
          <button type="button" class="delete-btn" @click="removeItem(index)">删除</button>
        </div>

        <div class="item-fields">
          <div v-for="child in children" :key="child.key" class="inline-field">
            <label>{{ child.label }}</label>
            <input
              type="text"
              :value="item[child.key] || ''"
              :placeholder="child.placeholder"
              @input="updateItem(index, child.key, ($event.target as HTMLInputElement).value)"
            />
          </div>
        </div>
      </div>
    </div>

    <button type="button" class="add-btn" @click="addItem">+ 添加</button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { FieldSchema } from '../types/ocr'

const props = defineProps<{
  modelValue: Record<string, string>[]
  label?: string
  children?: FieldSchema[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: Record<string, string>[]]
}>()

const items = computed(() => props.modelValue || [])

function getDefaultItem(): Record<string, string> {
  const item: Record<string, string> = {}
  props.children?.forEach(child => {
    item[child.key] = String(child.defaultValue ?? '')
  })
  return item
}

function updateItem(index: number, key: string, value: string) {
  const newItems = [...items.value]
  newItems[index] = { ...newItems[index], [key]: value }
  emit('update:modelValue', newItems)
}

function addItem() {
  const newItems = [...items.value, getDefaultItem()]
  emit('update:modelValue', newItems)
}

function removeItem(index: number) {
  const newItems = items.value.filter((_, i) => i !== index)
  emit('update:modelValue', newItems)
}
</script>

<style scoped>
.array-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.array-field > label {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.items {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.array-item {
  border: 1px solid #e8e8e8;
  border-radius: 6px;
  padding: 12px;
  background: #fafafa;
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.item-header span {
  font-weight: 500;
  color: #333;
}

.delete-btn {
  padding: 4px 12px;
  color: #ff4d4f;
  border: 1px solid #ff4d4f;
  background: transparent;
  border-radius: 4px;
  cursor: pointer;
  font-size: 13px;
  transition: background 0.2s;
}

.delete-btn:hover {
  background: #fff1f0;
}

.item-fields {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 12px;
}

.inline-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.inline-field label {
  font-size: 12px;
  color: #666;
}

.inline-field input {
  padding: 6px 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.inline-field input:focus {
  outline: none;
  border-color: #1890ff;
}

.add-btn {
  padding: 8px 16px;
  color: #1890ff;
  border: 1px dashed #1890ff;
  background: transparent;
  border-radius: 4px;
  cursor: pointer;
  align-self: flex-start;
  font-size: 14px;
  transition: background 0.2s;
}

.add-btn:hover {
  background: #e6f7ff;
}
</style>