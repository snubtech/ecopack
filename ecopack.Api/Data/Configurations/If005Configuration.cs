using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
	public class If005Configuration : IEntityTypeConfiguration<If005>
	{
		public void Configure(EntityTypeBuilder<If005> builder)
		{
			builder.ToTable("if005");
			builder.HasKey(e => e.Idx);

			builder.Property(e => e.Idx).HasColumnName("idx");
			builder.Property(e => e.EnvImpAssId).HasMaxLength(50).HasColumnName("envimpassid");
			builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
			builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
			builder.Property(e => e.MatFormNm).HasMaxLength(100).HasColumnName("matformnm");
			builder.Property(e => e.MassCo2Mat).HasMaxLength(50).HasColumnName("massco2mat");
			builder.Property(e => e.MassCo2Proc).HasMaxLength(50).HasColumnName("massco2proc");
			builder.Property(e => e.MassCo2Scrap).HasMaxLength(50).HasColumnName("massco2scrap");
			builder.Property(e => e.MassCo2Sum).HasMaxLength(50).HasColumnName("massco2sum");
			builder.Property(e => e.UnitCo2Mat).HasMaxLength(50).HasColumnName("unitco2mat");
			builder.Property(e => e.UnitCo2Proc).HasMaxLength(50).HasColumnName("unitco2proc");
			builder.Property(e => e.UnitCo2Scrap).HasMaxLength(50).HasColumnName("unitco2scrap");
			builder.Property(e => e.UnitCo2Sum).HasMaxLength(50).HasColumnName("unitco2sum");
			builder.Property(e => e.UnitCo2MgtVal).HasMaxLength(50).HasColumnName("unitco2mgtval");
			builder.Property(e => e.AreaDensity).HasMaxLength(50).HasColumnName("areadensity");
			builder.Property(e => e.Density).HasMaxLength(50).HasColumnName("density");
			builder.Property(e => e.MatCompCon).HasColumnType("text").HasColumnName("matcompcon");
			builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
			builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
			builder.Property(e => e.MatForm).HasMaxLength(50).HasColumnName("matform");
			builder.Property(e => e.CreatedAt).HasColumnName("createdat");
		}
	}
}