/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - AssistantPanel (우측 AI 채팅창)
 * ==============================================================================
 *
 * 1. 담당 범위
 *    - 3분할 화면 중 오른쪽 AI 채팅창입니다.
 *
 * 2. 대화가 유지되는 방식 (요건 ①)
 *    - 화면이 뜨면 openChatSession 을 불러 서버에 세션을 요청합니다.
 *      로그인 중에 이미 열려 있던 세션이 있으면 그 세션과 지난 대화를 그대로 받아옵니다.
 *      그래서 새로고침을 하거나 메뉴를 옮겨 다녀도 하던 대화가 이어집니다.
 *    - 세션 ID 는 sessionStorage 에도 남겨 둡니다(브라우저를 닫으면 함께 사라짐).
 *
 * 3. 무엇을 참조해 답하나 (요건 ③④)
 *    - 질문을 보낼 때 지금 보고 있는 프로젝트(prjId·packLevel)와
 *      중앙 화면의 메뉴 ID(curMenuId)를 함께 보냅니다.
 *    - 서버는 이 정보로 프로젝트 생성 → 기본사항 → 모의평가 → TD → DOC 까지
 *      전 과정을 DB 에서 읽어 답변에 반영합니다.
 *    - 답변과 함께 받은 "참조한 작업 단계"는 말풍선 아래에 접어서 보여 줍니다.
 *
 * 4. 지난 대화 보기
 *    - 상단 아이콘으로 지난 대화 목록을 엽니다.
 *      2일이 지나 백업된 대화는 '보관됨'으로 표시되고, 열어서 읽을 수 있습니다.
 *      (보관된 대화에는 이어서 질문할 수 없습니다. 새 대화로 물어야 합니다)
 * ==============================================================================
 */
import { useCallback, useEffect, useRef, useState } from 'react';
import { assistantSuggestions } from '../../config/navigation';
import { useAuth } from '../../context/AuthProvider';
import { renderMarkdown } from '../../utils/simpleMarkdown';
import {
    openChatSession,
    sendChatMessage,
    getChatSessions,
    getChatHistory,
} from '../../api/llmChat';

/** 세션 ID 를 담아 두는 sessionStorage 키 */
const SESSION_KEY = 'ecopack.llmSessionId';

export default function AssistantPanel({ currentMenu, projectInfo }) {
    const { user } = useAuth();
    const displayName = user?.profile?.BizNm || user?.profile?.bizNm || user?.repCustId || '사용자';

    const [chatSesId, setChatSesId] = useState(() => sessionStorage.getItem(SESSION_KEY) || '');
    const [messages, setMessages] = useState([]);
    const [prompt, setPrompt] = useState('');
    const [sending, setSending] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // 답변마다 "참조한 작업 단계"를 담아 둔다. 키는 메시지 순번(chatMsgSeq).
    const [contextSteps, setContextSteps] = useState({});
    const [openSteps, setOpenSteps] = useState(null);

    // 지난 대화 목록
    const [showHistory, setShowHistory] = useState(false);
    const [sessions, setSessions] = useState([]);
    const [viewingArchived, setViewingArchived] = useState(false);

    const bodyRef = useRef(null);
    const inputRef = useRef(null);

    // ── 새 말이 붙을 때마다 맨 아래로 내린다 ────────────────────────
    useEffect(() => {
        const el = bodyRef.current;
        if (el) el.scrollTop = el.scrollHeight;
    }, [messages, sending]);

    // ── 화면이 뜨면 세션을 연다(있으면 이어받는다) ──────────────────
    useEffect(() => {
        let alive = true;

        (async () => {
            try {
                const data = await openChatSession({
                    prjId: projectInfo?.id,
                    packLevel: projectInfo?.packLevel,
                });
                if (!alive) return;

                const sid = data?.session?.chatSesId || '';
                setChatSesId(sid);
                if (sid) sessionStorage.setItem(SESSION_KEY, sid);
                setMessages(data?.messages ?? []);
                setViewingArchived(false);
            } catch (e) {
                if (alive) setError(e?.response?.data?.message || '대화를 불러오지 못했습니다.');
            } finally {
                if (alive) setLoading(false);
            }
        })();

        return () => { alive = false; };
        // 세션은 로그인 중 하나만 쓰므로 처음 한 번만 연다.
        // 프로젝트를 바꿔도 세션은 그대로 두고, 질문할 때 그때의 프로젝트를 함께 보낸다.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // ── 질문 보내기 ─────────────────────────────────────────────────
    const handleSubmit = useCallback(async (event) => {
        event?.preventDefault();

        const question = prompt.trim();
        if (!question || sending) return;

        if (viewingArchived) {
            setError('보관된 대화에는 이어서 질문할 수 없습니다. 새 대화로 돌아가 주세요.');
            return;
        }

        setError('');
        setPrompt('');
        setSending(true);

        // 서버 응답을 기다리는 동안 내 질문을 먼저 화면에 띄운다(임시 항목)
        const pendingKey = `pending-${Date.now()}`;
        setMessages((prev) => [
            ...prev,
            { chatMsgId: pendingKey, chatMsgSeq: -1, chatRoleCd: 'user', chatMsgCntn: question, pending: true },
        ]);

        try {
            const result = await sendChatMessage({
                chatSesId,
                question,
                // 질문하는 그 순간 중앙 화면이 보고 있는 대상을 함께 보낸다
                prjId: projectInfo?.id,
                packLevel: projectInfo?.packLevel,
                curMenuId: currentMenu,
            });

            if (!result) throw new Error('AI 응답이 비어 있습니다.');

            if (result.chatSesId && result.chatSesId !== chatSesId) {
                setChatSesId(result.chatSesId);
                sessionStorage.setItem(SESSION_KEY, result.chatSesId);
            }

            // 임시 항목을 서버가 돌려준 실제 기록으로 바꾼다
            setMessages((prev) => [
                ...prev.filter((m) => m.chatMsgId !== pendingKey),
                result.userMessage,
                result.assistantMessage,
            ]);

            if (result.assistantMessage && result.contextSteps?.length) {
                setContextSteps((prev) => ({
                    ...prev,
                    [result.assistantMessage.chatMsgSeq]: result.contextSteps,
                }));
            }
        } catch (e) {
            // 실패하면 임시 항목을 되돌리고, 입력한 질문을 다시 넣어 준다
            setMessages((prev) => prev.filter((m) => m.chatMsgId !== pendingKey));
            setPrompt(question);
            setError(e?.message || 'AI 응답을 받지 못했습니다.');
        } finally {
            setSending(false);
            inputRef.current?.focus();
        }
    }, [prompt, sending, chatSesId, projectInfo, currentMenu, viewingArchived]);

    // ── 지난 대화 목록 열기 ─────────────────────────────────────────
    const toggleHistory = useCallback(async () => {
        if (showHistory) {
            setShowHistory(false);
            return;
        }
        setShowHistory(true);
        try {
            setSessions(await getChatSessions(true));
        } catch (e) {
            setError(e?.response?.data?.message || '지난 대화를 불러오지 못했습니다.');
        }
    }, [showHistory]);

    // ── 지난 대화 하나 열기 ─────────────────────────────────────────
    const openPastSession = useCallback(async (item) => {
        try {
            const data = await getChatHistory(item.chatSesId);
            setMessages(data?.messages ?? []);
            setViewingArchived(Boolean(data?.session?.archived));
            setShowHistory(false);
            setError('');
        } catch (e) {
            setError(e?.response?.data?.message || '대화를 열지 못했습니다.');
        }
    }, []);

    // ── 지금 쓰던 대화로 돌아가기 ───────────────────────────────────
    const backToCurrent = useCallback(async () => {
        setLoading(true);
        try {
            const data = await openChatSession({
                prjId: projectInfo?.id,
                packLevel: projectInfo?.packLevel,
            });
            const sid = data?.session?.chatSesId || '';
            setChatSesId(sid);
            if (sid) sessionStorage.setItem(SESSION_KEY, sid);
            setMessages(data?.messages ?? []);
            setViewingArchived(false);
            setError('');
        } catch (e) {
            setError(e?.response?.data?.message || '대화를 불러오지 못했습니다.');
        } finally {
            setLoading(false);
        }
    }, [projectInfo]);

    const empty = !loading && messages.length === 0;

    return (
        <aside className="dashboard-panel assistant-panel">
            {/* ── 머리말: 대화 상태와 지난 대화 버튼 ── */}
            <div className="assistant-head">
                <div className="assistant-head-title">
                    <span className="assistant-dot" aria-hidden="true" />
                    PackageView AI
                </div>
                <div className="assistant-head-actions">
                    {viewingArchived && (
                        <button type="button" className="assistant-icon-btn" onClick={backToCurrent} title="지금 대화로 돌아가기">
                            ↩
                        </button>
                    )}
                    <button
                        type="button"
                        className="assistant-icon-btn"
                        onClick={toggleHistory}
                        title="지난 대화"
                        aria-expanded={showHistory}
                    >
                        🕘
                    </button>
                </div>
            </div>

            {/* ── 지금 무엇을 보고 답하는지 알려 주는 줄 ── */}
            <div className="assistant-scope">
                {projectInfo?.id ? (
                    <>
                        <b>{projectInfo.name}</b>
                        <span className="assistant-scope-sep">·</span>
                        {projectInfo.packLevel || '1'}차 기준으로 답합니다
                    </>
                ) : (
                    '프로젝트를 선택하면 그 프로젝트의 전 과정을 참고해 답합니다'
                )}
            </div>

            {/* ── 지난 대화 목록 ── */}
            {showHistory && (
                <div className="assistant-history">
                    {sessions.length === 0 ? (
                        <p className="assistant-history-empty">지난 대화가 없습니다.</p>
                    ) : (
                        sessions.map((s) => (
                            <button
                                key={s.chatSesId}
                                type="button"
                                className="assistant-history-item"
                                onClick={() => openPastSession(s)}
                            >
                                <span className="assistant-history-title">
                                    {s.chatSesNm || '(제목 없음)'}
                                </span>
                                <span className="assistant-history-meta">
                                    {s.lastMsgDtm ? new Date(s.lastMsgDtm).toLocaleString('ko-KR') : ''}
                                    {' · '}
                                    {s.chatMsgCnt}건
                                    {s.archived && <em className="assistant-badge-arch">보관됨</em>}
                                </span>
                            </button>
                        ))
                    )}
                </div>
            )}

            {/* ── 대화 본문 ── */}
            <div className="assistant-body" ref={bodyRef} aria-label="AI 어시스턴트 대화 영역">
                {loading && <p className="assistant-note">대화를 불러오는 중…</p>}

                {viewingArchived && (
                    <p className="assistant-note assistant-note-arch">
                        보관된 대화를 보고 있습니다. 이어서 질문하려면 ↩ 버튼으로 지금 대화로 돌아가세요.
                    </p>
                )}

                {empty && (
                    <div className="assistant-empty">
                        <p className="assistant-greeting">
                            {displayName}님, 무엇을 도와드릴까요?
                        </p>
                        <p className="assistant-empty-hint">
                            지금까지 진행한 프로젝트 내용(기본사항·모의평가·TD·DOC)을 모두 참고해 답합니다.
                        </p>
                    </div>
                )}

                {messages.map((m) => (
                    <div
                        key={m.chatMsgId ?? `${m.chatMsgSeq}-${m.chatRoleCd}`}
                        className={`chat-row chat-row-${m.chatRoleCd}`}
                    >
                        <div className={`chat-bubble chat-bubble-${m.chatRoleCd} ${m.errCntn ? 'chat-bubble-error' : ''}`}>
                            {m.chatRoleCd === 'assistant' ? renderMarkdown(m.chatMsgCntn) : m.chatMsgCntn}
                        </div>

                        {/* 답변이 무엇을 근거로 삼았는지 접어서 보여 준다 */}
                        {m.chatRoleCd === 'assistant' && contextSteps[m.chatMsgSeq] && (
                            <div className="chat-context">
                                <button
                                    type="button"
                                    className="chat-context-toggle"
                                    onClick={() => setOpenSteps(openSteps === m.chatMsgSeq ? null : m.chatMsgSeq)}
                                >
                                    참조한 작업 단계 {contextSteps[m.chatMsgSeq].length}건 {openSteps === m.chatMsgSeq ? '▲' : '▼'}
                                </button>
                                {openSteps === m.chatMsgSeq && (
                                    <ul className="chat-context-list">
                                        {contextSteps[m.chatMsgSeq].map((step, i) => (
                                            <li key={i}>{step}</li>
                                        ))}
                                    </ul>
                                )}
                            </div>
                        )}
                    </div>
                ))}

                {sending && (
                    <div className="chat-row chat-row-assistant">
                        <div className="chat-bubble chat-bubble-assistant chat-typing">
                            <span /><span /><span />
                        </div>
                    </div>
                )}
            </div>

            {/* ── 입력부 ── */}
            <div className="assistant-footer">
                {error && <p className="assistant-error">{error}</p>}

                <form className="assistant-input-row" onSubmit={handleSubmit}>
                    <input
                        ref={inputRef}
                        type="text"
                        value={prompt}
                        onChange={(e) => setPrompt(e.target.value)}
                        placeholder={sending ? '답변을 기다리는 중…' : 'PackageView에게 물어보기'}
                        disabled={sending || viewingArchived}
                    />
                    <button
                        type="submit"
                        className="icon-button send-button"
                        aria-label="보내기"
                        disabled={sending || !prompt.trim() || viewingArchived}
                    >
                        ↑
                    </button>
                </form>

                {empty && (
                    <div className="assistant-suggestions">
                        {assistantSuggestions.map((text) => (
                            <button
                                key={text}
                                type="button"
                                className="suggestion-chip"
                                onClick={() => setPrompt(text)}
                            >
                                {text}
                            </button>
                        ))}
                    </div>
                )}
            </div>
        </aside>
    );
}
