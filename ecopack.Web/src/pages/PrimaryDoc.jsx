import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetPrimaryDoc,
    SavePrimaryDoc,
    UploadEvdDoc,
    DeleteEvdDoc,
} from '../api/primaryDoc';

/**
 * DOC (적합성 선언서) — PPWR 적합성 선언서 화면 / primary_doc 테이블
 *
 * 화면 규칙 (기술문서 화면과 동일)
 *  - [화면고정]            : 라벨/제목으로 고정 출력, 수정 불가
 *  - [insert][컬럼명 : x]  : primary_doc 컬럼과 1:1로 묶인 입력 필드 (CRUD 대상)
 *  - [고정 문구, 수정 가능] : 기본 문구가 채워지되 수정 가능 (값이 비었을 때만 적용)
 *  - 부속서 A~H            : F 이후는 알파벳 순으로 자동 생성
 *
 * primary_td 와 달리 컬럼이 전부 varchar(길이 지정)라, 입력값이 길이를 초과해
 * 저장이 실패하지 않도록 MAXLEN 으로 입력 길이를 제한한다.
 */

// ─────────────────────────────────────────────────────────────
// 행 추가/삭제가 가능한 표 정의
//   max     : DB 컬럼 슬롯 최대치 (더 늘리려면 DB에 컬럼 추가 필요)
//   initial : 문서 최초 작성 시 보여줄 기본 행 수
// ─────────────────────────────────────────────────────────────
const ROW_TABLES = {
    soc:    { max: 4, initial: 4, fields: ['sbst', 'testRslt'] },        // 4.3 중금속 (DDL 슬롯 4개)
    matInfo:{ max: 3, initial: 2, fields: ['compltem', 'mat'] },         // 4.4 재질정보
    evd:    { max: 8, initial: 6, fields: ['evdDocNm', 'evdDocUrl'] },   // 5. 부속서 A~H
};
const TABLE_KEYS = Object.keys(ROW_TABLES);

/** 행 시리즈에 속하지 않는 단일 컬럼 */
const SINGLE_KEYS = [
    'pkg1DocId', 'lastWrtDt',
    'bizNm', 'repNm', 'roleNm', 'emlAddr', 'mbTelNo',
    'prjfNm', 'prjId', 'pkg1TechDocId', 'revNo', 'cntryNm', 'dsgnTypeNm',
    'docPhrsCntn',
    'reuseReqCmplCntn', 'dsgnTmplMstrPrdExpl',
    'rcycReqCmplCntn1', 'rcycMainFeatCntn', 'rcycReqCmplCntn2',
    'soCHvyMetLmtCmplCntn1', 'sbstTot', 'testRsltTot',
    'matInfoTotWtVal', 'matInfoCntn',
    'evdDocCntn',
    'applRuleStdCntn',
    'lastDclCntn', 'bizNm2', 'repNm2', 'roleNm2',
];

const FIELD_KEYS = [
    ...SINGLE_KEYS,
    ...TABLE_KEYS.flatMap((key) => {
        const { fields, max } = ROW_TABLES[key];
        return fields.flatMap((f) => Array.from({ length: max }, (_, i) => `${f}${i + 1}`));
    }),
];

/** DDL varchar 길이 — 입력값이 컬럼 길이를 넘겨 저장에 실패하지 않도록 maxLength로 건다 */
const MAXLEN = {
    bizNm: 100,
    repNm: 50,
    roleNm: 30,
    emlAddr: 100,
    mbTelNo: 20,
    prjfNm: 100,
    prjId: 50,
    pkg1TechDocId: 50,
    revNo: 14,
    cntryNm: 50,
    dsgnTypeNm: 300,
    docPhrsCntn: 500,
    reuseReqCmplCntn: 300,
    dsgnTmplMstrPrdExpl: 300,
    rcycReqCmplCntn1: 100,
    rcycMainFeatCntn: 200,
    rcycReqCmplCntn2: 100,
    soCHvyMetLmtCmplCntn1: 100,
    sbst1: 50,
    sbst2: 50,
    sbst3: 50,
    sbst4: 50,
    sbstTot: 50,
    testRslt1: 50,
    testRslt2: 50,
    testRslt3: 50,
    testRslt4: 50,
    testRsltTot: 50,
    compltem1: 50,
    compltem2: 50,
    compltem3: 50,
    mat1: 50,
    mat2: 50,
    mat3: 50,
    matInfoTotWtVal: 50,
    matInfoCntn: 100,
    evdDocCntn: 100,
    evdDocUrl1: 500,
    evdDocUrl2: 500,
    evdDocUrl3: 500,
    evdDocUrl4: 500,
    evdDocUrl5: 500,
    evdDocUrl6: 500,
    evdDocUrl7: 500,
    evdDocUrl8: 500,
    evdDocNm1: 300,
    evdDocNm2: 300,
    evdDocNm3: 300,
    evdDocNm4: 300,
    evdDocNm5: 300,
    evdDocNm6: 300,
    evdDocNm7: 300,
    evdDocNm8: 300,
    applRuleStdCntn: 300,
    lastDclCntn: 200,
    bizNm2: 100,
    repNm2: 50,
    roleNm2: 30,
};

// ─────────────────────────────────────────────────────────────
// [고정 문구, 수정 가능] 기본값
// 값이 비어 있을 때만 채워지며 언제든 수정할 수 있다.
// 표의 행 항목은 '기본 행' 범위까지만 기본 문구를 갖는다(추가 행은 빈 값).
// ─────────────────────────────────────────────────────────────
const DEFAULTS = {
    // 3. 적합성 선언
    docPhrsCntn:
        '지피에스코리아는 본 선언서를 통하여 상기 제품이 유럽연합(EU)의 「포장 및 포장폐기물 규정' +
        '(Packaging and Packaging Waste Regulation, PPWR)」의 적용 요구사항을 충족함을 당사의 전적인 책임 하에 선언합니다. ' +
        '본 제품은 PPWR의 요구사항에 따라 설계, 제조 및 평가되었으며 관련 기술문서와 시험자료를 통해 적합성이 입증되었습니다.',

    // 4.1 재사용성 요구사항 적합
    reuseReqCmplCntn:
        '본 제품은 반복적인 물류 운송 환경에서 사용될 수 있도록 설계된 접이식 EPP 포장재입니다. 다음과 같은 성능을 검증하였습니다.\n' +
        '1. 접이 전개 반복시험 5,000회 이상 통과\n' +
        '2. 재사용 가능 횟수 50회 이상\n' +
        '3. 적재하중 25kg 검증\n' +
        '4. 낙하시험 통과\n' +
        '5. 3년 이상 사용 조건에 대한 내구성 검증\n' +
        '시험결과 본 제품은 PPWR 재사용성 요구사항에 적합함을 확인하였습니다.',

    // 4.2 재활용성 요구사항 적합
    rcycReqCmplCntn1:
        '본 제품은 재활용을 고려한 설계(Design for Recycling)가 적용되었습니다. 주요 특징은 다음과 같습니다.',
    rcycMainFeatCntn:
        '1. EPP 중심의 단일 소재 구조\n' +
        '2. 부품 분리가 용이한 설계\n' +
        '3. 기존 수거·선별 시스템과의 호환성 확보\n' +
        '4. 분쇄, 세척, 재용융, 재생원료 생산 공정을 통한 기계적 재활용 가능',
    rcycReqCmplCntn2:
        '재활용성 평가는 EN 13430 기준에 따라 수행되었으며 적합성을 확인하였습니다.',

    // 4.3 우려물질 및 중금속 제한 적합 — 기본 4행 + 총합행
    soCHvyMetLmtCmplCntn1: 'PPWR 요구사항에 따라 다음 중금속 함량을 평가하였습니다.',
    sbst1: '납(Pb)',
    sbst2: '카드뮴 (Cd)',
    sbst3: '수은(Hg)',
    sbst4: '6가 크롬 (Cr)',
    sbstTot: '총합',

    // 4.4 재질 정보 — 기본 2행
    compltem1: '본체',
    compltem2: '힌지부',
    matInfoCntn: '본 제품은 규제 기준을 초과하는 중금속 또는 제한물질을 의도적으로 포함하지 않습니다.',

    // 5. 근거 문서 — 부속서 A~F 기본 문서명
    evdDocCntn: '본 선언서는 다음 기술문서 및 시험자료를 근거로 작성되었습니다.',
    evdDocNm1: '제품 도면',
    evdDocNm2: '재질명세서(BOM)',
    evdDocNm3: '중금속 시험성적서',
    evdDocNm4: '재활용성 평가 보고서',
    evdDocNm5: '내구성 시험 보고서',
    evdDocNm6: '적합성 선언서',

    // 6. 적용 규정 및 표준
    applRuleStdCntn:
        '1. EU Packaging and Packaging Waste Regulation (PPWR)\n' +
        '2. EN 13430 재활용 가능 포장재 요구사항\n' +
        '3. EN ISO 13427 포장재 내구성 시험\n' +
        '4. EN ISO 16104 운송 포장재 성능 시험\n' +
        '5. IEC 62321 시리즈 유해물질 분석',

    // 7. 최종 선언
    lastDclCntn:
        '지피에스코리아는 상기 제품이 다음 요구사항을 충족함을 선언합니다.\n' +
        '1. 재사용성 요구사항 적합\n2. 재활용성 요구사항 적합\n3. 중금속 제한 요구사항 적합\n' +
        '4. 물질 정보 제공 요구사항 적합\n5. 순환경제(Design for Circular Economy) 설계 원칙 적용\n' +
        '본 선언서는 제조자의 전적인 책임하에 발행됩니다.',
};

/**
 * 4.3 총합행 아래 고정 문구.
 * PDF 스펙에는 [컬럼명 : socHvyMetLmtCmplCntn2] 로 적혀 있으나 primary_doc 에 해당 컬럼이 없어
 * 현재는 저장되지 않는 화면 고정 문구로 출력한다. (컬럼 추가 시 입력 필드로 전환)
 */
const SOC_FOOT_PHRASE =
    '총 중금속 함량은 PPWR 및 EU 포장재 규정에서 요구하는 100 mg/kg 이하 기준을 충분히 만족합니다. ' +
    '시험은 IEC 62321 시리즈 시험방법에 따라 수행되었습니다.';

/** 부속서 라벨 (1→부속서 A … 8→부속서 H) */
const annexLabel = (slot) => `부속서 ${String.fromCharCode(64 + slot)}`;

const emptyForm = () => {
    const base = {};
    FIELD_KEYS.forEach((k) => { base[k] = ''; });
    return base;
};

const mergeWithDefaults = (data) => {
    const merged = emptyForm();
    Object.keys(merged).forEach((k) => {
        const v = data?.[k];
        merged[k] = v === null || v === undefined ? '' : String(v);
    });
    Object.entries(DEFAULTS).forEach(([k, v]) => {
        if (!merged[k]) merged[k] = v;
    });
    return merged;
};

const detectRowCounts = (formData) => {
    const counts = {};
    TABLE_KEYS.forEach((key) => {
        const { fields, max, initial } = ROW_TABLES[key];
        let last = 0;
        for (let i = max; i >= 1; i -= 1) {
            if (fields.some((f) => formData[`${f}${i}`])) { last = i; break; }
        }
        counts[key] = Math.max(initial, last);
    });
    return counts;
};

const formatDate = (value) => {
    if (!value) return '';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    const pad = (n) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
};

// ─────────────────────────────────────────────────────────────
// 입력 컴포넌트
// ─────────────────────────────────────────────────────────────
function DocInput({ value, onChange, readOnly, maxLength, align }) {
    return (
        <input
            type="text"
            className="td-input"
            value={value ?? ''}
            onChange={(e) => onChange(e.target.value)}
            readOnly={readOnly}
            maxLength={maxLength}
            style={align ? { textAlign: align } : undefined}
        />
    );
}

function DocTextArea({ value, onChange, readOnly, maxLength, minRows = 2 }) {
    const ref = useRef(null);
    useEffect(() => {
        const el = ref.current;
        if (!el) return;
        el.style.height = 'auto';
        el.style.height = `${el.scrollHeight}px`;
    }, [value]);
    return (
        <textarea
            ref={ref}
            className="td-input td-textarea"
            rows={minRows}
            value={value ?? ''}
            onChange={(e) => onChange(e.target.value)}
            readOnly={readOnly}
            maxLength={maxLength}
        />
    );
}

function RowDeleteCell({ onDelete, disabled }) {
    return (
        <td className="td-noprint td-rowaction">
            <button type="button" className="td-btn td-btn-sm td-btn-danger"
                onClick={onDelete} disabled={disabled} title="행 삭제">행 삭제</button>
        </td>
    );
}

function AddRowButton({ label, onAdd, current, max }) {
    if (current >= max) {
        return (
            <div className="td-noprint td-addrow">
                <span className="td-addrow-limit">DB 컬럼 슬롯 최대치({max}행)에 도달했습니다.</span>
            </div>
        );
    }
    return (
        <div className="td-noprint td-addrow">
            <button type="button" className="td-btn td-btn-sm" onClick={onAdd}>+ {label}</button>
            <span className="td-addrow-count">{current} / {max}행</span>
        </div>
    );
}

// ─────────────────────────────────────────────────────────────
// 메인 화면
// ─────────────────────────────────────────────────────────────
export default function PrimaryDoc() {
    const prjId = useMemo(() => sessionStorage.getItem('currentPrjId') || '', []);
    const prjNm = useMemo(() => sessionStorage.getItem('currentPrjNm') || '', []);

    const [form, setForm] = useState(() => mergeWithDefaults(null));
    const [rowCounts, setRowCounts] = useState(() => {
        const init = {};
        TABLE_KEYS.forEach((k) => { init[k] = ROW_TABLES[k].initial; });
        return init;
    });
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [editing, setEditing] = useState(false);
    const [isNew, setIsNew] = useState(true);
    const [message, setMessage] = useState('');

    const snapshotRef = useRef(null);
    const docRef = useRef(null);
    const fileInputRefs = useRef({});

    const setField = useCallback((key) => (value) => {
        setForm((prev) => ({ ...prev, [key]: value }));
    }, []);

    // ── 최초 로드 ────────────────────────────────────────────
    useEffect(() => {
        let alive = true;

        const load = async () => {
            if (!prjId) {
                setLoading(false);
                setMessage('선택된 프로젝트가 없습니다. [프로젝트 현황]에서 프로젝트를 먼저 선택해 주세요.');
                return;
            }

            try {
                const res = await GetPrimaryDoc(prjId);
                if (!alive) return;

                const merged = mergeWithDefaults(res?.data ?? null);
                merged.prjId = prjId;
                if (!merged.prjfNm) merged.prjfNm = prjNm;
                if (!merged.revNo) merged.revNo = 'Rev.01';

                const counts = detectRowCounts(merged);
                setForm(merged);
                setRowCounts(counts);
                setIsNew(Boolean(res?.isNew));
                setEditing(Boolean(res?.isNew));
                snapshotRef.current = { form: merged, rowCounts: counts };
            } catch (err) {
                console.error('적합성선언서 조회 실패:', err);
                if (alive) setMessage('적합성선언서를 불러오는 중 오류가 발생했습니다.');
            } finally {
                if (alive) setLoading(false);
            }
        };

        load();
        return () => { alive = false; };
    }, [prjId, prjNm]);

    // ── 행 추가 / 삭제 ───────────────────────────────────────
    const addRow = useCallback((tableKey) => {
        setRowCounts((prev) => ({
            ...prev,
            [tableKey]: Math.min(ROW_TABLES[tableKey].max, prev[tableKey] + 1),
        }));
    }, []);

    /** 행 삭제: 아래 행 값을 위로 당기고 마지막 슬롯을 비운다 */
    const removeRow = useCallback((tableKey, rowIdx) => {
        const { fields, max } = ROW_TABLES[tableKey];
        setForm((prev) => {
            const next = { ...prev };
            for (let i = rowIdx; i < max; i += 1) {
                fields.forEach((f) => { next[`${f}${i}`] = prev[`${f}${i + 1}`] ?? ''; });
            }
            fields.forEach((f) => { next[`${f}${max}`] = ''; });
            return next;
        });
        setRowCounts((prev) => ({ ...prev, [tableKey]: Math.max(1, prev[tableKey] - 1) }));
    }, []);

    // ── 저장 ─────────────────────────────────────────────────
    const handleSave = async () => {
        if (!prjId) {
            alert('선택된 프로젝트가 없습니다.');
            return;
        }
        setSaving(true);
        try {
            const res = await SavePrimaryDoc({ ...form, prjId });
            const saved = mergeWithDefaults(res?.data ?? form);
            saved.prjId = prjId;
            const counts = detectRowCounts(saved);

            setForm(saved);
            setRowCounts(counts);
            snapshotRef.current = { form: saved, rowCounts: counts };
            setIsNew(false);
            setEditing(false);
            setMessage(`저장 완료 (발행일 ${formatDate(res?.lastWrtDt)})`);
            alert(res?.message || '저장되었습니다.');
        } catch (err) {
            console.error('적합성선언서 저장 실패:', err);
            alert('저장 중 오류가 발생했습니다.');
        } finally {
            setSaving(false);
        }
    };

    const handleEdit = () => {
        snapshotRef.current = { form, rowCounts };
        setEditing(true);
        setMessage('');
    };

    const handleCancel = () => {
        if (snapshotRef.current) {
            setForm(snapshotRef.current.form);
            setRowCounts(snapshotRef.current.rowCounts);
        }
        setEditing(false);
        setMessage('');
    };

    // ── 근거문서(부속서) ─────────────────────────────────────
    const handlePickFile = (slot) => {
        if (isNew) {
            alert('적합성선언서를 먼저 저장한 뒤 근거문서를 올려주세요.');
            return;
        }
        fileInputRefs.current[slot]?.click();
    };

    const handleUpload = async (slot, file) => {
        if (!file) return;
        try {
            const res = await UploadEvdDoc(prjId, slot, file);
            if (res?.success) {
                setForm((prev) => ({
                    ...prev,
                    [`evdDocNm${slot}`]: res.fileNm || '',
                    [`evdDocUrl${slot}`]: res.fileUrl || '',
                }));
                setMessage(`${annexLabel(slot)} 업로드 완료: ${res.fileNm}`);
            } else {
                alert(res?.message || '업로드에 실패했습니다.');
            }
        } catch (err) {
            console.error('근거문서 업로드 실패:', err);
            alert('업로드 중 오류가 발생했습니다.');
        } finally {
            if (fileInputRefs.current[slot]) fileInputRefs.current[slot].value = '';
        }
    };

    const handleDeleteFile = async (slot) => {
        if (!window.confirm(`${annexLabel(slot)}에 올린 파일을 삭제할까요?`)) return;
        try {
            const res = await DeleteEvdDoc(prjId, slot);
            if (res?.success) {
                setForm((prev) => ({ ...prev, [`evdDocUrl${slot}`]: '' }));
                setMessage(`${annexLabel(slot)} 파일을 삭제했습니다.`);
            }
        } catch (err) {
            console.error('근거문서 삭제 실패:', err);
            alert('삭제 중 오류가 발생했습니다.');
        }
    };

    const handleDeleteRow = async (slot) => {
        const hasFile = Boolean(form[`evdDocUrl${slot}`]);
        const msg = hasFile
            ? `${annexLabel(slot)} 행을 삭제합니다. 업로드된 파일도 함께 삭제되고 아래 행이 위로 당겨집니다. 계속할까요?`
            : `${annexLabel(slot)} 행을 삭제할까요? 아래 행이 위로 당겨집니다.`;
        if (!window.confirm(msg)) return;
        try {
            if (hasFile) await DeleteEvdDoc(prjId, slot);
            removeRow('evd', slot);
            setMessage('부속서 행을 삭제했습니다. [저장]을 눌러야 DB에 반영됩니다.');
        } catch (err) {
            console.error('부속서 행 삭제 실패:', err);
            alert('행 삭제 중 오류가 발생했습니다.');
        }
    };

    // ── PDF 추출 (브라우저 인쇄 → PDF로 저장) ────────────────
    const handleExportPdf = () => { window.print(); };

    // ── DOCX(Word) 추출 ──────────────────────────────────────
    const handleExportDocx = () => {
        const source = docRef.current;
        if (!source) return;

        const clone = source.cloneNode(true);
        clone.querySelectorAll('.td-noprint, .td-screen-only').forEach((el) => el.remove());
        clone.querySelectorAll('input, textarea').forEach((el) => {
            const span = document.createElement('span');
            span.textContent = el.value || '';
            // 여러 줄 입력은 줄바꿈을 유지한다
            if (el.tagName === 'TEXTAREA') span.style.whiteSpace = 'pre-wrap';
            el.replaceWith(span);
        });

        const html = `<!DOCTYPE html>
<html xmlns:o="urn:schemas-microsoft-com:office:office"
      xmlns:w="urn:schemas-microsoft-com:office:word"
      xmlns="http://www.w3.org/TR/REC-html40">
<head><meta charset="utf-8" />
<title>PPWR 적합성 선언서</title>
<style>
  body { font-family: 'Malgun Gothic', 'Pretendard', sans-serif; font-size: 10.5pt; line-height: 1.6; }
  h1 { font-size: 18pt; text-align: center; } h2 { font-size: 13pt; margin-top: 18pt; } h3 { font-size: 11pt; }
  table { border-collapse: collapse; width: 100%; margin: 8pt 0; }
  th, td { border: 1px solid #999; padding: 4pt 6pt; vertical-align: top; font-size: 9.5pt; }
  th { background: #f2f2f2; font-weight: bold; }
  .td-table-plain, .td-table-plain th, .td-table-plain td { border: none; background: transparent; }
  .doc-subtitle { text-align: center; font-weight: bold; }
  .td-result-phrase { margin: 8pt 0; }
</style>
</head><body>${clone.innerHTML}</body></html>`;

        const blob = new Blob(['﻿', html], { type: 'application/msword' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `적합성선언서_${form.prjfNm || prjId || 'primary_doc'}.doc`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    };

    // ── 렌더 헬퍼 ────────────────────────────────────────────
    const ro = !editing;
    const input = (key, align) => (
        <DocInput value={form[key]} onChange={setField(key)} readOnly={ro}
            maxLength={MAXLEN[key]} align={align} />
    );
    const area = (key, minRows) => (
        <DocTextArea value={form[key]} onChange={setField(key)} readOnly={ro}
            maxLength={MAXLEN[key]} minRows={minRows} />
    );
    const rows = (tableKey) => Array.from({ length: rowCounts[tableKey] }, (_, i) => i + 1);

    if (loading) {
        return <p style={{ color: '#6b7280', padding: '20px' }}>적합성선언서를 불러오는 중...</p>;
    }

    return (
        <div className="td-page">
            <style>{TD_STYLES}</style>

            {/* ── 상단 액션 바 (인쇄/추출 시 제외) ── */}
            <div className="td-toolbar td-noprint">
                <div className="td-toolbar-info">
                    <strong>DOC (적합성 선언서)</strong>
                    <span className="td-badge">{isNew ? '신규 작성' : '저장됨'}</span>
                    {form.pkg1DocId && <span className="td-docid">{form.pkg1DocId}</span>}
                    {message && <span className="td-message">{message}</span>}
                </div>
                <div className="td-toolbar-buttons">
                    <button type="button" className="td-btn" onClick={handleExportPdf}>PDF 추출</button>
                    <button type="button" className="td-btn" onClick={handleExportDocx}>DOCX 추출</button>
                    {editing ? (
                        <>
                            <button type="button" className="td-btn" onClick={handleCancel} disabled={saving}>취소</button>
                            <button type="button" className="td-btn td-btn-primary" onClick={handleSave} disabled={saving}>
                                {saving ? '저장 중...' : '저장'}
                            </button>
                        </>
                    ) : (
                        <button type="button" className="td-btn td-btn-primary" onClick={handleEdit}>수정</button>
                    )}
                </div>
            </div>

            {/* ── 문서 본문 ── */}
            <div className="td-doc" id="td-doc" ref={docRef}>
                <h1 className="td-title doc-title">PPWR 적합성 선언서</h1>
                <p className="doc-subtitle">(Declaration of Conformity)</p>

                {/* 1. 제조자 정보 */}
                <h2 className="td-h2">1. 제조자 정보</h2>
                <table className="td-table td-table-kv td-table-plain">
                    <tbody>
                        <tr><th>1. 회사명</th><td>{input('bizNm')}</td></tr>
                        <tr><th>2. 담당자</th><td>{input('repNm')}</td></tr>
                        <tr><th>3. 직책</th><td>{input('roleNm')}</td></tr>
                        <tr><th>4. 이메일</th><td>{input('emlAddr')}</td></tr>
                        <tr><th>5. 전화</th><td>{input('mbTelNo')}</td></tr>
                    </tbody>
                </table>

                {/* 2. 제품 식별 정보 */}
                <h2 className="td-h2">2. 제품 식별 정보</h2>
                <table className="td-table td-table-kv td-table-plain">
                    <tbody>
                        <tr><th>1. 제품명</th><td>{input('prjfNm')}</td></tr>
                        <tr><th>2. 제품코드</th><td>{input('prjId')}</td></tr>
                        <tr><th>3. 기술문서 번호</th><td>{input('pkg1TechDocId')}</td></tr>
                        <tr><th>4. 개정번호</th><td>{input('revNo')}</td></tr>
                        <tr><th>5. 제조국</th><td>{input('cntryNm')}</td></tr>
                        <tr><th>6. 제품 유형</th><td>{input('dsgnTypeNm')}</td></tr>
                    </tbody>
                </table>

                {/* 3. 적합성 선언 */}
                <h2 className="td-h2">3. 적합성 선언</h2>
                {area('docPhrsCntn', 5)}

                {/* 4. 적합성 평가 결과 */}
                <h2 className="td-h2">4. 적합성 평가 결과</h2>

                <h3 className="td-h3">4.1 재사용성 요구사항 적합 (PPWR 제10조)</h3>
                {area('reuseReqCmplCntn', 8)}

                <h3 className="td-h3">4.2 재활용성 요구사항 적합 (PPWR 제6조)</h3>
                {area('rcycReqCmplCntn1', 2)}
                {area('rcycMainFeatCntn', 4)}
                {area('rcycReqCmplCntn2', 2)}

                <h3 className="td-h3">4.3 우려물질 및 중금속 제한 적합</h3>
                {area('soCHvyMetLmtCmplCntn1', 2)}
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '50%' }}>물질</th>
                            <th>시험결과 (mg/kg)</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('soc').map((i) => (
                            <tr key={`soc-${i}`}>
                                <td>{input(`sbst${i}`)}</td>
                                <td>{input(`testRslt${i}`)}</td>
                                <RowDeleteCell onDelete={() => removeRow('soc', i)} disabled={ro || rowCounts.soc <= 1} />
                            </tr>
                        ))}
                        <tr className="td-total-row">
                            <td>{input('sbstTot')}</td>
                            <td>{input('testRsltTot')}</td>
                            <td className="td-noprint" />
                        </tr>
                    </tbody>
                </table>
                <AddRowButton label="물질 행 추가" onAdd={() => addRow('soc')}
                    current={rowCounts.soc} max={ROW_TABLES.soc.max} />
                <p className="doc-fixed-phrase">{SOC_FOOT_PHRASE}</p>

                <h3 className="td-h3">4.4 재질 정보</h3>
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '45%' }}>구성품</th>
                            <th>재질</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('matInfo').map((i) => (
                            <tr key={`mat-${i}`}>
                                <td>{input(`compltem${i}`)}</td>
                                <td>{input(`mat${i}`)}</td>
                                <RowDeleteCell onDelete={() => removeRow('matInfo', i)} disabled={ro || rowCounts.matInfo <= 1} />
                            </tr>
                        ))}
                    </tbody>
                </table>
                <AddRowButton label="재질 정보 행 추가" onAdd={() => addRow('matInfo')}
                    current={rowCounts.matInfo} max={ROW_TABLES.matInfo.max} />

                {/* 총 중량 + 하단 고정문구 (표 아님) */}
                <table className="td-table td-table-kv td-table-plain">
                    <tbody>
                        <tr><th style={{ width: '14%' }}>총 중량</th><td>{input('matInfoTotWtVal')}</td></tr>
                    </tbody>
                </table>
                {area('matInfoCntn', 2)}

                {/* 5. 근거 문서 */}
                <h2 className="td-h2">5. 근거 문서</h2>
                {area('evdDocCntn', 2)}
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '16%' }}>번호</th>
                            <th>문서명</th>
                            <th className="td-noprint" style={{ width: '30%' }}>파일 / 관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('evd').map((slot) => {
                            const nm = form[`evdDocNm${slot}`];
                            const url = form[`evdDocUrl${slot}`];
                            return (
                                <tr key={`evd-${slot}`}>
                                    <td className="td-row-label">{annexLabel(slot)}</td>
                                    <td>{input(`evdDocNm${slot}`)}</td>
                                    <td className="td-noprint">
                                        <input type="file" style={{ display: 'none' }}
                                            ref={(el) => { fileInputRefs.current[slot] = el; }}
                                            onChange={(e) => handleUpload(slot, e.target.files?.[0])} />
                                        <div className="td-atch-actions">
                                            <button type="button" className="td-btn td-btn-sm"
                                                onClick={() => handlePickFile(slot)}>업로드</button>
                                            {url && (
                                                <>
                                                    <a className="td-link" href={url} target="_blank" rel="noreferrer"
                                                        download={nm || undefined}>다운로드</a>
                                                    <button type="button" className="td-btn td-btn-sm"
                                                        onClick={() => handleDeleteFile(slot)}>파일 삭제</button>
                                                </>
                                            )}
                                            <button type="button" className="td-btn td-btn-sm td-btn-danger"
                                                onClick={() => handleDeleteRow(slot)}
                                                disabled={ro || rowCounts.evd <= 1}>행 삭제</button>
                                        </div>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                <AddRowButton label="부속서 행 추가" onAdd={() => addRow('evd')}
                    current={rowCounts.evd} max={ROW_TABLES.evd.max} />

                {/* 6. 적용 규정 및 표준 */}
                <h2 className="td-h2">6. 적용 규정 및 표준</h2>
                {area('applRuleStdCntn', 6)}

                {/* 7. 최종 선언 */}
                <h2 className="td-h2">7. 최종 선언</h2>
                {area('lastDclCntn', 8)}

                <table className="td-table td-table-kv td-table-plain doc-sign">
                    <tbody>
                        <tr>
                            <th style={{ width: '18%' }}>발행일</th>
                            <td>
                                <input type="text" className="td-input td-input-locked"
                                    value={formatDate(form.lastWrtDt)} readOnly
                                    placeholder="저장 시 자동 기록됩니다" />
                            </td>
                        </tr>
                        <tr><th>제조자</th><td>{input('bizNm2')}</td></tr>
                        <tr>
                            <th>대표자/책임자</th>
                            {/* 직책 + 공백 1칸 + 이름 형태로 출력된다 */}
                            <td className="doc-rep-cell">
                                <span className="doc-rep-role">{input('roleNm2')}</span>
                                {' '}
                                <span className="doc-rep-name">{input('repNm2')}</span>
                            </td>
                        </tr>
                        <tr><th>서명</th><td className="doc-sign-blank" /></tr>
                        <tr><th>직인</th><td className="doc-sign-blank" /></tr>
                    </tbody>
                </table>
            </div>
        </div>
    );
}

// ─────────────────────────────────────────────────────────────
// 화면 스타일 — 기술문서 화면(PrimaryTd)과 동일한 규칙을 사용한다
// ─────────────────────────────────────────────────────────────
const TD_STYLES = `
.td-page { width: 100%; box-sizing: border-box; }

/* 상단 액션 바 — 스크롤해도 상단에 고정.
   좌우 음수 마진으로 스크롤 컨테이너(padding: 1.5rem)의 여백까지 덮어서
   스크롤되는 본문이 바 옆/뒤로 비쳐 보이지 않게 한다. */
.td-toolbar {
  position: sticky; top: 0; z-index: 30;
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  margin: -1.5rem -1.5rem 14px;      /* 컨테이너 패딩만큼 바깥으로 확장 */
  padding: 17px 1.5rem;              /* 위/아래 대칭 — 문구가 바 정중앙에 오도록 */
  line-height: 1.2;
  background: #ffffff;
  border-bottom: 1px solid #d1d5db;
  box-shadow: 0 4px 10px -4px rgba(15, 23, 42, .18);
  flex-wrap: wrap;
}
/* 바 위쪽으로 남는 틈을 같은 배경으로 메워 본문이 위로 새어 나오지 않게 한다 */
.td-toolbar::before {
  content: ''; position: absolute; left: 0; right: 0; bottom: 100%;
  height: 2rem; background: #ffffff;
}
.td-toolbar-info { display: flex; align-items: center; gap: 8px; font-size: 14px; color: #111827; flex-wrap: wrap; line-height: 1.2; }
.td-toolbar-info > * { display: inline-flex; align-items: center; }
.td-badge { background: #e0f2fe; color: #0369a1; padding: 3px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; line-height: 1.2; }
.td-docid { color: #6b7280; font-size: 12px; font-family: ui-monospace, SFMono-Regular, Menlo, monospace; }
.td-message { color: #15803d; font-size: 12px; }
.td-toolbar-buttons { display: flex; align-items: center; gap: 8px; }

.td-btn {
  padding: 7px 16px; border: 1px solid #cbd5e1; background: #fff; color: #334155;
  border-radius: 6px; cursor: pointer; font-size: 13px; font-weight: 500;
}
.td-btn:hover { background: #f8fafc; }
.td-btn:disabled { opacity: .45; cursor: not-allowed; }
.td-btn-primary { background: #198754; border-color: #198754; color: #fff; }
.td-btn-primary:hover { background: #157347; }
.td-btn-sm { padding: 3px 10px; font-size: 12px; }
.td-btn-danger { color: #b91c1c; border-color: #fecaca; }
.td-btn-danger:hover { background: #fef2f2; }

/* 문서 본문 — 위에서 아래로 흐르는 수직 구성 */
.td-doc { background: #fff; padding: 8px 4px 60px; color: #111827; line-height: 1.7; }
.td-title { font-size: 24px; font-weight: 700; margin: 0 0 20px; }
.td-h2 { font-size: 17px; font-weight: 700; margin: 28px 0 10px; padding-bottom: 6px; border-bottom: 2px solid #111827; }
.td-h3 { font-size: 14px; font-weight: 600; margin: 16px 0 8px; color: #374151; }

.td-table { width: 100%; border-collapse: collapse; margin: 8px 0 4px; table-layout: fixed; }
.td-table th, .td-table td { border: 1px solid #d1d5db; padding: 6px 8px; vertical-align: middle; font-size: 13px; word-break: break-word; }
.td-table th { background: #f3f4f6; font-weight: 600; text-align: center; color: #374151; }
.td-table-kv th { text-align: left; width: 22%; }

/* 1. 제품 식별 정보 / 12. 책임자 정보 — 테두리 없이 배경 투명한 평문 표 */
.td-table-plain,
.td-table-plain th,
.td-table-plain td { border: none; background: transparent; }
.td-table-plain th { padding-left: 0; }

.td-row-label { background: #fafafa; font-weight: 500; }
.td-total-row td { background: #f9fafb; font-weight: 600; }
.td-table-bom { min-width: 1000px; }
.td-scroll-x { overflow-x: auto; }
.td-rowaction { text-align: center; background: #fcfcfd; }

/* 행 추가 버튼 영역 */
.td-addrow { display: flex; align-items: center; gap: 10px; margin: 6px 0 4px; }
.td-addrow-count { font-size: 12px; color: #9ca3af; }
.td-addrow-limit { font-size: 12px; color: #9ca3af; }

/* 입력 필드 — 문서처럼 보이도록 테두리를 최소화 */
.td-input {
  width: 100%; box-sizing: border-box; border: 1px solid transparent; background: transparent;
  padding: 4px 6px; font: inherit; font-size: 13px; color: #111827; border-radius: 4px;
}
.td-input:not([readonly]) { border-color: #e5e7eb; background: #fff; }
.td-input:not([readonly]):hover { border-color: #cbd5e1; }
.td-input:focus { outline: none; border-color: #198754; box-shadow: 0 0 0 2px rgba(25,135,84,.12); }
.td-input[readonly] { cursor: default; }
.td-input-locked { color: #6b7280; background: #f9fafb !important; border-color: #e5e7eb !important; }
.td-textarea { resize: none; overflow: hidden; line-height: 1.7; display: block; margin: 6px 0; }

.td-atch-actions { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; }
.td-link { font-size: 12px; color: #2563eb; text-decoration: underline; }
.td-closing { margin-top: 28px; padding-top: 14px; border-top: 1px solid #e5e7eb; }

/* 화면 전용 / 추출(PDF·DOCX) 전용 요소 전환 */
.td-export-only { display: none; }
.td-result-phrase { margin: 8px 0 4px; font-size: 13px; }

/* ── 인쇄 / PDF 추출 ──────────────────────────────────────
   문서 영역만 남기고 대시보드의 3분할 레이아웃 제약을 해제한다. */
@media print {
  .td-noprint { display: none !important; }

  html, body { height: auto !important; overflow: visible !important; background: #fff !important; }

  /* 대시보드 셸의 100vh / overflow:hidden 인라인 스타일 해제 */
  .dashboard-shell { display: block !important; width: auto !important; height: auto !important; overflow: visible !important; }
  .dashboard-panel, .main-panel { height: auto !important; overflow: visible !important; }
  .sidebar-nav, .assistant-panel { display: none !important; }

  body * { visibility: hidden !important; }
  #td-doc, #td-doc * { visibility: visible !important; }
  #td-doc {
    position: absolute !important; left: 0 !important; top: 0 !important;
    width: 100% !important; padding: 0 !important; margin: 0 !important;
  }

  .td-toolbar { display: none !important; }
  .td-input, .td-textarea {
    border: none !important; background: transparent !important; box-shadow: none !important;
    padding: 0 !important; color: #000 !important;
  }
  .td-table-plain, .td-table-plain th, .td-table-plain td { border: none !important; background: transparent !important; }
  .td-screen-only { display: none !important; }
  .td-export-only { display: block !important; }
  .td-scroll-x { overflow: visible !important; }
  .td-table-bom { min-width: 0 !important; }
  .td-table { page-break-inside: auto; }
  .td-table tr { page-break-inside: avoid; page-break-after: auto; }
  .td-h2 { page-break-after: avoid; }
}
/* ── DOC (적합성 선언서) 전용 ── */
.doc-title { text-align: center; margin-bottom: 4px; }
.doc-subtitle { text-align: center; font-weight: 600; color: #4b5563; margin: 0 0 24px; font-size: 14px; }
/* 저장 컬럼이 없어 화면 고정으로만 출력하는 문구 */
.doc-fixed-phrase { margin: 10px 0 4px; font-size: 13px; color: #111827; }
.doc-rep-cell .td-input { width: auto; min-width: 140px; display: inline-block; }
.doc-sign .doc-sign-blank { height: 34px; }
@media print {
  .doc-rep-cell .td-input { width: auto !important; min-width: 0 !important; }
}
`;
