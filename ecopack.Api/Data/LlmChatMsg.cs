namespace ecopack.Api.Data;

/// <summary>
/// AI채팅메시지기본 (llm_chat_msg)
/// 사용자 질문과 AI 답변이 각각 한 행이 된다.
/// </summary>
public partial class LlmChatMsg
{
    /// <summary>
    /// AI채팅메시지 고유 ID이다.
    /// </summary>
    public long ChatMsgId { get; set; }

    /// <summary>
    /// AI채팅세션 고유 ID
    /// </summary>
    public string ChatSesId { get; set; } = null!;

    /// <summary>
    /// 대표자 Master 고객 ID로, 대화의 소유자이다.
    /// </summary>
    public string RepCustId { get; set; } = null!;

    /// <summary>
    /// 세션 내 메시지 순번(1부터)
    /// </summary>
    public int ChatMsgSeq { get; set; }

    /// <summary>
    /// AI채팅 역할코드(user:사용자질문, assistant:AI답변)
    /// </summary>
    public string ChatRoleCd { get; set; } = null!;

    /// <summary>
    /// AI채팅 메시지 내용
    /// </summary>
    public string? ChatMsgCntn { get; set; }

    /// <summary>
    /// 질문 시점의 프로젝트 고유 ID
    /// </summary>
    public string? PrjId { get; set; }

    /// <summary>
    /// 질문 시점의 포장차수
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 질문 시점 중앙화면 메뉴 ID(td, doc, prjeval 등)
    /// </summary>
    public string? CurMenuId { get; set; }

    /// <summary>
    /// 질문 시점 작업상태 스냅샷 내용(JSON).
    /// 나중에 같은 대화를 다시 열었을 때 그때 무엇을 보고 답했는지 추적할 수 있다.
    /// </summary>
    public string? WrkStatSnpsCntn { get; set; }

    /// <summary>
    /// 답변에 사용한 AI 모델명
    /// </summary>
    public string? AiModelNm { get; set; }

    /// <summary>
    /// 입력 토큰 건수
    /// </summary>
    public int? InTknCnt { get; set; }

    /// <summary>
    /// 출력 토큰 건수
    /// </summary>
    public int? OutTknCnt { get; set; }

    /// <summary>
    /// 응답 소요시간 값(밀리초)
    /// </summary>
    public int? ElpsMsVal { get; set; }

    /// <summary>
    /// 오류 내용으로, 답변 생성에 실패한 경우에만 기록한다.
    /// </summary>
    public string? ErrCntn { get; set; }

    /// <summary>
    /// 최초생성일시
    /// </summary>
    public DateTime FrstCrtDtm { get; set; }
}
