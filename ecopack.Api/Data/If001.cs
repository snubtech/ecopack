using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 소재/물성 데이터 목록 (IF001)
/// </summary>
public partial class If001
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 소재물성기준 ID
    /// </summary>
    public string MatPrtBasId { get; set; } = null!;

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 적용소재명
    /// </summary>
    public string? AppliedMaterialNm { get; set; }

    /// <summary>
    /// 사용환경명
    /// </summary>
    public string? MatUseNm { get; set; }

    /// <summary>
    /// 포장재 구분명
    /// </summary>
    public string? MatTypeNm { get; set; }

    /// <summary>
    /// 소재의 구성명
    /// </summary>
    public string? MatFormNm { get; set; }

    /// <summary>
    /// 성능항목명
    /// </summary>
    public string? ItemNm { get; set; }

    /// <summary>
    /// 단위명
    /// </summary>
    public string? UnitNm { get; set; }

    /// <summary>
    /// 기준값 범위
    /// </summary>
    public string? AcceptableRange { get; set; }

    /// <summary>
    /// 포장차수
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 사용환경
    /// </summary>
    public string? MatUse { get; set; }

    /// <summary>
    /// 포장재 구분
    /// </summary>
    public string? MatType { get; set; }

    /// <summary>
    /// 소재의 구성
    /// </summary>
    public string? MatForm { get; set; }

    /// <summary>
    /// 성능항목
    /// </summary>
    public string? Item { get; set; }

    /// <summary>
    /// 단위
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
