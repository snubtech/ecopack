using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 패키징소재생산공정도정보 목록 (IF003)
/// </summary>
public partial class If003
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 패키징소재생산공정도 ID
    /// </summary>
    public string PackMmftProcId { get; set; } = null!;

    /// <summary>
    /// 적용소재명
    /// </summary>
    public string? AppliedMaterialNm { get; set; }

    /// <summary>
    /// 포장재구분명
    /// </summary>
    public string? MatTypeNm { get; set; }

    /// <summary>
    /// 디자인소재명
    /// </summary>
    public string? MatCompNm { get; set; }

    /// <summary>
    /// 소재의구성명
    /// </summary>
    public string? MatFormNm { get; set; }

    /// <summary>
    /// 제목
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 포장재구분
    /// </summary>
    public string? MatType { get; set; }

    /// <summary>
    /// 디자인소재
    /// </summary>
    public string? MatComp { get; set; }

    /// <summary>
    /// 소재의구성
    /// </summary>
    public string? MatForm { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
