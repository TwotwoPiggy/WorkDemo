<template>
  <div class="field string-field">
    <label v-if="label">{{ label }}</label>
    <input
      type="text"
      :value="modelValue"
      :placeholder="placeholder"
      @input="emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />
  </div>
</template>

<script setup lang="ts">
defineProps<{
  modelValue: string
  label?: string
  placeholder?: string
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()
</script>

<style scoped>
.string-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.string-field label {
  font-size: 14px;
  font-weight: 500;
  color: #333;
}

.string-field input {
  padding: 8px 12px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.string-field input:focus {
  outline: none;
  border-color: #1890ff;
}
</style>