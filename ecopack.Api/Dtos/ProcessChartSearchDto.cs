/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - ProcessChart 조회용 자료 묶음 (공정도)
 * ==============================================================================
 * 
 * 1. 구성
 *    - ProcessChart 조회용 자료 묶음 화면이 서버와 주고받는 자료입니다. 대상 테이블은 if003 / if003a 입니다.
 *    - 조회조건(Filters), 목록 한 행(Row), 목록+건수(Page) 로 나눠 담습니다.
 * 
 * 2. 알아둘 점
 *    - ProcessChartDetailDto : 공정도 이미지. memoImg 는 HTML 이라 그대로 넘기지 않고
 *      이미지 주소만 뽑아 MemoImageUris 에 담습니다(HTML 주입 방지).
 *    - FileExistYn 은 DB 컬럼이 아니라 if003a 에 파일이 있는지로 계산한 값입니다.
 * ==============================================================================
 */
namespace ecopack.Api.Dtos
{
    /// <summary>공정도 화면의 조회조건 셀렉트 목록</summary>
    public class ProcessChartFiltersDto
    {
        public List<CodeNameDto> AppliedMaterials { get; set; } = new();  // 적용소재
    }

    /// <summary>공정도 목록 한 행</summary>
    public class ProcessChartRowDto
    {
        /// <summary>내부 관리용 일련번호 (화면 미표시, 행 key 용)</summary>
        public long Idx { get; set; }

        /// <summary>패키징소재생산공정도 ID (상세 조회 키)</summary>
        public string? PackMmftProcId { get; set; }

        public string? Subject { get; set; }            // 탬플릿명
        public string? AppliedMaterialNm { get; set; }  // 적용소재
        public string? MatTypeNm { get; set; }          // 포장재 구분
        public string? MatCompNm { get; set; }          // 소재재질의 구성
        public string? MatFormNm { get; set; }          // 소재의 구성

        /// <summary>파일존재여부 — if003a 에 파일이 있으면 'Y'</summary>
        public string FileExistYn { get; set; } = "N";

        public DateTime? CreatedAt { get; set; }        // 수집일시
    }

    /// <summary>공정도 목록 조회 결과 (페이징)</summary>
    public class ProcessChartPageDto
    {
        public List<ProcessChartRowDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// 공정도 상세 — 공정도 이미지.
    /// memoImg 는 이미지가 포함된 HTML 이므로 화면에 그대로 주입하지 않고
    /// 서버에서 data URI 만 뽑아 전달한다.
    /// </summary>
    public class ProcessChartDetailDto
    {
        public string? PackMmftProcId { get; set; }
        public string? Subject { get; set; }
        public string? AppliedMaterialNm { get; set; }
        public string? MatTypeNm { get; set; }
        public string? MatCompNm { get; set; }
        public string? MatFormNm { get; set; }

        /// <summary>첨부 파일명 (확장자 포함)</summary>
        public string? FileNm { get; set; }

        /// <summary>첨부 파일 이미지 data URI (fileData 를 확장자에 맞는 MIME 으로 감쌈)</summary>
        public string? FileImageUri { get; set; }

        /// <summary>설명이미지(memoImg) 안에서 추출한 이미지 data URI 목록</summary>
        public List<string> MemoImageUris { get; set; } = new();
    }
}
