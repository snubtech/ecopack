namespace ecopack.Api.Support
{
    /// <summary>
    /// 우측 AI 채팅창 설정. appsettings.json 의 "LlmChat" 구역에서 읽는다.
    /// 연동 대상은 사내 생성형 AI 서버(Swagger: {BaseUrl}/docs)이다.
    /// </summary>
    public class LlmChatOptions
    {
        public const string SectionName = "LlmChat";

        // ─────────────────────────────────────────────────────────
        // 접속 정보
        // ─────────────────────────────────────────────────────────

        /// <summary>생성형 AI 서버 주소</summary>
        public string BaseUrl { get; set; } = "http://idc.openankus.org:50101";

        /// <summary>동기 질의응답 경로. 완성된 답변 1건을 받는다.</summary>
        public string ChatPath { get; set; } = "/api/chat";

        /// <summary>스트리밍 질의응답 경로. SSE(text/event-stream)로 조각을 받는다.</summary>
        public string ChatStreamPath { get; set; } = "/api/chat_stream";

        /// <summary>대화 로그에 남길 서비스 이름(어느 서버가 답했는지 구분용)</summary>
        public string ServiceNm { get; set; } = "openankus-chat";

        // ─────────────────────────────────────────────────────────
        // 인증
        //   명세서 기준 현재는 인증이 없다("None").
        //   추후 서버가 API Key 또는 Bearer Token 을 요구하면
        //   코드 수정 없이 이 설정만 바꿔 대응할 수 있게 해 둔다.
        // ─────────────────────────────────────────────────────────

        /// <summary>None(인증 없음) / ApiKey(헤더로 키 전달) / Bearer(Authorization: Bearer)</summary>
        public string AuthType { get; set; } = "None";

        /// <summary>
        /// 인증 값. AuthType 이 ApiKey 면 키, Bearer 면 토큰이다.
        /// ⚠️ 여기에 직접 적으면 Git 에 그대로 올라간다.
        ///    비워 두면 환경변수 ECOPACK_LLM_API_KEY 를 읽는다. 환경변수 사용을 권한다.
        /// </summary>
        public string? AuthValue { get; set; }

        /// <summary>AuthType 이 ApiKey 일 때 값을 실을 헤더 이름</summary>
        public string ApiKeyHeader { get; set; } = "X-API-Key";

        // ─────────────────────────────────────────────────────────
        // 답변 생성
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// 질문에 "지금까지의 작업 상태"를 함께 실어 보낼지 여부.
        /// 서버가 question 한 항목만 받으므로, 켜 두면 그 글 안에 작업 상태를 같이 적어 보낸다.
        /// 끄면 순수 질문만 보낸다(서버 RAG 답변만 받고 싶을 때).
        /// </summary>
        public bool IncludeWorkContext { get; set; } = true;

        /// <summary>질문에 실어 보낼 작업 상태 전문의 최대 길이(글자)</summary>
        public int MaxContextChars { get; set; } = 8000;

        /// <summary>질문에 함께 실어 보낼 직전 대화 개수(사용자+AI 합산)</summary>
        public int HistoryTurns { get; set; } = 6;

        /// <summary>직전 대화 한 줄당 최대 길이(글자). 길면 잘라서 싣는다.</summary>
        public int MaxHistoryCharsPerTurn { get; set; } = 500;

        /// <summary>서버 호출 제한 시간(초). 명세상 답변에 수 초~수십 초가 걸릴 수 있다.</summary>
        public int TimeoutSeconds { get; set; } = 180;

        // ─────────────────────────────────────────────────────────
        // 대화 이력 백업
        // ─────────────────────────────────────────────────────────

        /// <summary>대화 이력을 며칠 지나면 백업할지</summary>
        public int ArchiveAfterDays { get; set; } = 2;

        /// <summary>백업 JSON 파일을 떨어뜨릴 폴더(콘텐츠 루트 기준 상대경로)</summary>
        public string ArchiveDirectory { get; set; } = "App_Data/llmbackup";

        /// <summary>백업 자동 실행 주기(시간). 0 이하면 자동 실행하지 않는다.</summary>
        public int ArchiveIntervalHours { get; set; } = 6;
    }
}
