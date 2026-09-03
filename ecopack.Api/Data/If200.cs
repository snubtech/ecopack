using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecopack.Api.Data;

/// <summary>
/// 평가지 정보 통합 (IF200)
/// </summary>
[Table("if200")]
public partial class If200
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    [Key]
    [Column("idx")]
    public long Idx { get; set; }

    /// <summary>
    /// 평가지헤더ID
    /// </summary>
    [Column("asmtShtHdrId")]
    public string AsmtShtHdrId { get; set; } = null!;

    /// <summary>
    /// 레벨유형명
    /// </summary>
    [Column("levelTypeNm")]
    public string? LevelTypeNm { get; set; }

    /// <summary>
    /// 버전설명
    /// </summary>
    [Column("versionDesc")]
    public string? VersionDesc { get; set; }

    /// <summary>
    /// 헤더 비고
    /// </summary>
    [Column("hdrMemo")]
    public string? HdrMemo { get; set; }

    /// <summary>
    /// 포장차수명
    /// </summary>
    [Column("packLevelNm")]
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 레벨유형
    /// </summary>
    [Column("levelType")]
    public string? LevelType { get; set; }

    /// <summary>
    /// 포장차수
    /// </summary>
    [Column("packLevel")]
    public string? PackLevel { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    [Column("appliedMaterial")]
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 평가지질문ID
    /// </summary>
    [Column("asmtQstId")]
    public string? AsmtQstId { get; set; }

    /// <summary>
    /// 에코패키징대분류명
    /// </summary>
    [Column("ecoPackLarTypeNm")]
    public string? EcoPackLarTypeNm { get; set; }

    /// <summary>
    /// 에코패키징영역명
    /// </summary>
    [Column("ecoPackAreaNm")]
    public string? EcoPackAreaNm { get; set; }

    /// <summary>
    /// 평가질문내용
    /// </summary>
    [Column("asmtQstNm")]
    public string? AsmtQstNm { get; set; }

    /// <summary>
    /// 배점유형명
    /// </summary>
    [Column("scoringCriteriaTypeNm")]
    public string? ScoringCriteriaTypeNm { get; set; }

    /// <summary>
    /// 국가별규제제도분석
    /// </summary>
    [Column("natRglAls")]
    public string? NatRglAls { get; set; }

    /// <summary>
    /// 디자인추천을위한개선방안
    /// </summary>
    [Column("dsgnRecmImp")]
    public string? DsgnRecmImp { get; set; }

    /// <summary>
    /// 다음평가지질문ID
    /// </summary>
    [Column("nextAsmtQstId")]
    public string? NextAsmtQstId { get; set; }

    /// <summary>
    /// 부모평가지질문보기ID
    /// </summary>
    [Column("prtAsmtQstItemId")]
    public string? PrtAsmtQstItemId { get; set; }

    /// <summary>
    /// 루트여부
    /// </summary>
    [Column("rootYn")]
    public string? RootYn { get; set; }

    /// <summary>
    /// 에코패키징대분류
    /// </summary>
    [Column("ecoPackLarType")]
    public string? EcoPackLarType { get; set; }

    /// <summary>
    /// 에코패키징영역
    /// </summary>
    [Column("ecoPackArea")]
    public string? EcoPackArea { get; set; }

    /// <summary>
    /// 배점유형
    /// </summary>
    [Column("scoringCriteriaType")]
    public string? ScoringCriteriaType { get; set; }

    /// <summary>
    /// 평가지질문보기ID
    /// </summary>
    [Column("asmtQstItemId")]
    public string? AsmtQstItemId { get; set; }

    /// <summary>
    /// 평가지질문보기
    /// </summary>
    [Column("asmtQstItemNm")]
    public string? AsmtQstItemNm { get; set; }

    /// <summary>
    /// 배점
    /// </summary>
    [Column("scoringCriteria")]
    public string? ScoringCriteria { get; set; }

    /// <summary>
    /// 출시불가여부
    /// </summary>
    [Column("notReleaseYn")]
    public string? NotReleaseYn { get; set; }

    /// <summary>
    /// 항목 비고
    /// </summary>
    [Column("itemMemo")]
    public string? ItemMemo { get; set; }

    /// <summary>
    /// 디자인추천을위한개선방안여부
    /// </summary>
    [Column("dsgnRecmImpYn")]
    public string? DsgnRecmImpYn { get; set; }

    /// <summary>
    /// 화면표시순번
    /// </summary>
    [Column("dspSeq")]
    public int? DspSeq { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    [Column("createdAt")]
    public DateTime? CreatedAt { get; set; }
}