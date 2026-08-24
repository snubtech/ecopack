using System;
using System.Collections.Generic;

namespace ecopack.Api.TempModels;

/// <summary>
/// AI패키지그룹플라스틱모의평가정보기본
/// </summary>
public partial class AiPkgGrpPlstSimEvalInfoBsc
{
    /// <summary>
    /// AIPackage모의평가ID이며 채번규칙을 갖는다.
    /// </summary>
    public string AiPkgSimevlId { get; set; } = null!;

    /// <summary>
    /// 프로젝트 고유 ID로 채번규칙을 갖는다.
    /// </summary>
    public string PrjId { get; set; } = null!;

    /// <summary>
    /// 프로젝트 하위 포장차수별 ID이며, 채번규칙을 갖는다.
    /// </summary>
    public string? SubPrjId { get; set; }

    /// <summary>
    /// 프로젝트포장차수를 저장하는 필드이며, 고정값은 3이다.
    /// </summary>
    public int? PrjfPkgSeq { get; set; }

    /// <summary>
    /// 그룹포장재 안전성 및 유해물질(4대 중금속)
    /// </summary>
    public string? SftCntn1 { get; set; }

    /// <summary>
    /// 운송포장재의 재사용
    /// </summary>
    public int? ReuseCntn1 { get; set; }

    /// <summary>
    /// 인쇄면적의 감소(인쇄)
    /// </summary>
    public int? RedcCntn1 { get; set; }

    /// <summary>
    /// 인쇄면적의 감소(기타 각인수단)
    /// </summary>
    public int? RedcCntn2 { get; set; }

    /// <summary>
    /// 친환경 잉크의 사용(수성잉크 적용)
    /// </summary>
    public int? RedcCntn3 { get; set; }

    /// <summary>
    /// 친환경 잉크의 사용(소이잉크/라이스잉크)
    /// </summary>
    public int? RedcCntn4 { get; set; }

    /// <summary>
    /// 일회용 운송포장재의 빈공간 비율
    /// </summary>
    public int? RedcCntn5 { get; set; }

    /// <summary>
    /// 완충재 및 고정재의 사용
    /// </summary>
    public int? RedcCntn6 { get; set; }

    /// <summary>
    /// 포장 차수
    /// </summary>
    public int? RedcCntn7 { get; set; }

    /// <summary>
    /// 그룹포장재의재활용 디자인(DfR) 성능등급
    /// </summary>
    public int? RcycCntn1 { get; set; }

    /// <summary>
    /// 플라스틱 그룹포장재의 PCR 사용량
    /// </summary>
    public int? RcycCntn2 { get; set; }

    /// <summary>
    /// 그룹포장재의 재활용 표시 의무
    /// </summary>
    public int? RcycCntn3 { get; set; }

    /// <summary>
    /// 그룹포장재의 대체소재 사용
    /// </summary>
    public int? RplcCntn1 { get; set; }

    /// <summary>
    /// 그룹포장재의 경량화
    /// </summary>
    public int? InnoCntn1 { get; set; }

    /// <summary>
    /// 그룹포장재의 소재 단일화
    /// </summary>
    public int? InnoCntn2 { get; set; }

    /// <summary>
    /// 그룹포장재의 정보전달 기술
    /// </summary>
    public int? InnoCntn3 { get; set; }

    /// <summary>
    /// 그룹포장재의 개봉용이성(Easy-to-open)
    /// </summary>
    public int? InnoCntn4 { get; set; }

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

    /// <summary>
    /// 프로젝트 별 AI패키지모의평가 최초 시작일시
    /// </summary>
    public DateOnly? FrstEvlDtm { get; set; }

    /// <summary>
    /// 프로젝트 별 AI패키지모의평가 최종 적용일시
    /// </summary>
    public DateOnly? LastEvlDtm { get; set; }
}
