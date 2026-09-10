/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - llmChat API 클라이언트
 * ==============================================================================
 *
 * 1. 하는 일
 *    - 우측 AI 채팅창이 서버(LlmChatController)와 주고받는 통신을 모아 둔 곳입니다.
 *
 * 2. 왜 별도 axios 인스턴스를 쓰나
 *    - 공용 axiosInstance 는 제한 시간이 10초입니다. 일반 조회에는 넉넉하지만
 *      AI 답변은 10초를 넘기는 일이 흔해 그대로 쓰면 멀쩡한 답이 끊깁니다.
 *      그래서 채팅 전용으로 제한 시간을 넉넉히(180초) 잡은 인스턴스를 따로 만듭니다.
 *
 * 2-1. 질문 보내기는 왜 axios 가 아닌가
 *    - 답변을 SSE(text/event-stream)로 조각조각 받기 때문입니다.
 *      axios 는 응답을 다 받은 뒤에야 넘겨주므로 스트리밍에 맞지 않습니다.
 *    - 서버가 POST 를 받으므로 브라우저 EventSource 도 쓸 수 없습니다(EventSource 는 GET 전용).
 *      그래서 fetch + ReadableStream 으로 직접 읽습니다.
 *
 * 3. 로그인 정보
 *    - 다른 화면과 같은 방식으로 세션에 저장된 고객 ID(repCustId)를 함께 보냅니다.
 *      서버는 이 값으로 "본인 대화만" 다루도록 걸러 냅니다.
 * ==============================================================================
 */
import axios from 'axios';
import { getCurrentCustomerId } from '../utils/memberProfile';

/**
 * 채팅 전용 axios 인스턴스.
 * 공용 인스턴스(10초)와 달리 AI 응답을 기다릴 수 있게 제한 시간을 길게 잡는다.
 */
const chatAxios = axios.create({
    baseURL: '/api/LlmChat',
    timeout: 180000, // 180초. 서버 쪽 TimeoutSeconds(기본 180)와 맞춰 둔다.
    headers: { 'Content-Type': 'application/json' },
});

/**
 * 서버가 준 오류 메시지를 꺼낸다. 없으면 상황에 맞는 안내 문구를 만든다.
 */
function toMessage(error, fallback) {
    if (error?.code === 'ECONNABORTED') {
        return '응답이 너무 오래 걸려 중단했습니다. 잠시 후 다시 시도해 주세요.';
    }
    return error?.response?.data?.message || error?.message || fallback;
}

/**
 * 1. 대화 세션 열기
 * - 로그인 직후 한 번 부른다. 이미 열려 있는 세션이 있으면 그 세션과 지난 대화를 그대로 돌려받는다.
 *   그래서 새로고침을 해도 하던 대화가 이어진다.
 *
 * @param {{prjId?: string, packLevel?: string}} info 세션을 여는 시점의 프로젝트 정보
 * @returns {Promise<{session: object, messages: object[]}>}
 */
export async function openChatSession(info = {}) {
    const { data } = await chatAxios.post('/OpenSession', {
        repCustId: getCurrentCustomerId(),
        prjId: info.prjId || null,
        packLevel: info.packLevel || null,
    });
    return data?.data ?? { session: null, messages: [] };
}

/**
 * 2. 대화 세션 닫기
 * - 로그아웃할 때 부른다. 기록은 지우지 않고 상태만 CLOSED 로 바꾼다.
 * - 로그아웃을 막으면 안 되므로 실패해도 조용히 넘어간다.
 */
export async function closeChatSession(chatSesId) {
    if (!chatSesId) return;
    try {
        await chatAxios.post('/CloseSession', null, {
            params: { chatSesId, repCustId: getCurrentCustomerId() },
        });
    } catch (e) {
        console.warn('대화 세션을 닫지 못했습니다:', e?.message);
    }
}

/**
 * 3. 질문 보내기 — 스트리밍 (기본 경로)
 * - 답변 조각이 오는 대로 onDelta 로 넘겨줍니다. 화면은 받는 즉시 글자를 붙여 보여 주면 됩니다.
 * - 서버가 POST 로 SSE 를 내려 주므로 EventSource 대신 fetch + ReadableStream 을 씁니다.
 *
 * 이벤트 종류
 *   delta : {text}                                     답변 조각. 여러 번 온다.
 *   done  : {chatSesId, userMessage, assistantMessage,  전송 종료. 마지막 1회.
 *            elpsMsVal, warning, contextSteps}
 *   error : {code, message}                            처리 중 오류.
 *
 * @param {object}   payload           질문과 화면 정보
 * @param {Function} payload.onDelta   (text) => void   답변 조각이 올 때마다 호출
 * @param {AbortSignal} payload.signal 중간에 끊고 싶을 때
 * @returns {Promise<object>} done 이벤트의 내용
 */
export async function sendChatMessageStream(payload) {
    const { onDelta, signal, ...body } = payload;

    const res = await fetch('/api/LlmChat/SendStream', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            Accept: 'text/event-stream',
        },
        body: JSON.stringify({
            chatSesId: body.chatSesId || null,
            repCustId: getCurrentCustomerId(),
            question: body.question,
            prjId: body.prjId || null,
            packLevel: body.packLevel || null,
            curMenuId: body.curMenuId || null,
        }),
        signal,
    });

    // 스트리밍이 시작되기 전에 걸린 오류(로그인·권한·입력값)는 평소처럼 상태코드로 온다
    if (!res.ok) {
        let message = `요청이 거절되었습니다. (HTTP ${res.status})`;
        try {
            const err = await res.json();
            if (err?.message) message = err.message;
        } catch {
            // 본문이 JSON 이 아니면 위 기본 문구를 쓴다
        }
        throw new Error(message);
    }
    if (!res.body) {
        throw new Error('이 브라우저에서는 스트리밍 응답을 읽을 수 없습니다.');
    }

    const reader = res.body.getReader();
    const decoder = new TextDecoder('utf-8');

    let buffer = '';   // 아직 이벤트 하나를 이루지 못한 나머지 조각
    let result = null; // done 이벤트의 내용
    let failure = null;

    try {
        for (;;) {
            const { value, done } = await reader.read();
            if (done) break;

            // stream:true 로 두어야 한글이 조각나도 글자가 깨지지 않는다
            buffer += decoder.decode(value, { stream: true });

            // 이벤트는 빈 줄(\n\n)로 구분된다
            let sep;
            while ((sep = buffer.indexOf('\n\n')) !== -1) {
                const chunk = buffer.slice(0, sep);
                buffer = buffer.slice(sep + 2);

                const evt = parseSseChunk(chunk);
                if (!evt) continue;

                if (evt.event === 'delta') {
                    if (evt.data?.text) onDelta?.(evt.data.text);
                } else if (evt.event === 'done') {
                    result = evt.data;
                } else if (evt.event === 'error') {
                    // 오류가 나도 그때까지 받은 조각은 화면에 남겨 두고, 사유만 붙인다
                    failure = evt.data?.message || 'AI 응답 중 오류가 발생했습니다.';
                }
            }
        }
    } finally {
        reader.releaseLock();
    }

    if (!result && failure) throw new Error(failure);
    if (!result) throw new Error('답변을 끝까지 받지 못했습니다. 잠시 후 다시 시도해 주세요.');

    // done 은 받았지만 도중에 error 도 있었던 경우, 저장된 기록은 살리고 사유만 함께 넘긴다
    if (failure) result.streamError = failure;
    return result;
}

/**
 * SSE 이벤트 한 덩어리("event: xxx\ndata: {...}")를 객체로 바꾼다.
 */
function parseSseChunk(chunk) {
    let event = null;
    const dataLines = [];

    for (const raw of chunk.split('\n')) {
        const line = raw.replace(/\r$/, '');
        if (line.startsWith('event:')) {
            event = line.slice(6).trim();
        } else if (line.startsWith('data:')) {
            dataLines.push(line.slice(5).replace(/^ /, ''));
        }
        // ':' 로 시작하는 주석 줄 등은 무시한다
    }

    if (!event || dataLines.length === 0) return null;

    try {
        return { event, data: JSON.parse(dataLines.join('\n')) };
    } catch {
        return null;
    }
}

/**
 * 3-1. 질문 보내기 — 단건 응답 (대체 경로)
 * - 응답을 모아 두는 중간 서버 때문에 스트리밍이 막히는 환경에서 씁니다.
 * - 완성된 답변이 올 때까지 기다렸다가 한 번에 받습니다.
 */
export async function sendChatMessage(payload) {
    try {
        const { data } = await chatAxios.post('/Send', {
            chatSesId: payload.chatSesId || null,
            repCustId: getCurrentCustomerId(),
            question: payload.question,
            prjId: payload.prjId || null,
            packLevel: payload.packLevel || null,
            curMenuId: payload.curMenuId || null,
        });
        return data?.data ?? null;
    } catch (error) {
        // 화면에 보여 줄 문구로 바꾸되, 원래 오류를 cause 로 달아 둔다(디버깅용)
        throw new Error(toMessage(error, 'AI 응답을 받지 못했습니다.'), { cause: error });
    }
}

/**
 * 4. 지난 대화 목록
 * - 백업(2일 경과)으로 넘어간 세션도 함께 받아 온다. 목록에서 '보관됨'으로 구분해 보여 준다.
 */
export async function getChatSessions(includeArchived = true) {
    const { data } = await chatAxios.get('/GetSessions', {
        params: { repCustId: getCurrentCustomerId(), includeArchived },
    });
    return data?.data ?? [];
}

/**
 * 5. 지난 대화 하나 열기
 * - 백업된 세션이면 서버가 백업 테이블에서 읽어 돌려준다.
 */
export async function getChatHistory(chatSesId) {
    const { data } = await chatAxios.get('/GetHistory', {
        params: { chatSesId, repCustId: getCurrentCustomerId() },
    });
    return data?.data ?? { session: null, messages: [] };
}

/**
 * 6. 백업 즉시 실행 (관리자용)
 * - 평소에는 서버가 주기적으로 알아서 돈다. 이 함수는 지금 바로 돌리고 싶을 때 쓴다.
 */
export async function runChatArchive() {
    const { data } = await chatAxios.post('/Archive');
    return data;
}
