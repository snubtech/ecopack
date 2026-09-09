/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - applyDocxColumnWidths 유틸 (DOCX 추출용 표 폭 고정)
 * ==============================================================================
 *
 * 1. 왜 필요한가
 *    - 화면(.td-table)은 table-layout:fixed + 각 th의 width로 열 폭을 고정해 두지만,
 *      Word의 HTML 가져오기 엔진에는 이게 전혀 안 먹힌다. 아래 방식들을 순서대로
 *      시도했지만 전부 효과가 없었다(실제 재현 테스트로 확인됨):
 *        1) th/td에 style="width:Npx"                      → 무시됨
 *      2) <style> 블록에 table-layout:fixed 추가            → 무시됨
 *        3) <colgroup><col style="width:Npx"> 주입           → 무시됨
 *        4) 위 col에 mso-width-source:userset 추가           → 무시됨
 *
 * 2. 최종 방식 — "레거시 % width 속성을 모든 행에 직접"
 *    - Word의 HTML 필터는 CSS2 표 모델(table-layout, colgroup)을 사실상 지원하지 않고,
 *      1990년대 HTML3.2/4 스타일의 아주 오래된 표 렌더링 방식을 흉내 낸다. 이 시절 표는
 *      "표 전체 대비 이 칸이 몇 %"를 각 <td width="25%">에 직접 못박는 방식이었고,
 *      Word의 표 임포터가 가장 확실하게 지키는 게 바로 이 legacy 속성이다.
 *    - 그래서 colgroup(첫 행 기준)이 아니라, 폭을 정할 "열 번호"마다 표의 모든 행(각
 *      셀)에 직접 width="N%" 속성 + style="width:N%"를 반복해서 박아 넣는다. kv형 라벨
 *      표는애초에 행마다 독립된 <th>라서 이렇게 해야만 모든 행에 폭이 적용된다.
 *
 * 3. 쓰임새
 *    - handleExportDocx에서 Blob을 만들기 직전, cloneNode한 문서 사본의 모든 <table>에
 *      대해 applyDocxColumnWidths(clone)를 호출한다.
 * ==============================================================================
 */

/** 화면의 .td-table-kv th 기본 폭(170px)과 반드시 같은 값으로 맞춘다. */
const KV_LABEL_WIDTH_PX = 170;

/** 폭(px)을 표 전체 폭 대비 %로 환산하는 기준값. DOCX 본문 폭 17cm를 96dpi로 잡은 근사 픽셀값. */
const REFERENCE_TABLE_WIDTH_PX = 642;

function pxToPercent(px) {
    return Math.max(1, Math.round((px / REFERENCE_TABLE_WIDTH_PX) * 100));
}

/**
 * 문서 사본(clone) 안의 모든 <table>에 열 폭(%)을 모든 행에 직접 박아 넣는다.
 * @param {HTMLElement} root cloneNode한 문서 사본의 루트 요소
 */
export function applyDocxColumnWidths(root) {
    root.querySelectorAll('table').forEach((table) => applyToOneTable(table));
}

function applyToOneTable(table) {
    const rows = Array.from(table.querySelectorAll('tr'));
    if (rows.length === 0) return;

    const isKv = table.classList.contains('td-table-kv');
    const firstRowCells = Array.from(rows[0].children);

    // "열 번호 → 몇 %로 고정할지"를 첫 행 기준으로 정한다. kv 표는 각 행이 독립된
    // <th>(라벨)를 갖고 있어 첫 행의 th가 그 값을 대표한다 — 화면에서도 table-layout:fixed가
    // 첫 행만 보고 나머지 행에 똑같이 적용하는 것과 같은 논리다.
    const widthsPct = firstRowCells.map((cell) => {
        let px = null;
        if (cell.style.width) px = parseInt(cell.style.width, 10);
        else if (isKv && cell.tagName === 'TH') px = KV_LABEL_WIDTH_PX;
        return Number.isFinite(px) ? pxToPercent(px) : null;
    });

    if (!widthsPct.some((pct) => pct != null)) return; // 폭을 정할 열이 없으면 손댈 필요 없음

    rows.forEach((row) => {
        Array.from(row.children).forEach((cell, i) => {
            const pct = widthsPct[i];
            if (pct == null) return;
            // legacy width 속성(%) — Word의 HTML 표 임포터가 실제로 지키는 값.
            // style width는 곁다리로 같이 둔다(다른 뷰어에서도 최소한의 일관성 유지용).
            cell.setAttribute('width', `${pct}%`);
            cell.style.width = `${pct}%`;
        });
    });

    table.setAttribute('width', '100%');
}
