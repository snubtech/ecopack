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
    /// AI 패키지 모의평가 결과 저장/요청 DTO
    /// </summary>
    public class AiPkgEvalSaveDto
    {
        public string PrjId { get; set; } = null!;
        public string PrjUserId { get; set; } = null!;
        public string? PackLevel { get; set; }
        public string? PackLevelNm { get; set; }
        public string? AppliedMaterial { get; set; }
        public string EcoPackLarType { get; set; } = null!;
        public string? EcoPackAreaNm { get; set; }
        public string? asmtShtHdrId { get; set; }
        public string? AsmtQstId { get; set; }
        public string? AsmtQstItemId { get; set; }
        public string? PrtAsmtQstItemId { get; set; }
        public string? AsmtQstNm { get; set; }
        public string? AsmtQstItemNm { get; set; }
        public string? ScoringCriteria { get; set; }
        public string? Asmtpoint { get; set; }
        public string? NatRglAls { get; set; }
        public string? DsgnRecmImp { get; set; }
    }
}