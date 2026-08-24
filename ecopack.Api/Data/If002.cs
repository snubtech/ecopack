using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 패키징디자인템플릿정보 목록 (IF002)
/// </summary>
public partial class If002
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 패키징디자인템플릿 ID
    /// </summary>
    public string PackDsgnTplId { get; set; } = null!;

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 포장재구분명
    /// </summary>
    public string? MatTypeNm { get; set; }

    /// <summary>
    /// 제목
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 디자인유형명
    /// </summary>
    public string? DsgnTypeNm { get; set; }

    /// <summary>
    /// 디자인유형코드값
    /// </summary>
    public string? DsgnTypeCdVal { get; set; }

    /// <summary>
    /// 제품의설명
    /// </summary>
    public string? DsgnExpCon { get; set; }

    /// <summary>
    /// 적용소재명
    /// </summary>
    public string? AppliedMaterialNm { get; set; }

    /// <summary>
    /// 디자인특징
    /// </summary>
    public string? DsgnFeatDscr { get; set; }

    /// <summary>
    /// 제품의사용목적
    /// </summary>
    public string? OperDscr { get; set; }

    /// <summary>
    /// 포장차수
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 포장재구분
    /// </summary>
    public string? MatType { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
