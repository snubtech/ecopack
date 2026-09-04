/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - DesignTemplate 컴포넌트 (라이브러리 > 디자인 템플릿)
 * ==============================================================================
 * 
 * 1. 초기 데이터 로드 (useEffect)
 *    - 조회조건 셀렉트 3종(포장차수·적용소재·포장재 구분)을 받아온 뒤 전체 목록을 조회합니다.
 *    - 목록은 if002(디자인템플릿 목록)에서 가져오며, 무거운 이미지는 담기지 않습니다.
 * 
 * 2. 조회 조건 관리 (condition / appliedRef)
 *    - 템플릿명은 글자를 입력해 부분일치로 찾고, 나머지 3종은 셀렉트로 고릅니다.
 * 
 * 3. 상세 펼치기 (handleToggleDetail)
 *    - 탬플릿명을 누르면 그 행 아래로 디자인설명내용·디자인 특징·제품의 설명과 이미지를 펼칩니다.
 *    - 이미지는 용량이 크므로 펼칠 때 한 번만 서버(if002a)에서 받아오고,
 *      이미 받아온 행은 details 에 담아 두어 다시 요청하지 않습니다.
 * 
 * 4. 엑셀 내려받기 (handleExcel)
 *    - 조회조건에 해당하는 전체 건을 다시 받아 xlsx 파일로 내려받습니다.
 * 
 * 5. 화면 렌더링 (JSX)
 *    - 상단 툴바(건수/버튼) → 조회조건 → 목록 → 페이징 순서로 그립니다.
 *    - 3분할 레이아웃의 중간 패널을 세로로 가득 채우고, 목록 영역만 상하·좌우로
 *      스크롤되게 하여 툴바와 조회조건, 페이징은 항상 화면에 남아 있습니다.
 * ==============================================================================
 */
import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import {
    GetDesignTemplateFilters,
    GetDesignTemplateList,
    GetDesignTemplateDetail,
} from '../api/designTemplate';
import { downloadXlsx } from '../utils/excel';
import { LIB_STYLES } from '../styles/libScreen';

/**
 * 라이브러리 > 디자인 템플릿 (참조: 기준정보 > 기준정보등록 > 패키징디자인DB)
 * 목록 테이블: if002 / 상세(설명 3종 + 이미지): if002a
 *
 * - 3분할 레이아웃의 중간 패널에 배치되며, 목록이 넘치면 가로·세로 스크롤로 제어한다.
 * - 탬플릿명을 클릭하면 그 아래로 상세를 펼친다(참조 화면의 행 상세와 동일 구성).
 * - 공유여부·작성일·작성자·수정일·수정자는 우리가 수집하지 않는 항목이라 제외했다.
 */

/** 조회조건 — 참조 화면과 동일한 순서 */
const FILTERS = [
    { key: 'subject',         label: '템플릿명',    type: 'text' },
    { key: 'packLevel',       label: '포장차수',    type: 'select', source: 'packLevels' },
    { key: 'appliedMaterial', label: '적용소재',    type: 'select', source: 'appliedMaterials' },
    { key: 'matType',         label: '포장재 구분', type: 'select', source: 'matTypes' },
];

/** 그리드 컬럼 (생성일 → 수집일시) */
const COLUMNS = [
    { key: 'packLevelNm',       label: '포장차수',      width: 90 },
    { key: 'appliedMaterialNm', label: '적용소재',      width: 100 },
    { key: 'matTypeNm',         label: '포장재 구분',   width: 130 },
    { key: 'subject',           label: '탬플릿명',      width: 220 },
    { key: 'dsgnTypeNm',        label: '디자인유형명',  width: 170 },
    { key: 'dsgnTypeCdVal',     label: '디자인유형코드', width: 120 },
    { key: 'createdAt',         label: '수집일시',      width: 150 },
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

export default function DesignTemplate() {
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

    // 펼쳐진 행의 상세. 한 번 받아온 건 다시 요청하지 않는다.
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
            const res = await GetDesignTemplateList({ ...cond, page: pageNo, pageSize: size });
            const data = res?.data ?? {};
            setRows(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
            setMessage('');
        } catch (err) {
            console.error('디자인 템플릿 목록 조회 실패:', err);
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
                const res = await GetDesignTemplateFilters();
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

    /** 탬플릿명 클릭 — 상세를 펼치거나 접는다 */
    const handleToggleDetail = async (row) => {
        const id = row.packDsgnTplId;
        if (expandedId === id) {
            setExpandedId(null);
            return;
        }
        setExpandedId(id);
        if (details[id]) return;

        setDetailLoading(true);
        try {
            const res = await GetDesignTemplateDetail(id);
            setDetails((prev) => ({ ...prev, [id]: res?.data ?? null }));
        } catch (err) {
            console.error('디자인 템플릿 상세 조회 실패:', err);
            setDetails((prev) => ({ ...prev, [id]: null }));
        } finally {
            setDetailLoading(false);
        }
    };

    /** [엑셀] — 현재 조회조건에 해당하는 전체 건을 xlsx 로 내려받는다 */
    const handleExcel = async () => {
        setExporting(true);
        try {
            const res = await GetDesignTemplateList({ ...appliedRef.current, page: 1, pageSize: 0 });
            const items = res?.data?.items ?? [];
            if (items.length === 0) {
                alert('내려받을 데이터가 없습니다.');
                return;
            }
            await downloadXlsx({
                fileName: '디자인템플릿',
                sheetName: '디자인템플릿',
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

    /** 펼쳐진 행 아래에 붙는 상세 영역 — 참조 화면과 동일하게 설명 3종 + 이미지 */
    const renderDetail = (row) => {
        const d = details[row.packDsgnTplId];
        if (detailLoading && !d) {
            return <div className="lib-detail-empty">상세 정보를 불러오는 중입니다...</div>;
        }
        if (!d) {
            return <div className="lib-detail-empty">상세 정보를 찾을 수 없습니다.</div>;
        }

        const images = d.memoImageUris?.length ? d.memoImageUris : (d.fileImageUri ? [d.fileImageUri] : []);

        return (
            <div className="lib-detail">
                <div className="tpl-detail">
                    <div className="tpl-detail-texts">
                        <div className="tpl-field">
                            <div className="tpl-field-label">디자인설명내용</div>
                            <div className="tpl-field-value">{d.dsgnExpCon || '-'}</div>
                        </div>
                        <div className="tpl-field">
                            <div className="tpl-field-label">디자인 특징</div>
                            <div className="tpl-field-value">{d.dsgnFeatDscr || '-'}</div>
                        </div>
                        <div className="tpl-field">
                            <div className="tpl-field-label">제품의 설명</div>
                            <div className="tpl-field-value">{d.operDscr || '-'}</div>
                        </div>
                    </div>
                    <div className="tpl-detail-image">
                        <div className="tpl-field-label">
                            이미지
                            {d.fileImageUri && (
                                <a className="lib-link tpl-download" href={d.fileImageUri} download={d.fileNm || '디자인템플릿'}>
                                    내려받기
                                </a>
                            )}
                        </div>
                        {images.length === 0 ? (
                            <div className="lib-detail-empty">등록된 이미지가 없습니다.</div>
                        ) : (
                            images.map((uri, i) => (
                                <img key={i} src={uri} alt={`${row.subject ?? '디자인 템플릿'} ${i + 1}`} />
                            ))
                        )}
                    </div>
                </div>
            </div>
        );
    };

    return (
        <div className="lib-page">
            <style>{LIB_STYLES}{TEMPLATE_STYLES}</style>

            {/* ── 상단 타이틀 + 액션 ── */}
            <div className="lib-toolbar">
                <div className="lib-toolbar-info">
                    <strong>디자인 템플릿</strong>
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
                                const opened = expandedId === r.packDsgnTplId;
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
                                                        title="클릭하면 상세를 펼칩니다"
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

/** 디자인 템플릿 화면 전용 — 행 펼침 상세 영역 */
const TEMPLATE_STYLES = `
.lib-linkbtn {
  border: none; background: none; padding: 0; font: inherit; font-size: 13px;
  color: #2563eb; text-decoration: underline; cursor: pointer; text-align: left;
  max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
}
.lib-linkbtn:hover { color: #1d4ed8; }
.lib-row-open > td { background: #f0f9ff; }
.lib-detail-row > td { background: #fafafa; padding: 0 !important; white-space: normal !important; max-width: none !important; }
.lib-detail { padding: 14px 16px; }
.lib-detail-empty { padding: 18px 4px; color: #9ca3af; font-size: 13px; }
.lib-link { font-size: 12px; color: #2563eb; text-decoration: underline; }

.tpl-detail { display: flex; gap: 28px; align-items: flex-start; flex-wrap: wrap; }
.tpl-detail-texts { flex: 1 1 340px; min-width: 0; display: flex; flex-direction: column; gap: 14px; }
.tpl-detail-image { flex: 0 1 420px; min-width: 0; }
.tpl-field-label { font-weight: 600; color: #6b7280; font-size: 12px; margin-bottom: 4px; }
.tpl-field-value { font-size: 13px; color: #111827; line-height: 1.7; white-space: pre-wrap; word-break: break-word; }
.tpl-download { margin-left: 8px; font-weight: 400; }
.tpl-detail-image img {
  max-width: 100%; height: auto; border: 1px solid #e5e7eb; border-radius: 6px; background: #fff;
}
`;
