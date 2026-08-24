using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class If002aConfiguration : IEntityTypeConfiguration<If002a>
    {
        public void Configure(EntityTypeBuilder<If002a> builder)
        {
            builder.ToTable("if002a");
            builder.HasKey(e => e.Idx);

            builder.Property(e => e.Idx).HasColumnName("idx");
            builder.Property(e => e.PackDsgnTplId).HasMaxLength(50).HasColumnName("packdsgntplid");
            builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
            builder.Property(e => e.MatTypeNm).HasMaxLength(100).HasColumnName("mattypenm");
            builder.Property(e => e.Subject).HasMaxLength(255).HasColumnName("subject");
            builder.Property(e => e.DsgnTypeNm).HasMaxLength(100).HasColumnName("dsgntypenm");
            builder.Property(e => e.DsgnTypeCdVal).HasMaxLength(50).HasColumnName("dsgntypecdval");
            builder.Property(e => e.DsgnExpCon).HasColumnType("text").HasColumnName("dsgnexpcon");
            builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
            builder.Property(e => e.DsgnFeatDscr).HasColumnType("text").HasColumnName("dsgnfeatdscr");
            builder.Property(e => e.OperDscr).HasColumnType("text").HasColumnName("operdscr");
            builder.Property(e => e.MemoImg).HasMaxLength(255).HasColumnName("memoimg");
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
            builder.Property(e => e.MatType).HasMaxLength(50).HasColumnName("mattype");
            builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
            builder.Property(e => e.FileNm).HasMaxLength(255).HasColumnName("filenm");
            builder.Property(e => e.FileData).HasColumnType("longblob").HasColumnName("filedata");
            builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        }
    }
}