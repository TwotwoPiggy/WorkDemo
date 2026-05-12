export type FieldType = 'string' | 'number' | 'boolean' | 'array' | 'object'

export interface FieldSchema {
  key: string
  label: string
  type: FieldType
  placeholder?: string
  children?: FieldSchema[]
  defaultValue?: unknown
}

export interface OcrItem {
  child1: string
  child2: string
}

export interface OcrData {
  item1?: string
  item2s?: OcrItem[]
  [key: string]: unknown
}

export interface OcrResponse {
  id: number
  data: OcrData
  version: number
  status: string
  updatedAt: string
}

export interface UpdateRequest {
  data: OcrData
  version: number
}

export const FORM_SCHEMA: FieldSchema[] = [
  {
    key: 'item1',
    label: '项目1',
    type: 'string',
    placeholder: '请输入内容...'
  },
  {
    key: 'item2s',
    label: '项目列表',
    type: 'array',
    children: [
      { key: 'child1', label: '子项1', type: 'string', defaultValue: '' },
      { key: 'child2', label: '子项2', type: 'string', defaultValue: '' }
    ]
  }
]