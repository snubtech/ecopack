/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ProfilePage 컴포넌트 (회원정보 수정)
 * ==============================================================================
 * 
 * 1. 진입 경로
 *    - 좌측 사이드바 유저 카드의 톱니바퀴 버튼으로 열리는 모달 화면입니다.
 * 
 * 2. 초기 데이터 로드 (useEffect)
 *    - 로그인한 아이디로 서버에서 회원정보를 받아 입력칸을 채웁니다.
 * 
 * 3. 저장 (handleSubmit)
 *    - 사업자·담당자·주소 정보를 서버로 보내 수정합니다.
 *    - 비밀번호는 입력했을 때만 바꾸며, 현재 비밀번호가 맞아야 변경됩니다.
 *    - 저장에 성공하면 상위 화면에 새 정보를 넘겨 사이드바 표시와 세션을 함께 갱신합니다.
 * 
 * 4. 화면 렌더링 (JSX)
 *    - 배경을 눌러도 닫히게 하고, 내용이 길어지면 모달 안쪽만 스크롤되게 했습니다.
 * ==============================================================================
 */
import { useEffect, useState } from 'react';
import { authApi } from '../api/auth';

/**
 * 회원정보 수정 — 좌측 사이드바 유저 카드의 톱니바퀴 버튼으로 열리는 모달.
 * 여기서 바꾼 회사·담당자 정보는 기술문서/적합성선언서 화면에도 그대로 반영된다.
 *
 * @param {string}   repCustId 로그인한 아이디
 * @param {Function} onClose   닫기
 * @param {Function} onSaved   저장 성공 시 갱신된 프로필을 넘긴다
 */

const SECTIONS = [
    {
        title: '사업자 정보',
        fields: [
            { key: 'bizNm',      label: '회사명',      maxLength: 100, hint: '기술문서의 제조사' },
            { key: 'bizNo',      label: '사업자번호',  maxLength: 14 },
            { key: 'custTypeNm', label: '사업자 구분', type: 'select', options: ['법인', '개인사업자'] },
            { key: 'indstNm',    label: '업종',        maxLength: 100 },
            { key: 'cntryNm',    label: '국가',        maxLength: 50, hint: '기술문서의 제조국' },
        ],
    },
    {
        title: '담당자 정보',
        fields: [
            { key: 'repNm',    label: '담당자명',   maxLength: 50 },
            { key: 'roleNm',   label: '직책',       maxLength: 30 },
            { key: 'emlAddr',  label: '이메일',     maxLength: 100, type: 'email' },
            { key: 'repTelNo', label: '대표번호',   maxLength: 20 },
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

const PROFILE_KEYS = SECTIONS.flatMap((s) => s.fields.map((f) => f.key));

export default function ProfilePage({ repCustId, onClose, onSaved }) {
    const [form, setForm] = useState(() => {
        const f = {};
        PROFILE_KEYS.forEach((k) => { f[k] = ''; });
        return f;
    });
    const [pwd, setPwd] = useState({ currentPwd: '', repCustPwd: '', pwdConfirm: '' });
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState('');

    const setField = (key) => (value) => setForm((prev) => ({ ...prev, [key]: value }));
    const setPwdField = (key) => (value) => setPwd((prev) => ({ ...prev, [key]: value }));

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const res = await authApi.getProfile(repCustId);
                if (!alive) return;
                const p = res?.data ?? {};
                setForm((prev) => {
                    const next = { ...prev };
                    PROFILE_KEYS.forEach((k) => { next[k] = p[k] ?? ''; });
                    return next;
                });
            } catch (err) {
                console.error('회원정보 조회 실패:', err);
                if (alive) setError('회원정보를 불러오지 못했습니다.');
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, [repCustId]);

    const handleSubmit = async (event) => {
        event.preventDefault();
        setError('');

        const changingPwd = Boolean(pwd.repCustPwd || pwd.pwdConfirm || pwd.currentPwd);
        if (changingPwd) {
            if (!pwd.currentPwd) {
                setError('비밀번호를 바꾸려면 현재 비밀번호를 입력해 주세요.');
                return;
            }
            if (pwd.repCustPwd !== pwd.pwdConfirm) {
                setError('새 비밀번호가 서로 일치하지 않습니다.');
                return;
            }
            if (!pwd.repCustPwd) {
                setError('새 비밀번호를 입력해 주세요.');
                return;
            }
        }

        setSaving(true);
        try {
            const payload = { repCustId, ...form };
            if (changingPwd) {
                payload.currentPwd = pwd.currentPwd;
                payload.repCustPwd = pwd.repCustPwd;
            }

            const res = await authApi.updateProfile(payload);
            if (res?.success) {
                alert(res.message || '회원정보가 수정되었습니다.');
                if (typeof onSaved === 'function') onSaved(res.data);
                if (typeof onClose === 'function') onClose();
            } else {
                setError(res?.message || '수정에 실패했습니다.');
            }
        } catch (err) {
            console.error('회원정보 수정 실패:', err);
            setError(err.message || '수정 중 오류가 발생했습니다.');
        } finally {
            setSaving(false);
        }
    };

    return (
        <div className="pf-backdrop" onClick={(e) => { if (e.target === e.currentTarget) onClose?.(); }}>
            <style>{PROFILE_STYLES}</style>
            <div className="pf-modal" role="dialog" aria-label="회원정보 수정">
                <div className="pf-head">
                    <div>
                        <strong>회원정보 수정</strong>
                        <span className="pf-id">{repCustId}</span>
                    </div>
                    <button type="button" className="pf-close" onClick={onClose} aria-label="닫기">×</button>
                </div>

                {loading ? (
                    <p className="pf-loading">회원정보를 불러오는 중입니다...</p>
                ) : (
                    <form className="pf-body" onSubmit={handleSubmit}>
                        {SECTIONS.map((section) => (
                            <fieldset key={section.title} className="pf-section">
                                <legend>{section.title}</legend>
                                <div className="pf-grid">
                                    {section.fields.map((f) => (
                                        <label key={f.key} className={`pf-field${f.wide ? ' pf-field-wide' : ''}`}>
                                            <span className="pf-label">{f.label}</span>
                                            {f.type === 'select' ? (
                                                <select
                                                    className="pf-input"
                                                    value={form[f.key]}
                                                    onChange={(e) => setField(f.key)(e.target.value)}
                                                >
                                                    <option value="">선택</option>
                                                    {f.options.map((o) => <option key={o} value={o}>{o}</option>)}
                                                </select>
                                            ) : (
                                                <input
                                                    type={f.type || 'text'}
                                                    className="pf-input"
                                                    value={form[f.key]}
                                                    onChange={(e) => setField(f.key)(e.target.value)}
                                                    maxLength={f.maxLength}
                                                />
                                            )}
                                            {f.hint && <span className="pf-hint">{f.hint}</span>}
                                        </label>
                                    ))}
                                </div>
                            </fieldset>
                        ))}

                        <fieldset className="pf-section">
                            <legend>비밀번호 변경 <span className="pf-hint">(바꿀 때만 입력)</span></legend>
                            <div className="pf-grid">
                                <label className="pf-field">
                                    <span className="pf-label">현재 비밀번호</span>
                                    <input type="password" className="pf-input" autoComplete="current-password"
                                        value={pwd.currentPwd} onChange={(e) => setPwdField('currentPwd')(e.target.value)} />
                                </label>
                                <label className="pf-field">
                                    <span className="pf-label">새 비밀번호</span>
                                    <input type="password" className="pf-input" autoComplete="new-password"
                                        value={pwd.repCustPwd} onChange={(e) => setPwdField('repCustPwd')(e.target.value)} />
                                </label>
                                <label className="pf-field">
                                    <span className="pf-label">새 비밀번호 확인</span>
                                    <input type="password" className="pf-input" autoComplete="new-password"
                                        value={pwd.pwdConfirm} onChange={(e) => setPwdField('pwdConfirm')(e.target.value)} />
                                </label>
                            </div>
                        </fieldset>

                        {error && <p className="pf-error">{error}</p>}

                        <div className="pf-actions">
                            <button type="button" className="pf-btn" onClick={onClose} disabled={saving}>취소</button>
                            <button type="submit" className="pf-btn pf-btn-primary" disabled={saving}>
                                {saving ? '저장 중...' : '저장'}
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    );
}

const PROFILE_STYLES = `
.pf-backdrop {
  position: fixed; inset: 0; z-index: 1000;
  background: rgba(15, 23, 42, .45);
  display: flex; align-items: center; justify-content: center; padding: 24px;
}
.pf-modal {
  background: #fff; border-radius: 10px; width: 720px; max-width: 100%;
  max-height: 90vh; display: flex; flex-direction: column;
  box-shadow: 0 12px 40px rgba(15,23,42,.25);
}
.pf-head {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  padding: 16px 20px; border-bottom: 1px solid #e5e7eb; flex: 0 0 auto;
}
.pf-head strong { font-size: 16px; color: #111827; }
.pf-id { margin-left: 8px; font-size: 12px; color: #6b7280; }
.pf-close {
  border: none; background: none; font-size: 24px; line-height: 1;
  color: #9ca3af; cursor: pointer; padding: 0 4px;
}
.pf-close:hover { color: #374151; }

.pf-loading { padding: 40px; text-align: center; color: #6b7280; }
.pf-body { padding: 16px 20px 20px; overflow-y: auto; display: flex; flex-direction: column; gap: 16px; }

.pf-section { border: 1px solid #e5e7eb; border-radius: 8px; padding: 12px 14px 14px; margin: 0; }
.pf-section > legend { padding: 0 6px; font-size: 13px; font-weight: 600; color: #374151; }
.pf-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 10px 14px; }
.pf-field { display: flex; flex-direction: column; gap: 4px; min-width: 0; }
.pf-field-wide { grid-column: 1 / -1; }
.pf-label { font-size: 12px; font-weight: 500; color: #4b5563; }
.pf-hint { font-size: 11px; color: #9ca3af; font-weight: 400; }
.pf-input {
  box-sizing: border-box; width: 100%;
  padding: 7px 9px; font: inherit; font-size: 13px; color: #111827;
  border: 1px solid #d1d5db; border-radius: 6px; background: #fff;
}
.pf-input:focus { outline: none; border-color: #198754; box-shadow: 0 0 0 2px rgba(25,135,84,.12); }

.pf-error { margin: 0; color: #dc2626; font-size: 13px; }
.pf-actions { display: flex; justify-content: flex-end; gap: 10px; }
.pf-btn {
  padding: 8px 18px; border: 1px solid #cbd5e1; background: #fff; color: #334155;
  border-radius: 6px; cursor: pointer; font-size: 13px; font-weight: 500;
}
.pf-btn:hover:not(:disabled) { background: #f8fafc; }
.pf-btn:disabled { opacity: .45; cursor: not-allowed; }
.pf-btn-primary { background: #198754; border-color: #198754; color: #fff; }
.pf-btn-primary:hover:not(:disabled) { background: #157347; }
`;
