import type { OcrResponse, UpdateRequest } from '../types/ocr'

const BASE_URL = 'http://localhost:5084/api'

export async function fetchOcr(id: number): Promise<OcrResponse> {
  const res = await fetch(`${BASE_URL}/ocr/${id}`)
  if (!res.ok) throw new Error('加载失败')
  return res.json()
}

export async function createOcr(rawJson: string): Promise<{ id: number; status: string; version: number }> {
  const res = await fetch(`${BASE_URL}/ocr`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ rawJson: JSON.parse(rawJson) })
  })
  if (!res.ok) throw new Error('创建失败')
  return res.json()
}

export async function updateOcr(id: number, request: UpdateRequest): Promise<{ version: number }> {
  const res = await fetch(`${BASE_URL}/ocr/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request)
  })

  if (res.status === 409) {
    throw new Error('数据已被修改，请刷新页面')
  }

  if (!res.ok) throw new Error('保存失败')

  return res.json()
}