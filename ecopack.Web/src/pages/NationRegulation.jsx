/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - NationRegulation 컴포넌트 (라이브러리 > 환경규제)
 * ==============================================================================
 * 
 * 1. 초기 데이터 로드 (useEffect)
 *    - 조회조건 셀렉트 3종(포장차수·적용소재·국가)을 받아온 뒤 전체 목록을 조회합니다.
 * 
 * 2. 조회 조건 관리 (condition / appliedRef)
 *    - 셀렉트 3종에 더해 관련규정·규제항목은 글자를 입력해 부분일치로 찾습니다.
 *    - 입력칸에서 Enter 를 눌러도 [조회] 와 같게 동작합니다.
 * 
 * 3. 목록 조회 (fetchList)
 *    - if004(국가규제정보)에서 규제내용·기준치·적용기간·원문 등을 받아옵니다.
 * 
 * 4. 엑셀 내려받기 (handleExcel)
 *    - 조회조건에 해당하는 전체 건을 다시 받아 xlsx 파일로 내려받습니다.
 * 
 * 5. 화면 렌더링 (JSX)
 *    - 상단 툴바(건수/버튼) → 조회조건 → 목록 → 페이징 순서로 그립니다.
 *    - 3분할 레이아웃의 중간 패널을 세로로 가득 채우고, 목록 영역만 상하·좌우로
 *      스크롤되게 하여 툴바와 조회조건, 페이징은 항상 화면에 남아 있습니다.
 *    - 규제내용·비고·원문처럼 긴 글은 말줄임 처리하고 마우스를 올리면 전체가 보이게 했습니다.
 * ==============================================================================
 */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetNationRegulationFilters,
    GetNationRegulationList,
} from '../api/nationRegulation';
import { downloadXlsx } from '../utils/excel';
import { LIB_STYLES } from '../styles/libScreen';

/**
 * 라이브러리 > 환경규제 (참조: 기준정보 > 기준정보등록 > 국가규제DB)
 * 대상 테이블: if004 (국가규제정보 목록)
 *
 * - 3분할 레이아웃의 중간 패널에 배치되며, 목록이 넘치면 가로·세로 스크롤로 제어한다.
 * - 규제내용·비고·원문처럼 긴 글이 담기는 컬럼은 말줄임 후 툴팁으로 전체를 볼 수 있게 한다.
 * - 공유여부·작성일·작성자는 우리가 수집하지 않는 항목이라 제외했다.
 */

/** 조회조건 — 참조 화면과 동일한 순서 */
const FILTERS = [
    { key: 'packLevel',       label: '포장차수', type: 'select', source: 'packLevels' },
    { key: 'appliedMaterial', label: '적용소재', type: 'select', source: 'appliedMaterials' },
    { key: 'countryCode',     label: '국가',     type: 'select', source: 'countries' },
    { key: 'relatedReg',      label: '관련규정', type: 'text' },
    { key: 'regItem',         label: '규제항목', type: 'text' },
];

/** 그리드 컬럼 — 참조 화면의 제목을 그대로 사용 (생성일 → 수집일시) */
const COLUMNS = [
    { key: 'packLevelNm',       label: '포장차수',    width: 90 },
    { key: 'appliedMaterialNm', label: '적용소재',    width: 90 },
    { key: 'countryCodeNm',     label: '국가',        width: 80 },
    { key: 'relatedReg',        label: '관련규정',    width: 180 },
    { key: 'regItem',           label: '규제항목',    width: 240 },
    { key: 'dtlCont',           label: '규제내용',    width: 220 },
    { key: 'unitNm',            label: '단위',        width: 70 },
    { key: 'minCont',           label: '기준치',      width: 160 },
    { key: 'minOperatorNm',     label: '범위',        width: 70 },
    { key: 'prepDeadline',      label: '적용시작일',  width: 90 },
    { key: 'prepDeadlineEnd',   label: '적용종료일',  width: 90 },
    { key: 'decisionOut',       label: '기술문서',    width: 260 },
    { key: 'isRequired',        label: '필수여부',    width: 80 },
    { key: 'memo',              label: '비고',        width: 200 },
    { key: 'originalText',      label: '원문',        width: 300 },
    { key: 'createdAt',         label: '수집일시',    width: 140 },
];

const PAGE_SIZES = [50, 100, 200];

const emptyCondition = () => {
    const c = {};
    FILTERS.forEach((f) => { c[f.key] = ''; });
    return c;
};

const formatDateTime = (value) => {
    if (!value) return '';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    const pad = (n) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const cellText = (row, col) =>
    col.key === 'createdAt' ? formatDateTime(row[col.key]) : (row[col.key] ?? '');

export default function NationRegulation() {
    const [condition, setCondition] = useState(emptyCondition);
    const appliedRef = useRef(emptyCondition());

    const [filters, setFilters] = useState(null);
    const [rows, setRows] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [page, setPage] = useState(1);
    const [pageSize, setPageSize] = useState(PAGE_SIZES[0]);
    const [loading, setLoading] = useState(true);
    const [exporting, setExporting] = useState(false);
    const [message, setMessage] = useState('');

    const totalPages = useMemo(
        () => Math.max(1, Math.ceil(totalCount / pageSize)),
        [totalCount, pageSize],
    );

    const setField = (key) => (value) => setCondition((prev) => ({ ...prev, [key]: value }));

    const fetchList = useCallback(async (cond, pageNo, size) => {
        setLoading(true);
        try {
            const res = await GetNationRegulationList({ ...cond, page: pageNo, pageSize: size });
            const data = res?.data ?? {};
            setRows(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
            setMessage('');
        } catch (err) {
            console.error('환경규제 목록 조회 실패:', err);
            setRows([]);
            setTotalCount(0);
            setMessage('목록을 불러오는 중 오류가 발생했습니다.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const res = await GetNationRegulationFilters();
                if (alive) setFilters(res?.data ?? null);
            } catch (err) {
                console.error('조회조건 로드 실패:', err);
                if (alive) setMessage('조회조건을 불러오지 못했습니다.');
            }
            if (alive) await fetchList(appliedRef.current, 1, PAGE_SIZES[0]);
        })();
        return () => { alive = false; };
    }, [fetchList]);

    const handleSearch = () => {
        appliedRef.current = { ...condition };
        setPage(1);
        fetchList(appliedRef.current, 1, pageSize);
    };

    const handleReset = () => {
        const empty = emptyCondition();
        setCondition(empty);
        appliedRef.current = empty;
        setPage(1);
        fetchList(empty, 1, pageSize);
    };

    const handlePage = (next) => {
        if (next < 1 || next > totalPages || next === page) return;
        setPage(next);
        fetchList(appliedRef.current, next, pageSize);
    };

    const handlePageSize = (size) => {
        setPageSize(size);
        setPage(1);
        fetchList(appliedRef.current, 1, size);
    };

    /** [엑셀] — 현재 조회조건에 해당하는 전체 건을 xlsx 로 내려받는다 */
    const handleExcel = async () => {
        setExporting(true);
        try {
            const res = await GetNationRegulationList({ ...appliedRef.current, page: 1, pageSize: 0 });
            const items = res?.data?.items ?? [];
            if (items.length === 0) {
                alert('내려받을 데이터가 없습니다.');
                return;
            }
            await downloadXlsx({
                fileName: '환경규제',
                sheetName: '환경규제',
                columns: COLUMNS,
                rows: items,
                cellText,
            });
            setMessage(`${items.length.toLocaleString()}건을 내려받았습니다.`);
        } catch (err) {
            console.error('엑셀 내려받기 실패:', err);
            alert('내려받기 중 오류가 발생했습니다.');
        } finally {
            setExporting(false);
        }
    };

    const optionsOf = (source) => filters?.[source] ?? [];

    return (
        <div className="lib-page">
            <style>{LIB_STYLES}</style>

            {/* ── 상단 타이틀 + 액션 ── */}
            <div className="lib-toolbar">
                <div className="lib-toolbar-info">
                    <strong>환경규제</strong>
                    <span className="lib-count">
                        {loading ? '조회 중...' : `${totalCount.toLocaleString()}건`}
                    </span>
                    {message && <span className="lib-message">{message}</span>}
                </div>
                <div className="lib-toolbar-buttons">
                    <button type="button" className="lib-btn" onClick={handleReset} disabled={loading}>초기화</button>
                    <button type="button" className="lib-btn" onClick={handleExcel} disabled={loading || exporting}>
                        {exporting ? '내려받는 중...' : '엑셀'}
                    </button>
                    <button type="button" className="lib-btn lib-btn-primary" onClick={handleSearch} disabled={loading}>조회</button>
                </div>
            </div>

            {/* ── 조회조건 ── */}
            <div className="lib-search">
                {FILTERS.map((f) => (
                    <label key={f.key} className="lib-field">
                        <span className="lib-field-label">{f.label}</span>
                        {f.type === 'select' ? (
                            <select
                                className="lib-select"
                                value={condition[f.key]}
                                onChange={(e) => setField(f.key)(e.target.value)}
                            >
                                <option value="">전체</option>
                                {optionsOf(f.source).map((o) => (
                                    <option key={`${f.key}-${o.code}`} value={o.code}>{o.name}</option>
                                ))}
                            </select>
                        ) : (
                            <input
                                type="text"
                                className="lib-input"
                                value={condition[f.key]}
                                onChange={(e) => setField(f.key)(e.target.value)}
                                onKeyDown={(e) => { if (e.key === 'Enter') handleSearch(); }}
                                placeholder={`${f.label}으로 검색`}
                            />
                        )}
                    </label>
                ))}
            </div>

            {/* ── 목록 ── */}
            <div className="lib-grid-wrap">
                <table className="lib-grid">
                    <thead>
                        <tr>
                            <th className="lib-col-no">No</th>
                            {COLUMNS.map((c) => (
                                <th key={c.key} style={{ minWidth: c.width }}>{c.label}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {loading ? (
                            <tr><td colSpan={COLUMNS.length + 1} className="lib-empty">조회 중입니다...</td></tr>
                        ) : rows.length === 0 ? (
                            <tr><td colSpan={COLUMNS.length + 1} className="lib-empty">조회된 데이터가 없습니다.</td></tr>
                        ) : (
                            rows.map((r, i) => (
                                <tr key={r.idx}>
                                    <td className="lib-col-no">{(page - 1) * pageSize + i + 1}</td>
                                    {COLUMNS.map((c) => (
                                        <td key={c.key} title={cellText(r, c)}>{cellText(r, c)}</td>
                                    ))}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {/* ── 페이징 ── */}
            <div className="lib-paging">
                <div className="lib-pagesize">
                    <span>페이지당</span>
                    <select
                        className="lib-select lib-select-sm"
                        value={pageSize}
                        onChange={(e) => handlePageSize(Number(e.target.value))}
                    >
                        {PAGE_SIZES.map((s) => <option key={s} value={s}>{s}건</option>)}
                    </select>
                </div>
                <div className="lib-pager">
                    <button type="button" className="lib-btn lib-btn-sm" onClick={() => handlePage(1)} disabled={page <= 1}>처음</button>
                    <button type="button" className="lib-btn lib-btn-sm" onClick={() => handlePage(page - 1)} disabled={page <= 1}>이전</button>
                    <span className="lib-pageinfo">{page} / {totalPages}</span>
                    <button type="button" className="lib-btn lib-btn-sm" onClick={() => handlePage(page + 1)} disabled={page >= totalPages}>다음</button>
                    <button type="button" className="lib-btn lib-btn-sm" onClick={() => handlePage(totalPages)} disabled={page >= totalPages}>마지막</button>
                </div>
            </div>
        </div>
    );
}
