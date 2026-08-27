using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

public partial class ProjectDetail
{
    /// <summary>
    /// 프로젝트 고유 ID
    /// </summary>
    public string PrjId { get; set; } = null!;

    /// <summary>
    /// 포장차수
    /// </summary>
    public string PackLevel { get; set; } = null!;

    public string? PrjRevNo { get; set; }

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 적용소재명
    /// </summary>
    public string? AppliedMaterialNm { get; set; }

    /// <summary>
    /// 사용환경
    /// </summary>
    public string? MatUse { get; set; }

    /// <summary>
    /// 사용환경명
    /// </summary>
    public string? MatUseNm { get; set; }

    /// <summary>
    /// 포장재 구분
    /// </summary>
    public string? MatType { get; set; }

    /// <summary>
    /// 포장재 구분명
    /// </summary>
    public string? MatTypeNm { get; set; }

    /// <summary>
    /// 소재의 구성
    /// </summary>
    public string? MatForm { get; set; }

    /// <summary>
    /// 소재의 구성명
    /// </summary>
    public string? MatFormNm { get; set; }

    /// <summary>
    /// 패키징디자인템플릿 ID
    /// </summary>
    public string? PackDsgnTplId { get; set; }

    /// <summary>
    /// 프로젝트 진행단계
    /// </summary>
    public string? Projstatus { get; set; }

    public string? PrdExpCntry { get; set; }

    public string? PrdExpCntryNm { get; set; }

    /// <summary>
    /// 저장수정시간
    /// </summary>
    public DateTime? Updatedate { get; set; }
}
