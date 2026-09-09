namespace ecopack.Api.Support
{
    /// <summary>
    /// 프로젝트 한 건의 "전 과정 작업 상태"를 모아 담는 그릇.
    /// LLM 에게는 Text 를 그대로 넘기고, 화면에는 Steps 를 근거로 보여 준다.
    /// </summary>
    public class ProjectWorkContext
    {
        /// <summary>LLM 시스템 프롬프트에 붙여 넣을 작업 상태 전문</summary>
        public string Text { get; set; } = "";

        /// <summary>진행 단계 요약. 예: "① 프로젝트 생성 — 완료(2026-09-01)"</summary>
        public List<string> Steps { get; set; } = new();

        /// <summary>대화 로그에 남길 스냅샷(JSON 직렬화 대상)</summary>
        public object? Snapshot { get; set; }
    }
}
