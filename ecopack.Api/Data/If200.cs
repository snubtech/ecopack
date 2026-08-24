using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 평가지 정보 통합 (IF200)
/// </summary>
public partial class If200
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 평가지헤더ID
    /// </summary>
    public string AsmtShtHdrId { get; set; } = null!;

    /// <summary>
    /// 레벨유형명
    /// </summary>
    public string? LevelTypeNm { get; set; }

    /// <summary>
    /// 버전설명
    /// </summary>
    public string? VersionDesc { get; set; }

    /// <summary>
    /// 헤더 비고
    /// </summary>
    public string? HdrMemo { get; set; }

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 레벨유형
    /// </summary>
    public string? LevelType { get; set; }

    /// <summary>
    /// 포장차수
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 평가지질문ID
    /// </summary>
    public string? AsmtQstId { get; set; }

    /// <summary>
    /// 에코패키징대분류명
    /// </summary>
    public string? EcoPackLarTypeNm { get; set; }

    /// <summary>
    /// 에코패키징영역명
    /// </summary>
    public string? EcoPackAreaNm { get; set; }

    /// <summary>
    /// 평가질문내용
    /// </summary>
    public string? AsmtQstNm { get; set; }

    /// <summary>
    /// 배점유형명
    /// </summary>
    public string? ScoringCriteriaTypeNm { get; set; }

    /// <summary>
    /// 국가별규제제도분석
    /// </summary>
    public string? NatRglAls { get; set; }

    /// <summary>
    /// 디자인추천을위한개선방안
    /// </summary>
    public string? DsgnRecmImp { get; set; }

    /// <summary>
    /// 다음평가지질문ID
    /// </summary>
    public string? NextAsmtQstId { get; set; }

    /// <summary>
    /// 부모평가지질문보기ID
    /// </summary>
    public string? PrtAsmtQstItemId { get; set; }

    /// <summary>
    /// 루트여부
    /// </summary>
    public string? RootYn { get; set; }

    /// <summary>
    /// 에코패키징대분류
    /// </summary>
    public string? EcoPackLarType { get; set; }

    /// <summary>
    /// 에코패키징영역
    /// </summary>
    public string? EcoPackArea { get; set; }

    /// <summary>
    /// 배점유형
    /// </summary>
    public string? ScoringCriteriaType { get; set; }

    /// <summary>
    /// 평가지질문보기ID
    /// </summary>
    public string? AsmtQstItemId { get; set; }

    /// <summary>
    /// 평가지질문보기
    /// </summary>
    public string? AsmtQstItemNm { get; set; }

    /// <summary>
    /// 배점
    /// </summary>
    public string? ScoringCriteria { get; set; }

    /// <summary>
    /// 출시불가여부
    /// </summary>
    public string? NotReleaseYn { get; set; }

    /// <summary>
    /// 항목 비고
    /// </summary>
    public string? ItemMemo { get; set; }

    /// <summary>
    /// 디자인추천을위한개선방안여부
    /// </summary>
    public string? DsgnRecmImpYn { get; set; }

    /// <summary>
    /// 화면표시순번
    /// </summary>
    public int? DspSeq { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
