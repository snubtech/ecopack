namespace ecopack.Api.Data;

/// <summary>
/// AI채팅세션기본 (llm_chat_session)
/// 로그인 한 번이 세션 하나에 대응한다.
/// 로그아웃 전까지 같은 세션에 대화를 이어 쌓고, 로그아웃하면 CLOSED 로 닫는다.
/// </summary>
public partial class LlmChatSession
{
    /// <summary>
    /// AI채팅세션 고유 ID로 채번규칙을 갖는다. (S + yyyyMMddHHmmssfff)
    /// </summary>
    public string ChatSesId { get; set; } = null!;

    /// <summary>
    /// 대표자 Master 고객 ID로, 대화의 소유자이다. 이 값이 일치할 때만 조회를 허용한다.
    /// </summary>
    public string RepCustId { get; set; } = null!;

    /// <summary>
    /// 프로젝트 고유 ID로, 세션을 시작한 시점의 프로젝트이다.
    /// </summary>
    public string? PrjId { get; set; }

    /// <summary>
    /// 포장차수(1:제품포장, 2:운송포장, 3:수송포장)
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// AI채팅세션명으로, 첫 질문에서 만든다.
    /// </summary>
    public string? ChatSesNm { get; set; }

    /// <summary>
    /// AI채팅세션 상태코드(ACTIVE:로그인중, CLOSED:로그아웃, ARCHIVED:백업완료)
    /// </summary>
    public string ChatSesStatCd { get; set; } = "ACTIVE";

    /// <summary>
    /// AI채팅 누적 메시지 건수(질문+답변)
    /// </summary>
    public int ChatMsgCnt { get; set; }

    /// <summary>
    /// 세션을 연 로그인일시
    /// </summary>
    public DateTime? LgnDtm { get; set; }

    /// <summary>
    /// 최종 대화일시로, 대화이력 백업 판정의 기준이 된다.
    /// </summary>
    public DateTime? LastMsgDtm { get; set; }

    /// <summary>
    /// 최초생성일시
    /// </summary>
    public DateTime FrstCrtDtm { get; set; }

    /// <summary>
    /// 최종수정일시
    /// </summary>
    public DateTime LastUpdDtm { get; set; }
}
