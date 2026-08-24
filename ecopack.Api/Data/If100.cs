using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 3차 패키징데이터셋 (IF100)
/// </summary>
public partial class If100
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 프로젝트패키징데이터셋ID
    /// </summary>
    public string PackDsetId { get; set; } = null!;

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 제품군명
    /// </summary>
    public string? PrdtPackTypeNm { get; set; }

    /// <summary>
    /// 포장상자형태명
    /// </summary>
    public string? PackagingBoxTypeNm { get; set; }

    /// <summary>
    /// 수출국가명
    /// </summary>
    public string? ExportCountryNm { get; set; }

    /// <summary>
    /// 제품치수-길이 mm
    /// </summary>
    public string? PrdtLength { get; set; }

    /// <summary>
    /// 제품치수-너비 mm
    /// </summary>
    public string? PrdtWidth { get; set; }

    /// <summary>
    /// 제품치수-높이 mm
    /// </summary>
    public string? PrdtHeight { get; set; }

    /// <summary>
    /// 완충제소재명
    /// </summary>
    public string? CushMatTypeNm { get; set; }

    /// <summary>
    /// 제품중량
    /// </summary>
    public string? PrdtWeight { get; set; }

    /// <summary>
    /// 수송포장중량
    /// </summary>
    public string? ShipPackWeight { get; set; }

    /// <summary>
    /// 요구포장차수
    /// </summary>
    public string? RqstPackCnt { get; set; }

    /// <summary>
    /// 요구빈공간비율%
    /// </summary>
    public string? ReqEmptyRatio { get; set; }

    /// <summary>
    /// 최소포장체적cm3
    /// </summary>
    public string? MinPackVol { get; set; }

    /// <summary>
    /// 최소물류환경_적재N
    /// </summary>
    public string? MinLoadCond { get; set; }

    /// <summary>
    /// 최소물류환경_낙하mm
    /// </summary>
    public string? MinDropCond { get; set; }

    /// <summary>
    /// 최소물류환경_진동분석
    /// </summary>
    public string? MinVibCond { get; set; }

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
    /// 패키징소재생산공정ID
    /// </summary>
    public string? PackMmftProcId { get; set; }

    /// <summary>
    /// 환경영향평가ID
    /// </summary>
    public string? EnvImpAssId { get; set; }

    /// <summary>
    /// 패키징디자인템플릿ID
    /// </summary>
    public string? PackDsgnTplId { get; set; }

    /// <summary>
    /// 포장차수
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 제품군
    /// </summary>
    public string? PrdtPackType { get; set; }

    /// <summary>
    /// 포장상자형태
    /// </summary>
    public string? PackagingBoxType { get; set; }

    /// <summary>
    /// 수출국가
    /// </summary>
    public string? ExportCountry { get; set; }

    /// <summary>
    /// 완충제소재
    /// </summary>
    public string? CushMatType { get; set; }

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
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
