/**
 * ==============================================================================
 * [프로그램 전체 흐름 및 구조 요약] - PrimaryTdConfiguration (primary_td 테이블 매핑)
 * ==============================================================================
 * 
 * 1. 하는 일
 *    - 엔티티 PrimaryTd 를 실제 테이블 primary_td 에 이어 줍니다.
 *    - 실제 DB의 SHOW CREATE TABLE 결과를 그대로 옮겨 적었습니다(2026-09).
 * 
 * 2. 처음 만들어졌을 때 잘못돼 있던 점
 *    - 테이블명이 primarytd 로 되어 있어 실제 테이블(primary_td)을 찾지 못했습니다.
 *    - 컬럼명도 전부 소문자로 적혀 있었고, 문자열 길이도 실제와 달랐습니다.
 * 
 * 3. 맞춘 내용
 *    - 테이블명 primary_td, 컬럼명은 실제와 같은 대소문자(prjId, prdExplPhrsCntn …)
 *    - 문자열은 text, 중량·비율은 decimal(10,2), BOM 수량은 int
 *    - lastWrtDtm 은 DB 기본값 CURRENT_TIMESTAMP 를 따릅니다.
 * 
 * 4. 참고
 *    - 컬럼별 한글 설명은 엔티티 PrimaryTd.cs 의 주석에 적혀 있습니다.
 * ==============================================================================
 */
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    /// <summary>
    /// primary_td (1차포장기술서기본) 테이블 매핑.
    /// 실제 DB의 SHOW CREATE TABLE 결과에 맞춰 작성됨 (2026-09):
    ///  - 테이블명: primary_td (언더스코어)
    ///  - PK: pkg1TechDocId varchar(50)
    ///  - 문자열 컬럼은 전부 MySQL text
    ///  - 금액/중량류는 decimal(10,2), BOM 수량은 int
    ///  - lastWrtDtm 은 DB DEFAULT CURRENT_TIMESTAMP
    /// 컬럼 정의(한글 설명)는 엔티티 PrimaryTd.cs 의 XML 주석 참고.
    /// </summary>
    public class PrimaryTdConfiguration : IEntityTypeConfiguration<PrimaryTd>
    {
        public void Configure(EntityTypeBuilder<PrimaryTd> builder)
        {
            builder.ToTable("primary_td");
            builder.HasKey(e => e.Pkg1TechDocId);

            builder.Property(e => e.Pkg1TechDocId).HasColumnName("pkg1TechDocId").HasMaxLength(50);
            builder.Property(e => e.PrjId).HasColumnName("prjId").HasColumnType("text");
            builder.Property(e => e.PrjfNm).HasColumnName("prjfNm").HasColumnType("text");
            builder.Property(e => e.BizNm).HasColumnName("bizNm").HasColumnType("text");
            builder.Property(e => e.CntryNm).HasColumnName("cntryNm").HasColumnType("text");
            builder.Property(e => e.DocNo).HasColumnName("docNo").HasColumnType("text");
            builder.Property(e => e.LastWrtDtm).HasColumnName("lastWrtDtm").HasColumnType("datetime")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            builder.Property(e => e.RevNo).HasColumnName("revNo").HasColumnType("text");
            builder.Property(e => e.PrdExplPhrsCntn).HasColumnName("prdExplPhrsCntn").HasColumnType("text");
            builder.Property(e => e.PrdIdfyCntn).HasColumnName("prdIdfyCntn").HasColumnType("text");
            builder.Property(e => e.MainMatVal).HasColumnName("mainMatVal").HasColumnType("text");
            builder.Property(e => e.DsgnFeatCntn).HasColumnName("dsgnFeatCntn").HasColumnType("text");
            builder.Property(e => e.PrdExtDimSpecVal).HasColumnName("prdExtDimSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdIntDimSpecVal).HasColumnName("prdIntDimSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdWtSpecVal).HasColumnName("prdWtSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdMatSpecVal).HasColumnName("prdMatSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdClrSpecVal).HasColumnName("prdClrSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdUseTempSpecVal).HasColumnName("prdUseTempSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdLoadWtSpecVal).HasColumnName("prdLoadWtSpecVal").HasColumnType("text");
            builder.Property(e => e.PrdDsgnLifeSpecVal).HasColumnName("prdDsgnLifeSpecVal").HasColumnType("text");
            builder.Property(e => e.MfrDrwUrl).HasColumnName("mfrDrwUrl").HasColumnType("text");
            builder.Property(e => e.MatComplItem1).HasColumnName("matComplItem1").HasColumnType("text");
            builder.Property(e => e.MatComplItem2).HasColumnName("matComplItem2").HasColumnType("text");
            builder.Property(e => e.MatComplItem3).HasColumnName("matComplItem3").HasColumnType("text");
            builder.Property(e => e.MatComplItem4).HasColumnName("matComplItem4").HasColumnType("text");
            builder.Property(e => e.MatCompTot).HasColumnName("matCompTot").HasColumnType("text");
            builder.Property(e => e.MatNm1).HasColumnName("matNm1").HasColumnType("text");
            builder.Property(e => e.MatNm2).HasColumnName("matNm2").HasColumnType("text");
            builder.Property(e => e.MatNm3).HasColumnName("matNm3").HasColumnType("text");
            builder.Property(e => e.MatNm4).HasColumnName("matNm4").HasColumnType("text");
            builder.Property(e => e.MatNmTot).HasColumnName("matNmTot").HasColumnType("text");
            builder.Property(e => e.WtVal1).HasColumnName("wtVal1").HasPrecision(10, 2);
            builder.Property(e => e.WtVal2).HasColumnName("wtVal2").HasPrecision(10, 2);
            builder.Property(e => e.WtVal3).HasColumnName("wtVal3").HasPrecision(10, 2);
            builder.Property(e => e.WtVal4).HasColumnName("wtVal4").HasPrecision(10, 2);
            builder.Property(e => e.WtValTot).HasColumnName("wtValTot").HasPrecision(10, 2);
            builder.Property(e => e.WtRt1).HasColumnName("wtRt1").HasPrecision(10, 2);
            builder.Property(e => e.WtRt2).HasColumnName("wtRt2").HasPrecision(10, 2);
            builder.Property(e => e.WtRt3).HasColumnName("wtRt3").HasPrecision(10, 2);
            builder.Property(e => e.WtRt4).HasColumnName("wtRt4").HasPrecision(10, 2);
            builder.Property(e => e.WtRtTot).HasColumnName("wtRtTot").HasPrecision(10, 2);
            builder.Property(e => e.BomCmpnNm1).HasColumnName("bomCmpnNm1").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm2).HasColumnName("bomCmpnNm2").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm3).HasColumnName("bomCmpnNm3").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm4).HasColumnName("bomCmpnNm4").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm5).HasColumnName("bomCmpnNm5").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm6).HasColumnName("bomCmpnNm6").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm7).HasColumnName("bomCmpnNm7").HasColumnType("text");
            builder.Property(e => e.BomCmpnNm8).HasColumnName("bomCmpnNm8").HasColumnType("text");
            builder.Property(e => e.BomMatNm1).HasColumnName("bomMatNm1").HasColumnType("text");
            builder.Property(e => e.BomMatNm2).HasColumnName("bomMatNm2").HasColumnType("text");
            builder.Property(e => e.BomMatNm3).HasColumnName("bomMatNm3").HasColumnType("text");
            builder.Property(e => e.BomMatNm4).HasColumnName("bomMatNm4").HasColumnType("text");
            builder.Property(e => e.BomMatNm5).HasColumnName("bomMatNm5").HasColumnType("text");
            builder.Property(e => e.BomMatNm6).HasColumnName("bomMatNm6").HasColumnType("text");
            builder.Property(e => e.BomMatNm7).HasColumnName("bomMatNm7").HasColumnType("text");
            builder.Property(e => e.BomMatNm8).HasColumnName("bomMatNm8").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd1).HasColumnName("bomMatStdCd1").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd2).HasColumnName("bomMatStdCd2").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd3).HasColumnName("bomMatStdCd3").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd4).HasColumnName("bomMatStdCd4").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd5).HasColumnName("bomMatStdCd5").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd6).HasColumnName("bomMatStdCd6").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd7).HasColumnName("bomMatStdCd7").HasColumnType("text");
            builder.Property(e => e.BomMatStdCd8).HasColumnName("bomMatStdCd8").HasColumnType("text");
            builder.Property(e => e.BomQtyVal1).HasColumnName("bomQtyVal1");
            builder.Property(e => e.BomQtyVal2).HasColumnName("bomQtyVal2");
            builder.Property(e => e.BomQtyVal3).HasColumnName("bomQtyVal3");
            builder.Property(e => e.BomQtyVal4).HasColumnName("bomQtyVal4");
            builder.Property(e => e.BomQtyVal5).HasColumnName("bomQtyVal5");
            builder.Property(e => e.BomQtyVal6).HasColumnName("bomQtyVal6");
            builder.Property(e => e.BomQtyVal7).HasColumnName("bomQtyVal7");
            builder.Property(e => e.BomQtyVal8).HasColumnName("bomQtyVal8");
            builder.Property(e => e.BomUnitWtVal1).HasColumnName("bomUnitWtVal1").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal2).HasColumnName("bomUnitWtVal2").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal3).HasColumnName("bomUnitWtVal3").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal4).HasColumnName("bomUnitWtVal4").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal5).HasColumnName("bomUnitWtVal5").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal6).HasColumnName("bomUnitWtVal6").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal7).HasColumnName("bomUnitWtVal7").HasPrecision(10, 2);
            builder.Property(e => e.BomUnitWtVal8).HasColumnName("bomUnitWtVal8").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal1).HasColumnName("bomTotWtVal1").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal2).HasColumnName("bomTotWtVal2").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal3).HasColumnName("bomTotWtVal3").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal4).HasColumnName("bomTotWtVal4").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal5).HasColumnName("bomTotWtVal5").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal6).HasColumnName("bomTotWtVal6").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal7).HasColumnName("bomTotWtVal7").HasPrecision(10, 2);
            builder.Property(e => e.BomTotWtVal8).HasColumnName("bomTotWtVal8").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal1).HasColumnName("bomWtRtVal1").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal2).HasColumnName("bomWtRtVal2").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal3).HasColumnName("bomWtRtVal3").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal4).HasColumnName("bomWtRtVal4").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal5).HasColumnName("bomWtRtVal5").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal6).HasColumnName("bomWtRtVal6").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal7).HasColumnName("bomWtRtVal7").HasPrecision(10, 2);
            builder.Property(e => e.BomWtRtVal8).HasColumnName("bomWtRtVal8").HasPrecision(10, 2);
            builder.Property(e => e.PrdDsgnPhrsCntn).HasColumnName("prdDsgnPhrsCntn").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestItemCntn1).HasColumnName("prdDsgnTestItemCntn1").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestItemCntn2).HasColumnName("prdDsgnTestItemCntn2").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestItemCntn3).HasColumnName("prdDsgnTestItemCntn3").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestItemCntn4).HasColumnName("prdDsgnTestItemCntn4").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestItemCntn5).HasColumnName("prdDsgnTestItemCntn5").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestItemCntn6).HasColumnName("prdDsgnTestItemCntn6").HasColumnType("text");
            builder.Property(e => e.PrdDsgnCritCntn1).HasColumnName("prdDsgnCritCntn1").HasColumnType("text");
            builder.Property(e => e.PrdDsgnCritCntn2).HasColumnName("prdDsgnCritCntn2").HasColumnType("text");
            builder.Property(e => e.PrdDsgnCritCntn3).HasColumnName("prdDsgnCritCntn3").HasColumnType("text");
            builder.Property(e => e.PrdDsgnCritCntn4).HasColumnName("prdDsgnCritCntn4").HasColumnType("text");
            builder.Property(e => e.PrdDsgnCritCntn5).HasColumnName("prdDsgnCritCntn5").HasColumnType("text");
            builder.Property(e => e.PrdDsgnCritCntn6).HasColumnName("prdDsgnCritCntn6").HasColumnType("text");
            builder.Property(e => e.PrdDsgnRsltVal1).HasColumnName("prdDsgnRsltVal1").HasColumnType("text");
            builder.Property(e => e.PrdDsgnRsltVal2).HasColumnName("prdDsgnRsltVal2").HasColumnType("text");
            builder.Property(e => e.PrdDsgnRsltVal3).HasColumnName("prdDsgnRsltVal3").HasColumnType("text");
            builder.Property(e => e.PrdDsgnRsltVal4).HasColumnName("prdDsgnRsltVal4").HasColumnType("text");
            builder.Property(e => e.PrdDsgnRsltVal5).HasColumnName("prdDsgnRsltVal5").HasColumnType("text");
            builder.Property(e => e.PrdDsgnRsltVal6).HasColumnName("prdDsgnRsltVal6").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestMthd1).HasColumnName("prdDsgnTestMthd1").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestMthd2).HasColumnName("prdDsgnTestMthd2").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestMthd3).HasColumnName("prdDsgnTestMthd3").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestMthd4).HasColumnName("prdDsgnTestMthd4").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestMthd5).HasColumnName("prdDsgnTestMthd5").HasColumnType("text");
            builder.Property(e => e.PrdDsgnTestMthd6).HasColumnName("prdDsgnTestMthd6").HasColumnType("text");
            builder.Property(e => e.ReusePerfRsltVal).HasColumnName("reusePerfRsltVal").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncPhrsCntn).HasColumnName("rcycDsgnPrncPhrsCntn").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncItemCntn1).HasColumnName("rcycDsgnPrncItemCntn1").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncItemCntn2).HasColumnName("rcycDsgnPrncItemCntn2").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncItemCntn3).HasColumnName("rcycDsgnPrncItemCntn3").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncItemCntn4).HasColumnName("rcycDsgnPrncItemCntn4").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncItemCntn5).HasColumnName("rcycDsgnPrncItemCntn5").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncItemCntn6).HasColumnName("rcycDsgnPrncItemCntn6").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlRslt1).HasColumnName("rcycDsgnPrncEvlRslt1").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlRslt2).HasColumnName("rcycDsgnPrncEvlRslt2").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlRslt3).HasColumnName("rcycDsgnPrncEvlRslt3").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlRslt4).HasColumnName("rcycDsgnPrncEvlRslt4").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlRslt5).HasColumnName("rcycDsgnPrncEvlRslt5").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlRslt6).HasColumnName("rcycDsgnPrncEvlRslt6").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn1).HasColumnName("rcycDsgnPrncEvlTestMthdCntn1").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn2).HasColumnName("rcycDsgnPrncEvlTestMthdCntn2").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn3).HasColumnName("rcycDsgnPrncEvlTestMthdCntn3").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn4).HasColumnName("rcycDsgnPrncEvlTestMthdCntn4").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn5).HasColumnName("rcycDsgnPrncEvlTestMthdCntn5").HasColumnType("text");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn6).HasColumnName("rcycDsgnPrncEvlTestMthdCntn6").HasColumnType("text");
            builder.Property(e => e.RcycPathPhrsCntn).HasColumnName("rcycPathPhrsCntn").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngFixPhrsCntn).HasColumnName("socHvyMetMngFixPhrsCntn").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn1).HasColumnName("socHvyMetMngTestRsltSbstCntn1").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn2).HasColumnName("socHvyMetMngTestRsltSbstCntn2").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn3).HasColumnName("socHvyMetMngTestRsltSbstCntn3").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn4).HasColumnName("socHvyMetMngTestRsltSbstCntn4").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn5).HasColumnName("socHvyMetMngTestRsltSbstCntn5").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRsltTot).HasColumnName("socHvyMetMngRsltTot").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn1).HasColumnName("socHvyMetMngTestRsltCntn1").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn2).HasColumnName("socHvyMetMngTestRsltCntn2").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn3).HasColumnName("socHvyMetMngTestRsltCntn3").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn4).HasColumnName("socHvyMetMngTestRsltCntn4").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn5).HasColumnName("socHvyMetMngTestRsltCntn5").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltTot).HasColumnName("socHvyMetMngTestRsltTot").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRegCritCntn1).HasColumnName("socHvyMetMngRegCritCntn1").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRegCritCntn2).HasColumnName("socHvyMetMngRegCritCntn2").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRegCritCntn3).HasColumnName("socHvyMetMngRegCritCntn3").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRegCritCntn4).HasColumnName("socHvyMetMngRegCritCntn4").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRegCritCntn5).HasColumnName("socHvyMetMngRegCritCntn5").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngRegCritTot).HasColumnName("socHvyMetMngRegCritTot").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn1).HasColumnName("socHvyMetMngTestMthdCntn1").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn2).HasColumnName("socHvyMetMngTestMthdCntn2").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn3).HasColumnName("socHvyMetMngTestMthdCntn3").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn4).HasColumnName("socHvyMetMngTestMthdCntn4").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn5).HasColumnName("socHvyMetMngTestMthdCntn5").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn6).HasColumnName("socHvyMetMngTestMthdCntn6").HasColumnType("text");
            builder.Property(e => e.SocHvyMetMngTestRsltPhrs).HasColumnName("socHvyMetMngTestRsltPhrs").HasColumnType("text");
            builder.Property(e => e.MfrPrcsUrl).HasColumnName("mfrPrcsUrl").HasColumnType("text");
            builder.Property(e => e.MfrPrcsCntn).HasColumnName("mfrPrcsCntn").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn1).HasColumnName("qltMngInspItemCntn1").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn2).HasColumnName("qltMngInspItemCntn2").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn3).HasColumnName("qltMngInspItemCntn3").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn4).HasColumnName("qltMngInspItemCntn4").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn5).HasColumnName("qltMngInspItemCntn5").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn6).HasColumnName("qltMngInspItemCntn6").HasColumnType("text");
            builder.Property(e => e.QltMngInspItemCntn7).HasColumnName("qltMngInspItemCntn7").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn1).HasColumnName("qltMngInspMthdCntn1").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn2).HasColumnName("qltMngInspMthdCntn2").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn3).HasColumnName("qltMngInspMthdCntn3").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn4).HasColumnName("qltMngInspMthdCntn4").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn5).HasColumnName("qltMngInspMthdCntn5").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn6).HasColumnName("qltMngInspMthdCntn6").HasColumnType("text");
            builder.Property(e => e.QltMngInspMthdCntn7).HasColumnName("qltMngInspMthdCntn7").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn1).HasColumnName("qltMngFreqCntn1").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn2).HasColumnName("qltMngFreqCntn2").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn3).HasColumnName("qltMngFreqCntn3").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn4).HasColumnName("qltMngFreqCntn4").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn5).HasColumnName("qltMngFreqCntn5").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn6).HasColumnName("qltMngFreqCntn6").HasColumnType("text");
            builder.Property(e => e.QltMngFreqCntn7).HasColumnName("qltMngFreqCntn7").HasColumnType("text");
            builder.Property(e => e.CmplDclCntn).HasColumnName("cmplDclCntn").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl1).HasColumnName("atchDocUrl1").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl2).HasColumnName("atchDocUrl2").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl3).HasColumnName("atchDocUrl3").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl4).HasColumnName("atchDocUrl4").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl5).HasColumnName("atchDocUrl5").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl6).HasColumnName("atchDocUrl6").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl7).HasColumnName("atchDocUrl7").HasColumnType("text");
            builder.Property(e => e.AtchDocUrl8).HasColumnName("atchDocUrl8").HasColumnType("text");
            builder.Property(e => e.AtchDocNm1).HasColumnName("atchDocNm1").HasColumnType("text");
            builder.Property(e => e.AtchDocNm2).HasColumnName("atchDocNm2").HasColumnType("text");
            builder.Property(e => e.AtchDocNm3).HasColumnName("atchDocNm3").HasColumnType("text");
            builder.Property(e => e.AtchDocNm4).HasColumnName("atchDocNm4").HasColumnType("text");
            builder.Property(e => e.AtchDocNm5).HasColumnName("atchDocNm5").HasColumnType("text");
            builder.Property(e => e.AtchDocNm6).HasColumnName("atchDocNm6").HasColumnType("text");
            builder.Property(e => e.AtchDocNm7).HasColumnName("atchDocNm7").HasColumnType("text");
            builder.Property(e => e.AtchDocNm8).HasColumnName("atchDocNm8").HasColumnType("text");
            builder.Property(e => e.BizNm2).HasColumnName("bizNm2").HasColumnType("text");
            builder.Property(e => e.RepNm).HasColumnName("repNm").HasColumnType("text");
            builder.Property(e => e.RoleNm).HasColumnName("roleNm").HasColumnType("text");
            builder.Property(e => e.EmlAddr).HasColumnName("emlAddr").HasColumnType("text");
            builder.Property(e => e.MbTelNo).HasColumnName("mbTelNo").HasColumnType("text");
            builder.Property(e => e.TechDocLastPhrsCntn).HasColumnName("techDocLastPhrsCntn").HasColumnType("text");
        }
    }
}
