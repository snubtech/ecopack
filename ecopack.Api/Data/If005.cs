using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 환경영향평가정보 목록 조회 (IF005)
/// </summary>
public partial class If005
{
    /// <summary>
    /// 내부 관리용 일련번호
    /// </summary>
    public long Idx { get; set; }

    /// <summary>
    /// 환경영향평가ID
    /// </summary>
    public string EnvImpAssId { get; set; } = null!;

    /// <summary>
    /// 포장차수명
    /// </summary>
    public string? PackLevelNm { get; set; }

    /// <summary>
    /// 적용소재명
    /// </summary>
    public string? AppliedMaterialNm { get; set; }

    /// <summary>
    /// 포장재 구분명
    /// </summary>
    public string? MatTypeNm { get; set; }

    /// <summary>
    /// 소재의 종류명
    /// </summary>
    public string? MatFormNm { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(kgCO2.eq/kg)-원료
    /// </summary>
    public string? MassCo2Mat { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(kgCO2.eq/kg)-제조
    /// </summary>
    public string? MassCo2Proc { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(kgCO2.eq/kg)-폐기
    /// </summary>
    public string? MassCo2Scrap { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(kgCO2.eq/kg)-합계
    /// </summary>
    public string? MassCo2Sum { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(kgCO2.eq/kg)-UNIT
    /// </summary>
    public string? MassCo2MgtVal { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(kgCO2.eq/관리단위)-원료
    /// </summary>
    public string? UnitCo2Mat { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(kgCO2.eq/관리단위)-제조
    /// </summary>
    public string? UnitCo2Proc { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(kgCO2.eq/관리단위)-폐기
    /// </summary>
    public string? UnitCo2Scrap { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(kgCO2.eq/관리단위)-합계
    /// </summary>
    public string? UnitCo2Sum { get; set; }

    /// <summary>
    /// 단위당 탄소배출량의 관리단위
    /// </summary>
    public string? UnitCo2MgtVal { get; set; }

    /// <summary>
    /// 단위당 탄소배출량-비고
    /// </summary>
    public string? UnitCo2Desc { get; set; }

    /// <summary>
    /// 물리적 인자-면적당 중량(kg/m2)
    /// </summary>
    public string? AreaDensity { get; set; }

    /// <summary>
    /// 물리적 인자-단위
    /// </summary>
    public string? PhyQntyUnit { get; set; }

    /// <summary>
    /// 원료물질 구성
    /// </summary>
    public string? MatCompCon { get; set; }

    /// <summary>
    /// 포장차수코드
    /// </summary>
    public string? PackLevel { get; set; }

    /// <summary>
    /// 적용소재코드
    /// </summary>
    public string? AppliedMaterial { get; set; }

    /// <summary>
    /// 포장재 구분코드
    /// </summary>
    public string? MatType { get; set; }

    /// <summary>
    /// 소재의 종류코드
    /// </summary>
    public string? MatForm { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
