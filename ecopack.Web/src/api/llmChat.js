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
 * 3. 질문 보내기
 * - 지금 보고 있는 프로젝트와 화면(메뉴)을 함께 보낸다.
 *   서버는 이 정보로 프로젝트 생성부터 지금까지의 전 과정을 읽어 답변에 반영한다.
 *
 * @param {object} payload
 * @param {string} payload.chatSesId  현재 대화 세션 ID
 * @param {string} payload.question   질문
 * @param {string} payload.prjId      지금 보고 있는 프로젝트 ID
 * @param {string} payload.packLevel  지금 보고 있는 포장차수
 * @param {string} payload.curMenuId  지금 중앙 화면의 메뉴 ID
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
