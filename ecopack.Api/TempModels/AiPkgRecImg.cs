using System;
using System.Collections.Generic;

namespace ecopack.Api.TempModels;

/// <summary>
/// AI패키지추천이미지기본
/// </summary>
public partial class AiPkgRecImg
{
    /// <summary>
    /// AI패키지모의평가ID이며 채번규칙을 갖는다.
    /// </summary>
    public string AiPkgRcmdImgBsc { get; set; } = null!;

    /// <summary>
    /// AI패키지모의평가ID이며 채번규칙을 갖는다.
    /// </summary>
    public string? AiPkgSimEvlId { get; set; }

    /// <summary>
    /// 프로젝트 고유 ID로 채번규칙을 갖는다.
    /// </summary>
    public string? PrjId { get; set; }

    /// <summary>
    /// 프로젝트 하위 포장차수별 ID이며, 채번규칙을 갖는다.
    /// </summary>
    public string? SubPrjId { get; set; }

    /// <summary>
    /// 프로젝트 포장차수를 저장하는 필드이다.
    /// </summary>
    public int? PrjfPkgSeq { get; set; }

    /// <summary>
    /// Safety 영역 점수 합계를 저장하는 필드
    /// </summary>
    public string? SftTotScr { get; set; }

    /// <summary>
    /// Reuse 영역 점수 합계를 저장하는 필드
    /// </summary>
    public int? ReuseTotScr { get; set; }

    /// <summary>
    /// Reduce 영역 점수 합계를 저장하는 필드
    /// </summary>
    public int? RedcTotScr { get; set; }

    /// <summary>
    /// Recycle 영역 점수 합계를 저장하는 필드
    /// </summary>
    public int? RcycTotScr { get; set; }

    /// <summary>
    /// Replace 영역 점수 합계를 저장하는 필드
    /// </summary>
    public int? RplcTotScr { get; set; }

    /// <summary>
    /// Innovation 영역 점수 합계를 저장하는 필드
    /// </summary>
    public int? InnoTotScr { get; set; }

    /// <summary>
    /// 모의평가 점수 합계를 저장하는 필드
    /// </summary>
    public int? SimEvlTotScr { get; set; }
}
