using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class AiPkgEvalInfoBscConfiguration : IEntityTypeConfiguration<AiPkgEvalInfoBsc>
    {
        public void Configure(EntityTypeBuilder<AiPkgEvalInfoBsc> builder)
        {
            builder.ToTable("ai_pkg_eval_info_bsc");
            builder.HasKey(e => e.EvalResultId);

            builder.Property(e => e.EvalResultId).HasColumnName("eval_result_id");
            builder.Property(e => e.Prjid).HasMaxLength(50).HasColumnName("prjid").IsRequired();
            builder.Property(e => e.Prjuserid).HasMaxLength(50).HasColumnName("prjuserid").IsRequired();
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packLevel");
            builder.Property(e => e.PackLevelNm).HasMaxLength(50).HasColumnName("packLevelNm");
            builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedMaterial");
            builder.Property(e => e.EcoPackLarType).HasMaxLength(50).HasColumnName("ecoPackLarType").IsRequired();
            builder.Property(e => e.EcoPackAreaNm).HasMaxLength(100).HasColumnName("ecoPackAreaNm");

            // 💡 실제 DB 컬럼명 대소문자 반영 (asmtShtHdrid)
            builder.Property(e => e.asmtShtHdrId).HasMaxLength(50).HasColumnName("asmtShtHdrid");
            builder.Property(e => e.AsmtQstId).HasMaxLength(50).HasColumnName("asmtQstid");
            builder.Property(e => e.AsmtQstItemId).HasMaxLength(50).HasColumnName("asmtQstItemId");
            builder.Property(e => e.PrtAsmtQstItemId).HasMaxLength(50).HasColumnName("prtAsmtQstItemId");

            builder.Property(e => e.AsmtQstNm).HasColumnType("text").HasColumnName("asmtQstNm");
            builder.Property(e => e.AsmtQstItemNm).HasMaxLength(255).HasColumnName("asmtQstItemNm");
            builder.Property(e => e.ScoringCriteria).HasMaxLength(50).HasColumnName("scoringCriteria");
            builder.Property(e => e.Asmtpoint).HasMaxLength(50).HasColumnName("asmtpoint");

            builder.Property(e => e.NatRglAls).HasColumnType("text").HasColumnName("natRglAls");

            // 💡 실제 DB 컬럼명 언더바 반영 (dsgn_recm_imp)
            builder.Property(e => e.DsgnRecmImp).HasColumnType("text").HasColumnName("dsgn_recm_imp");

            builder.Property(e => e.Frstevldtm)
                .HasColumnName("frstevldtm")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.Lastevldtm)
                .HasColumnName("lastevldtm")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
        }
    }
}