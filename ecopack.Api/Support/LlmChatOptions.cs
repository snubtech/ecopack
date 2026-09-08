namespace ecopack.Api.Support
{
    /// <summary>
    /// 우측 AI 채팅창 설정. appsettings.json 의 "LlmChat" 구역에서 읽는다.
    /// </summary>
    public class LlmChatOptions
    {
        public const string SectionName = "LlmChat";

        /// <summary>
        /// Anthropic API 키. 비워 두면 환경변수 ANTHROPIC_API_KEY 를 쓴다.
        /// ⚠️ 키를 appsettings.json 에 직접 적어 두면 Git 에 올라간다. 환경변수 사용을 권한다.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>사용할 모델. 기본값은 Claude Opus 5.</summary>
        public string Model { get; set; } = "claude-opus-5";

        /// <summary>한 번의 답변에 쓸 최대 출력 토큰</summary>
        public int MaxTokens { get; set; } = 4096;

        /// <summary>
        /// 생각 깊이. low / medium / high / xhigh / max.
        /// 채팅창은 응답 속도가 중요해 기본을 medium 으로 둔다.
        /// 어려운 판단이 필요하면 high 로 올리면 된다.
        /// </summary>
        public string Effort { get; set; } = "medium";

        /// <summary>LLM 에게 함께 넘길 직전 대화 개수(사용자+AI 합산)</summary>
        public int HistoryTurns { get; set; } = 20;

        /// <summary>대화 이력을 며칠 지나면 백업할지</summary>
        public int ArchiveAfterDays { get; set; } = 2;

        /// <summary>백업 JSON 파일을 떨어뜨릴 폴더(콘텐츠 루트 기준 상대경로)</summary>
        public string ArchiveDirectory { get; set; } = "App_Data/llmbackup";

        /// <summary>백업 자동 실행 주기(시간). 0 이하면 자동 실행하지 않는다.</summary>
        public int ArchiveIntervalHours { get; set; } = 6;

        /// <summary>API 호출 제한 시간(초)</summary>
        public int TimeoutSeconds { get; set; } = 180;
    }
}
