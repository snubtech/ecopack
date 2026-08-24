using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class If100Configuration : IEntityTypeConfiguration<If100>
    {
        public void Configure(EntityTypeBuilder<If100> builder)
        {
            builder.ToTable("if100");
            builder.HasKey(e => e.Idx);

            builder.Property(e => e.Idx).HasColumnName("idx");
            builder.Property(e => e.PackDsetId).HasMaxLength(50).HasColumnName("packdsetid");
            builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
            builder.Property(e => e.PrdtPackTypeNm).HasMaxLength(100).HasColumnName("prdtpacktypenm");
            builder.Property(e => e.PackagingBoxTypeNm).HasMaxLength(100).HasColumnName("packagingboxtypenm");
            builder.Property(e => e.ExportCountryNm).HasMaxLength(100).HasColumnName("exportcountrynm");
            builder.Property(e => e.PrdtLength).HasMaxLength(50).HasColumnName("prdtlength");
            builder.Property(e => e.PrdtWidth).HasMaxLength(50).HasColumnName("prdtwidth");
            builder.Property(e => e.PrdtHeight).HasMaxLength(50).HasColumnName("prdtheight");
            builder.Property(e => e.CushMatTypeNm).HasMaxLength(100).HasColumnName("cushmattypenm");
            builder.Property(e => e.PrdtWeight).HasMaxLength(50).HasColumnName("prdtweight");
            builder.Property(e => e.ShipPackWeight).HasMaxLength(50).HasColumnName("shippackweight");
            builder.Property(e => e.RqstPackCnt).HasMaxLength(50).HasColumnName("rqstpackcnt");
            builder.Property(e => e.ReqEmptyRatio).HasMaxLength(50).HasColumnName("reqemptyratio");
            builder.Property(e => e.MinPackVol).HasMaxLength(50).HasColumnName("minpackvol");
            builder.Property(e => e.MinLoadCond).HasMaxLength(50).HasColumnName("minloadcond");
            builder.Property(e => e.MinDropCond).HasMaxLength(50).HasColumnName("mindropcond");
            builder.Property(e => e.MinVibCond).HasMaxLength(50).HasColumnName("minvibcond");
            builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
            builder.Property(e => e.MatUseNm).HasMaxLength(100).HasColumnName("matusenm");
            builder.Property(e => e.MatTypeNm).HasMaxLength(100).HasColumnName("mattypenm");
            builder.Property(e => e.MatFormNm).HasMaxLength(100).HasColumnName("matformnm");
            builder.Property(e => e.PackMmftProcId).HasMaxLength(50).HasColumnName("packmmftprocid");
            builder.Property(e => e.EnvImpAssId).HasMaxLength(50).HasColumnName("envimpassid");
            builder.Property(e => e.PackDsgnTplId).HasMaxLength(50).HasColumnName("packdsgntplid");
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
            builder.Property(e => e.PrdtPackType).HasMaxLength(50).HasColumnName("prdtpacktype");
            builder.Property(e => e.PackagingBoxType).HasMaxLength(50).HasColumnName("packagingboxtype");
            builder.Property(e => e.ExportCountry).HasMaxLength(50).HasColumnName("exportcountry");
            builder.Property(e => e.CushMatType).HasMaxLength(50).HasColumnName("cushmattype");
            builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
            builder.Property(e => e.MatUse).HasMaxLength(50).HasColumnName("matuse");
            builder.Property(e => e.MatType).HasMaxLength(50).HasColumnName("mattype");
            builder.Property(e => e.MatForm).HasMaxLength(50).HasColumnName("matform");
            builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        }
    }
}