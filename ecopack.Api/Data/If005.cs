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
    /// 소재의 종류명
    /// </summary>
    public string? MatFormNm { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(소재)
    /// </summary>
    public string? MassCo2Mat { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(공정)
    /// </summary>
    public string? MassCo2Proc { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(스크랩)
    /// </summary>
    public string? MassCo2Scrap { get; set; }

    /// <summary>
    /// 중량 당 탄소배출량(합계)
    /// </summary>
    public string? MassCo2Sum { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(소재)
    /// </summary>
    public string? UnitCo2Mat { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(공정)
    /// </summary>
    public string? UnitCo2Proc { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(스크랩)
    /// </summary>
    public string? UnitCo2Scrap { get; set; }

    /// <summary>
    /// 단위당 탄소배출량(합계)
    /// </summary>
    public string? UnitCo2Sum { get; set; }

    /// <summary>
    /// 단위당 탄소배출량 관리값
    /// </summary>
    public string? UnitCo2MgtVal { get; set; }

    /// <summary>
    /// 물리적 인자-면적당 중량
    /// </summary>
    public string? AreaDensity { get; set; }

    /// <summary>
    /// 물리적 인자-밀도
    /// </summary>
    public string? Density { get; set; }

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
    /// 소재의 종류코드
    /// </summary>
    public string? MatForm { get; set; }

    /// <summary>
    /// 데이터 수집일시
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
