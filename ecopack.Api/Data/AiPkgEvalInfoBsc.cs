using System;

namespace ecopack.Api.Data
{
    /// <summary>
    /// AI 패키지 모의평가 결과 정보
    /// </summary>
    public partial class AiPkgEvalInfoBsc
    {
        /// <summary>
        /// 평가 결과 고유 ID (PK)
        /// </summary>
        public long EvalResultId { get; set; }

        /// <summary>
        /// 프로젝트 헤더 ID
        /// </summary>
        public string Prjid { get; set; } = null!;

        /// <summary>
        /// 프로젝트 사용자 ID
        /// </summary>
        public string Prjuserid { get; set; } = null!;

        /// <summary>
        /// 팩레벨 (예: 1)
        /// </summary>
        public string? PackLevel { get; set; }

        /// <summary>
        /// 포장차수 명칭 (예: 판매(1차))
        /// </summary>
        public string? PackLevelNm { get; set; }

        /// <summary>
        /// 포장소재 (예: PLASTIC)
        /// </summary>
        public string? AppliedMaterial { get; set; }

        /// <summary>
        /// 에코패키징 대분류 (SAFETY, REDUCE 등)
        /// </summary>
        public string EcoPackLarType { get; set; } = null!;

        /// <summary>
        /// 에코패키징 영역명
        /// </summary>
        public string? EcoPackAreaNm { get; set; }

        /// <summary>
        /// 설문지 버전 및 헤더 ID
        /// </summary>
        public string? asmtShtHdrId { get; set; }

        /// <summary>
        /// 평가 질문 ID
        /// </summary>
        public string? AsmtQstId { get; set; }

        /// <summary>
        /// 평가 질문 보기 ID
        /// </summary>
        public string? AsmtQstItemId { get; set; }

        /// <summary>
        /// 상위 보기 ID (분기/트리 구조용)
        /// </summary>
        public string? PrtAsmtQstItemId { get; set; }

        /// <summary>
        /// 평가 질문 내용
        /// </summary>
        public string? AsmtQstNm { get; set; }

        /// <summary>
        /// 평가지 질문 보기 명칭 (예: 예, 아니오)
        /// </summary>
        public string? AsmtQstItemNm { get; set; }

        /// <summary>
        /// 배점 기준 (예: 10, PASS 등)
        /// </summary>
        public string? ScoringCriteria { get; set; }

        /// <summary>
        /// 사용자 평가 결과 점수 (예: 10, PASS, FAIL 등)
        /// </summary>
        public string? Asmtpoint { get; set; }

        /// <summary>
        /// 국가별 규제제도 분석
        /// </summary>
        public string? NatRglAls { get; set; }

        /// <summary>
        /// 디자인 추천을 위한 개선 방안
        /// </summary>
        public string? DsgnRecmImp { get; set; }

        /// <summary>
        /// 프로젝트 별 AI패키지모의평가 최초 시작일시
        /// </summary>
        public DateTime Frstevldtm { get; set; }

        /// <summary>
        /// 프로젝트 별 AI패키지모의평가 최종 적용일시
        /// </summary>
        public DateTime Lastevldtm { get; set; }
    }
}