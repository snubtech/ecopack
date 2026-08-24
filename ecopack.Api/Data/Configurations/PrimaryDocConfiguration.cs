using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class PrimaryDocConfiguration : IEntityTypeConfiguration<PrimaryDoc>
    {
        public void Configure(EntityTypeBuilder<PrimaryDoc> builder)
        {
            builder.ToTable("primarydoc");
            builder.HasKey(e => e.Pkg1DocId);

            builder.Property(e => e.Pkg1DocId).HasMaxLength(50).HasColumnName("pkg1docid");
            builder.Property(e => e.BizNm).HasMaxLength(100).HasColumnName("biznm");
            builder.Property(e => e.RepNm).HasMaxLength(50).HasColumnName("repnm");
            builder.Property(e => e.RoleNm).HasMaxLength(50).HasColumnName("rolenm");
            builder.Property(e => e.EmlAddr).HasMaxLength(100).HasColumnName("emladdr");
            builder.Property(e => e.MbTelNo).HasMaxLength(30).HasColumnName("mbtelno");
            builder.Property(e => e.PrjfNm).HasMaxLength(100).HasColumnName("prjfnm");
            builder.Property(e => e.PrjId).HasMaxLength(50).HasColumnName("prjid");
            builder.Property(e => e.Pkg1TechDocId).HasMaxLength(50).HasColumnName("pkg1techdocid");
            builder.Property(e => e.RevNo).HasMaxLength(20).HasColumnName("revno");
            builder.Property(e => e.CntryNm).HasMaxLength(50).HasColumnName("cntrynm");
            builder.Property(e => e.DsgnTypeNm).HasMaxLength(100).HasColumnName("dsgntypenm");
            builder.Property(e => e.DocPhrsCntn).HasColumnType("text").HasColumnName("docphrscntn");
            builder.Property(e => e.ReuseReqCmplCntn).HasColumnType("text").HasColumnName("reusereqcmplcntn");
            builder.Property(e => e.DsgnTmplMstrPrdExpl).HasColumnType("text").HasColumnName("dsgntmplmstrprdexpl");
            builder.Property(e => e.RcycReqCmplCntn1).HasColumnType("text").HasColumnName("rcycreqcmplcntn1");
            builder.Property(e => e.RcycMainFeatCntn).HasColumnType("text").HasColumnName("rcycmainfeatcntn");
            builder.Property(e => e.RcycReqCmplCntn2).HasColumnType("text").HasColumnName("rcycreqcmplcntn2");
            builder.Property(e => e.SoChvyMetLmtCmplCntn1).HasColumnType("text").HasColumnName("sochvymetlmtcmplcntn1");
            builder.Property(e => e.Sbst1).HasMaxLength(100).HasColumnName("sbst1");
            builder.Property(e => e.Sbst2).HasMaxLength(100).HasColumnName("sbst2");
            builder.Property(e => e.Sbst3).HasMaxLength(100).HasColumnName("sbst3");
            builder.Property(e => e.Sbst4).HasMaxLength(100).HasColumnName("sbst4");
            builder.Property(e => e.SbstTot).HasMaxLength(100).HasColumnName("sbsttot");
            builder.Property(e => e.TestRslt1).HasMaxLength(100).HasColumnName("testrslt1");
            builder.Property(e => e.TestRslt2).HasMaxLength(100).HasColumnName("testrslt2");
            builder.Property(e => e.TestRslt3).HasMaxLength(100).HasColumnName("testrslt3");
            builder.Property(e => e.TestRslt4).HasMaxLength(100).HasColumnName("testrslt4");
            builder.Property(e => e.TestRsltTot).HasMaxLength(100).HasColumnName("testrslttot");
            builder.Property(e => e.Compltem1).HasMaxLength(100).HasColumnName("compltem1");
            builder.Property(e => e.Compltem2).HasMaxLength(100).HasColumnName("compltem2");
            builder.Property(e => e.Compltem3).HasMaxLength(100).HasColumnName("compltem3");
            builder.Property(e => e.Mat1).HasMaxLength(100).HasColumnName("mat1");
            builder.Property(e => e.Mat2).HasMaxLength(100).HasColumnName("mat2");
            builder.Property(e => e.Mat3).HasMaxLength(100).HasColumnName("mat3");
            builder.Property(e => e.MatInfoTotWtVal).HasMaxLength(50).HasColumnName("matinfototwtval");
            builder.Property(e => e.MatInfoCntn).HasColumnType("text").HasColumnName("matinfocntn");
            builder.Property(e => e.EvdDocCntn).HasColumnType("text").HasColumnName("evddoccntn");
            builder.Property(e => e.EvdDocUrl1).HasMaxLength(255).HasColumnName("evddocurl1");
            builder.Property(e => e.EvdDocUrl2).HasMaxLength(255).HasColumnName("evddocurl2");
            builder.Property(e => e.EvdDocUrl3).HasMaxLength(255).HasColumnName("evddocurl3");
            builder.Property(e => e.EvdDocUrl4).HasMaxLength(255).HasColumnName("evddocurl4");
            builder.Property(e => e.EvdDocUrl5).HasMaxLength(255).HasColumnName("evddocurl5");
            builder.Property(e => e.EvdDocUrl6).HasMaxLength(255).HasColumnName("evddocurl6");
            builder.Property(e => e.EvdDocUrl7).HasMaxLength(255).HasColumnName("evddocurl7");
            builder.Property(e => e.EvdDocUrl8).HasMaxLength(255).HasColumnName("evddocurl8");
            builder.Property(e => e.EvdDocNm1).HasMaxLength(100).HasColumnName("evddocnm1");
            builder.Property(e => e.EvdDocNm2).HasMaxLength(100).HasColumnName("evddocnm2");
            builder.Property(e => e.EvdDocNm3).HasMaxLength(100).HasColumnName("evddocnm3");
            builder.Property(e => e.EvdDocNm4).HasMaxLength(100).HasColumnName("evddocnm4");
            builder.Property(e => e.EvdDocNm5).HasMaxLength(100).HasColumnName("evddocnm5");
            builder.Property(e => e.EvdDocNm6).HasMaxLength(100).HasColumnName("evddocnm6");
            builder.Property(e => e.EvdDocNm7).HasMaxLength(100).HasColumnName("evddocnm7");
            builder.Property(e => e.EvdDocNm8).HasMaxLength(100).HasColumnName("evddocnm8");
            builder.Property(e => e.ApplRuleStdCntn).HasColumnType("text").HasColumnName("applrulestdcntn");
            builder.Property(e => e.LastDclCntn).HasColumnType("text").HasColumnName("lastdclcntn");
            builder.Property(e => e.LastWrtDt).HasColumnName("lastwrtdt");
            builder.Property(e => e.BizNm2).HasMaxLength(100).HasColumnName("biznm2");
            builder.Property(e => e.RepNm2).HasMaxLength(50).HasColumnName("repnm2");
            builder.Property(e => e.RoleNm2).HasMaxLength(50).HasColumnName("rolenm2");
        }
    }
}