import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetCarbonEmissionFilters,
    GetCarbonEmissionList,
} from '../api/carbonEmission';
import { downloadXlsx } from '../utils/excel';
import { LIB_STYLES } from '../styles/libScreen';

/**
 * 라이브러리 > 탄소배출량 (참조: 기준정보 > 기준정보등록 > 소재공정 탄소배출량DB)
 * 대상 테이블: if005 (환경영향평가정보 목록)
 *
 * - 3분할 레이아웃의 중간 패널에 배치되며, 목록이 넘치면 가로·세로 스크롤로 제어한다.
 * - 컬럼 제목이 길어 헤더를 여러 줄로 접어 보여준다(lib-grid-wrapheader).
 * - 공유여부·작성일·작성자는 우리가 수집하지 않는 항목이라 제외했다.
 */

/** 조회조건 — 참조 화면과 동일한 순서 */
const FILTERS = [
    { key: 'packLevel',       label: '포장차수',    source: 'packLevels' },
    { key: 'appliedMaterial', label: '적용소재',    source: 'appliedMaterials' },
    { key: 'matForm',         label: '소재의 구성', source: 'matForms' },
];

/** 그리드 컬럼 — 참조 화면의 제목을 그대로 사용 (생성일 → 수집일시) */
const COLUMNS = [
    { key: 'packLevelNm',       label: '포장차수',                              width: 90 },
    { key: 'appliedMaterialNm', label: '적용소재',                              width: 90 },
    { key: 'matFormNm',         label: '소재의 구성',                           width: 200 },
    { key: 'massCo2Mat',        label: '중량 당 탄소배출량(kgCO2.eq/kg)-원료',   width: 120, num: true },
    { key: 'massCo2Proc',       label: '중량 당 탄소배출량(kgCO2.eq/kg)-제조',   width: 120, num: true },
    { key: 'massCo2Scrap',      label: '중량당 탄소배출량(kgCO2.eq/kg)-폐기',    width: 120, num: true },
    { key: 'massCo2Sum',        label: '중량 당 탄소배출량(kgCO2.eq/kg)합계',    width: 120, num: true },
    { key: 'unitCo2Mat',        label: '단위당 탄소배출량(kgCO2.eq/관리단위)-원료', width: 130, num: true },
    { key: 'unitCo2Proc',       label: '단위당 탄소배출량(kgCO2.eq/관리단위)-제조', width: 130, num: true },
    { key: 'unitCo2Scrap',      label: '단위당 탄소배출량(kgCO2.eq/관리단위)-폐기', width: 130, num: true },
    { key: 'unitCo2Sum',        label: '단위당 탄소배출량(kgCO2.eq/관리단위)-합계', width: 130, num: true },
    { key: 'unitCo2MgtVal',     label: '단위당 탄소배출량의 관리단위',           width: 110 },
    { key: 'areaDensity',       label: '물리적 인자-면적당 중량(kg/m2)',         width: 120, num: true },
    { key: 'density',           label: '물리적 인자-밀도(kg/m3)',               width: 110, num: true },
    { key: 'matCompCon',        label: '원료물질 구성',                         width: 230 },
    { key: 'createdAt',         label: '수집일시',                              width: 140 },
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

export default function CarbonEmission() {
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
            const res = await GetCarbonEmissionList({ ...cond, page: pageNo, pageSize: size });
            const data = res?.data ?? {};
            setRows(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
            setMessage('');
        } catch (err) {
            console.error('탄소배출량 목록 조회 실패:', err);
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
                const res = await GetCarbonEmissionFilters();
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
            const res = await GetCarbonEmissionList({ ...appliedRef.current, page: 1, pageSize: 0 });
            const items = res?.data?.items ?? [];
            if (items.length === 0) {
                alert('내려받을 데이터가 없습니다.');
                return;
            }
            await downloadXlsx({
                fileName: '탄소배출량',
                sheetName: '탄소배출량',
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
                    <strong>탄소배출량</strong>
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
            </div>

            {/* ── 목록 ── */}
            <div className="lib-grid-wrap">
                <table className="lib-grid lib-grid-wrapheader">
                    <thead>
                        <tr>
                            <th className="lib-col-no">No</th>
                            {COLUMNS.map((c) => (
                                <th key={c.key} className={c.num ? 'lib-num' : undefined} style={{ minWidth: c.width }}>
                                    {c.label}
                                </th>
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
                                        <td key={c.key} className={c.num ? 'lib-num' : undefined} title={cellText(r, c)}>
                                            {cellText(r, c)}
                                        </td>
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
