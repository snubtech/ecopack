using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class PrimaryTdConfiguration : IEntityTypeConfiguration<PrimaryTd>
    {
        public void Configure(EntityTypeBuilder<PrimaryTd> builder)
        {
            builder.ToTable("primarytd");
            builder.HasKey(e => e.Pkg1TechDocId);

            builder.Property(e => e.Pkg1TechDocId).HasMaxLength(50).HasColumnName("pkg1techdocid");
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjid");
            builder.Property(e => e.PrjfNm).HasMaxLength(100).HasColumnName("prjfnm");
            builder.Property(e => e.BizNm).HasMaxLength(100).HasColumnName("biznm");
            builder.Property(e => e.CntryNm).HasMaxLength(50).HasColumnName("cntrynm");
            builder.Property(e => e.DocNo).HasMaxLength(50).HasColumnName("docno");
            builder.Property(e => e.LastWrtDtm).HasColumnName("lastwrtdtm");
            builder.Property(e => e.RevNo).HasMaxLength(20).HasColumnName("revno");
            builder.Property(e => e.PrdExplPhrsCntn).HasColumnType("text").HasColumnName("prdexplphrscntn");
            builder.Property(e => e.PrdIdfyCntn).HasColumnType("text").HasColumnName("prdidfycntn");
            builder.Property(e => e.MainMatVal).HasMaxLength(100).HasColumnName("mainmatval");
            builder.Property(e => e.DsgnFeatCntn).HasColumnType("text").HasColumnName("dsgnfeatcntn");
            builder.Property(e => e.PrdExtDimSpecVal).HasMaxLength(100).HasColumnName("prdextdimspecval");
            builder.Property(e => e.PrdIntDimSpecVal).HasMaxLength(100).HasColumnName("prdintdimspecval");
            builder.Property(e => e.PrdWtSpecVal).HasMaxLength(50).HasColumnName("prdwtspecval");
            builder.Property(e => e.PrdMatSpecVal).HasMaxLength(100).HasColumnName("prdmatspecval");
            builder.Property(e => e.PrdClrSpecVal).HasMaxLength(50).HasColumnName("prdclrspecval");
            builder.Property(e => e.PrdUseTempSpecVal).HasMaxLength(50).HasColumnName("prdusetempspecval");
            builder.Property(e => e.PrdLoadWtSpecVal).HasMaxLength(50).HasColumnName("prdloadwtspecval");
            builder.Property(e => e.PrdDsgnLifeSpecVal).HasMaxLength(50).HasColumnName("prddsgnlifespecval");
            builder.Property(e => e.MfrDrwUrl).HasMaxLength(255).HasColumnName("mfrdrwurl");

            // 재질구성 (4.1)
            builder.Property(e => e.MatComplItem1).HasMaxLength(100).HasColumnName("matcomplitem1");
            builder.Property(e => e.MatComplItem2).HasMaxLength(100).HasColumnName("matcomplitem2");
            builder.Property(e => e.MatComplItem3).HasMaxLength(100).HasColumnName("matcomplitem3");
            builder.Property(e => e.MatComplItem4).HasMaxLength(100).HasColumnName("matcomplitem4");
            builder.Property(e => e.MatCompTot).HasMaxLength(100).HasColumnName("matcomptot");
            builder.Property(e => e.MatNm1).HasMaxLength(100).HasColumnName("matnm1");
            builder.Property(e => e.MatNm2).HasMaxLength(100).HasColumnName("matnm2");
            builder.Property(e => e.MatNm3).HasMaxLength(100).HasColumnName("matnm3");
            builder.Property(e => e.MatNm4).HasMaxLength(100).HasColumnName("matnm4");
            builder.Property(e => e.MatNmTot).HasMaxLength(100).HasColumnName("matnmtot");
            builder.Property(e => e.WtVal1).HasColumnName("wtval1");
            builder.Property(e => e.WtVal2).HasColumnName("wtval2");
            builder.Property(e => e.WtVal3).HasColumnName("wtval3");
            builder.Property(e => e.WtVal4).HasColumnName("wtval4");
            builder.Property(e => e.WtValTot).HasColumnName("wtvaltot");
            builder.Property(e => e.WtRt1).HasColumnName("wtrt1");
            builder.Property(e => e.WtRt2).HasColumnName("wtrt2");
            builder.Property(e => e.WtRt3).HasColumnName("wtrt3");
            builder.Property(e => e.WtRt4).HasColumnName("wtrt4");
            builder.Property(e => e.WtRtTot).HasColumnName("wtrttot");

            // BOM (4.2)
            builder.Property(e => e.BomCmpnNm1).HasMaxLength(100).HasColumnName("bomcmpnnm1");
            builder.Property(e => e.BomCmpnNm2).HasMaxLength(100).HasColumnName("bomcmpnnm2");
            builder.Property(e => e.BomCmpnNm3).HasMaxLength(100).HasColumnName("bomcmpnnm3");
            builder.Property(e => e.BomCmpnNm4).HasMaxLength(100).HasColumnName("bomcmpnnm4");
            builder.Property(e => e.BomCmpnNm5).HasMaxLength(100).HasColumnName("bomcmpnnm5");
            builder.Property(e => e.BomCmpnNm6).HasMaxLength(100).HasColumnName("bomcmpnnm6");
            builder.Property(e => e.BomCmpnNm7).HasMaxLength(100).HasColumnName("bomcmpnnm7");
            builder.Property(e => e.BomCmpnNm8).HasMaxLength(100).HasColumnName("bomcmpnnm8");
            builder.Property(e => e.BomMatNm1).HasMaxLength(100).HasColumnName("bommatnm1");
            builder.Property(e => e.BomMatNm2).HasMaxLength(100).HasColumnName("bommatnm2");
            builder.Property(e => e.BomMatNm3).HasMaxLength(100).HasColumnName("bommatnm3");
            builder.Property(e => e.BomMatNm4).HasMaxLength(100).HasColumnName("bommatnm4");
            builder.Property(e => e.BomMatNm5).HasMaxLength(100).HasColumnName("bommatnm5");
            builder.Property(e => e.BomMatNm6).HasMaxLength(100).HasColumnName("bommatnm6");
            builder.Property(e => e.BomMatNm7).HasMaxLength(100).HasColumnName("bommatnm7");
            builder.Property(e => e.BomMatNm8).HasMaxLength(100).HasColumnName("bommatnm8");
            builder.Property(e => e.BomMatStdCd1).HasMaxLength(50).HasColumnName("bommatstdcd1");
            builder.Property(e => e.BomMatStdCd2).HasMaxLength(50).HasColumnName("bommatstdcd2");
            builder.Property(e => e.BomMatStdCd3).HasMaxLength(50).HasColumnName("bommatstdcd3");
            builder.Property(e => e.BomMatStdCd4).HasMaxLength(50).HasColumnName("bommatstdcd4");
            builder.Property(e => e.BomMatStdCd5).HasMaxLength(50).HasColumnName("bommatstdcd5");
            builder.Property(e => e.BomMatStdCd6).HasMaxLength(50).HasColumnName("bommatstdcd6");
            builder.Property(e => e.BomMatStdCd7).HasMaxLength(50).HasColumnName("bommatstdcd7");
            builder.Property(e => e.BomMatStdCd8).HasMaxLength(50).HasColumnName("bommatstdcd8");
            builder.Property(e => e.BomQtyVal1).HasColumnName("bomqtyval1");
            builder.Property(e => e.BomQtyVal2).HasColumnName("bomqtyval2");
            builder.Property(e => e.BomQtyVal3).HasColumnName("bomqtyval3");
            builder.Property(e => e.BomQtyVal4).HasColumnName("bomqtyval4");
            builder.Property(e => e.BomQtyVal5).HasColumnName("bomqtyval5");
            builder.Property(e => e.BomQtyVal6).HasColumnName("bomqtyval6");
            builder.Property(e => e.BomQtyVal7).HasColumnName("bomqtyval7");
            builder.Property(e => e.BomQtyVal8).HasColumnName("bomqtyval8");
            builder.Property(e => e.BomUnitWtVal1).HasColumnName("bomunitwtval1");
            builder.Property(e => e.BomUnitWtVal2).HasColumnName("bomunitwtval2");
            builder.Property(e => e.BomUnitWtVal3).HasColumnName("bomunitwtval3");
            builder.Property(e => e.BomUnitWtVal4).HasColumnName("bomunitwtval4");
            builder.Property(e => e.BomUnitWtVal5).HasColumnName("bomunitwtval5");
            builder.Property(e => e.BomUnitWtVal6).HasColumnName("bomunitwtval6");
            builder.Property(e => e.BomUnitWtVal7).HasColumnName("bomunitwtval7");
            builder.Property(e => e.BomUnitWtVal8).HasColumnName("bomunitwtval8");
            builder.Property(e => e.BomTotWtVal1).HasColumnName("bomtotwtval1");
            builder.Property(e => e.BomTotWtVal2).HasColumnName("bomtotwtval2");
            builder.Property(e => e.BomTotWtVal3).HasColumnName("bomtotwtval3");
            builder.Property(e => e.BomTotWtVal4).HasColumnName("bomtotwtval4");
            builder.Property(e => e.BomTotWtVal5).HasColumnName("bomtotwtval5");
            builder.Property(e => e.BomTotWtVal6).HasColumnName("bomtotwtval6");
            builder.Property(e => e.BomTotWtVal7).HasColumnName("bomtotwtval7");
            builder.Property(e => e.BomTotWtVal8).HasColumnName("bomtotwtval8");
            builder.Property(e => e.BomWtRtVal1).HasColumnName("bomwtrtval1");
            builder.Property(e => e.BomWtRtVal2).HasColumnName("bomwtrtval2");
            builder.Property(e => e.BomWtRtVal3).HasColumnName("bomwtrtval3");
            builder.Property(e => e.BomWtRtVal4).HasColumnName("bomwtrtval4");
            builder.Property(e => e.BomWtRtVal5).HasColumnName("bomwtrtval5");
            builder.Property(e => e.BomWtRtVal6).HasColumnName("bomwtrtval6");
            builder.Property(e => e.BomWtRtVal7).HasColumnName("bomwtrtval7");
            builder.Property(e => e.BomWtRtVal8).HasColumnName("bomwtrtval8");

            // 재사용성평가 제품설계 (5.1 ~ 5.2)
            builder.Property(e => e.PrdDsgnPhrsCntn).HasColumnType("text").HasColumnName("prddsgnphrscntn");
            builder.Property(e => e.PrdDsgnTestItemCntn1).HasMaxLength(100).HasColumnName("prddsgntestitemcntn1");
            builder.Property(e => e.PrdDsgnTestItemCntn2).HasMaxLength(100).HasColumnName("prddsgntestitemcntn2");
            builder.Property(e => e.PrdDsgnTestItemCntn3).HasMaxLength(100).HasColumnName("prddsgntestitemcntn3");
            builder.Property(e => e.PrdDsgnTestItemCntn4).HasMaxLength(100).HasColumnName("prddsgntestitemcntn4");
            builder.Property(e => e.PrdDsgnTestItemCntn5).HasMaxLength(100).HasColumnName("prddsgntestitemcntn5");
            builder.Property(e => e.PrdDsgnTestItemCntn6).HasMaxLength(100).HasColumnName("prddsgntestitemcntn6");
            builder.Property(e => e.PrdDsgnCritCntn1).HasMaxLength(100).HasColumnName("prddsgncritcntn1");
            builder.Property(e => e.PrdDsgnCritCntn2).HasMaxLength(100).HasColumnName("prddsgncritcntn2");
            builder.Property(e => e.PrdDsgnCritCntn3).HasMaxLength(100).HasColumnName("prddsgncritcntn3");
            builder.Property(e => e.PrdDsgnCritCntn4).HasMaxLength(100).HasColumnName("prddsgncritcntn4");
            builder.Property(e => e.PrdDsgnCritCntn5).HasMaxLength(100).HasColumnName("prddsgncritcntn5");
            builder.Property(e => e.PrdDsgnCritCntn6).HasMaxLength(100).HasColumnName("prddsgncritcntn6");
            builder.Property(e => e.PrdDsgnRsltVal1).HasMaxLength(100).HasColumnName("prddsgnrsltval1");
            builder.Property(e => e.PrdDsgnRsltVal2).HasMaxLength(100).HasColumnName("prddsgnrsltval2");
            builder.Property(e => e.PrdDsgnRsltVal3).HasMaxLength(100).HasColumnName("prddsgnrsltval3");
            builder.Property(e => e.PrdDsgnRsltVal4).HasMaxLength(100).HasColumnName("prddsgnrsltval4");
            builder.Property(e => e.PrdDsgnRsltVal5).HasMaxLength(100).HasColumnName("prddsgnrsltval5");
            builder.Property(e => e.PrdDsgnRsltVal6).HasMaxLength(100).HasColumnName("prddsgnrsltval6");
            builder.Property(e => e.PrdDsgnTestMthd1).HasMaxLength(100).HasColumnName("prddsgntestmthd1");
            builder.Property(e => e.PrdDsgnTestMthd2).HasMaxLength(100).HasColumnName("prddsgntestmthd2");
            builder.Property(e => e.PrdDsgnTestMthd3).HasMaxLength(100).HasColumnName("prddsgntestmthd3");
            builder.Property(e => e.PrdDsgnTestMthd4).HasMaxLength(100).HasColumnName("prddsgntestmthd4");
            builder.Property(e => e.PrdDsgnTestMthd5).HasMaxLength(100).HasColumnName("prddsgntestmthd5");
            builder.Property(e => e.PrdDsgnTestMthd6).HasMaxLength(100).HasColumnName("prddsgntestmthd6");
            builder.Property(e => e.ReusePerfRsltVal).HasMaxLength(100).HasColumnName("reuseperfrsltval");

            // 재활용성 설계원칙 및 경로 (6.1 ~ 6.2)
            builder.Property(e => e.RcycDsgnPrncPhrsCntn).HasColumnType("text").HasColumnName("rcycdsgnprncphrscntn");
            builder.Property(e => e.RcycDsgnPrncItemCntn1).HasMaxLength(100).HasColumnName("rcycdsgnprncitemcntn1");
            builder.Property(e => e.RcycDsgnPrncItemCntn2).HasMaxLength(100).HasColumnName("rcycdsgnprncitemcntn2");
            builder.Property(e => e.RcycDsgnPrncItemCntn3).HasMaxLength(100).HasColumnName("rcycdsgnprncitemcntn3");
            builder.Property(e => e.RcycDsgnPrncItemCntn4).HasMaxLength(100).HasColumnName("rcycdsgnprncitemcntn4");
            builder.Property(e => e.RcycDsgnPrncItemCntn5).HasMaxLength(100).HasColumnName("rcycdsgnprncitemcntn5");
            builder.Property(e => e.RcycDsgnPrncItemCntn6).HasMaxLength(100).HasColumnName("rcycdsgnprncitemcntn6");
            builder.Property(e => e.RcycDsgnPrncEvlRslt1).HasMaxLength(100).HasColumnName("rcycdsgnprncevlrslt1");
            builder.Property(e => e.RcycDsgnPrncEvlRslt2).HasMaxLength(100).HasColumnName("rcycdsgnprncevlrslt2");
            builder.Property(e => e.RcycDsgnPrncEvlRslt3).HasMaxLength(100).HasColumnName("rcycdsgnprncevlrslt3");
            builder.Property(e => e.RcycDsgnPrncEvlRslt4).HasMaxLength(100).HasColumnName("rcycdsgnprncevlrslt4");
            builder.Property(e => e.RcycDsgnPrncEvlRslt5).HasMaxLength(100).HasColumnName("rcycdsgnprncevlrslt5");
            builder.Property(e => e.RcycDsgnPrncEvlRslt6).HasMaxLength(100).HasColumnName("rcycdsgnprncevlrslt6");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn1).HasMaxLength(100).HasColumnName("rcycdsgnprncevltestmthdcntn1");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn2).HasMaxLength(100).HasColumnName("rcycdsgnprncevltestmthdcntn2");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn3).HasMaxLength(100).HasColumnName("rcycdsgnprncevltestmthdcntn3");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn4).HasMaxLength(100).HasColumnName("rcycdsgnprncevltestmthdcntn4");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn5).HasMaxLength(100).HasColumnName("rcycdsgnprncevltestmthdcntn5");
            builder.Property(e => e.RcycDsgnPrncEvlTestMthdCntn6).HasMaxLength(100).HasColumnName("rcycdsgnprncevltestmthdcntn6");
            builder.Property(e => e.RcycPathPhrsCntn).HasColumnType("text").HasColumnName("rcycpathphrscntn");

            // 우려물질 및 중금속관리 (7.1)
            builder.Property(e => e.SocHvyMetMngFixPhrsCntn).HasColumnType("text").HasColumnName("sochvymetmngfixphrscntn");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn1).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltsbstcntn1");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn2).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltsbstcntn2");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn3).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltsbstcntn3");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn4).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltsbstcntn4");
            builder.Property(e => e.SocHvyMetMngTestRsltSbstCntn5).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltsbstcntn5");
            builder.Property(e => e.SocHvyMetMngRsltTot).HasMaxLength(100).HasColumnName("sochvymetmngrslttot");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn1).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltcntn1");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn2).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltcntn2");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn3).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltcntn3");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn4).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltcntn4");
            builder.Property(e => e.SocHvyMetMngTestRsltCntn5).HasMaxLength(100).HasColumnName("sochvymetmngtestrsltcntn5");
            builder.Property(e => e.SocHvyMetMngTestRsltTot).HasMaxLength(100).HasColumnName("sochvymetmngtestrslttot");
            builder.Property(e => e.SocHvyMetMngRegCritCntn1).HasMaxLength(100).HasColumnName("sochvymetmngregcritcntn1");
            builder.Property(e => e.SocHvyMetMngRegCritCntn2).HasMaxLength(100).HasColumnName("sochvymetmngregcritcntn2");
            builder.Property(e => e.SocHvyMetMngRegCritCntn3).HasMaxLength(100).HasColumnName("sochvymetmngregcritcntn3");
            builder.Property(e => e.SocHvyMetMngRegCritCntn4).HasMaxLength(100).HasColumnName("sochvymetmngregcritcntn4");
            builder.Property(e => e.SocHvyMetMngRegCritCntn5).HasMaxLength(100).HasColumnName("sochvymetmngregcritcntn5");
            builder.Property(e => e.SocHvyMetMngRegCritTot).HasMaxLength(100).HasColumnName("sochvymetmngregcrittot");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn1).HasMaxLength(100).HasColumnName("sochvymetmngtestmthdcntn1");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn2).HasMaxLength(100).HasColumnName("sochvymetmngtestmthdcntn2");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn3).HasMaxLength(100).HasColumnName("sochvymetmngtestmthdcntn3");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn4).HasMaxLength(100).HasColumnName("sochvymetmngtestmthdcntn4");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn5).HasMaxLength(100).HasColumnName("sochvymetmngtestmthdcntn5");
            builder.Property(e => e.SocHvyMetMngTestMthdCntn6).HasMaxLength(100).HasColumnName("sochvymetmngtestmthdcntn6");
            builder.Property(e => e.SocHvyMetMngTestRsltPhrs).HasColumnType("text").HasColumnName("sochvymetmngtestrsltphrs");

            // 제조공정 (8)
            builder.Property(e => e.MfrPrcsUrl).HasMaxLength(255).HasColumnName("mfrprcsurl");
            builder.Property(e => e.MfrPrcsCntn).HasColumnType("text").HasColumnName("mfrprcscntn");

            // 품질관리 (9)
            builder.Property(e => e.QltMngInspItemCntn1).HasMaxLength(100).HasColumnName("qltmnginspitemcntn1");
            builder.Property(e => e.QltMngInspItemCntn2).HasMaxLength(100).HasColumnName("qltmnginspitemcntn2");
            builder.Property(e => e.QltMngInspItemCntn3).HasMaxLength(100).HasColumnName("qltmnginspitemcntn3");
            builder.Property(e => e.QltMngInspItemCntn4).HasMaxLength(100).HasColumnName("qltmnginspitemcntn4");
            builder.Property(e => e.QltMngInspItemCntn5).HasMaxLength(100).HasColumnName("qltmnginspitemcntn5");
            builder.Property(e => e.QltMngInspItemCntn6).HasMaxLength(100).HasColumnName("qltmnginspitemcntn6");
            builder.Property(e => e.QltMngInspItemCntn7).HasMaxLength(100).HasColumnName("qltmnginspitemcntn7");
            builder.Property(e => e.QltMngInspMthdCntn1).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn1");
            builder.Property(e => e.QltMngInspMthdCntn2).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn2");
            builder.Property(e => e.QltMngInspMthdCntn3).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn3");
            builder.Property(e => e.QltMngInspMthdCntn4).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn4");
            builder.Property(e => e.QltMngInspMthdCntn5).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn5");
            builder.Property(e => e.QltMngInspMthdCntn6).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn6");
            builder.Property(e => e.QltMngInspMthdCntn7).HasMaxLength(100).HasColumnName("qltmnginspmthdcntn7");
            builder.Property(e => e.QltMngFreqCntn1).HasMaxLength(100).HasColumnName("qltmngfreqcntn1");
            builder.Property(e => e.QltMngFreqCntn2).HasMaxLength(100).HasColumnName("qltmngfreqcntn2");
            builder.Property(e => e.QltMngFreqCntn3).HasMaxLength(100).HasColumnName("qltmngfreqcntn3");
            builder.Property(e => e.QltMngFreqCntn4).HasMaxLength(100).HasColumnName("qltmngfreqcntn4");
            builder.Property(e => e.QltMngFreqCntn5).HasMaxLength(100).HasColumnName("qltmngfreqcntn5");
            builder.Property(e => e.QltMngFreqCntn6).HasMaxLength(100).HasColumnName("qltmngfreqcntn6");
            builder.Property(e => e.QltMngFreqCntn7).HasMaxLength(100).HasColumnName("qltmngfreqcntn7");

            // 준수선언 및 첨부문서 (10 ~ 12)
            builder.Property(e => e.CmplDclCntn).HasColumnType("text").HasColumnName("cmpldclcntn");
            builder.Property(e => e.AtchDocUrl1).HasMaxLength(255).HasColumnName("atchdocurl1");
            builder.Property(e => e.AtchDocUrl2).HasMaxLength(255).HasColumnName("atchdocurl2");
            builder.Property(e => e.AtchDocUrl3).HasMaxLength(255).HasColumnName("atchdocurl3");
            builder.Property(e => e.AtchDocUrl4).HasMaxLength(255).HasColumnName("atchdocurl4");
            builder.Property(e => e.AtchDocUrl5).HasMaxLength(255).HasColumnName("atchdocurl5");
            builder.Property(e => e.AtchDocUrl6).HasMaxLength(255).HasColumnName("atchdocurl6");
            builder.Property(e => e.AtchDocUrl7).HasMaxLength(255).HasColumnName("atchdocurl7");
            builder.Property(e => e.AtchDocUrl8).HasMaxLength(255).HasColumnName("atchdocurl8");
            builder.Property(e => e.AtchDocNm1).HasMaxLength(100).HasColumnName("atchdocnm1");
            builder.Property(e => e.AtchDocNm2).HasMaxLength(100).HasColumnName("atchdocnm2");
            builder.Property(e => e.AtchDocNm3).HasMaxLength(100).HasColumnName("atchdocnm3");
            builder.Property(e => e.AtchDocNm4).HasMaxLength(100).HasColumnName("atchdocnm4");
            builder.Property(e => e.AtchDocNm5).HasMaxLength(100).HasColumnName("atchdocnm5");
            builder.Property(e => e.AtchDocNm6).HasMaxLength(100).HasColumnName("atchdocnm6");
            builder.Property(e => e.AtchDocNm7).HasMaxLength(100).HasColumnName("atchdocnm7");
            builder.Property(e => e.AtchDocNm8).HasMaxLength(100).HasColumnName("atchdocnm8");
            builder.Property(e => e.BizNm2).HasMaxLength(100).HasColumnName("biznm2");
            builder.Property(e => e.RepNm).HasMaxLength(50).HasColumnName("repnm");
            builder.Property(e => e.RoleNm).HasMaxLength(50).HasColumnName("rolenm");
            builder.Property(e => e.EmlAddr).HasMaxLength(100).HasColumnName("emladdr");
            builder.Property(e => e.MbTelNo).HasMaxLength(30).HasColumnName("mbtelno");
            builder.Property(e => e.TechDocLastPhrsCntn).HasColumnType("text").HasColumnName("techdoclastphrscntn");
        }
    }
}