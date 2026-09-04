import writeXlsxFile from 'write-excel-file/browser';

/**
 * 목록 데이터를 xlsx 파일로 내려받는다.
 * 라이브러리 계열 조회 화면(소재물성·공정도·탄소배출량·환경규제·디자인템플릿)이 공용으로 사용한다.
 *
 * @param {object}   options
 * @param {string}   options.fileName  확장자를 뺀 파일명 (내려받을 때 날짜가 자동으로 붙는다)
 * @param {string}   options.sheetName 시트명
 * @param {Array}    options.columns   [{ key, label, width }] 형태의 컬럼 정의
 * @param {Array}    options.rows      행 데이터 배열
 * @param {Function} [options.cellText] (row, column) => 표시 문자열. 생략하면 row[column.key] 사용
 */
export async function downloadXlsx({ fileName, sheetName, columns, rows, cellText }) {
    const text = cellText ?? ((row, col) => row[col.key]);

    // 1행: 헤더 / 2행부터: 데이터
    const header = columns.map((c) => ({
        value: c.label,
        fontWeight: 'bold',
        align: 'center',
        backgroundColor: '#F3F4F6',
        borderStyle: 'thin',
        borderColor: '#D1D5DB',
    }));

    const body = rows.map((r) =>
        columns.map((c) => ({
            value: text(r, c) ?? '',
            type: String,
            borderStyle: 'thin',
            borderColor: '#E5E7EB',
            wrap: false,
        })),
    );

    // 화면 컬럼 폭(px)을 엑셀 문자폭으로 환산 — 대략 7px = 1글자
    const columnSetting = columns.map((c) => ({
        width: Math.min(60, Math.max(10, Math.round((c.width ?? 120) / 7))),
    }));

    const today = new Date().toISOString().slice(0, 10);

    await writeXlsxFile([header, ...body], {
        columns: columnSetting,
        sheet: sheetName || 'Sheet1',
        fileName: `${fileName}_${today}.xlsx`,
        stickyRowsCount: 1,
    });
}
