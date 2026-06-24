import axios from 'axios'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000/api',
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('gameCenterToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export function toQuery(params: Record<string, string | undefined>) {
  const search = new URLSearchParams()
  Object.entries(params).forEach(([key, value]) => {
    if (value) search.set(key, value)
  })
  return search.toString()
}
