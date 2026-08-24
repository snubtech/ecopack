using System;
using System.Collections.Generic;

namespace ecopack.Api.TempModels;

/// <summary>
/// 고객문서기본
/// </summary>
public partial class CustomerDocument
{
    /// <summary>
    /// 고객의 등록문서관리용 ID
    /// </summary>
    public string CustDocId { get; set; } = null!;

    /// <summary>
    /// 대표자 Master 고객 ID
    /// </summary>
    public string RepCustId { get; set; } = null!;

    /// <summary>
    /// 법인/개인사업자등록증 및 기타 문서
    /// </summary>
    public string? DocTypeNm { get; set; }

    /// <summary>
    /// 파일URL저장경로
    /// </summary>
    public string? FileUrl { get; set; }

    /// <summary>
    /// 파일 유효기간 시작년월일
    /// </summary>
    public DateOnly? VldBgngDt { get; set; }

    /// <summary>
    /// 파일 유효기간 종료년월일
    /// </summary>
    public DateOnly? VldEndDt { get; set; }

    /// <summary>
    /// 파일추가년월일
    /// </summary>
    public DateTime? FileAddDt { get; set; }
}
