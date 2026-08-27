/**
 * [환경 설정 및 토큰 저장소 이름 정의]
 * - VITE_API_BASE: 환경변수에 설정된 API 주소가 있다면 그걸 쓰고, 없으면 기본값으로 '/api'를 사용합니다.
 * - TOKEN_KEY: 브라우저에 로그인 토큰을 저장할 때 쓸 '상자 이름표(키 값)'를 뜻합니다.
 */
const API_BASE = import.meta.env.VITE_API_BASE ?? '/api'
const TOKEN_KEY = 'ecopack.accessToken' //서버가 발급해주는 토튼의 닉네임 정도...

// [기존에 주석 처리되어 있던 코드 - localStorage(영구 저장) 대신 아래의 sessionStorage를 사용하기로 변경됨]
// export function getAccessToken() {   return localStorage.getItem(TOKEN_KEY) }
// export function setAccessToken(token) {   localStorage.setItem(TOKEN_KEY, token) }
// export function clearAccessToken() {   localStorage.removeItem(TOKEN_KEY) }

/**
 * [토큰 가져오기]
 * 브라우저를 껐다 켜거나 탭을 닫으면 사라지는 '세션 스토리지(sessionStorage)'에서 로그인 토큰을 꺼내옵니다.
 */
export function getAccessToken() {
    return sessionStorage.getItem(TOKEN_KEY)
}

/**
 * [토큰 저장하기]
 * 로그인에 성공했을 때 서버가 준 토큰을 세션 스토리지 상자에 쏙 집어넣어 보관합니다.
 */
export function setAccessToken(token) {
    sessionStorage.setItem(TOKEN_KEY, token)
}

/**
 * [토큰 삭제하기 (로그아웃)]
 * 로그아웃할 때 세션 스토리지에 보관했던 토큰을 깔끔하게 지워버립니다.
 */
export function clearAccessToken() {
    sessionStorage.removeItem(TOKEN_KEY)
}

/**
 * [에러 메시지 해석기]
 * 서버와 통신하다가 에러(실패)가 났을 때, 서버가 준 에러 메시지를 읽거나
 * 상태 코드(401, 404, 502 등)에 맞춰 사람이 이해하기 쉬운 안내 문구로 바꿔서 돌려줍니다.
 */
async function parseError(response) {
    try {
        const body = await response.json()
        if (body?.message) {
            return body.message // 서버가 구체적인 에러 이유를 보냈다면 그 문장을 반환
        }
    } catch {
        // JSON 변환에 실패하면 무시하고 아래 기본 에러 처리로 넘어감
    }

    // 401 에러: 로그인이 안 되어 있거나 토큰이 만료되었을 때
    if (response.status === 401) {
        return '인증이 필요합니다. 다시 로그인해 주세요.'
    }

    // 404 에러: 요청한 주소(엔드포인트)를 찾지 못했을 때
    if (response.status === 404) {
        return 'API 엔드포인트를 찾을 수 없습니다. ecopack.Api 서버를 재시작하세요. (dotnet run)'
    }

    // 502 에러: 백엔드 API 서버가 아예 켜져있지 않거나 연결이 안 될 때
    if (response.status === 502) {
        return 'API 서버에 연결할 수 없습니다. ecopack.Api 폴더에서 dotnet run 을 실행하세요.'
    }

    // 그 외의 에러는 상태 번호와 함께 그대로 반환
    return `API request failed: ${response.status}`
}

/**
 * [공통 서버 요청(통신) 함수]
 * 앞으로 프론트엔드에서 서버로 데이터를 보내거나 가져올 때 항상 이 함수를 거쳐서 갑니다.
 */
export async function request(path, options = {}) {
    // 기본 헤더(주문서 봉투) 설정: 데이터를 JSON 형식으로 보낸다고 명시
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
    }

    // 만약 내 주머니에 로그인 토큰이 있다면?
    const token = getAccessToken()
    if (token) {
        // 봉투(헤더)에 "나 로그인한 사람이야!" 하고 인증 토큰을 동봉합니다.
        headers.Authorization = `Bearer ${token}`
    }

    // fetch를 이용해 실제로 백엔드 서버로 요청을 날립니다. (주소 + 옵션)
    const response = await fetch(`${API_BASE}${path}`, {
        ...options,
        headers,
    })

    // 서버 응답이 실패(ok가 아님)라면, 에러 메시지를 해석해서 강제로 에러를 터트립니다.
    if (!response.ok) {
        throw new Error(await parseError(response))
    }

    // 서버가 "성공했고 돌려줄 데이터는 없어(204)"라고 하면 빈 값(null)을 돌려줍니다.
    if (response.status === 204) {
        return null
    }

    // 성공적으로 받아온 데이터를 자바스크립트가 읽을 수 있는 JSON 형태로 변환해서 돌려줍니다.
    return response.json()
}

/**
 * [API 주머니 모음 객체]
 * 컴포넌트들에서 편하게 api.getHealth() 같은 식으로 서버 기능을 불러다 쓸 수 있도록 모아둔 곳입니다.
 */
export const api = {
    // 서버가 잘 살아있는지 확인(Health Check)하는 기본 API 요청 함수
    getHealth: () => request('/health'),
}