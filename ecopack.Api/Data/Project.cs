using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 프로젝트기본
/// </summary>
public partial class Project
{
    /// <summary>
    /// 프로젝트 고유 ID로 채번규칙을 갖는다.
    /// </summary>
    public string PrjId { get; set; } = null!;

    /// <summary>
    /// 프로젝트명으로, 판매제품명을 의미함
    /// </summary>
    public string? PrjNm { get; set; }

    /// <summary>
    /// 대표자 Master 고객 ID
    /// </summary>
    public string? RepCustId { get; set; }

    /// <summary>
    /// 법인등록번호/개인사업자번호
    /// </summary>
    public string? BizNo { get; set; }

    /// <summary>
    /// 법인/개인사업명
    /// </summary>
    public string? BizNm { get; set; }

    /// <summary>
    /// 대표자 직책명
    /// </summary>
    public string? RepNm { get; set; }

    /// <summary>
    /// 법인/개인사업 대표자명
    /// </summary>
    public string? RoleNm { get; set; }

    /// <summary>
    /// 법인/개인사업자 업종명
    /// </summary>
    public string? IndstNm { get; set; }

    /// <summary>
    /// 국가명(제조국가명)
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
    /// 제품수출국가명(미국)USA 여부
    /// </summary>
    public string? PrdExpCntryNm1 { get; set; }

    /// <summary>
    /// 제품수출국가명(유럽)EU 여부
    /// </summary>
    public string? PrdExpCntryNm2 { get; set; }

    /// <summary>
    /// 제품수출국가명(중국)CHN 여부
    /// </summary>
    public string? PrdExpCntryNm3 { get; set; }

    /// <summary>
    /// 제품수출국가명(베트남)VNM 여부
    /// </summary>
    public string? PrdExpCntryNm4 { get; set; }

    /// <summary>
    /// 제품수출국가명(인도네시아)IDN 여부
    /// </summary>
    public string? PrdExpCntryNm5 { get; set; }

    /// <summary>
    /// 제품수출국가명(일본)JPN 여부
    /// </summary>
    public string? PrdExpCntryNm6 { get; set; }

    /// <summary>
    /// 제품수출국가명(호주)AUS 여부
    /// </summary>
    public string? PrdExpCntryNm7 { get; set; }

    /// <summary>
    /// 제품수출국가명(대한민국)KOR 여부
    /// </summary>
    public string? PrdExpCntryNm8 { get; set; }

    /// <summary>
    /// 제품포장(Primary) 차수여부
    /// </summary>
    public string? PrdPkgSeq1 { get; set; }

    /// <summary>
    /// 운송포장(Secondary) 차수여부
    /// </summary>
    public string? PrdPkgSeq2 { get; set; }

    /// <summary>
    /// 수송포장(Tertiary) 차수여부
    /// </summary>
    public string? PrdPkgSeq3 { get; set; }

    /// <summary>
    /// 프로젝트 개정번호로 채번규칙을 갖는다.
    /// </summary>
    public string? PrjRevNo { get; set; }

    /// <summary>
    /// 프로젝트 생성일자로, 기술문서 및 적합성선언서의 작성일로 사용함
    /// </summary>
    public DateOnly? PrjFcrtDt { get; set; }
}
