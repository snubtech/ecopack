using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 1차포장적합성선언서기본
/// </summary>
public partial class PrimaryDoc
{
    /// <summary>
    /// 1차포장 적합성선언서의 고유 ID
    /// </summary>
    public string Pkg1DocId { get; set; } = null!;

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
    /// 이메일 주소
    /// </summary>
    public string? EmlAddr { get; set; }

    /// <summary>
    /// 대표자 휴대폰번호
    /// </summary>
    public string? MbTelNo { get; set; }

    /// <summary>
    /// 프로젝트명으로, 판매제품명을 의미함
    /// </summary>
    public string? PrjfNm { get; set; }

    /// <summary>
    /// 프로젝트 고유 ID로 채번규칙을 갖는다.
    /// </summary>
    public string? PrjId { get; set; }

    /// <summary>
    /// 1차포장기술문서 고유 ID이다.
    /// </summary>
    public string? Pkg1TechDocId { get; set; }

    /// <summary>
    /// 적합성선언서의 개정번호이며, 채번규칙을 가진 개정번호이다.
    /// </summary>
    public string? RevNo { get; set; }

    /// <summary>
    /// 국가명(제조국가명)
    /// </summary>
    public string? CntryNm { get; set; }

    /// <summary>
    /// IF200에서 참조하는 디자인유형명
    /// </summary>
    public string? DsgnTypeNm { get; set; }

    /// <summary>
    /// 적합성 선언 문구로 고정되어하며, 회사명만 변경해야한다.
    /// </summary>
    public string? DocPhrsCntn { get; set; }

    /// <summary>
    /// 4.1. 재사용성요구사항적합선언서 고정부분이다.
    /// </summary>
    public string? ReuseReqCmplCntn { get; set; }

    /// <summary>
    /// IF200에 아직 없음
    /// </summary>
    public string? DsgnTmplMstrPrdExpl { get; set; }

    /// <summary>
    /// 4.2. 재활용성 요구사항 적합 부분의 고정내용 중 전반부이다.
    /// </summary>
    public string? RcycReqCmplCntn1 { get; set; }

    /// <summary>
    /// 주요특징 중 변경사항 부분이다.
    /// </summary>
    public string? RcycMainFeatCntn { get; set; }

    /// <summary>
    /// 4.2. 재활용성 요구사항 적합 부분의 고정내용 중 후반부이다.
    /// </summary>
    public string? RcycReqCmplCntn2 { get; set; }

    /// <summary>
    /// 4.3. 우려물질 및 중금속 제한 적합의 고정 내용이다.
    /// </summary>
    public string? SoChvyMetLmtCmplCntn1 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 물질 첫번째 행의 물질내용이다.
    /// </summary>
    public string? Sbst1 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 물질 두번째 행의 물질내용이다.
    /// </summary>
    public string? Sbst2 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 물질 세번째 행의 물질내용이다.
    /// </summary>
    public string? Sbst3 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 물질 네번째 행의 물질내용이다.
    /// </summary>
    public string? Sbst4 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 물질 다섯번째 행의 물질내용이며, 총합 이라는 디폴트값을 사용한다.
    /// </summary>
    public string? SbstTot { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 시험결과 첫번째 행의 시험결과 내용이다.
    /// </summary>
    public string? TestRslt1 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 시험결과 두번째 행의 시험결과 내용이다.
    /// </summary>
    public string? TestRslt2 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 시험결과 세번째 행의 시험결과 내용이다.
    /// </summary>
    public string? TestRslt3 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 시험결과 네번째 행의 시험결과 내용이다.
    /// </summary>
    public string? TestRslt4 { get; set; }

    /// <summary>
    /// 4.3.의 하위 표 중 시험결과 다섯번째 행의 시험결과 내용이다.
    /// </summary>
    public string? TestRsltTot { get; set; }

    /// <summary>
    /// 4.4. 재질정보 하위표 중 첫번째행의 구성품 내용이다.
    /// </summary>
    public string? Compltem1 { get; set; }

    /// <summary>
    /// 4.4. 재질정보 하위표 중 두번째행의 구성품 내용이다.
    /// </summary>
    public string? Compltem2 { get; set; }

    /// <summary>
    /// 4.4. 재질정보 하위표 중 세번째행의 구성품 내용이다.
    /// </summary>
    public string? Compltem3 { get; set; }

    /// <summary>
    /// 4.4. 재질정보 하위표 중 첫번째행의 재질 내용이다.
    /// </summary>
    public string? Mat1 { get; set; }

    /// <summary>
    /// 4.4. 재질정보 하위표 중 두번째행의 재질 내용이다.
    /// </summary>
    public string? Mat2 { get; set; }

    /// <summary>
    /// 4.4. 재질정보 하위표 중 세번째행의 재질 내용이다.
    /// </summary>
    public string? Mat3 { get; set; }

    /// <summary>
    /// 4.4. 총 중량 : 의 데이터 값을 입력한다. 입력받을 총 중량값은 기술문서에서 계산하여 추가한다.
    /// </summary>
    public string? MatInfoTotWtVal { get; set; }

    /// <summary>
    /// 4.4.의 재질정보 하단부의 고정문구이다.
    /// </summary>
    public string? MatInfoCntn { get; set; }

    /// <summary>
    /// 5. 근거문서의 도입부 고정문구 내용이다.
    /// </summary>
    public string? EvdDocCntn { get; set; }

    /// <summary>
    /// 부속서 A의 URL이다.
    /// </summary>
    public string? EvdDocUrl1 { get; set; }

    /// <summary>
    /// 부속서 B의 URL이다.
    /// </summary>
    public string? EvdDocUrl2 { get; set; }

    /// <summary>
    /// 부속서 C의 URL이다.
    /// </summary>
    public string? EvdDocUrl3 { get; set; }

    /// <summary>
    /// 부속서 D의 URL이다.
    /// </summary>
    public string? EvdDocUrl4 { get; set; }

    /// <summary>
    /// 부속서 E의 URL이다.
    /// </summary>
    public string? EvdDocUrl5 { get; set; }

    /// <summary>
    /// 부속서 F의 URL이다.
    /// </summary>
    public string? EvdDocUrl6 { get; set; }

    /// <summary>
    /// 부속서 G의 URL이다.
    /// </summary>
    public string? EvdDocUrl7 { get; set; }

    /// <summary>
    /// 부속서 H의 URL이다.
    /// </summary>
    public string? EvdDocUrl8 { get; set; }

    /// <summary>
    /// 부속서 A의 파일명이다.
    /// </summary>
    public string? EvdDocNm1 { get; set; }

    /// <summary>
    /// 부속서 B의 파일명이다.
    /// </summary>
    public string? EvdDocNm2 { get; set; }

    /// <summary>
    /// 부속서 C의 파일명이다.
    /// </summary>
    public string? EvdDocNm3 { get; set; }

    /// <summary>
    /// 부속서 D의 파일명이다.
    /// </summary>
    public string? EvdDocNm4 { get; set; }

    /// <summary>
    /// 부속서 E의 파일명이다.
    /// </summary>
    public string? EvdDocNm5 { get; set; }

    /// <summary>
    /// 부속서 F의 파일명이다.
    /// </summary>
    public string? EvdDocNm6 { get; set; }

    /// <summary>
    /// 부속서 G의 파일명이다.
    /// </summary>
    public string? EvdDocNm7 { get; set; }

    /// <summary>
    /// 부속서 H의 파일명이다.
    /// </summary>
    public string? EvdDocNm8 { get; set; }

    /// <summary>
    /// 6. 적용 규정 및 표준의 고정 내용이다.
    /// </summary>
    public string? ApplRuleStdCntn { get; set; }

    /// <summary>
    /// 7. 최종선언 고정내용으로 작성하며, 회사명 및 시료명은 변경사항이다.
    /// </summary>
    public string? LastDclCntn { get; set; }

    /// <summary>
    /// 발행일로 사용한다
    /// </summary>
    public DateOnly? LastWrtDt { get; set; }

    /// <summary>
    /// 법인/개인사업 명으로 제조자 항목으로 작성한다
    /// </summary>
    public string? BizNm2 { get; set; }

    /// <summary>
    /// 법인/개인사업 대표자명으로 사용한다.
    /// </summary>
    public string? RepNm2 { get; set; }

    /// <summary>
    /// 대표자 직책명으로 대표자명 옆에 괄호로 넣는다.
    /// </summary>
    public string? RoleNm2 { get; set; }
}
