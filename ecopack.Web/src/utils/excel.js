/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - downloadXlsx 유틸 (엑셀 내려받기 공용)
 * ==============================================================================
 * 
 * 1. 쓰임새
 *    - 라이브러리 조회 화면 5종(소재물성·공정도·탄소배출량·환경규제·디자인템플릿)이
 *      [엑셀] 버튼에서 함께 쓰는 내려받기 함수입니다.
 * 
 * 2. 만드는 방식
 *    - 1행은 굵은 글씨에 회색 배경을 준 머리글, 2행부터 데이터가 들어갑니다.
 *    - 화면에서 쓰는 컬럼 정의(label/width)를 그대로 받아 엑셀 열 너비로 환산합니다.
 *    - 머리글 행은 틀고정해 스크롤해도 항목명이 보이게 합니다.
 * 
 * 3. 파일명
 *    - 넘겨받은 이름 뒤에 오늘 날짜를 붙입니다. (예: 소재물성_2026-09-04.xlsx)
 * ==============================================================================
 */
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
