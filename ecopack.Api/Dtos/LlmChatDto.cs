namespace ecopack.Api.Dtos
{
    // ═══════════════════════════════════════════════════════════════
    // 요청 DTO
    // ═══════════════════════════════════════════════════════════════

    /// <summary>대화 세션 열기(로그인 직후 화면이 호출)</summary>
    public class LlmChatOpenSessionDto
    {
        /// <summary>로그인한 고객 ID</summary>
        public string? RepCustId { get; set; }

        /// <summary>세션을 여는 시점의 프로젝트 ID(없으면 비워 둔다)</summary>
        public string? PrjId { get; set; }

        /// <summary>세션을 여는 시점의 포장차수</summary>
        public string? PackLevel { get; set; }
    }

    /// <summary>질문 전송</summary>
    public class LlmChatSendDto
    {
        /// <summary>대화 세션 ID. 비어 있으면 서버가 새 세션을 열어 준다.</summary>
        public string? ChatSesId { get; set; }

        /// <summary>로그인한 고객 ID</summary>
        public string? RepCustId { get; set; }

        /// <summary>사용자가 입력한 질문</summary>
        public string? Question { get; set; }

        /// <summary>지금 중앙 화면이 보고 있는 프로젝트 ID</summary>
        public string? PrjId { get; set; }

        /// <summary>지금 중앙 화면이 보고 있는 포장차수(1/2/3)</summary>
        public string? PackLevel { get; set; }

        /// <summary>지금 중앙 화면의 메뉴 ID. 예: project-history, start-project, prjeval, td, doc</summary>
        public string? CurMenuId { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 응답 DTO
    // ═══════════════════════════════════════════════════════════════

    /// <summary>대화 메시지 한 줄</summary>
    public class LlmChatMsgDto
    {
        public long ChatMsgId { get; set; }
        public int ChatMsgSeq { get; set; }

        /// <summary>user / assistant</summary>
        public string ChatRoleCd { get; set; } = "";
        public string? ChatMsgCntn { get; set; }
        public string? PrjId { get; set; }
        public string? PackLevel { get; set; }
        public string? CurMenuId { get; set; }
        public string? ErrCntn { get; set; }
        public DateTime FrstCrtDtm { get; set; }
    }

    /// <summary>대화 세션 요약(목록용)</summary>
    public class LlmChatSessionDto
    {
        public string ChatSesId { get; set; } = "";
        public string? ChatSesNm { get; set; }
        public string ChatSesStatCd { get; set; } = "";
        public string? PrjId { get; set; }
        public string? PackLevel { get; set; }
        public int ChatMsgCnt { get; set; }
        public DateTime? LastMsgDtm { get; set; }
        public DateTime FrstCrtDtm { get; set; }

        /// <summary>백업(아카이브)된 세션인지 여부. 목록에서 구분해 보여 준다.</summary>
        public bool Archived { get; set; }
    }

    /// <summary>세션 하나의 전체 대화</summary>
    public class LlmChatHistoryDto
    {
        public LlmChatSessionDto? Session { get; set; }
        public List<LlmChatMsgDto> Messages { get; set; } = new();
    }

    /// <summary>질문 한 번의 결과</summary>
    public class LlmChatAnswerDto
    {
        public string ChatSesId { get; set; } = "";

        /// <summary>화면에 그려 넣을 사용자 메시지(순번·시각이 채워진 상태)</summary>
        public LlmChatMsgDto? UserMessage { get; set; }

        /// <summary>화면에 그려 넣을 AI 답변</summary>
        public LlmChatMsgDto? AssistantMessage { get; set; }

        /// <summary>답변한 AI 서비스 이름</summary>
        public string? AiModelNm { get; set; }

        /// <summary>답변 생성 소요 시간(ms). AI 서버가 알려 준 값.</summary>
        public int ElpsMsVal { get; set; }

        /// <summary>
        /// AI 서버가 알려 준 부분 실패 안내(warning).
        /// 답변이 정상이면 비어 있다. 채워져 있으면 일부 구성요소 조회에 실패했다는 뜻이다.
        /// </summary>
        public string? Warning { get; set; }

        /// <summary>
        /// AI 서버가 질문을 되돌려보내, 우리 DB 값으로 직접 만든 답변인지 여부.
        /// true 면 화면에 "저장된 값으로 정리한 답변"임을 표시해 준다.
        /// </summary>
        public bool AnsweredLocally { get; set; }

        /// <summary>답변에 참고한 작업 단계 요약. 화면 하단에 근거로 보여 줄 수 있다.</summary>
        public List<string> ContextSteps { get; set; } = new();
    }

    /// <summary>백업(아카이브) 실행 결과</summary>
    public class LlmChatArchiveResultDto
    {
        /// <summary>이 시각보다 마지막 대화가 오래된 세션을 백업했다</summary>
        public DateTime CutoffDtm { get; set; }
        public int ArchivedSessions { get; set; }
        public int ArchivedMessages { get; set; }
        public List<string> Files { get; set; } = new();
        public string? ErrCntn { get; set; }
    }
}
