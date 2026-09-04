namespace ecopack.Api.Dtos
{
    /// <summary>탄소배출량 화면의 조회조건 셀렉트 목록</summary>
    public class CarbonEmissionFiltersDto
    {
        public List<CodeNameDto> PackLevels { get; set; } = new();        // 포장차수
        public List<CodeNameDto> AppliedMaterials { get; set; } = new();  // 적용소재
        public List<CodeNameDto> MatForms { get; set; } = new();          // 소재의 구성
    }

    /// <summary>탄소배출량(환경영향평가) 목록 한 행</summary>
    public class CarbonEmissionRowDto
    {
        /// <summary>내부 관리용 일련번호 (화면 미표시, 행 key 용)</summary>
        public long Idx { get; set; }

        /// <summary>환경영향평가 ID (화면 미표시)</summary>
        public string? EnvImpAssId { get; set; }

        public string? PackLevelNm { get; set; }        // 포장차수
        public string? AppliedMaterialNm { get; set; }  // 적용소재
        public string? MatFormNm { get; set; }          // 소재의 구성

        // 중량 당 탄소배출량 (kgCO2.eq/kg)
        public string? MassCo2Mat { get; set; }         // 원료
        public string? MassCo2Proc { get; set; }        // 제조
        public string? MassCo2Scrap { get; set; }       // 폐기
        public string? MassCo2Sum { get; set; }         // 합계

        // 단위당 탄소배출량 (kgCO2.eq/관리단위)
        public string? UnitCo2Mat { get; set; }         // 원료
        public string? UnitCo2Proc { get; set; }        // 제조
        public string? UnitCo2Scrap { get; set; }       // 폐기
        public string? UnitCo2Sum { get; set; }         // 합계
        public string? UnitCo2MgtVal { get; set; }      // 관리단위

        // 물리적 인자
        public string? AreaDensity { get; set; }        // 면적당 중량 (kg/m2)
        public string? Density { get; set; }            // 밀도 (kg/m3)

        public string? MatCompCon { get; set; }         // 원료물질 구성
        public DateTime? CreatedAt { get; set; }        // 수집일시
    }

    /// <summary>탄소배출량 목록 조회 결과 (페이징)</summary>
    public class CarbonEmissionPageDto
    {
        public List<CarbonEmissionRowDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
