using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 국가규제정보 목록 조회 (IF004)
/// </summary>
public partial class If004
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 국가규제ID
    /// </summary>
    public string NatRegId { get; set; } = null!;

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 적용소재명
    /// </summary>
    public string? AppliedMaterialNm { get; set; }

    /// <summary>
    /// 국가명
    /// </summary>
    public string? CountryCodeNm { get; set; }

    /// <summary>
    /// 관련규정
    /// </summary>
    public string? RelatedReg { get; set; }

    /// <summary>
    /// 규제항목
    /// </summary>
    public string? RegItem { get; set; }

    /// <summary>
    /// 규제내용
    /// </summary>
    public string? DtlCont { get; set; }

    /// <summary>
    /// 단위명
    /// </summary>
    public string? UnitNm { get; set; }

    /// <summary>
    /// 최소함량
    /// </summary>
    public string? MinCont { get; set; }

    /// <summary>
    /// 최소함량구간명
    /// </summary>
    public string? MinOperatorNm { get; set; }

    /// <summary>
    /// 최대함량
    /// </summary>
    public string? MaxCont { get; set; }

    /// <summary>
    /// 최대함량구간명
    /// </summary>
    public string? MaxOperatorNm { get; set; }

    /// <summary>
    /// 적용시작일
    /// </summary>
    public string? PrepDeadline { get; set; }

    /// <summary>
    /// 적용종료일
    /// </summary>
    public string? PrepDeadlineEnd { get; set; }

    /// <summary>
    /// 기술문서
    /// </summary>
    public string? DecisionOut { get; set; }

    /// <summary>
    /// 필수여부
    /// </summary>
    public string? IsRequired { get; set; }

    /// <summary>
    /// 비고
    /// </summary>
    public string? Memo { get; set; }

    /// <summary>
    /// 원문
    /// </summary>
    public string? OriginalText { get; set; }

    /// <summary>
    /// 포장차수
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 적용소재
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 국가
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// 단위
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 최소함량구간
    /// </summary>
    public string? MinOperator { get; set; }

    /// <summary>
    /// 최대함량구간
    /// </summary>
    public string? MaxOperator { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
