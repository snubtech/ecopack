using ecopack.Api.Data; // 💡 AiPkgEvalInfoBsc 엔티티가 있는 네임스페이스 추가
namespace ecopack.Api.Dtos
{
    /// <summary>
    /// 최신 평가지 문항 조회용 DTO
    /// </summary>
    public class If200EvalQueryDto
    {
        public string? PackLevel { get; set; }
        public string? PackLevelNm { get; set; }
        public string? AppliedMaterial { get; set; }
        public string EcoPackLarType { get; set; } = null!;
        public string? EcoPackArea { get; set; }
        public string? EcoPackAreaNm { get; set; }
        public string? AsmtQstNm { get; set; }
        public string? AsmtQstItemNm { get; set; }
        public long Idx { get; set; }
        public string? asmtShtHdrId { get; set; }
        public string? AsmtQstId { get; set; }
        public string? AsmtQstItemId { get; set; }
        public string? PrtAsmtQstItemId { get; set; }
        public string? NextAsmtQstId { get; set; }
        public string? ScoringCriteria { get; set; }
        public string? NatRglAls { get; set; }
        public string? DsgnRecmImp { get; set; }
    }

    /// <summary>
    /// AI 패키지 모의평가 결과 저장 요청 DTO
    /// </summary>
    public class AiPkgEvalSaveDto
    {
        public string prjid { get; set; } = null!;
        public string prjuserid { get; set; } = null!;
        public string? packLevel { get; set; }
        public string? packLevelnm { get; set; }
        public string? appliedMaterial { get; set; }
        public string ecoPackLarType { get; set; } = null!;
        public string? ecoPackAreaNm { get; set; }
        public string? asmtShtHdrId { get; set; }
        public string? asmtQstId { get; set; }
        public string? asmtQstItemId { get; set; }
        public string? nextAsmtQstId { get; set; } // 💡 다음 질문 ID 저장 필드 추가
        public string? prtAsmtQstItemId { get; set; }
        public string? asmtQstNm { get; set; }
        public string? asmtQstItemNm { get; set; }
        public string? scoringCriteria { get; set; }
        public string? asmtpoint { get; set; }
        public string? natRglAls { get; set; }
        public string? dsgn_recm_imp { get; set; }
    }
    /// <summary>
    /// 모의평가 종합점수 조회 DTO
    /// </summary>
    public class AiPkgEvalSummaryResponseDto
    {
        public int TotalScore { get; set; } // 획득 점수 합계 (예: 89)
        public int MaxScore { get; set; }   // 만점 기준 (예: 100)
        public List<AiPkgEvalInfoBsc> SavedItems { get; set; } = new(); // 우측 상세 규제 카드용 데이터 리스트
    }
}