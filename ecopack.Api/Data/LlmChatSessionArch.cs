namespace ecopack.Api.Data;

/// <summary>
/// AI채팅세션백업 (llm_chat_session_arch)
/// 최종 대화가 2일보다 오래된 세션이 여기로 옮겨진다.
/// 원본 llm_chat_session 은 이 시점에 지워져 운영 테이블이 가벼워진다.
/// </summary>
public partial class LlmChatSessionArch
{
    /// <summary>AI채팅세션 고유 ID</summary>
    public string ChatSesId { get; set; } = null!;

    /// <summary>대표자 Master 고객 ID로, 대화의 소유자이다.</summary>
    public string RepCustId { get; set; } = null!;

    /// <summary>프로젝트 고유 ID</summary>
    public string? PrjId { get; set; }

    /// <summary>포장차수(1:제품포장, 2:운송포장, 3:수송포장)</summary>
    public string? PackLevel { get; set; }

    /// <summary>AI채팅세션명</summary>
    public string? ChatSesNm { get; set; }

    /// <summary>AI채팅세션 상태코드(백업본은 ARCHIVED)</summary>
    public string ChatSesStatCd { get; set; } = "ARCHIVED";

    /// <summary>AI채팅 누적 메시지 건수(질문+답변)</summary>
    public int ChatMsgCnt { get; set; }

    /// <summary>세션을 연 로그인일시</summary>
    public DateTime? LgnDtm { get; set; }

    /// <summary>최종 대화일시</summary>
    public DateTime? LastMsgDtm { get; set; }

    /// <summary>원본 최초생성일시</summary>
    public DateTime? FrstCrtDtm { get; set; }

    /// <summary>원본 최종수정일시</summary>
    public DateTime? LastUpdDtm { get; set; }

    /// <summary>백업처리일시</summary>
    public DateTime ArchDtm { get; set; }

    /// <summary>백업 JSON 파일 경로</summary>
    public string? ArchFileUrl { get; set; }
}
