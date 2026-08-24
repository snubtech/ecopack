using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ecopack.Api.Data;

namespace ecopack.Api.Data.Configurations
{
	public class If004Configuration : IEntityTypeConfiguration<If004>
	{
		public void Configure(EntityTypeBuilder<If004> builder)
		{
			builder.ToTable("if004");
			builder.HasKey(e => e.Idx);

			builder.Property(e => e.Idx).HasColumnName("idx");
			builder.Property(e => e.NatRegId).HasMaxLength(50).HasColumnName("natregid");
			builder.Property(e => e.PackLevelNm).HasMaxLength(100).HasColumnName("packlevelnm");
			builder.Property(e => e.AppliedMaterialNm).HasMaxLength(100).HasColumnName("appliedmaterialnm");
			builder.Property(e => e.CountryCodeNm).HasMaxLength(100).HasColumnName("countrycodenm");
			builder.Property(e => e.RelatedReg).HasMaxLength(255).HasColumnName("relatedreg");
			builder.Property(e => e.RegItem).HasMaxLength(100).HasColumnName("regitem");
			builder.Property(e => e.DtlCont).HasColumnType("text").HasColumnName("dtlcont");
			builder.Property(e => e.UnitNm).HasMaxLength(50).HasColumnName("unitnm");
			builder.Property(e => e.MinCont).HasMaxLength(50).HasColumnName("mincont");
			builder.Property(e => e.MinOperatorNm).HasMaxLength(50).HasColumnName("minoperatornm");
			builder.Property(e => e.MaxCont).HasMaxLength(50).HasColumnName("maxcont");
			builder.Property(e => e.MaxOperatorNm).HasMaxLength(50).HasColumnName("maxoperatornm");
			builder.Property(e => e.PrepDeadline).HasMaxLength(20).HasColumnName("prepdeadline");
			builder.Property(e => e.PrepDeadlineEnd).HasMaxLength(20).HasColumnName("prepdeadlineend");
			builder.Property(e => e.DecisionOut).HasMaxLength(255).HasColumnName("decisionout");
			builder.Property(e => e.IsRequired).HasMaxLength(10).HasColumnName("isrequired");
			builder.Property(e => e.Memo).HasColumnType("text").HasColumnName("memo");
			builder.Property(e => e.OriginalText).HasColumnType("text").HasColumnName("originaltext");
			builder.Property(e => e.PackLevel).HasMaxLength(50).HasColumnName("packlevel");
			builder.Property(e => e.AppliedMaterial).HasMaxLength(50).HasColumnName("appliedmaterial");
			builder.Property(e => e.CountryCode).HasMaxLength(50).HasColumnName("countrycode");
			builder.Property(e => e.Unit).HasMaxLength(50).HasColumnName("unit");
			builder.Property(e => e.MinOperator).HasMaxLength(50).HasColumnName("minoperator");
			builder.Property(e => e.MaxOperator).HasMaxLength(50).HasColumnName("maxoperator");
			builder.Property(e => e.CreatedAt).HasColumnName("createdat");
		}
	}
}