/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - NationRegulation 조회용 자료 묶음 (환경규제)
 * ==============================================================================
 * 
 * 1. 구성
 *    - NationRegulation 조회용 자료 묶음 화면이 서버와 주고받는 자료입니다. 대상 테이블은 if004 입니다.
 *    - 조회조건(Filters), 목록 한 행(Row), 목록+건수(Page) 로 나눠 담습니다.
 * 
 * 2. 알아둘 점
 *    - 규제내용·비고·원문처럼 긴 글이 담기는 항목이 있어 화면에서는 말줄임 처리합니다.
 * ==============================================================================
 */
namespace ecopack.Api.Dtos
{
    /// <summary>환경규제 화면의 조회조건 셀렉트 목록</summary>
    public class NationRegulationFiltersDto
    {
        public List<CodeNameDto> PackLevels { get; set; } = new();        // 포장차수
        public List<CodeNameDto> AppliedMaterials { get; set; } = new();  // 적용소재
        public List<CodeNameDto> Countries { get; set; } = new();         // 국가
    }

    /// <summary>환경규제(국가규제정보) 목록 한 행</summary>
    public class NationRegulationRowDto
    {
        /// <summary>내부 관리용 일련번호 (화면 미표시, 행 key 용)</summary>
        public long Idx { get; set; }

        /// <summary>국가규제 ID (화면 미표시)</summary>
        public string? NatRegId { get; set; }

        public string? PackLevelNm { get; set; }        // 포장차수
        public string? AppliedMaterialNm { get; set; }  // 적용소재
        public string? CountryCodeNm { get; set; }      // 국가
        public string? RelatedReg { get; set; }         // 관련규정
        public string? RegItem { get; set; }            // 규제항목
        public string? DtlCont { get; set; }            // 규제내용
        public string? UnitNm { get; set; }             // 단위
        public string? MinCont { get; set; }            // 기준치
        public string? MinOperatorNm { get; set; }      // 범위
        public string? PrepDeadline { get; set; }       // 적용시작일
        public string? PrepDeadlineEnd { get; set; }    // 적용종료일
        public string? DecisionOut { get; set; }        // 기술문서
        public string? IsRequired { get; set; }         // 필수여부
        public string? Memo { get; set; }               // 비고
        public string? OriginalText { get; set; }       // 원문
        public DateTime? CreatedAt { get; set; }        // 수집일시
    }

    /// <summary>환경규제 목록 조회 결과 (페이징)</summary>
    public class NationRegulationPageDto
    {
        public List<NationRegulationRowDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
