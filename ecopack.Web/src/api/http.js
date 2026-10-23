/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - http (서버 호출 공통 모듈)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - 예전에는 서버를 부르는 방법이 화면마다 달랐습니다(axios 직접 호출, 별도 인스턴스, fetch 등).
 *      그중 하나만 로그인 토큰을 실어 보냈습니다.
 *    - 이제 서버가 모든 요청에 로그인 토큰을 요구하므로, 서버 호출은 반드시 이 모듈을 거칩니다.
 *      api/ 폴더의 모든 파일이 여기서 내보내는 것만 씁니다.
 *
 * 2. 내보내는 것
 *    - http(기본)      : 일반 요청용. 토큰을 자동으로 싣습니다. (axios 와 쓰는 법이 같습니다)
 *    - createHttp()    : 제한 시간 등 설정이 다른 요청용(AI 채팅 등). 토큰 처리는 같습니다.
 *    - authFetch()     : 스트리밍 응답처럼 fetch 가 필요한 요청용. 토큰 처리는 같습니다.
 *    - downloadFile()  : 첨부파일 내려받기. 링크(<a href>)로는 토큰을 실을 수 없어 이 함수로 받습니다.
 *
 * 3. 401 처리
 *    - 서버가 401(로그인 필요·토큰 만료)을 주면 토큰을 지우고 UNAUTHORIZED_EVENT 를 알립니다.
 *      AuthProvider 가 이 알림을 받아 로그인 화면으로 돌려보냅니다.
 *    - 로그인 요청 자체의 401(아이디·비밀번호 틀림)은 이 처리를 타지 않습니다(skipAuthRedirect).
 *
 * 4. 토큰 보관
 *    - 세션 스토리지에 둡니다. 탭이나 브라우저를 닫으면 사라집니다.
 * ==============================================================================
 */
import axios from 'axios';

const TOKEN_KEY = 'ecopack.accessToken';
const ISSUED_AT_KEY = 'ecopack.tokenIssuedAt';
const EXPIRES_IN_KEY = 'ecopack.tokenExpiresIn';

/** 서버가 401 을 줬을 때 window 로 알리는 이벤트 이름 */
export const UNAUTHORIZED_EVENT = 'ecopack:unauthorized';

export function getAccessToken() {
    return sessionStorage.getItem(TOKEN_KEY);
}

/** 토큰과 유효 시간(초)을 보관한다. 갱신 시점을 정하는 데 받은 시각도 함께 적어 둔다. */
export function setAccessToken(token, expiresInSeconds) {
    sessionStorage.setItem(TOKEN_KEY, token);
    sessionStorage.setItem(ISSUED_AT_KEY, String(Date.now()));
    if (expiresInSeconds) sessionStorage.setItem(EXPIRES_IN_KEY, String(expiresInSeconds));
}

export function clearAccessToken() {
    sessionStorage.removeItem(TOKEN_KEY);
    sessionStorage.removeItem(ISSUED_AT_KEY);
    sessionStorage.removeItem(EXPIRES_IN_KEY);
}

/** 토큰을 받은 뒤 지난 시간과 전체 유효 시간(밀리초). 토큰이 없으면 null. */
export function getTokenAge() {
    const issuedAt = Number(sessionStorage.getItem(ISSUED_AT_KEY));
    const expiresIn = Number(sessionStorage.getItem(EXPIRES_IN_KEY));
    if (!getAccessToken() || !issuedAt || !expiresIn) return null;
    return { ageMs: Date.now() - issuedAt, lifetimeMs: expiresIn * 1000 };
}

function notifyUnauthorized() {
    clearAccessToken();
    window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
}

/** 토큰 자동 첨부와 401 처리가 붙은 요청 도구를 만든다. */
export function createHttp(config = {}) {
    const instance = axios.create(config);

    instance.interceptors.request.use((cfg) => {
        const token = getAccessToken();
        if (token) cfg.headers.Authorization = `Bearer ${token}`;
        return cfg;
    });

    instance.interceptors.response.use(
        (response) => response,
        (error) => {
            if (error?.response?.status === 401 && !error.config?.skipAuthRedirect) {
                notifyUnauthorized();
            }
            return Promise.reject(error);
        },
    );

    return instance;
}

const http = createHttp();
export default http;

/** fetch 가 필요한 요청(스트리밍 등)에 쓴다. 토큰을 싣고 401 을 같은 방식으로 처리한다. */
export async function authFetch(url, options = {}) {
    const headers = { ...(options.headers || {}) };
    const token = getAccessToken();
    if (token) headers.Authorization = `Bearer ${token}`;

    const res = await fetch(url, { ...options, headers });
    if (res.status === 401) notifyUnauthorized();
    return res;
}

/** 서버가 준 안내 문구(message)를 꺼낸다. 없으면 fallback. */
export function errorMessage(error, fallback) {
    return error?.response?.data?.message || fallback;
}

/**
 * 파일을 내려받아 저장한다.
 * 링크(<a href>)는 로그인 토큰을 실을 수 없으므로, 토큰을 실은 요청으로 내용을 받은 뒤 브라우저에 저장시킨다.
 * @param {string} url 내려받기 API 주소
 * @param {object} params 쿼리 값
 * @param {string} [fallbackName] 서버가 파일명을 알려 주지 않았을 때 쓸 이름
 */
export async function downloadFile(url, params, fallbackName = 'download') {
    let response;
    try {
        response = await http.get(url, { params, responseType: 'blob' });
    } catch (error) {
        // 오류 응답도 blob 으로 오므로 안내 문구를 읽으려면 글자로 풀어야 한다
        let message = '파일을 내려받지 못했습니다.';
        try {
            const text = await error?.response?.data?.text?.();
            if (text) message = JSON.parse(text)?.message || message;
        } catch {
            // 본문이 JSON 이 아니면 기본 문구를 쓴다
        }
        throw new Error(message, { cause: error });
    }

    const objectUrl = URL.createObjectURL(response.data);
    const link = document.createElement('a');
    link.href = objectUrl;
    link.download = fileNameFrom(response.headers?.['content-disposition']) || fallbackName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(objectUrl);
}

/** Content-Disposition 헤더에서 파일명을 꺼낸다(한글 파일명은 filename*=UTF-8'' 형식으로 온다). */
function fileNameFrom(disposition) {
    if (!disposition) return '';
    const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
    if (utf8) {
        try {
            return decodeURIComponent(utf8[1]);
        } catch {
            return utf8[1];
        }
    }
    const plain = /filename="?([^";]+)"?/i.exec(disposition);
    return plain ? plain[1] : '';
}
