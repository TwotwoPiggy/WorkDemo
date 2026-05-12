<template>
  <div class="dynamic-form">
    <div v-for="field in schema" :key="field.key" class="form-field">
      <StringField
        v-if="field.type === 'string'"
        :model-value="String(getValue(field.key) ?? '')"
        :label="field.label"
        :placeholder="field.placeholder"
        @update:model-value="updateField(field.key, $event)"
      />

      <ArrayField
        v-else-if="field.type === 'array'"
        :model-value="(getValue(field.key) as Record<string, string>[]) || []"
        :label="field.label"
        :children="field.children"
        @update:model-value="updateField(field.key, $event)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FieldSchema } from '../types/ocr'
import StringField from './StringField.vue'
import ArrayField from './ArrayField.vue'

const props = defineProps<{
  modelValue: Record<string, unknown>
  schema: FieldSchema[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: Record<string, unknown>]
}>()

function getValue(key: string): unknown {
  return props.modelValue?.[key]
}

function updateField(key: string, value: unknown) {
  emit('update:modelValue', {
    ...props.modelValue,
    [key]: value
  })
}
</script>

<style scoped>
.dynamic-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-field {
  min-width: 300px;
}
</style>