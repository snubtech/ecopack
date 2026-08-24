using System;
using System.Collections.Generic;

namespace ecopack.Api.Data;

/// <summary>
/// 1차포장기술서기본
/// </summary>
public partial class PrimaryTd
{
    /// <summary>
    /// 1차포장기술문서 고유 ID이다.
    /// </summary>
    public string Pkg1TechDocId { get; set; } = null!;

    /// <summary>
    /// 프로젝트 고유 ID로 채번규칙을 갖는다.
    /// </summary>
    public string? PrjId { get; set; }

    /// <summary>
    /// 프로젝트명으로, 판매제품명을 의미함
    /// </summary>
    public string? PrjfNm { get; set; }

    /// <summary>
    /// 법인/개인사업 명
    /// </summary>
    public string? BizNm { get; set; }

    /// <summary>
    /// 국가명(제조국가명)
    /// </summary>
    public string? CntryNm { get; set; }

    /// <summary>
    /// 기술문서의 문서번호이다. 프로젝트ID로 사용한다.
    /// </summary>
    public string? DocNo { get; set; }

    /// <summary>
    /// 최종작성년월일시
    /// </summary>
    public DateTime? LastWrtDtm { get; set; }

    /// <summary>
    /// 기술문서의 개정번호를 관리하며 코드 테이블 채번규칙을 갖는다
    /// </summary>
    public string? RevNo { get; set; }

    /// <summary>
    /// 2. 제품설명의 고정문구이다.
    /// </summary>
    public string? PrdExplPhrsCntn { get; set; }

    /// <summary>
    /// 2.1. 제품식별내용
    /// </summary>
    public string? PrdIdfyCntn { get; set; }

    /// <summary>
    /// 2.2. 주요재질 값을 저장하는 필드이다.
    /// </summary>
    public string? MainMatVal { get; set; }

    /// <summary>
    /// 2.3. 노이다의 디자인 특징 내용을 저장하는 필드이다.
    /// </summary>
    public string? DsgnFeatCntn { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 외측 치수의 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdExtDimSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 내부 치수의 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdIntDimSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 중량 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdWtSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 재질 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdMatSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 색상의 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdClrSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 사용온도 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdUseTempSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 적재하중의 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdLoadWtSpecVal { get; set; }

    /// <summary>
    /// 3.1. 제품사양의 설계 수명의 규격값을 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnLifeSpecVal { get; set; }

    /// <summary>
    /// 3.2. 제조도면 이미지 저장 URL을 넣는 필드이다.
    /// </summary>
    public string? MfrDrwUrl { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 첫번째 행의 구성품 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatComplItem1 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 두번째 행의 구성품 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatComplItem2 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 세번째 행의 구성품 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatComplItem3 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 네번째 행의 구성품 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatComplItem4 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 다섯번째 행의 구성품 데이터를 넣는 필드이며, 총계(합계)의 데이터이다.
    /// </summary>
    public string? MatCompTot { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 첫번째 행의 재질 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatNm1 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 두번째 행의 재질 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatNm2 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 세번째 행의 재질 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatNm3 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 네번째 행의 재질 데이터를 넣는 필드이다.
    /// </summary>
    public string? MatNm4 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 다섯번째 행의 재질 데이터를 넣는 필드이며, 총계(합계)의 데이터이다.
    /// </summary>
    public string? MatNmTot { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 첫번째 행의 중량 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtVal1 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 두번째 행의 중량 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtVal2 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 세번째 행의 중량 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtVal3 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 네번째 행의 중량 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtVal4 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 다섯번째 행의 중량 데이터를 넣는 필드이며, 총계(합계)의 데이터이다.
    /// </summary>
    public decimal? WtValTot { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 첫번째 행의 중량비율 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtRt1 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 두번째 행의 중량비율 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtRt2 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 세번째 행의 중량비율 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtRt3 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 네번째 행의 중량비율 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? WtRt4 { get; set; }

    /// <summary>
    /// 4.1. 재질구성의 다섯번째 행의 중량비율 데이터를 넣는 필드이며, 총계(합계)의 데이터이다.
    /// </summary>
    public decimal? WtRtTot { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Component Name 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomCmpnNm8 { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Material 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatNm8 { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Material Standard 데이터를 넣는 필드이다.
    /// </summary>
    public string? BomMatStdCd8 { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Qty 데이터를 넣는 필드이다.
    /// </summary>
    public int? BomQtyVal8 { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Unit Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomUnitWtVal8 { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Total Weight 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomTotWtVal8 { get; set; }

    /// <summary>
    /// 4.2. BOM의 첫번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal1 { get; set; }

    /// <summary>
    /// 4.2. BOM의 두번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal2 { get; set; }

    /// <summary>
    /// 4.2. BOM의 세번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal3 { get; set; }

    /// <summary>
    /// 4.2. BOM의 네번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal4 { get; set; }

    /// <summary>
    /// 4.2. BOM의 다섯번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal5 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여섯번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal6 { get; set; }

    /// <summary>
    /// 4.2. BOM의 일곱번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal7 { get; set; }

    /// <summary>
    /// 4.2. BOM의 여덟번째 행의 Weight Ratio 데이터를 넣는 필드이다.
    /// </summary>
    public decimal? BomWtRtVal8 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 항목의 고정문구를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnPhrsCntn { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 첫번째 행의 시험항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestItemCntn1 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 두번째 행의 시험항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestItemCntn2 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 세번째 행의 시험항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestItemCntn3 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 네번째 행의 시험항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestItemCntn4 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 다섯번째 행의 시험항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestItemCntn5 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 여섯번째 행의 시험항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestItemCntn6 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 첫번째 행의 기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnCritCntn1 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 두번째 행의 기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnCritCntn2 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 세번째 행의 기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnCritCntn3 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 네번째 행의 기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnCritCntn4 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 다섯번째 행의 기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnCritCntn5 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 여섯번째 행의 기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnCritCntn6 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 첫번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnRsltVal1 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 두번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnRsltVal2 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 세번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnRsltVal3 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 네번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnRsltVal4 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 다섯번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnRsltVal5 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 여섯번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnRsltVal6 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 첫번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestMthd1 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 두번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestMthd2 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 세번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestMthd3 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 네번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestMthd4 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 다섯번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestMthd5 { get; set; }

    /// <summary>
    /// 5.1. 재사용성평가 제품설계 하위 표의 여섯번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? PrdDsgnTestMthd6 { get; set; }

    /// <summary>
    /// 5.2. 재사용성결과 값을 넣는 필드이다.
    /// </summary>
    public string? ReusePerfRsltVal { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙의 고정 문구를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncPhrsCntn { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 첫번째 행의 항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncItemCntn1 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 두번째 행의 항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncItemCntn2 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 세번째 행의 항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncItemCntn3 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 네번째 행의 항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncItemCntn4 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 다섯번째 행의 항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncItemCntn5 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 여섯번째 행의 항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncItemCntn6 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 첫번째 행의 평가 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlRslt1 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 두번째 행의 평가 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlRslt2 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 세번째 행의 평가 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlRslt3 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 네번째 행의 평가 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlRslt4 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 다섯번째 행의 평가 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlRslt5 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 여섯번째 행의 평가 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlRslt6 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 첫번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlTestMthdCntn1 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 두번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlTestMthdCntn2 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 세번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlTestMthdCntn3 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 네번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlTestMthdCntn4 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 다섯번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlTestMthdCntn5 { get; set; }

    /// <summary>
    /// 6.1. 재활용성 설계원칙 하위 표의 여섯번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? RcycDsgnPrncEvlTestMthdCntn6 { get; set; }

    /// <summary>
    /// 6.2. 재활용 경로의 고정 문구를 넣는 필드이다.
    /// </summary>
    public string? RcycPathPhrsCntn { get; set; }

    /// <summary>
    /// 7. 우려물질 및 중금속관리의 고정문구를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngFixPhrsCntn { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 첫번째 행의 물질 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltSbstCntn1 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 두번째 행의 물질 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltSbstCntn2 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 세번째 행의 물질 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltSbstCntn3 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 네번째 행의 물질 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltSbstCntn4 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 다섯번째 행의 물질 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltSbstCntn5 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 물질 합계 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRsltTot { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 첫번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltCntn1 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 두번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltCntn2 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 세번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltCntn3 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 네번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltCntn4 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 다섯번째 행의 결과 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltCntn5 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 시험결과 합계 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltTot { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 첫번째 행의 규제기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRegCritCntn1 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 두번째 행의 규제기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRegCritCntn2 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 세번째 행의 규제기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRegCritCntn3 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 네번째 행의 규제기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRegCritCntn4 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 다섯번째 행의 규제기준 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRegCritCntn5 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 규제기준 합계 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngRegCritTot { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 첫번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestMthdCntn1 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 두번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestMthdCntn2 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 세번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestMthdCntn3 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 네번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestMthdCntn4 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 다섯번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestMthdCntn5 { get; set; }

    /// <summary>
    /// 7.1. 시험결과 표의 여섯번째 행의 시험방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestMthdCntn6 { get; set; }

    /// <summary>
    /// 7.1. 시험결과의 최종 결과 값을 넣는 필드이다.
    /// </summary>
    public string? SocHvyMetMngTestRsltPhrs { get; set; }

    /// <summary>
    /// 8. 제조공정의 제조공정 이미지 저장 URL을 넣는 필드이다.
    /// </summary>
    public string? MfrPrcsUrl { get; set; }

    /// <summary>
    /// 8. 제조공정의 제조공정 이미지에 대한 설명을 고정 문구로 넣는 필드이다.
    /// </summary>
    public string? MfrPrcsCntn { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 첫번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn1 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 두번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn2 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 세번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn3 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 네번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn4 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 다섯번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn5 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 여섯번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn6 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 일곱번째 행의 검사항목 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspItemCntn7 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 첫번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn1 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 두번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn2 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 세번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn3 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 네번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn4 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 다섯번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn5 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 여섯번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn6 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 일곱번째 행의 검사방법 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngInspMthdCntn7 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 첫번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn1 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 두번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn2 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 세번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn3 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 네번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn4 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 다섯번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn5 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 여섯번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn6 { get; set; }

    /// <summary>
    /// 9. 품질관리 표의 일곱번째 행의 빈도 데이터를 넣는 필드이다.
    /// </summary>
    public string? QltMngFreqCntn7 { get; set; }

    /// <summary>
    /// 10. 준수선언 고정내용이다.
    /// </summary>
    public string? CmplDclCntn { get; set; }

    /// <summary>
    /// 11. 첫번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl1 { get; set; }

    /// <summary>
    /// 11. 두번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl2 { get; set; }

    /// <summary>
    /// 11. 세번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl3 { get; set; }

    /// <summary>
    /// 11. 네번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl4 { get; set; }

    /// <summary>
    /// 11. 다섯번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl5 { get; set; }

    /// <summary>
    /// 11. 여섯번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl6 { get; set; }

    /// <summary>
    /// 11. 일곱번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl7 { get; set; }

    /// <summary>
    /// 11. 여덟번째 행에 저장되는 첨부문서의 저장 URL이다.
    /// </summary>
    public string? AtchDocUrl8 { get; set; }

    /// <summary>
    /// 11. 첫번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm1 { get; set; }

    /// <summary>
    /// 11. 두번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm2 { get; set; }

    /// <summary>
    /// 11. 세번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm3 { get; set; }

    /// <summary>
    /// 11. 네번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm4 { get; set; }

    /// <summary>
    /// 11. 다섯번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm5 { get; set; }

    /// <summary>
    /// 11. 여섯번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm6 { get; set; }

    /// <summary>
    /// 11. 일곱번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm7 { get; set; }

    /// <summary>
    /// 11. 여덟번째 행에 저장되는 첨부문서명(파일명)이다.
    /// </summary>
    public string? AtchDocNm8 { get; set; }

    /// <summary>
    /// 법인/개인사업자명을 저장하는 필드이다.
    /// </summary>
    public string? BizNm2 { get; set; }

    /// <summary>
    /// 법인/개인사업 대표자명
    /// </summary>
    public string? RepNm { get; set; }

    /// <summary>
    /// 대표자 직책명으로 대표자명 옆에 괄호로 넣는다.
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
    /// 12. 책임자 정보 마지막 하단의 문구를 저장하는 필드이다.
    /// </summary>
    public string? TechDocLastPhrsCntn { get; set; }
}
