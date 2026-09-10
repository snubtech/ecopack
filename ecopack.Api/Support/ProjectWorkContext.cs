namespace ecopack.Api.Support
{
    /// <summary>
    /// 포장차수 한 단계의 작업 내용. 화면에서 고른 값들을 그대로 담는다.
    /// AI 서버가 답을 만들지 못했을 때 우리가 직접 답을 조립하는 재료가 된다.
    /// </summary>
    public class ProjectLevelFacts
    {
        /// <summary>포장차수(1/2/3)</summary>
        public string PackLevel { get; set; } = "";

        /// <summary>포장차수명. 예: 판매(1차)</summary>
        public string? PackLevelNm { get; set; }

        /// <summary>기본사항이 저장된 차수인지</summary>
        public bool DetailSaved { get; set; }

        public string? AppliedMaterial { get; set; }
        public string? MatUse { get; set; }
        public string? MatType { get; set; }
        public string? MatForm { get; set; }
        public string? PackDsgnTplId { get; set; }
        public string? PrdExpCntry { get; set; }
        public string? Projstatus { get; set; }
        public DateTime? Updatedate { get; set; }

        /// <summary>모의평가 총점. 진행하지 않았으면 null</summary>
        public int? EvalScore { get; set; }

        /// <summary>모의평가 응답 문항 수</summary>
        public int EvalItemCnt { get; set; }

        public DateTime? EvalLastDtm { get; set; }

        /// <summary>기술문서(TD) 작성 여부</summary>
        public bool TdWritten { get; set; }
        public string? TdDocNo { get; set; }
        public string? TdLastWrt { get; set; }

        /// <summary>적합성 선언서(DOC) 작성 여부</summary>
        public bool DocWritten { get; set; }
        public string? DocNo { get; set; }
        public string? DocLastWrt { get; set; }
    }

    /// <summary>
    /// 프로젝트 한 건의 "전 과정 작업 상태"를 모아 담는 그릇.
    ///
    /// - Text   : AI 서버에 함께 실어 보낼 줄글
    /// - Steps  : 화면에 근거로 보여 줄 진행 단계 요약
    /// - Levels : 차수별 값. AI 서버가 답을 만들지 못했을 때 직접 답을 조립하는 재료
    /// </summary>
    public class ProjectWorkContext
    {
        /// <summary>AI 서버에 붙여 보낼 작업 상태 전문</summary>
        public string Text { get; set; } = "";

        /// <summary>진행 단계 요약. 예: "① 프로젝트 생성 — 완료 (2026-09-01, 친환경 박스)"</summary>
        public List<string> Steps { get; set; } = new();

        /// <summary>대화 로그에 남길 스냅샷(JSON 직렬화 대상)</summary>
        public object? Snapshot { get; set; }

        // ── 아래는 직접 답을 조립할 때 쓰는 재료 ──────────────────

        /// <summary>프로젝트를 찾았는지. false 면 아래 값들은 비어 있다.</summary>
        public bool HasProject { get; set; }

        public string? PrjId { get; set; }
        public string? PrjNm { get; set; }
        public DateOnly? PrjFcrtDt { get; set; }

        /// <summary>지금 화면이 보고 있는 포장차수</summary>
        public string? CurPackLevel { get; set; }

        /// <summary>수출 대상국 목록</summary>
        public List<string> ExportCountries { get; set; } = new();

        /// <summary>차수별 작업 내용. 데이터가 있는 차수만 담긴다.</summary>
        public List<ProjectLevelFacts> Levels { get; set; } = new();

        /// <summary>프로젝트를 고르지 않았을 때, 이 회원이 가진 프로젝트 수</summary>
        public int ProjectCount { get; set; }
    }
}
