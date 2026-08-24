using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 고객기본
/// </summary>
public partial class Customer
{
    /// <summary>
    /// 대표자 Master 고객 ID
    /// </summary>
    public string RepCustId { get; set; } = null!;

    /// <summary>
    /// 대표자 계정 비밀번호
    /// </summary>
    public string? RepCustPwd { get; set; }

    /// <summary>
    /// 법인등록번호/개인사업자번호
    /// </summary>
    public string? BizNo { get; set; }

    /// <summary>
    /// 법인/개인사업 구분명
    /// </summary>
    public string? CustTypeNm { get; set; }

    /// <summary>
    /// 법인/개인사업 명
    /// </summary>
    public string? BizNm { get; set; }

    /// <summary>
    /// 법인/개인사업 대표자명
    /// </summary>
    public string? RepNm { get; set; }

    /// <summary>
    /// 대표자 직책명
    /// </summary>
    public string? RoleNm { get; set; }

    /// <summary>
    /// 법인/개인사업자 업종명
    /// </summary>
    public string? IndstNm { get; set; }

    /// <summary>
    /// 국가명
    /// </summary>
    public string? CntryNm { get; set; }

    /// <summary>
    /// 우편번호
    /// </summary>
    public string? AddrCd { get; set; }

    /// <summary>
    /// 사업자 상세주소(도로명주소 까지)
    /// </summary>
    public string? DtlAddr1 { get; set; }

    /// <summary>
    /// 사업자 상세주소(동 호수)
    /// </summary>
    public string? DtlAddr2 { get; set; }

    /// <summary>
    /// 이메일 주소
    /// </summary>
    public string? EmlAddr { get; set; }

    /// <summary>
    /// 법인/개인사업자 대표번호
    /// </summary>
    public string? RepTelNo { get; set; }

    /// <summary>
    /// 대표자 휴대폰번호
    /// </summary>
    public string? MblTelNo { get; set; }

    /// <summary>
    /// 회원가입 년월일
    /// </summary>
    public DateTime? JoinDt { get; set; }

    /// <summary>
    /// 개인정보 변경 시, 최종 수정년월일
    /// </summary>
    public DateTime? PrvcChgDt { get; set; }

    /// <summary>
    /// 사용자최종로그인일시
    /// </summary>
    public DateTime? LastLgnDtm { get; set; }
}
