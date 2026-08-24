const API_BASE = import.meta.env.VITE_API_BASE ?? '/api'
const TOKEN_KEY = 'ecopack.accessToken'

// export function getAccessToken() {
//   return localStorage.getItem(TOKEN_KEY)
// }

// export function setAccessToken(token) {
//   localStorage.setItem(TOKEN_KEY, token)
// }

// export function clearAccessToken() {
//   localStorage.removeItem(TOKEN_KEY)
// }

export function getAccessToken() {
    return sessionStorage.getItem(TOKEN_KEY) // sessionStorage로 변경
}

export function setAccessToken(token) {
    sessionStorage.setItem(TOKEN_KEY, token) // sessionStorage로 변경
}

export function clearAccessToken() {
    sessionStorage.removeItem(TOKEN_KEY) // sessionStorage로 변경
}


async function parseError(response) {
  try {
    const body = await response.json()
    if (body?.message) {
      return body.message
    }
  } catch {
    // ignore parse errors
  }

  if (response.status === 401) {
    return '인증이 필요합니다. 다시 로그인해 주세요.'
  }

  if (response.status === 404) {
    return 'API 엔드포인트를 찾을 수 없습니다. ecopack.Api 서버를 재시작하세요. (dotnet run)'
  }

  if (response.status === 502) {
    return 'API 서버에 연결할 수 없습니다. ecopack.Api 폴더에서 dotnet run 을 실행하세요.'
  }

  return `API request failed: ${response.status}`
}

export async function request(path, options = {}) {
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  }

  const token = getAccessToken()
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    throw new Error(await parseError(response))
  }

  if (response.status === 204) {
    return null
  }

  return response.json()
}

export const api = {
  getHealth: () => request('/health'),
}
