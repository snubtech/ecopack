import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetProcessChartFilters,
    GetProcessChartList,
    GetProcessChartDetail,
} from '../api/processChart';
import { downloadXlsx } from '../utils/excel';
import { LIB_STYLES } from '../styles/libScreen';

/**
 * 라이브러리 > 공정도 (참조: 기준정보 > 기준정보등록 > 공정DB)
 * 목록 테이블: if003 / 상세(공정도 이미지): if003a
 *
 * - 3분할 레이아웃의 중간 패널에 배치되며, 목록이 넘치면 가로·세로 스크롤로 제어한다.
 * - 탬플릿명을 클릭하면 그 아래로 공정도 이미지를 펼쳐 보여준다(참조 화면의 행 상세와 동일).
 * - 공유여부·작성자·수정일·수정자는 우리가 수집하지 않는 항목이라 제외했다.
 */

/** 조회조건 — 참조 화면과 동일한 순서 */
const FILTERS = [
    { key: 'subject',         label: '템플릿명', type: 'text' },
    { key: 'appliedMaterial', label: '적용소재', type: 'select', source: 'appliedMaterials' },
];

/** 그리드 컬럼 (생성일 → 수집일시) */
const COLUMNS = [
    { key: 'subject',           label: '탬플릿명',        width: 300 },
    { key: 'appliedMaterialNm', label: '적용소재',        width: 100 },
    { key: 'matTypeNm',         label: '포장재 구분',     width: 160 },
    { key: 'matCompNm',         label: '소재재질의 구성', width: 140 },
    { key: 'matFormNm',         label: '소재의 구성',     width: 140 },
    { key: 'fileExistYn',       label: '파일존재여부',    width: 100 },
    { key: 'createdAt',         label: '수집일시',        width: 150 },
];

const PAGE_SIZES = [50, 100, 200];

const emptyCondition = () => ({ subject: '', appliedMaterial: '' });

const formatDateTime = (value) => {
    if (!value) return '';
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return String(value);
    const pad = (n) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const cellText = (row, col) =>
    col.key === 'createdAt' ? formatDateTime(row[col.key]) : (row[col.key] ?? '');

export default function ProcessChart() {
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

    // 펼쳐진 행의 상세(공정도 이미지). 한 번 받아온 건 다시 요청하지 않는다.
    const [expandedId, setExpandedId] = useState(null);
    const [details, setDetails] = useState({});
    const [detailLoading, setDetailLoading] = useState(false);

    const totalPages = useMemo(
        () => Math.max(1, Math.ceil(totalCount / pageSize)),
        [totalCount, pageSize],
    );

    const setField = (key) => (value) => setCondition((prev) => ({ ...prev, [key]: value }));

    const fetchList = useCallback(async (cond, pageNo, size) => {
        setLoading(true);
        setExpandedId(null);
        try {
            const res = await GetProcessChartList({ ...cond, page: pageNo, pageSize: size });
            const data = res?.data ?? {};
            setRows(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
            setMessage('');
        } catch (err) {
            console.error('공정도 목록 조회 실패:', err);
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
                const res = await GetProcessChartFilters();
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

    /** 탬플릿명 클릭 — 공정도 이미지를 펼치거나 접는다 */
    const handleToggleDetail = async (row) => {
        const id = row.packMmftProcId;
        if (expandedId === id) {
            setExpandedId(null);
            return;
        }
        setExpandedId(id);

        if (details[id]) return; // 이미 받아온 상세는 재요청하지 않는다

        setDetailLoading(true);
        try {
            const res = await GetProcessChartDetail(id);
            setDetails((prev) => ({ ...prev, [id]: res?.data ?? null }));
        } catch (err) {
            console.error('공정도 상세 조회 실패:', err);
            setDetails((prev) => ({ ...prev, [id]: null }));
        } finally {
            setDetailLoading(false);
        }
    };

    /** [엑셀] — 현재 조회조건에 해당하는 전체 건을 xlsx 로 내려받는다 */
    const handleExcel = async () => {
        setExporting(true);
        try {
            const res = await GetProcessChartList({ ...appliedRef.current, page: 1, pageSize: 0 });
            const items = res?.data?.items ?? [];
            if (items.length === 0) {
                alert('내려받을 데이터가 없습니다.');
                return;
            }
            await downloadXlsx({
                fileName: '공정도',
                sheetName: '공정도',
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

    /** 펼쳐진 행 아래에 붙는 공정도 이미지 영역 */
    const renderDetail = (row) => {
        const d = details[row.packMmftProcId];
        if (detailLoading && !d) {
            return <div className="lib-detail-empty">공정도를 불러오는 중입니다...</div>;
        }
        if (!d) {
            return <div className="lib-detail-empty">공정도 정보를 찾을 수 없습니다.</div>;
        }

        // 설명이미지(memoImg)가 있으면 그것을, 없으면 첨부파일 이미지를 보여준다
        const images = d.memoImageUris?.length ? d.memoImageUris : (d.fileImageUri ? [d.fileImageUri] : []);
        if (images.length === 0) {
            return <div className="lib-detail-empty">등록된 공정도 이미지가 없습니다.</div>;
        }

        return (
            <div className="lib-detail">
                <div className="lib-detail-head">
                    <strong>공정도 이미지</strong>
                    {d.fileNm && <span className="lib-detail-file">{d.fileNm}</span>}
                    {d.fileImageUri && (
                        <a className="lib-link" href={d.fileImageUri} download={d.fileNm || '공정도'}>
                            원본 내려받기
                        </a>
                    )}
                </div>
                <div className="lib-detail-images">
                    {images.map((uri, i) => (
                        <img key={i} src={uri} alt={`${row.subject ?? '공정도'} ${i + 1}`} />
                    ))}
                </div>
            </div>
        );
    };

    return (
        <div className="lib-page">
            <style>{LIB_STYLES}{PROCESS_STYLES}</style>

            {/* ── 상단 타이틀 + 액션 ── */}
            <div className="lib-toolbar">
                <div className="lib-toolbar-info">
                    <strong>공정도</strong>
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
                                placeholder="템플릿명으로 검색"
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
                            rows.map((r, i) => {
                                const opened = expandedId === r.packMmftProcId;
                                return [
                                    <tr key={r.idx} className={opened ? 'lib-row-open' : undefined}>
                                        <td className="lib-col-no">{(page - 1) * pageSize + i + 1}</td>
                                        {COLUMNS.map((c) => (
                                            c.key === 'subject' ? (
                                                <td key={c.key}>
                                                    <button
                                                        type="button"
                                                        className="lib-linkbtn"
                                                        onClick={() => handleToggleDetail(r)}
                                                        title="클릭하면 공정도 이미지를 펼칩니다"
                                                    >
                                                        {opened ? '▾ ' : '▸ '}{cellText(r, c)}
                                                    </button>
                                                </td>
                                            ) : (
                                                <td key={c.key} title={cellText(r, c)}>{cellText(r, c)}</td>
                                            )
                                        ))}
                                    </tr>,
                                    opened && (
                                        <tr key={`${r.idx}-detail`} className="lib-detail-row">
                                            <td colSpan={COLUMNS.length + 1}>{renderDetail(r)}</td>
                                        </tr>
                                    ),
                                ];
                            })
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

/** 공정도 화면 전용 — 행 펼침 상세 영역 */
const PROCESS_STYLES = `
.lib-linkbtn {
  border: none; background: none; padding: 0; font: inherit; font-size: 13px;
  color: #2563eb; text-decoration: underline; cursor: pointer; text-align: left;
  max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.lib-linkbtn:hover { color: #1d4ed8; }
.lib-row-open > td { background: #f0f9ff; }
.lib-detail-row > td { background: #fafafa; padding: 0 !important; white-space: normal !important; max-width: none !important; }
.lib-detail { padding: 14px 16px; }
.lib-detail-head { display: flex; align-items: center; gap: 10px; margin-bottom: 10px; font-size: 13px; }
.lib-detail-head > strong { color: #374151; }
.lib-detail-file { color: #6b7280; font-size: 12px; }
.lib-link { font-size: 12px; color: #2563eb; text-decoration: underline; }
.lib-detail-images { display: flex; flex-wrap: wrap; gap: 14px; }
.lib-detail-images img {
  max-width: 100%; height: auto; border: 1px solid #e5e7eb; border-radius: 6px; background: #fff;
}
.lib-detail-empty { padding: 22px 16px; color: #9ca3af; font-size: 13px; }
`;
