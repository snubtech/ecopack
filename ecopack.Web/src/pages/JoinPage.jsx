/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - JoinPage 컴포넌트 (회원가입)
 * ==============================================================================
 * 
 * 1. 진입 경로
 *    - 로그인 화면의 [회원가입] 버튼으로 들어오고, 가입을 마치면 다시 로그인 화면으로 돌아갑니다.
 *    - 돌아갈 때 가입한 아이디를 넘겨 로그인 칸에 미리 채워 줍니다.
 * 
 * 2. 입력 항목 구성 (SECTIONS)
 *    - 계정 정보 / 사업자 정보 / 담당자 정보 / 주소 네 묶음으로 나눠 그립니다.
 *    - 각 항목은 customer(고객기본) 테이블 컬럼과 1:1로 대응하며,
 *      여기서 입력한 회사·담당자 정보가 기술문서와 적합성선언서에 자동으로 채워집니다.
 * 
 * 3. 아이디 중복확인 (handleCheckId)
 *    - 서버에 같은 아이디가 있는지 물어보고 결과를 화면에 표시합니다.
 *    - 아이디를 다시 고치면 확인 상태를 지워 다시 확인받게 합니다.
 * 
 * 4. 가입 처리 (handleSubmit)
 *    - 필수값·비밀번호 일치·중복확인 여부를 차례로 검사한 뒤 서버로 보냅니다.
 *    - 화면 전용 항목인 비밀번호 확인(pwdConfirm)은 서버로 보내지 않습니다.
 * 
 * 5. 화면 렌더링 (JSX)
 *    - 항목이 많아 카드 안에서 스크롤되게 하고,
 *      [회원가입 완료] 버튼은 하단에 고정해 언제나 보이도록 했습니다.
 * ==============================================================================
 */
import { useState } from 'react';
import { authApi } from '../api/auth';

/**
 * 회원가입 화면 — 로그인 화면의 [회원가입] 버튼으로 진입한다.
 * 입력 항목은 customer(고객기본) 테이블 컬럼과 1:1로 대응하며,
 * 여기서 입력한 회사·담당자 정보가 기술문서/적합성선언서 화면에 자동으로 채워진다.
 *
 * @param {Function} onDone   가입 완료 후 로그인 화면으로 돌아갈 때 호출 (가입한 아이디를 넘긴다)
 * @param {Function} onCancel 취소 시 로그인 화면으로 돌아갈 때 호출
 */

/** 입력 항목 정의 — customer 컬럼과 대응 */
const SECTIONS = [
    {
        title: '계정 정보',
        fields: [
            { key: 'repCustId',  label: '아이디',     required: true, maxLength: 50, autoComplete: 'username' },
            { key: 'repCustPwd', label: '비밀번호',   required: true, type: 'password', autoComplete: 'new-password' },
            { key: 'pwdConfirm', label: '비밀번호 확인', required: true, type: 'password', autoComplete: 'new-password' },
        ],
    },
    {
        title: '사업자 정보',
        fields: [
            { key: 'bizNm',      label: '회사명',     maxLength: 100, hint: '기술문서의 제조사로 사용됩니다' },
            { key: 'bizNo',      label: '사업자번호', maxLength: 14 },
            { key: 'custTypeNm', label: '사업자 구분', type: 'select', options: ['법인', '개인사업자'] },
            { key: 'indstNm',    label: '업종',       maxLength: 100 },
            { key: 'cntryNm',    label: '국가',       maxLength: 50, hint: '기술문서의 제조국으로 사용됩니다' },
        ],
    },
    {
        title: '담당자 정보',
        fields: [
            { key: 'repNm',    label: '담당자명', maxLength: 50 },
            { key: 'roleNm',   label: '직책',     maxLength: 30 },
            { key: 'emlAddr',  label: '이메일',   maxLength: 100, type: 'email', autoComplete: 'email' },
            { key: 'repTelNo', label: '대표번호', maxLength: 20 },
            { key: 'mblTelNo', label: '휴대폰번호', maxLength: 20 },
        ],
    },
    {
        title: '주소',
        fields: [
            { key: 'addrCd',   label: '우편번호', maxLength: 20 },
            { key: 'dtlAddr1', label: '주소',     maxLength: 200, wide: true },
            { key: 'dtlAddr2', label: '상세주소', maxLength: 100, wide: true },
        ],
    },
];

const ALL_KEYS = SECTIONS.flatMap((s) => s.fields.map((f) => f.key));

const emptyForm = () => {
    const f = {};
    ALL_KEYS.forEach((k) => { f[k] = ''; });
    return f;
};

export default function JoinPage({ onDone, onCancel }) {
    const [form, setForm] = useState(emptyForm);
    const [idChecked, setIdChecked] = useState(null); // null=미확인 / true=사용가능 / false=중복
    const [checking, setChecking] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState('');
    const [notice, setNotice] = useState('');

    const setField = (key) => (value) => {
        setForm((prev) => ({ ...prev, [key]: value }));
        if (key === 'repCustId') {
            setIdChecked(null); // 아이디를 바꾸면 중복확인을 다시 받아야 한다
            setNotice('');
        }
    };

    const handleCheckId = async () => {
        const id = form.repCustId.trim();
        if (!id) {
            setError('아이디를 입력해 주세요.');
            return;
        }
        setChecking(true);
        setError('');
        try {
            const res = await authApi.checkId(id);
            setIdChecked(Boolean(res?.available));
            setNotice(res?.message ?? '');
        } catch (err) {
            console.error('아이디 중복확인 실패:', err);
            setError('아이디 중복확인 중 오류가 발생했습니다.');
        } finally {
            setChecking(false);
        }
    };

    const handleSubmit = async (event) => {
        event.preventDefault();
        setError('');

        if (!form.repCustId.trim() || !form.repCustPwd) {
            setError('아이디와 비밀번호는 필수입니다.');
            return;
        }
        if (form.repCustPwd !== form.pwdConfirm) {
            setError('비밀번호가 서로 일치하지 않습니다.');
            return;
        }
        if (idChecked !== true) {
            setError('아이디 중복확인을 먼저 해주세요.');
            return;
        }

        setSubmitting(true);
        try {
            // 화면 전용 항목(pwdConfirm)은 서버로 보내지 않는다
            const { pwdConfirm, ...payload } = form;
            void pwdConfirm;

            const res = await authApi.join(payload);
            if (res?.success) {
                alert(res.message || '회원가입이 완료되었습니다.');
                if (typeof onDone === 'function') onDone(form.repCustId.trim());
            } else {
                setError(res?.message || '회원가입에 실패했습니다.');
            }
        } catch (err) {
            console.error('회원가입 실패:', err);
            setError(err.message || '회원가입 중 오류가 발생했습니다.');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div className="auth-page join-page">
            <style>{JOIN_STYLES}</style>
            <section className="auth-card join-card">
                <p className="eyebrow">PACKAGEVIEW Join</p>
                <h1>회원가입</h1>
                <p className="lead">
                    입력하신 회사·담당자 정보는 기술문서와 적합성 선언서에 자동으로 채워집니다.
                </p>

                <form className="join-form" onSubmit={handleSubmit}>
                    {SECTIONS.map((section) => (
                        <fieldset key={section.title} className="join-section">
                            <legend>{section.title}</legend>
                            <div className="join-grid">
                                {section.fields.map((f) => (
                                    <label
                                        key={f.key}
                                        className={`join-field${f.wide ? ' join-field-wide' : ''}`}
                                    >
                                        <span className="join-label">
                                            {f.label}
                                            {f.required && <em className="join-required">*</em>}
                                        </span>

                                        <span className="join-control">
                                            {f.type === 'select' ? (
                                                <select
                                                    className="join-input"
                                                    value={form[f.key]}
                                                    onChange={(e) => setField(f.key)(e.target.value)}
                                                >
                                                    <option value="">선택</option>
                                                    {f.options.map((o) => <option key={o} value={o}>{o}</option>)}
                                                </select>
                                            ) : (
                                                <input
                                                    type={f.type || 'text'}
                                                    className="join-input"
                                                    value={form[f.key]}
                                                    onChange={(e) => setField(f.key)(e.target.value)}
                                                    maxLength={f.maxLength}
                                                    autoComplete={f.autoComplete}
                                                    required={f.required}
                                                />
                                            )}

                                            {f.key === 'repCustId' && (
                                                <button
                                                    type="button"
                                                    className="join-btn join-btn-check"
                                                    onClick={handleCheckId}
                                                    disabled={checking}
                                                >
                                                    {checking ? '확인 중' : '중복확인'}
                                                </button>
                                            )}
                                        </span>

                                        {f.key === 'repCustId' && notice && (
                                            <span className={idChecked ? 'join-ok' : 'join-ng'}>{notice}</span>
                                        )}
                                        {f.hint && <span className="join-hint">{f.hint}</span>}
                                    </label>
                                ))}
                            </div>
                        </fieldset>
                    ))}

                    {error && <p className="error join-error">{error}</p>}

                    <div className="join-actions">
                        <button type="button" className="join-btn" onClick={onCancel} disabled={submitting}>
                            취소
                        </button>
                        <button type="submit" className="primary-button" disabled={submitting}>
                            {submitting ? '가입 중...' : '회원가입 완료'}
                        </button>
                    </div>
                </form>
            </section>
        </div>
    );
}

const JOIN_STYLES = `
/* .auth-page 는 grid + place-items:center 라 내용이 길어지면 잘린다.
   회원가입은 항목이 많으므로 위에서부터 쌓이게 하고 화면 자체가 스크롤되도록 바꾼다. */
.auth-page.join-page {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  height: 100vh;
  min-height: 0;
  padding: 24px 16px;
  overflow-y: auto;
  box-sizing: border-box;
}
.join-card {
  max-width: 760px; width: 100%;
  display: flex; flex-direction: column;
  max-height: calc(100vh - 48px);
  box-sizing: border-box;
}
.join-form {
  display: flex; flex-direction: column; gap: 18px; margin-top: 18px; text-align: left;
  overflow-y: auto; min-height: 0; flex: 1 1 auto;
  padding-right: 4px;
}

.join-section { border: 1px solid #e5e7eb; border-radius: 8px; padding: 14px 16px 16px; margin: 0; }
.join-section > legend { padding: 0 6px; font-size: 13px; font-weight: 600; color: #374151; }

.join-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 12px 16px; }
.join-field { display: flex; flex-direction: column; gap: 4px; min-width: 0; }
.join-field-wide { grid-column: 1 / -1; }
.join-label { font-size: 12px; font-weight: 500; color: #4b5563; }
.join-required { color: #dc2626; font-style: normal; margin-left: 3px; }
.join-control { display: flex; align-items: center; gap: 6px; }
.join-input {
  flex: 1 1 auto; min-width: 0; box-sizing: border-box;
  padding: 8px 10px; font: inherit; font-size: 13px; color: #111827;
  border: 1px solid #d1d5db; border-radius: 6px; background: #fff;
}
.join-input:focus { outline: none; border-color: #198754; box-shadow: 0 0 0 2px rgba(25,135,84,.12); }
.join-hint { font-size: 11px; color: #9ca3af; }
.join-ok { font-size: 11px; color: #15803d; }
.join-ng { font-size: 11px; color: #dc2626; }

.join-btn {
  flex: 0 0 auto; padding: 8px 14px; border: 1px solid #cbd5e1; background: #fff; color: #334155;
  border-radius: 6px; cursor: pointer; font-size: 13px; font-weight: 500; white-space: nowrap;
}
.join-btn:hover:not(:disabled) { background: #f8fafc; }
.join-btn:disabled { opacity: .45; cursor: not-allowed; }
.join-btn-check { padding: 8px 10px; font-size: 12px; }

.join-error { margin: 0; color: #dc2626; font-size: 13px; }

/* 항목이 많아 스크롤되더라도 [회원가입 완료] 버튼은 항상 보이도록 하단에 고정 */
.join-actions {
  position: sticky; bottom: 0; z-index: 2;
  display: flex; justify-content: flex-end; gap: 10px;
  padding: 12px 0 2px;
  margin-top: 4px;
  background: var(--white, #fff);
  border-top: 1px solid #e5e7eb;
}
.join-actions .primary-button { flex: 0 0 auto; width: auto; padding: 10px 24px; }
`;
