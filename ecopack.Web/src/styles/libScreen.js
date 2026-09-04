/**
 * 라이브러리 계열 조회 화면(소재물성·공정도·탄소배출량·환경규제·디자인템플릿) 공용 스타일.
 * 3분할 레이아웃의 중간 패널을 채우고, 목록 영역만 상하·좌우로 스크롤되게 한다.
 */
export const LIB_STYLES = `
/* 중간 패널 높이를 채우고, 목록만 스크롤되게 한다 */
.lib-page {
  display: flex; flex-direction: column; gap: 12px;
  height: 100%; min-height: 0; box-sizing: border-box;
}

.lib-toolbar {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  flex-wrap: wrap; flex: 0 0 auto;
  padding-bottom: 10px; border-bottom: 1px solid #e5e7eb;
}
.lib-toolbar-info { display: flex; align-items: center; gap: 8px; line-height: 1.2; }
.lib-toolbar-info > strong { font-size: 17px; color: #111827; }
.lib-count { font-size: 13px; color: #2563eb; font-weight: 500; }
.lib-message { font-size: 12px; color: #15803d; }
.lib-toolbar-buttons { display: flex; align-items: center; gap: 8px; flex: 0 0 auto; }

.lib-btn {
  padding: 7px 16px; border: 1px solid #cbd5e1; background: #fff; color: #334155;
  border-radius: 6px; cursor: pointer; font-size: 13px; font-weight: 500; white-space: nowrap;
}
.lib-btn:hover:not(:disabled) { background: #f8fafc; }
.lib-btn:disabled { opacity: .45; cursor: not-allowed; }
.lib-btn-primary { background: #198754; border-color: #198754; color: #fff; }
.lib-btn-primary:hover:not(:disabled) { background: #157347; }
.lib-btn-sm { padding: 4px 10px; font-size: 12px; }

/* 조회조건 — 폭에 따라 자동 줄바꿈 */
.lib-search {
  display: grid; grid-template-columns: repeat(auto-fill, minmax(230px, 1fr)); gap: 10px 14px;
  flex: 0 0 auto; padding: 14px; background: #f9fafb;
  border: 1px solid #e5e7eb; border-radius: 8px;
}
.lib-field { display: flex; align-items: center; gap: 8px; min-width: 0; }
.lib-field-wide { grid-column: 1 / -1; }
.lib-field-label {
  flex: 0 0 76px; font-size: 13px; font-weight: 500; color: #374151; text-align: right;
}
.lib-select, .lib-input {
  flex: 1 1 auto; min-width: 0; box-sizing: border-box;
  padding: 6px 8px; font: inherit; font-size: 13px; color: #111827;
  border: 1px solid #d1d5db; border-radius: 6px; background: #fff;
}
.lib-select:focus, .lib-input:focus {
  outline: none; border-color: #198754; box-shadow: 0 0 0 2px rgba(25,135,84,.12);
}
.lib-select-sm { flex: 0 0 auto; padding: 4px 6px; font-size: 12px; }

/* 목록 — 가로/세로 스크롤은 이 영역 안에서만 발생 */
.lib-grid-wrap {
  flex: 1 1 auto; min-height: 120px; overflow: auto;
  border: 1px solid #e5e7eb; border-radius: 8px; background: #fff;
}
.lib-grid { width: 100%; border-collapse: separate; border-spacing: 0; font-size: 13px; }
.lib-grid th, .lib-grid td {
  border-bottom: 1px solid #f1f5f9; border-right: 1px solid #f1f5f9;
  padding: 7px 10px; text-align: left; vertical-align: top;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 380px;
}
.lib-grid th {
  position: sticky; top: 0; z-index: 2;
  background: #f3f4f6; color: #374151; font-weight: 600; white-space: nowrap;
  border-bottom: 1px solid #d1d5db;
}
.lib-grid tbody tr:hover td { background: #f8fafc; }
.lib-col-no { width: 56px; min-width: 56px; text-align: right; color: #9ca3af; }
/* 제목이 긴 컬럼이 많은 표(예: 탄소배출량)에서 헤더를 두 줄 이상으로 접어 보여준다 */
.lib-grid-wrapheader th { white-space: normal; line-height: 1.35; vertical-align: middle; }
/* 수치 컬럼은 오른쪽 정렬 */
.lib-grid td.lib-num, .lib-grid th.lib-num { text-align: right; }
.lib-empty { text-align: center; color: #9ca3af; padding: 40px 0 !important; white-space: normal !important; }

.lib-paging {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  flex: 0 0 auto; flex-wrap: wrap;
}
.lib-pagesize { display: flex; align-items: center; gap: 6px; font-size: 12px; color: #6b7280; }
.lib-pager { display: flex; align-items: center; gap: 6px; }
.lib-pageinfo { font-size: 13px; color: #374151; padding: 0 8px; }
`;
