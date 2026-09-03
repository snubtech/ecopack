using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class If200Configuration : IEntityTypeConfiguration<If200>
    {
        public void Configure(EntityTypeBuilder<If200> builder)
        {
            builder.ToTable("if200");
            builder.HasKey(e => e.Idx);

            builder.Property(e => e.Idx).HasColumnName("idx");
            builder.Property(e => e.AsmtShtHdrId).HasMaxLength(50).HasColumnName("asmtshthdrid");
            builder.Property(e => e.LevelTypeNm).HasMaxLength(100).HasColumnName("leveltypenm");
            builder.Property(e => e.VersionDesc).HasMaxLength(255).HasColumnName("versiondesc");
            builder.Property(e => e.HdrMemo).HasColumnType("text").HasColumnName("hdrmemo");
            builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
            builder.Property(e => e.LevelType).HasMaxLength(50).HasColumnName("leveltype");
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
            builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
            builder.Property(e => e.AsmtQstId).HasMaxLength(50).HasColumnName("asmtqstid");
            builder.Property(e => e.EcoPackLarTypeNm).HasMaxLength(100).HasColumnName("ecopacklartypenm");
            builder.Property(e => e.EcoPackAreaNm).HasMaxLength(100).HasColumnName("ecopackareanm");
            builder.Property(e => e.AsmtQstNm).HasColumnType("text").HasColumnName("asmtqstnm");
            builder.Property(e => e.ScoringCriteriaTypeNm).HasMaxLength(100).HasColumnName("scoringcriteriatypenm");
            builder.Property(e => e.NatRglAls).HasColumnType("text").HasColumnName("natrglals");
            builder.Property(e => e.DsgnRecmImp).HasColumnType("text").HasColumnName("dsgnrecmimp");
            builder.Property(e => e.NextAsmtQstId).HasMaxLength(50).HasColumnName("nextasmtqstid");
            builder.Property(e => e.PrtAsmtQstItemId).HasMaxLength(50).HasColumnName("prtasmtqstitemid");
            builder.Property(e => e.RootYn).HasMaxLength(10).HasColumnName("rootyn");
            builder.Property(e => e.EcoPackLarType).HasMaxLength(50).HasColumnName("ecopacklartype");
            builder.Property(e => e.EcoPackArea).HasMaxLength(50).HasColumnName("ecopackarea");
            builder.Property(e => e.ScoringCriteriaType).HasMaxLength(50).HasColumnName("scoringcriteriatype");
            builder.Property(e => e.AsmtQstItemId).HasMaxLength(50).HasColumnName("asmtqstitemid");
            builder.Property(e => e.AsmtQstItemNm).HasColumnType("text").HasColumnName("asmtqstitemnm");
            builder.Property(e => e.ScoringCriteria).HasMaxLength(50).HasColumnName("scoringcriteria");
            builder.Property(e => e.NotReleaseYn).HasMaxLength(10).HasColumnName("notreleaseyn");
            builder.Property(e => e.ItemMemo).HasColumnType("text").HasColumnName("itemmemo");
            builder.Property(e => e.DsgnRecmImpYn).HasMaxLength(10).HasColumnName("dsgnrecmimpyn");
            builder.Property(e => e.DspSeq).HasColumnName("dspseq");
            builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        }
    }
}