using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
    public class If001Configuration : IEntityTypeConfiguration<If001>
    {
        public void Configure(EntityTypeBuilder<If001> builder)
        {
            builder.ToTable("if001");
            builder.HasKey(e => e.Idx);

            builder.Property(e => e.Idx).HasColumnName("idx");
            builder.Property(e => e.MatPrtBasId).HasMaxLength(50).HasColumnName("matprtbasid");
            builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
            builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
            builder.Property(e => e.MatUseNm).HasMaxLength(100).HasColumnName("matusenm");
            builder.Property(e => e.MatTypeNm).HasMaxLength(100).HasColumnName("mattypenm");
            builder.Property(e => e.MatFormNm).HasMaxLength(100).HasColumnName("matformnm");
            builder.Property(e => e.ItemNm).HasMaxLength(100).HasColumnName("itemnm");
            builder.Property(e => e.UnitNm).HasMaxLength(50).HasColumnName("unitnm");
            builder.Property(e => e.AcceptableRange).HasMaxLength(100).HasColumnName("acceptablerange");
            builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
            builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
            builder.Property(e => e.MatUse).HasMaxLength(50).HasColumnName("matuse");
            builder.Property(e => e.MatType).HasMaxLength(50).HasColumnName("mattype");
            builder.Property(e => e.MatForm).HasMaxLength(50).HasColumnName("matform");
            builder.Property(e => e.Item).HasMaxLength(50).HasColumnName("item");
            builder.Property(e => e.Unit).HasMaxLength(50).HasColumnName("unit");
            builder.Property(e => e.CreatedAt).HasColumnName("createdat");
        }
    }
}