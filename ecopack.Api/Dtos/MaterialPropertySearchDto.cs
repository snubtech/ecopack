namespace ecopack.Api.Dtos
{
    /// <summary>조회조건 셀렉트 한 항목 (코드 + 표시명)</summary>
    public class CodeNameDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    /// <summary>
    /// 소재물성 화면의 조회조건 셀렉트 목록.
    /// if001 을 코드/코드명으로 GroupBy 해서 한 번에 내려준다.
    /// </summary>
    public class MaterialPropertyFiltersDto
    {
        public List<CodeNameDto> PackLevels { get; set; } = new();        // 소재물성(포장차수)
        public List<CodeNameDto> AppliedMaterials { get; set; } = new();  // 적용소재
        public List<CodeNameDto> MatUses { get; set; } = new();           // 사용환경
        public List<CodeNameDto> MatTypes { get; set; } = new();          // 포장재 구분
        public List<CodeNameDto> MatForms { get; set; } = new();          // 소재의 구성
        public List<CodeNameDto> Items { get; set; } = new();             // 성능항목
        public List<CodeNameDto> Units { get; set; } = new();             // 단위
    }

    /// <summary>소재물성 목록 한 행</summary>
    public class MaterialPropertyRowDto
    {
        /// <summary>내부 관리용 일련번호 (화면 미표시, 행 key 용)</summary>
        public long Idx { get; set; }

        /// <summary>소재물성기준 ID (화면 미표시, 상세 조회 키)</summary>
        public string? MatPrtBasId { get; set; }

        public string? PackLevelNm { get; set; }        // 포장차수
        public string? AppliedMaterialNm { get; set; }  // 적용소재
        public string? MatUseNm { get; set; }           // 사용환경
        public string? MatTypeNm { get; set; }          // 포장재 구분
        public string? MatFormNm { get; set; }          // 소재의 구성
        public string? ItemNm { get; set; }             // 성능항목
        public string? UnitNm { get; set; }             // 단위
        public string? AcceptableRange { get; set; }    // 기준값 범위
        public DateTime? CreatedAt { get; set; }        // 수집일시
    }

    /// <summary>소재물성 목록 조회 결과 (페이징)</summary>
    public class MaterialPropertyPageDto
    {
        public List<MaterialPropertyRowDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
