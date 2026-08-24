using System;
using System.Collections.Generic;

namespace ecopack.Api.TempModels;

/// <summary>
/// 3차포장기본
/// </summary>
public partial class TertiaryPkg
{
    /// <summary>
    /// 프로젝트 하위 포장차수별 ID이며, 채번규칙을 갖는다.
    /// </summary>
    public string SubPrjId { get; set; } = null!;

    /// <summary>
    /// 프로젝트 고유 ID로 채번규칙을 갖는다.
    /// </summary>
    public string? PrjId { get; set; }

    /// <summary>
    /// 리스트로 관리되는 3차포장차수의 적용소재이다.
    /// </summary>
    public string? ApplMatNm { get; set; }

    /// <summary>
    /// 리스트로 관리되는 3차포장차수의 사용환경내용이다.
    /// </summary>
    public string? UseEnvCntn { get; set; }

    /// <summary>
    /// 리스트로 관리되는 3차포장차수의 포장재종류명이다.
    /// </summary>
    public string? PkgMatTypeNm { get; set; }
}
