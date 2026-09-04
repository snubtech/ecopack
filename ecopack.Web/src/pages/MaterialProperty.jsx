import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetMaterialPropertyFilters,
    GetMaterialPropertyList,
} from '../api/materialProperty';
import { downloadXlsx } from '../utils/excel';
import { LIB_STYLES } from '../styles/libScreen';

/**
 * 라이브러리 > 소재물성 (참조: 기준정보 > 기준정보등록 > 소재물성DB)
 * 대상 테이블: if001 (소재/물성 데이터 목록)
 *
 * - 3분할 레이아웃의 중간 패널에 배치되며, 목록이 넘치면 가로·세로 스크롤로 제어한다.
 * - 조회조건 구성은 참조 화면과 동일하게 셀렉트 7종 + 키워드 검색으로 맞췄다.
 * - 공유여부(shareYn)는 우리가 수집하지 않는 항목이라 제외했다.
 */

/** 조회조건 정의 — 화면 순서 그대로 (라벨은 참조 화면 표기를 따름) */
const FILTERS = [
    { key: 'packLevel',       label: '소재물성',    source: 'packLevels' },
    { key: 'appliedMaterial', label: '적용소재',    source: 'appliedMaterials' },
    { key: 'matUse',          label: '사용환경',    source: 'matUses' },
    { key: 'matType',         label: '포장재 구분', source: 'matTypes' },
    { key: 'matForm',         label: '소재의 구성', source: 'matForms' },
    { key: 'item',            label: '성능항목',    source: 'items' },
    { key: 'unit',            label: '단위',        source: 'units' },
];

/** 그리드 컬럼 — 참조 화면과 동일 구성 (공유여부·키워드 컬럼 제외, 생성일 → 수집일시) */
const COLUMNS = [
    { key: 'packLevelNm',       label: '포장차수',    width: 100 },
    { key: 'appliedMaterialNm', label: '적용소재',    width: 110 },
    { key: 'matUseNm',          label: '사용환경',    width: 260 },
    { key: 'matTypeNm',         label: '포장재 구분', width: 180 },
    { key: 'matFormNm',         label: '소재의 구성', width: 260 },
    { key: 'itemNm',            label: '성능항목',    width: 200 },
    { key: 'unitNm',            label: '단위',        width: 90 },
    { key: 'acceptableRange',   label: '기준값 범위', width: 150 },
    { key: 'createdAt',         label: '수집일시',    width: 150 },
];

const PAGE_SIZES = [50, 100, 200];

const emptyCondition = () => {
    const c = {};
    FILTERS.forEach((f) => { c[f.key] = ''; });
    c.keywords = '';
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

export default function MaterialProperty() {
    // 입력 중인 조회조건과 실제로 조회에 쓰인 조건을 분리한다.
    // (조건을 바꿔도 [조회]를 눌러야 목록이 갱신되도록 — 참조 화면과 동일한 동작)
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

    /** 목록 조회 — 넘겨받은 조건/페이지 기준으로 서버를 호출한다 */
    const fetchList = useCallback(async (cond, pageNo, size) => {
        setLoading(true);
        try {
            const res = await GetMaterialPropertyList({ ...cond, page: pageNo, pageSize: size });
            const data = res?.data ?? {};
            setRows(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
            setMessage('');
        } catch (err) {
            console.error('소재물성 목록 조회 실패:', err);
            setRows([]);
            setTotalCount(0);
            setMessage('목록을 불러오는 중 오류가 발생했습니다.');
        } finally {
            setLoading(false);
        }
    }, []);

    // 최초 진입: 조회조건 목록 + 전체 목록
    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const res = await GetMaterialPropertyFilters();
                if (alive) setFilters(res?.data ?? null);
            } catch (err) {
                console.error('조회조건 로드 실패:', err);
                if (alive) setMessage('조회조건을 불러오지 못했습니다.');
            }
            if (alive) await fetchList(appliedRef.current, 1, PAGE_SIZES[0]);
        })();
        return () => { alive = false; };
    }, [fetchList]);

    /** [조회] — 현재 입력값을 확정하고 1페이지부터 다시 조회 */
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
            const res = await GetMaterialPropertyList({ ...appliedRef.current, page: 1, pageSize: 0 });
            const items = res?.data?.items ?? [];
            if (items.length === 0) {
                alert('내려받을 데이터가 없습니다.');
                return;
            }

            await downloadXlsx({
                fileName: '소재물성',
                sheetName: '소재물성',
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
                    <strong>소재물성</strong>
                    <span className="lib-count">
                        {loading ? '조회 중...' : `${totalCount.toLocaleString()}건`}
                    </span>
                    {message && <span className="lib-message">{message}</span>}
                </div>
                <div className="lib-toolbar-buttons">
                    <button type="button" className="lib-btn" onClick={handleReset} disabled={loading}>
                        초기화
                    </button>
                    <button type="button" className="lib-btn" onClick={handleExcel} disabled={loading || exporting}>
                        {exporting ? '내려받는 중...' : '엑셀'}
                    </button>
                    <button type="button" className="lib-btn lib-btn-primary" onClick={handleSearch} disabled={loading}>
                        조회
                    </button>
                </div>
            </div>

            {/* ── 조회조건 ── */}
            <div className="lib-search">
                {FILTERS.map((f) => (
                    <label key={f.key} className="lib-field">
                        <span className="lib-field-label">{f.label}</span>
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
                    </label>
                ))}
                <label className="lib-field lib-field-wide">
                    <span className="lib-field-label">키워드</span>
                    <input
                        type="text"
                        className="lib-input"
                        value={condition.keywords}
                        onChange={(e) => setField('keywords')(e.target.value)}
                        onKeyDown={(e) => { if (e.key === 'Enter') handleSearch(); }}
                        placeholder="적용소재·사용환경·포장재구분·소재구성·성능항목·단위·기준값 범위에서 검색"
                    />
                </label>
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

