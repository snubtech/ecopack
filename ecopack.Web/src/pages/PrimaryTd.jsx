import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetPrimaryTd,
    SavePrimaryTd,
    UploadAtchDoc,
    DeleteAtchDoc,
} from '../api/primaryTd';

/**
 * 기술문서 — 1차포장 기술문서 화면 / primary_td 테이블
 *
 * 화면 규칙
 *  - [화면고정]           : 라벨/제목으로 고정 출력, 수정 불가
 *  - [insert][컬럼명 : x] : primary_td 컬럼과 1:1로 묶인 입력 필드 (CRUD 대상)
 *  - [고정 문구, 수정 가능]: 문단 항목에만 기본 문구가 채워지고 수정 가능
 *  - [default]            : 첨부문서 Annex 라벨. F 이후는 알파벳 순으로 자동 생성
 *
 * 행 추가/삭제가 가능한 표(ROW_TABLES)는 DB의 번호 시리즈 컬럼 슬롯 개수까지 늘릴 수 있다.
 * 행 삭제 시 아래 행의 값을 위로 당겨서 슬롯이 비지 않도록 유지한다.
 */

// ─────────────────────────────────────────────────────────────
// 행 추가/삭제가 가능한 표 정의
//   max     : DB 컬럼 슬롯 최대치 (더 늘리려면 DB에 컬럼 추가 필요)
//   initial : 문서 최초 작성 시 보여줄 기본 행 수
//   fields  : 해당 표를 구성하는 컬럼 접두어들
// ─────────────────────────────────────────────────────────────
const ROW_TABLES = {
    matComp: {
        max: 4, initial: 2,
        fields: ['matComplItem', 'matNm', 'wtVal', 'wtRt'],
    },
    bom: {
        max: 8, initial: 6,
        fields: ['bomCmpnNm', 'bomMatNm', 'bomMatStdCd', 'bomQtyVal', 'bomUnitWtVal', 'bomTotWtVal', 'bomWtRtVal'],
    },
    reuse: {
        max: 6, initial: 4,
        fields: ['prdDsgnTestItemCntn', 'prdDsgnCritCntn', 'prdDsgnRsltVal', 'prdDsgnTestMthd'],
    },
    rcyc: {
        max: 6, initial: 4,
        fields: ['rcycDsgnPrncItemCntn', 'rcycDsgnPrncEvlRslt', 'rcycDsgnPrncEvlTestMthdCntn'],
    },
    soc: {
        max: 5, initial: 4,
        fields: ['socHvyMetMngTestRsltSbstCntn', 'socHvyMetMngTestRsltCntn', 'socHvyMetMngRegCritCntn', 'socHvyMetMngTestMthdCntn'],
    },
    qlt: {
        max: 7, initial: 5,
        fields: ['qltMngInspItemCntn', 'qltMngInspMthdCntn', 'qltMngFreqCntn'],
    },
    atch: {
        max: 8, initial: 6,
        fields: ['atchDocNm', 'atchDocUrl'],
    },
};

const TABLE_KEYS = Object.keys(ROW_TABLES);

/** 행 시리즈에 속하지 않는 단일 컬럼들 */
const SINGLE_KEYS = [
    'pkg1TechDocId', 'lastWrtDtm',
    'prjId', 'prjfNm', 'bizNm', 'cntryNm', 'docNo', 'revNo',
    'prdExplPhrsCntn', 'prdIdfyCntn', 'mainMatVal', 'dsgnFeatCntn',
    'prdExtDimSpecVal', 'prdIntDimSpecVal', 'prdWtSpecVal', 'prdMatSpecVal',
    'prdClrSpecVal', 'prdUseTempSpecVal', 'prdLoadWtSpecVal', 'prdDsgnLifeSpecVal', 'mfrDrwUrl',
    'matCompTot', 'matNmTot', 'wtValTot', 'wtRtTot',
    'prdDsgnPhrsCntn', 'reusePerfRsltVal',
    'rcycDsgnPrncPhrsCntn', 'rcycPathPhrsCntn',
    'socHvyMetMngFixPhrsCntn',
    'socHvyMetMngRsltTot', 'socHvyMetMngTestRsltTot', 'socHvyMetMngRegCritTot',
    // 합계행 시험방법: 데이터행이 Cntn1~5를 쓰므로 합계는 Cntn6을 사용한다
    'socHvyMetMngTestMthdCntn6', 'socHvyMetMngTestRsltPhrs',
    'mfrPrcsUrl', 'mfrPrcsCntn',
    'cmplDclCntn',
    'bizNm2', 'repNm', 'roleNm', 'emlAddr', 'mbTelNo', 'techDocLastPhrsCntn',
];

/** 화면에서 다루는 primary_td 컬럼 전체 */
const FIELD_KEYS = [
    ...SINGLE_KEYS,
    ...TABLE_KEYS.flatMap((key) => {
        const { fields, max } = ROW_TABLES[key];
        return fields.flatMap((f) => Array.from({ length: max }, (_, i) => `${f}${i + 1}`));
    }),
];

// ─────────────────────────────────────────────────────────────
// [고정 문구, 수정 가능] 기본값
// 값이 비어 있을 때만 채워지며, 언제든 수정할 수 있다.
//
// 표의 행 항목은 '기본 행'(ROW_TABLES.initial) 범위까지만 기본 문구를 갖는다.
// → 행 추가로 새로 생기는 행(예: 제품설계 5~6행, 품질관리 6~7행)은
//   DEFAULTS에 키가 없으므로 자동으로 빈 값으로 시작한다.
// ─────────────────────────────────────────────────────────────
const DEFAULTS = {
    prdExplPhrsCntn:
        '본 제품은 Expanded Polypropylene (EPP) 소재를 사용하여 제작된 접이식 운송용 포장재이다. ' +
        '제품은 반복사용을 목적으로 설계되었고, 판매포장 및 그룹포장을 운송하기 위한 목적으로 사용된다. ' +
        '사용하지 않을 때에는 접을 수 있고, 물류 회수 시 부피를 감소시켜 운송효율을 향상이 가능하다.',
    prdDsgnPhrsCntn:
        '본 제품은 반복적인 물류 운송 환경에서 사용될 수 있도록 설계되었다. ' +
        '접이 및 전개 기능은 5,000회 이상의 반복 시험을 통해 검증되었다.',
    rcycDsgnPrncPhrsCntn:
        '본 제품은 단일 소재(EPP) 중심 구조로 설계되어 기계적 재활용이 가능하다. ' +
        '부품 분리가 용이하며 재활용 공정에서 재질 분류가 가능하다.',
    rcycPathPhrsCntn:
        '사용 후 제품 수거 선별 시스템을 통해 수거되어 분쇄 세척 재용융 재생 EPP 원료 생산 공정을 통해 재활용될 수 있다.',
    socHvyMetMngFixPhrsCntn:
        'PPWR 요구사항에 따라 납(Pb), 카드뮴(Cd), 수은(Hg), 6가 크롬(Cr6+)의 총 함량을 평가하였다.',
    cmplDclCntn:
        '지피에스코리아는 본 제품이 EU Packaging and Packaging Waste Regulation (PPWR)의 적용 요구사항을 충족함을 선언한다. ' +
        '특히 다음 사항에 적합함을 확인한다. • 재사용성 요구사항 • 재활용성 요구사항 • 중금속 제한 요구사항 • 물질 정보 제공 요구사항',
    techDocLastPhrsCntn: '본 기술문서는 PPWR 규정 준수를 입증하기 위하여 작성되었다.',

    // ── 5. 제품 설계 시험 — 기본 4행 ──────────────────────────
    prdDsgnTestItemCntn1: '접이 반복 시험',
    prdDsgnTestItemCntn2: '낙하 시험',
    prdDsgnTestItemCntn3: '적재 시험',
    prdDsgnTestItemCntn4: '내구성 시험',

    // ── 6. 재활용 설계 원칙 — 기본 4행 ────────────────────────
    rcycDsgnPrncItemCntn1: '재활용 공정 호환성',
    rcycDsgnPrncItemCntn2: '수거 선별 시스템',
    rcycDsgnPrncItemCntn3: '재생원료의 품질',
    rcycDsgnPrncItemCntn4: '재활용 포장설계',

    // ── 7. 중금속 시험 결과 — 기본 4행 + 합계행 ───────────────
    socHvyMetMngTestRsltSbstCntn1: 'Lead (Pb)',
    socHvyMetMngTestRsltSbstCntn2: 'Cadmium (Cd)',
    socHvyMetMngTestRsltSbstCntn3: 'Mercury (Hg)',
    socHvyMetMngTestRsltSbstCntn4: 'Hexavalent Chromium',
    socHvyMetMngTestMthdCntn1: 'IEC 62321-5',
    socHvyMetMngTestMthdCntn2: 'IEC 62321-5',
    socHvyMetMngTestMthdCntn3: 'IEC 62321-4',
    socHvyMetMngTestMthdCntn4: 'IEC 62321-7-2',
    socHvyMetMngRsltTot: '합계',

    // ── 9. 품질관리 — 기본 5행 ────────────────────────────────
    qltMngInspItemCntn1: '치수 검사',
    qltMngInspItemCntn2: '중량 검사',
    qltMngInspItemCntn3: '외관 검사',
    qltMngInspItemCntn4: '접이 기능 검사',
    qltMngInspItemCntn5: '적재 강도 시험',
    qltMngFreqCntn1: 'LOT 별',
    qltMngFreqCntn2: 'LOT 별',
    qltMngFreqCntn3: '100%',
    qltMngFreqCntn4: 'LOT 별',
    qltMngFreqCntn5: 'LOT 별',
};

/** [default] — 슬롯 번호를 Annex 알파벳 라벨로 변환 (1→Annex A … 8→Annex H) */
const annexLabel = (slot) => `Annex ${String.fromCharCode(64 + slot)}`;

const emptyForm = () => {
    const base = {};
    FIELD_KEYS.forEach((k) => { base[k] = ''; });
    return base;
};

/** 서버에서 받은 값 + 문단 기본 문구를 합쳐 폼 상태를 만든다 (서버 값이 우선) */
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

/** 저장된 데이터에서 표별로 값이 채워진 마지막 행을 찾아 초기 행 수를 정한다 */
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

const formatDateTime = (value) => {
    if (!value) return '';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    const pad = (n) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
};

// ─────────────────────────────────────────────────────────────
// 입력 컴포넌트
// ─────────────────────────────────────────────────────────────

/** [insert] 한 줄 입력 필드 */
function TdInput({ value, onChange, readOnly, align }) {
    return (
        <input
            type="text"
            className="td-input"
            value={value ?? ''}
            onChange={(e) => onChange(e.target.value)}
            readOnly={readOnly}
            style={align ? { textAlign: align } : undefined}
        />
    );
}

/** [insert] 여러 줄 입력 필드 — 내용에 맞춰 높이가 자동으로 늘어난다(인쇄 시 잘림 방지) */
function TdTextArea({ value, onChange, readOnly, minRows = 2 }) {
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
        />
    );
}

/** 행 삭제 버튼 (각 행 우측) */
function RowDeleteCell({ onDelete, disabled }) {
    return (
        <td className="td-noprint td-rowaction">
            <button
                type="button"
                className="td-btn td-btn-sm td-btn-danger"
                onClick={onDelete}
                disabled={disabled}
                title="행 삭제"
            >
                행 삭제
            </button>
        </td>
    );
}

/** 표 하단 행 추가 버튼 */
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
export default function PrimaryTd() {
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

    const snapshotRef = useRef(null);   // 취소 시 되돌릴 값 { form, rowCounts }
    const docRef = useRef(null);        // 인쇄/추출 대상 영역
    const fileInputRefs = useRef({});   // 첨부 슬롯별 file input

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
                const res = await GetPrimaryTd(prjId);
                if (!alive) return;

                const merged = mergeWithDefaults(res?.data ?? null);
                merged.prjId = prjId;
                if (!merged.prjfNm) merged.prjfNm = prjNm;
                if (!merged.revNo) merged.revNo = 'Rev.01';

                const counts = detectRowCounts(merged);

                setForm(merged);
                setRowCounts(counts);
                setIsNew(Boolean(res?.isNew));
                setEditing(Boolean(res?.isNew)); // 신규면 바로 작성 가능하도록
                snapshotRef.current = { form: merged, rowCounts: counts };
            } catch (err) {
                console.error('기술문서 조회 실패:', err);
                if (alive) setMessage('기술문서를 불러오는 중 오류가 발생했습니다.');
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

        setRowCounts((prev) => ({
            ...prev,
            [tableKey]: Math.max(1, prev[tableKey] - 1),
        }));
    }, []);

    // ── 저장 ─────────────────────────────────────────────────
    const handleSave = async () => {
        if (!prjId) {
            alert('선택된 프로젝트가 없습니다.');
            return;
        }

        setSaving(true);
        try {
            const res = await SavePrimaryTd({ ...form, prjId });
            const saved = mergeWithDefaults(res?.data ?? form);
            saved.prjId = prjId;
            const counts = detectRowCounts(saved);

            setForm(saved);
            setRowCounts(counts);
            snapshotRef.current = { form: saved, rowCounts: counts };
            setIsNew(false);
            setEditing(false);
            setMessage(`저장 완료 (작성일시 ${formatDateTime(res?.lastWrtDtm)})`);
            alert(res?.message || '저장되었습니다.');
        } catch (err) {
            console.error('기술문서 저장 실패:', err);
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

    // ── 첨부문서 ─────────────────────────────────────────────
    const handlePickFile = (slot) => {
        if (isNew) {
            alert('기술문서를 먼저 저장한 뒤 첨부문서를 올려주세요.');
            return;
        }
        fileInputRefs.current[slot]?.click();
    };

    const handleUpload = async (slot, file) => {
        if (!file) return;
        try {
            const res = await UploadAtchDoc(prjId, slot, file);
            if (res?.success) {
                setForm((prev) => ({
                    ...prev,
                    [`atchDocNm${slot}`]: res.fileNm || '',
                    [`atchDocUrl${slot}`]: res.fileUrl || '',
                }));
                setMessage(`${annexLabel(slot)} 업로드 완료: ${res.fileNm}`);
            } else {
                alert(res?.message || '업로드에 실패했습니다.');
            }
        } catch (err) {
            console.error('첨부문서 업로드 실패:', err);
            alert('업로드 중 오류가 발생했습니다.');
        } finally {
            if (fileInputRefs.current[slot]) fileInputRefs.current[slot].value = '';
        }
    };

    /** 첨부파일만 삭제 (행은 그대로 유지) */
    const handleDeleteAtchFile = async (slot) => {
        if (!window.confirm(`${annexLabel(slot)}에 올린 파일을 삭제할까요?`)) return;
        try {
            const res = await DeleteAtchDoc(prjId, slot);
            if (res?.success) {
                setForm((prev) => ({
                    ...prev,
                    [`atchDocNm${slot}`]: '',
                    [`atchDocUrl${slot}`]: '',
                }));
                setMessage(`${annexLabel(slot)} 첨부파일을 삭제했습니다.`);
            }
        } catch (err) {
            console.error('첨부파일 삭제 실패:', err);
            alert('삭제 중 오류가 발생했습니다.');
        }
    };

    /** 첨부문서 행 삭제 — 올라간 파일이 있으면 서버 파일까지 지운 뒤 아래 행을 당긴다 */
    const handleDeleteAtchRow = async (slot) => {
        const hasFile = Boolean(form[`atchDocUrl${slot}`]);
        const msg = hasFile
            ? `${annexLabel(slot)} 행을 삭제합니다. 업로드된 파일도 함께 삭제되고 아래 행이 위로 당겨집니다. 계속할까요?`
            : `${annexLabel(slot)} 행을 삭제할까요? 아래 행이 위로 당겨집니다.`;
        if (!window.confirm(msg)) return;

        try {
            if (hasFile) {
                await DeleteAtchDoc(prjId, slot);
            }
            removeRow('atch', slot);
            setMessage('첨부문서 행을 삭제했습니다. [저장]을 눌러야 DB에 반영됩니다.');
        } catch (err) {
            console.error('첨부문서 행 삭제 실패:', err);
            alert('행 삭제 중 오류가 발생했습니다.');
        }
    };

    // ── PDF 추출 (브라우저 인쇄 → PDF로 저장) ────────────────
    const handleExportPdf = () => {
        window.print();
    };

    // ── DOCX(Word) 추출 ──────────────────────────────────────
    const handleExportDocx = () => {
        const source = docRef.current;
        if (!source) return;

        // 입력 요소를 값 텍스트로 치환한 사본을 만든다 (Word에서 폼 컨트롤이 아닌 문서로 보이도록)
        const clone = source.cloneNode(true);
        clone.querySelectorAll('.td-noprint, .td-screen-only').forEach((el) => el.remove());
        clone.querySelectorAll('input, textarea').forEach((el) => {
            const span = document.createElement('span');
            span.textContent = el.value || '';
            el.replaceWith(span);
        });

        const html = `<!DOCTYPE html>
<html xmlns:o="urn:schemas-microsoft-com:office:office"
      xmlns:w="urn:schemas-microsoft-com:office:word"
      xmlns="http://www.w3.org/TR/REC-html40">
<head><meta charset="utf-8" />
<title>기술문서</title>
<style>
  body { font-family: 'Malgun Gothic', 'Pretendard', sans-serif; font-size: 10.5pt; line-height: 1.6; }
  h1 { font-size: 18pt; } h2 { font-size: 13pt; margin-top: 18pt; } h3 { font-size: 11pt; }
  table { border-collapse: collapse; width: 100%; margin: 8pt 0; }
  th, td { border: 1px solid #999; padding: 4pt 6pt; vertical-align: top; font-size: 9.5pt; }
  th { background: #f2f2f2; font-weight: bold; }
  .td-table-plain, .td-table-plain th, .td-table-plain td { border: none; background: transparent; }
  .td-result-phrase { margin: 8pt 0; }
  .td-row-label { font-weight: bold; }
</style>
</head><body>${clone.innerHTML}</body></html>`;

        const blob = new Blob(['﻿', html], { type: 'application/msword' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `기술문서_${form.prjfNm || prjId || 'primary_td'}.doc`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    };

    // ── 렌더 헬퍼 ────────────────────────────────────────────
    const ro = !editing;
    const input = (key, align) => (
        <TdInput value={form[key]} onChange={setField(key)} readOnly={ro} align={align} />
    );
    const area = (key, minRows) => (
        <TdTextArea value={form[key]} onChange={setField(key)} readOnly={ro} minRows={minRows} />
    );
    /** 행 번호 배열 */
    const rows = (tableKey) => Array.from({ length: rowCounts[tableKey] }, (_, i) => i + 1);

    if (loading) {
        return <p style={{ color: '#6b7280', padding: '20px' }}>기술문서를 불러오는 중...</p>;
    }

    return (
        <div className="td-page">
            <style>{TD_STYLES}</style>

            {/* ── 상단 액션 바 (인쇄/추출 시 제외) ── */}
            <div className="td-toolbar td-noprint">
                <div className="td-toolbar-info">
                    <strong>기술문서</strong>
                    <span className="td-badge">{isNew ? '신규 작성' : '저장됨'}</span>
                    {form.pkg1TechDocId && <span className="td-docid">{form.pkg1TechDocId}</span>}
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

            {/* ── 문서 본문 (수직 배치, 이 영역만 PDF/DOCX로 추출) ── */}
            <div className="td-doc" id="td-doc" ref={docRef}>
                <h1 className="td-title td-title-center">기술문서</h1>

                {/* 1. 제품 식별 정보 */}
                <h2 className="td-h2">1. 제품 식별 정보</h2>
                <table className="td-table td-table-kv td-table-plain">
                    <tbody>
                        <tr><th>1. 제품명</th><td>{input('prjfNm')}</td></tr>
                        <tr><th>2. 제품코드</th><td>{input('prjId')}</td></tr>
                        <tr><th>3. 제조사</th><td>{input('bizNm')}</td></tr>
                        <tr><th>4. 제조국</th><td>{input('cntryNm')}</td></tr>
                        <tr><th>5. 문서번호</th><td>{input('docNo')}</td></tr>
                        <tr>
                            <th>6. 작성일</th>
                            <td>
                                <input type="text" className="td-input td-input-locked"
                                    value={formatDateTime(form.lastWrtDtm)} readOnly
                                    placeholder="저장 시 자동 기록됩니다" />
                            </td>
                        </tr>
                        <tr><th>7. 개정번호</th><td>{input('revNo')}</td></tr>
                    </tbody>
                </table>

                {/* 2. 제품 설명 */}
                <h2 className="td-h2">2. 제품 설명</h2>
                {area('prdExplPhrsCntn', 4)}
                <table className="td-table td-table-kv">
                    <tbody>
                        <tr><th>1. 제품식별</th><td>{input('prdIdfyCntn')}</td></tr>
                        <tr><th>2. 주요재질</th><td>{input('mainMatVal')}</td></tr>
                        <tr><th>3. 디자인 특징</th><td>{area('dsgnFeatCntn', 2)}</td></tr>
                    </tbody>
                </table>

                {/* 3. 제품 사양 및 제조 도면 — 항목별 고정 컬럼이라 행 수 고정 */}
                <h2 className="td-h2">3. 제품 사양 및 제조 도면</h2>
                <h3 className="td-h3">1) 제품사양</h3>
                <table className="td-table">
                    <thead>
                        <tr><th style={{ width: '28%' }}>항목</th><th>규격</th></tr>
                    </thead>
                    <tbody>
                        <tr><td className="td-row-label">외부 치수</td><td>{input('prdExtDimSpecVal')}</td></tr>
                        <tr><td className="td-row-label">내부 치수</td><td>{input('prdIntDimSpecVal')}</td></tr>
                        <tr><td className="td-row-label">중량</td><td>{input('prdWtSpecVal')}</td></tr>
                        <tr><td className="td-row-label">소재</td><td>{input('prdMatSpecVal')}</td></tr>
                        <tr><td className="td-row-label">색상</td><td>{input('prdClrSpecVal')}</td></tr>
                        <tr><td className="td-row-label">사용온도</td><td>{input('prdUseTempSpecVal')}</td></tr>
                        <tr><td className="td-row-label">적재하중</td><td>{input('prdLoadWtSpecVal')}</td></tr>
                        <tr><td className="td-row-label">설계 수명</td><td>{input('prdDsgnLifeSpecVal')}</td></tr>
                    </tbody>
                </table>

                <h3 className="td-h3">2) 제조 도면</h3>
                {input('mfrDrwUrl')}

                {/* 4. 재질구성 */}
                <h2 className="td-h2">4. 재질구성(Material Composition)</h2>
                <h3 className="td-h3">1) 재질구성</h3>
                <table className="td-table">
                    <thead>
                        <tr>
                            <th>구성품</th><th>재질</th>
                            <th style={{ width: '16%' }}>중량(g)</th><th style={{ width: '16%' }}>중량비율(%)</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('matComp').map((i) => (
                            <tr key={`mat-${i}`}>
                                <td>{input(`matComplItem${i}`)}</td>
                                <td>{input(`matNm${i}`)}</td>
                                <td>{input(`wtVal${i}`, 'right')}</td>
                                <td>{input(`wtRt${i}`, 'right')}</td>
                                <RowDeleteCell onDelete={() => removeRow('matComp', i)} disabled={ro || rowCounts.matComp <= 1} />
                            </tr>
                        ))}
                        <tr className="td-total-row">
                            <td>{input('matCompTot')}</td>
                            <td>{input('matNmTot')}</td>
                            <td>{input('wtValTot', 'right')}</td>
                            <td>{input('wtRtTot', 'right')}</td>
                            <td className="td-noprint" />
                        </tr>
                    </tbody>
                </table>
                <AddRowButton label="재질구성 행 추가" onAdd={() => addRow('matComp')}
                    current={rowCounts.matComp} max={ROW_TABLES.matComp.max} />

                <h3 className="td-h3">2) BOM</h3>
                <div className="td-scroll-x">
                    <table className="td-table td-table-bom">
                        <thead>
                            <tr>
                                <th>Component Name</th><th>Material</th><th>Material Standard</th>
                                <th>Qty.</th><th>Unit Weight (g)</th><th>Total Weight (g)</th><th>Weight Ratio (%)</th>
                                <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                            </tr>
                        </thead>
                        <tbody>
                            {rows('bom').map((i) => (
                                <tr key={`bom-${i}`}>
                                    <td>{input(`bomCmpnNm${i}`)}</td>
                                    <td>{input(`bomMatNm${i}`)}</td>
                                    <td>{input(`bomMatStdCd${i}`)}</td>
                                    <td>{input(`bomQtyVal${i}`, 'right')}</td>
                                    <td>{input(`bomUnitWtVal${i}`, 'right')}</td>
                                    <td>{input(`bomTotWtVal${i}`, 'right')}</td>
                                    <td>{input(`bomWtRtVal${i}`, 'right')}</td>
                                    <RowDeleteCell onDelete={() => removeRow('bom', i)} disabled={ro || rowCounts.bom <= 1} />
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
                <AddRowButton label="BOM 행 추가" onAdd={() => addRow('bom')}
                    current={rowCounts.bom} max={ROW_TABLES.bom.max} />

                {/* 5. 재사용성 평가 */}
                <h2 className="td-h2">5. 재사용성 평가 (PPWR Article 10)</h2>
                <h3 className="td-h3">1) 제품 설계</h3>
                {area('prdDsgnPhrsCntn', 3)}
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '24%' }}>시험항목</th><th>기준</th><th>결과</th><th>시험방법</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('reuse').map((i) => (
                            <tr key={`reuse-${i}`}>
                                <td>{input(`prdDsgnTestItemCntn${i}`)}</td>
                                <td>{input(`prdDsgnCritCntn${i}`)}</td>
                                <td>{input(`prdDsgnRsltVal${i}`)}</td>
                                <td>{input(`prdDsgnTestMthd${i}`)}</td>
                                <RowDeleteCell onDelete={() => removeRow('reuse', i)} disabled={ro || rowCounts.reuse <= 1} />
                            </tr>
                        ))}
                    </tbody>
                </table>
                <AddRowButton label="제품 설계 행 추가" onAdd={() => addRow('reuse')}
                    current={rowCounts.reuse} max={ROW_TABLES.reuse.max} />

                <h3 className="td-h3">2) 재사용 성능</h3>
                {input('reusePerfRsltVal')}

                {/* 6. 재활용성 평가 */}
                <h2 className="td-h2">6. 재활용성 평가 (PPWR Article 6)</h2>
                <h3 className="td-h3">1) 재활용 설계 원칙</h3>
                {area('rcycDsgnPrncPhrsCntn', 3)}
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '28%' }}>항목</th><th>평가</th><th>시험방법</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('rcyc').map((i) => (
                            <tr key={`rcyc-${i}`}>
                                <td>{input(`rcycDsgnPrncItemCntn${i}`)}</td>
                                <td>{input(`rcycDsgnPrncEvlRslt${i}`)}</td>
                                <td>{input(`rcycDsgnPrncEvlTestMthdCntn${i}`)}</td>
                                <RowDeleteCell onDelete={() => removeRow('rcyc', i)} disabled={ro || rowCounts.rcyc <= 1} />
                            </tr>
                        ))}
                    </tbody>
                </table>
                <AddRowButton label="재활용 설계 원칙 행 추가" onAdd={() => addRow('rcyc')}
                    current={rowCounts.rcyc} max={ROW_TABLES.rcyc.max} />

                <h3 className="td-h3">2) 재활용 경로</h3>
                {area('rcycPathPhrsCntn', 3)}

                {/* 7. 우려물질(SoC) 및 중금속 관리 */}
                <h2 className="td-h2">7. 우려물질(SoC) 및 중금속 관리</h2>
                {area('socHvyMetMngFixPhrsCntn', 2)}
                <h3 className="td-h3">1) 시험 결과</h3>
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '25%' }}>물질</th><th>결과(mg/kg)</th><th>규제 기준</th><th>시험방법</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('soc').map((i) => (
                            <tr key={`soc-${i}`}>
                                <td>{input(`socHvyMetMngTestRsltSbstCntn${i}`)}</td>
                                <td>{input(`socHvyMetMngTestRsltCntn${i}`)}</td>
                                <td>{input(`socHvyMetMngRegCritCntn${i}`)}</td>
                                <td>{input(`socHvyMetMngTestMthdCntn${i}`)}</td>
                                <RowDeleteCell onDelete={() => removeRow('soc', i)} disabled={ro || rowCounts.soc <= 1} />
                            </tr>
                        ))}
                        <tr className="td-total-row">
                            <td>{input('socHvyMetMngRsltTot')}</td>
                            <td>{input('socHvyMetMngTestRsltTot')}</td>
                            <td>{input('socHvyMetMngRegCritTot')}</td>
                            <td>{input('socHvyMetMngTestMthdCntn6')}</td>
                            <td className="td-noprint" />
                        </tr>
                    </tbody>
                </table>
                <AddRowButton label="시험 결과 행 추가" onAdd={() => addRow('soc')}
                    current={rowCounts.soc} max={ROW_TABLES.soc.max} />
                {/* 화면에서는 표 형태로 입력받고, PDF/DOCX 추출 시에는 아래 문구 형태로 나간다 */}
                <table className="td-table td-table-kv td-table-plain td-screen-only">
                    <tbody>
                        <tr><th style={{ width: '18%' }}>결과</th><td>{input('socHvyMetMngTestRsltPhrs')}</td></tr>
                    </tbody>
                </table>
                <p className="td-export-only td-result-phrase">
                    결과 : {form.socHvyMetMngTestRsltPhrs}
                </p>

                {/* 8. 제조 공정 */}
                <h2 className="td-h2">8. 제조 공정</h2>
                {input('mfrPrcsUrl')}
                {area('mfrPrcsCntn', 3)}

                {/* 9. 품질관리 */}
                <h2 className="td-h2">9. 품질관리</h2>
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '30%' }}>검사 항목</th><th>검사 방법</th><th style={{ width: '20%' }}>빈도</th>
                            <th className="td-noprint" style={{ width: '92px' }}>관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('qlt').map((i) => (
                            <tr key={`qlt-${i}`}>
                                <td>{input(`qltMngInspItemCntn${i}`)}</td>
                                <td>{input(`qltMngInspMthdCntn${i}`)}</td>
                                <td>{input(`qltMngFreqCntn${i}`)}</td>
                                <RowDeleteCell onDelete={() => removeRow('qlt', i)} disabled={ro || rowCounts.qlt <= 1} />
                            </tr>
                        ))}
                    </tbody>
                </table>
                <AddRowButton label="품질관리 행 추가" onAdd={() => addRow('qlt')}
                    current={rowCounts.qlt} max={ROW_TABLES.qlt.max} />

                {/* 10. 준수 선언 */}
                <h2 className="td-h2">10. 준수 선언</h2>
                {area('cmplDclCntn', 4)}

                {/* 11. 첨부 문서 */}
                <h2 className="td-h2">11. 첨부 문서</h2>
                <table className="td-table">
                    <thead>
                        <tr>
                            <th style={{ width: '14%' }}>번호</th>
                            <th>문서명</th>
                            <th className="td-noprint" style={{ width: '30%' }}>파일 / 관리</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows('atch').map((slot) => {
                            const nm = form[`atchDocNm${slot}`];
                            const url = form[`atchDocUrl${slot}`];
                            return (
                                <tr key={`atch-${slot}`}>
                                    <td className="td-row-label">{annexLabel(slot)}</td>
                                    <td>{input(`atchDocNm${slot}`)}</td>
                                    <td className="td-noprint">
                                        <input
                                            type="file"
                                            style={{ display: 'none' }}
                                            ref={(el) => { fileInputRefs.current[slot] = el; }}
                                            onChange={(e) => handleUpload(slot, e.target.files?.[0])}
                                        />
                                        <div className="td-atch-actions">
                                            <button type="button" className="td-btn td-btn-sm" onClick={() => handlePickFile(slot)}>
                                                업로드
                                            </button>
                                            {url && (
                                                <>
                                                    <a className="td-link" href={url} target="_blank" rel="noreferrer" download={nm || undefined}>
                                                        다운로드
                                                    </a>
                                                    <button type="button" className="td-btn td-btn-sm" onClick={() => handleDeleteAtchFile(slot)}>
                                                        파일 삭제
                                                    </button>
                                                </>
                                            )}
                                            <button
                                                type="button"
                                                className="td-btn td-btn-sm td-btn-danger"
                                                onClick={() => handleDeleteAtchRow(slot)}
                                                disabled={ro || rowCounts.atch <= 1}
                                            >
                                                행 삭제
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
                <AddRowButton label="Annex 행 추가" onAdd={() => addRow('atch')}
                    current={rowCounts.atch} max={ROW_TABLES.atch.max} />

                {/* 12. 책임자 정보 */}
                <h2 className="td-h2">12. 책임자 정보</h2>
                <table className="td-table td-table-kv td-table-plain">
                    <tbody>
                        <tr><th style={{ width: '20%' }}>회사명</th><td>{input('bizNm2')}</td></tr>
                        <tr><th>담당자</th><td>{input('repNm')}</td></tr>
                        <tr><th>직책</th><td>{input('roleNm')}</td></tr>
                        <tr><th>이메일</th><td>{input('emlAddr')}</td></tr>
                        <tr><th>전화번호</th><td>{input('mbTelNo')}</td></tr>
                    </tbody>
                </table>

                <div className="td-closing">{area('techDocLastPhrsCntn', 2)}</div>
            </div>
        </div>
    );
}

// ─────────────────────────────────────────────────────────────
// 화면 스타일 (기존 전역 CSS를 건드리지 않도록 이 페이지 안에서만 적용)
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
.td-toolbar-info {
  display: flex; align-items: center; align-content: center; gap: 8px;
  font-size: 14px; color: #111827; flex-wrap: wrap; line-height: 1.2;
  /* 문구가 길어져도 버튼을 아랫줄로 밀지 않고 이 영역이 줄어들며 감싼다 */
  flex: 1 1 auto; min-width: 0;
}
.td-toolbar-info > * { display: inline-flex; align-items: center; }
.td-badge { background: #e0f2fe; color: #0369a1; padding: 3px 8px; border-radius: 4px; font-size: 12px; font-weight: 500; line-height: 1.2; }
.td-docid { color: #6b7280; font-size: 12px; font-family: ui-monospace, SFMono-Regular, Menlo, monospace; }
.td-message { color: #15803d; font-size: 12px; }
.td-toolbar-buttons {
  display: flex; align-items: center; gap: 8px;
  /* 버튼은 줄어들지 않고 툴바 세로 중앙에 고정 */
  flex: 0 0 auto; align-self: center;
}

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
.td-title-center { text-align: center; }
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
`;
